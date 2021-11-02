
namespace MedEasy.ContinuousIntegration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Nuke.Common;
    using Nuke.Common.CI;
    using Nuke.Common.CI.AzurePipelines;
    using Nuke.Common.CI.GitHubActions;
    using Nuke.Common.Execution;
    using Nuke.Common.Git;
    using Nuke.Common.IO;
    using Nuke.Common.ProjectModel;
    using Nuke.Common.Tooling;
    using Nuke.Common.Tools.Coverlet;
    using Nuke.Common.Tools.DotNet;
    using Nuke.Common.Tools.EntityFramework;
    using Nuke.Common.Tools.GitVersion;
    using Nuke.Common.Tools.ReportGenerator;
    using Nuke.Common.Utilities;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    using static Nuke.Common.ChangeLog.ChangelogTasks;
    using static Nuke.Common.IO.FileSystemTasks;
    using static Nuke.Common.IO.PathConstruction;
    using static Nuke.Common.IO.TextTasks;
    using static Nuke.Common.Logger;
    using static Nuke.Common.Tools.DotNet.DotNetTasks;
    using static Nuke.Common.Tools.EntityFramework.EntityFrameworkTasks;
    using static Nuke.Common.Tools.Git.GitTasks;
    using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

    [GitHubActions(
        "continuous",
        GitHubActionsImage.WindowsLatest,
        OnPushBranches = new[] { DevelopBranch, FeatureBranchPrefix + "/*"},
        OnPushBranchesIgnore = new[] { MainBranchName },
        OnPullRequestBranches = new[] { DevelopBranch },
        PublishArtifacts = true,
        InvokedTargets = new[] { nameof(UnitTests) },
        CacheKeyFiles = new[] { "global.json", "Nuget.config" },
        ImportGitHubTokenAs = nameof(GitHubToken),
        ImportSecrets = new[]
        {
            nameof(NugetApiKey)
        },
        OnPullRequestExcludePaths = new[]
        {
            "**/*.md",
            "LICENCE",
            "docs/*"
        }
    )]
    [GitHubActions(
        "deployment",
        GitHubActionsImage.WindowsLatest,
        OnPushBranches = new[] { MainBranchName, ReleaseBranchPrefix + "/*" },
        InvokedTargets = new[] { nameof(Tests), nameof(Publish) },
        CacheKeyFiles = new[] { "global.json", "Nuget.config", ".config/dotnet-tools.json" },
        ImportGitHubTokenAs = nameof(GitHubToken),
        ImportSecrets = new[]
                        {
                            nameof(NugetApiKey)
                        },
        OnPullRequestExcludePaths = new[]
        {
            "**/*.md",
            "LICENCE",
            "docs/*"
        }
    )]
    [CheckBuildProjectConfigurations]
    [UnsetVisualStudioEnvironmentVariables]
    public partial class Build : NukeBuild
    {
        public static int Main() => Execute<Build>(x => x.Compile);

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Parameter("Indicates wheter to restore nuget in interactive mode - Default is false")]
        public readonly bool Interactive = false;

        [Required] [Solution] public readonly Solution Solution;
        [Required] [GitRepository] public readonly GitRepository GitRepository;
        [Required] [GitVersion(Framework = "net5.0")] public readonly GitVersion GitVersion;


        [Partition(3)] public readonly Partition TestPartition;


        private AbsolutePath DotnetToolsConfigDirectory => RootDirectory / ".config";

        public AbsolutePath DotnetToolsLocalConfigFile => DotnetToolsConfigDirectory / "dotnet-tools.json";

        public AbsolutePath SourceDirectory => RootDirectory / "src";

        public AbsolutePath TestDirectory => RootDirectory / "test";

        public AbsolutePath OutputDirectory => RootDirectory / "output";

        public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

        public AbsolutePath CoverageReportUnitTestsDirectory => CoverageReportDirectory / "unit-tests";

        public AbsolutePath CoverageReportIntegrationTestsDirectory => CoverageReportDirectory / "integration-tests";

        public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

        public AbsolutePath IntegrationTestsResultDirectory => TestResultDirectory / "integration-tests";

        public AbsolutePath UnitTestsResultDirectory => TestResultDirectory / "unit-tests";

        public AbsolutePath ArtifactsDirectory => OutputDirectory / "artifacts";

        public AbsolutePath ScriptsDirectory => OutputDirectory / "scripts";

        public AbsolutePath SqlScriptsDirectory => ScriptsDirectory / "sql";

        public AbsolutePath CoverageReportHistoryDirectory => OutputDirectory / "coverage-history";

        /// <summary>
        /// Path to code coverage report history for unit tests
        /// </summary>
        public AbsolutePath CoverageReportUnitTestsHistoryDirectory => CoverageReportHistoryDirectory / "unit-tests";

        /// <summary>
        /// Path to code coverage report history for integration tests
        /// </summary>
        public AbsolutePath CoverageReportintegrationTestsHistoryDirectory => CoverageReportHistoryDirectory / "integration-tests";

        /// <summary>
        /// Path to the folder that contains databases used by integration tests.
        /// </summary>
        public AbsolutePath DatabaseFolder => OutputDirectory / "databases";

        /// <summary>
        /// Path to the tye configuration file.
        /// </summary>
        public AbsolutePath TyeConfigurationFile => RootDirectory / "tye.yaml";

        public const string MainBranchName = "main";

        public const string DevelopBranch = "develop";

        public const string FeatureBranchPrefix = "feature";

        public const string HotfixBranchPrefix = "hotfix";

        public const string ColdfixBranchPrefix = "coldfix";

        public const string ReleaseBranchPrefix = "release";

        [Parameter("Indicates if any changes should be stashed automatically prior to switching branch (Default : true)")]
        public readonly bool AutoStash = true;

        [PackageExecutable("microsoft.tye", "tye.dll")]
        public readonly Tool Tye;

        [Parameter("Token required when publishing artifacts to GitHub")]
        public readonly string GitHubToken;

        [Parameter("Defines which services should start when calling 'run' command (agenda, identity, documents, patients, measures)."
            + "You can also use 'backends' to start all apis or 'datastores' to start all databases at once)"
        )]
        public readonly MedEasyServices[] Services = { MedEasyServices.Backends };

        [Parameter("Indicates to watch code source changes. Used when calling 'run' target")]
        public readonly bool Watch = false;

        [Parameter("Services to debug")]
        public readonly MedEasyServices[] DebugServices = { };

        [Parameter("Generic name placeholder. Can be used wherever a name is required")]
        public readonly string Name;

        public Target Clean => _ => _
            .Executes(() =>
            {
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                EnsureCleanDirectory(ArtifactsDirectory);
                EnsureCleanDirectory(CoverageReportUnitTestsDirectory);
                EnsureCleanDirectory(CoverageReportIntegrationTestsDirectory);

                EnsureExistingDirectory(CoverageReportintegrationTestsHistoryDirectory);
                EnsureExistingDirectory(CoverageReportUnitTestsHistoryDirectory);
            });

        public Target Restore => _ => _
            .Executes(() =>
            {
                DotNetRestore(s => s
                    .SetProjectFile(Solution)
                    .SetIgnoreFailedSources(true)
                    .SetDisableParallel(false)
                    .When(IsLocalBuild && Interactive, _ => _.SetProperty("NugetInteractive", IsLocalBuild && Interactive))
                );

                DotNetToolRestore(s => s
                    .SetIgnoreFailedSources(true));
            });

        public Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetNoRestore(InvokedTargets.Contains(Restore))
                    .SetConfiguration(Configuration)
                    .SetProjectFile(Solution)
                    );
            });

        public Target UnitTests => _ => _
            .DependsOn(Compile)
            .Description("Run unit tests and collect code coverage")
            .Produces(UnitTestsResultDirectory / "*.trx")
            .Produces(UnitTestsResultDirectory / "*.xml")
            .Produces(CoverageReportUnitTestsDirectory / "*.xml")
            .Executes(() =>
            {
                IEnumerable<Project> projects = Solution.GetProjects("*.UnitTests");
                IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

                testsProjects.ForEach(project => Info(project));

                DotNetTest(s => s
                    .SetConfiguration(Configuration)
                    .EnableCollectCoverage()
                    .SetNoBuild(InvokedTargets.Contains(Compile))
                    .SetResultsDirectory(UnitTestsResultDirectory)
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                        .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                            .SetFramework(framework)
                            .SetLoggers($"trx;LogFileName={project.Name}.{framework}.trx")
                            .SetCollectCoverage(true)
                            .SetCoverletOutput(UnitTestsResultDirectory / $"{project.Name}.xml"))
                        )
                );


                // TODO Move this to a separate "coverage" target once https://github.com/nuke-build/nuke/issues/562 is solved !
                ReportGenerator(_ => _
                        .SetFramework("net5.0")
                        .SetReports(UnitTestsResultDirectory / "*.xml")
                        .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlChart, ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTargetDirectory(CoverageReportUnitTestsDirectory)
                        .SetHistoryDirectory(CoverageReportUnitTestsHistoryDirectory)
                    );
            });

        public Target CleanDatabaseFolder => _ => _
            .Unlisted()
            .Description($"Cleans '{DatabaseFolder}'")
            .Executes(() =>
            {
                EnsureCleanDirectory(DatabaseFolder);
            });

        [Parameter("Indicates if the connection strings should be updated in appsettings.integrationtest.json file (Default = true)")]
        public readonly bool UpdateConnectionString = true;

        public Target UpdateDatabases => _ => _
            .Description("Applies any pending migrations on databases")
            .DependsOn(Compile, CleanDatabaseFolder)
            .Produces(DatabaseFolder / "*.db")
            .Executes(async () =>
            {
                const string connectionStringsPropertyName = "ConnectionStrings";
                Project[] datastoresProjects = Solution.AllProjects
                                                       .Where(project => project.Name.Like("*.datastores", true)
                                                                         || project.Name.Like("*.context", true))
                                                       .OrderBy(project => project.Name)
                                                       .ToArray();
                foreach (Project datastoreProject in datastoresProjects)
                {
                    Info($"Updating database associated with '{datastoreProject.Name}'");
                    string databaseName = datastoreProject.Name.Replace(".Context", string.Empty)
                                                               .Replace(".DataStores", string.Empty);

                    string apiProjectName = datastoreProject.Name.Replace(".Context", ".API")
                                                                 .Replace(".DataStores", ".API");

                    string connectionString = DatabaseFolder / $"{databaseName}.db".ToLowerInvariant();
                    Project apiProject = Solution.GetProject(apiProjectName);
                    Info($"API project is '{apiProjectName}' ({apiProject.Path})");
                    string dataSource = string.Empty;
                    if (UpdateConnectionString)
                    {
                        Info("Updating appsettings.IntegrationTest.json file");
                        AbsolutePath appSettingsFilePath = apiProject.Path.Parent.GlobFiles("appsettings.*.json")
                                                                       .SingleOrDefault(file => new FileInfo(file).Name.Like("appsettings.IntegrationTest.json", true));
                        if (appSettingsFilePath is not null)
                        {
                            string appSettingsJson = await File.ReadAllTextAsync(appSettingsFilePath)
                                                               .ConfigureAwait(false);
                            JObject appSettings = JObject.Parse(appSettingsJson);
                            JObject connectionStrings = appSettings[connectionStringsPropertyName].As<JObject>() ?? new JObject();
                            dataSource = @$"DataSource=""{connectionString}""";
                            connectionStrings.TryAdd($"{databaseName}", dataSource);
                            appSettings.Remove(connectionStringsPropertyName);
                            appSettings.Add(connectionStringsPropertyName, connectionStrings);

                            string tempFileName = Path.GetRandomFileName();
                            Log(LogLevel.Trace, $"Generating temporary file '{tempFileName}'");

                            await File.WriteAllLinesAsync(tempFileName, new[] { JsonConvert.SerializeObject(appSettings, Formatting.Indented) })
                                      .ConfigureAwait(false);

                            File.Replace(tempFileName, appSettingsFilePath, null);
                        }
                        else
                        {
                            Warn("'appsettings.integrationTest.json' file not found. ");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(dataSource))
                    {
                        Info("Pending migrations : ");
                        EntityFrameworkMigrationsList(_ => _
                            .SetProject(datastoreProject)
                            .SetStartupProject(apiProject)
                            .When(!SkippedTargets.Contains(Compile), _ => _.EnableNoBuild())
                            .SetProcessArgumentConfigurator(args => args.Add($@"-- --connectionstrings:{databaseName}=""{dataSource}"""))
                            .SetProcessEnvironmentVariable("DOTNET_ENVIRONMENT", "IntegrationTest")
                        );

                        Info($"Updating '{databaseName}' database");

                        EntityFrameworkDatabaseUpdate(_ => _
                            .SetStartupProject(apiProject)
                            .SetProject(datastoreProject)
                            .SetProcessWorkingDirectory(datastoreProject.Path.Parent)
                            .ToggleJson()
                            .When(!SkippedTargets.Contains(Compile), _ => _.EnableNoBuild())
                            .SetProcessArgumentConfigurator(args => args.Add($@"-- --connectionstrings:{databaseName}=""{dataSource}"""))
                            .SetProcessEnvironmentVariable("DOTNET_ENVIRONMENT", "IntegrationTest")
                        );

                        Info($"'{databaseName}' database updated");
                    }
                }
            });

        public Target IntegrationTests => _ => _
            .DependsOn(Compile, UpdateDatabases)
            .Description("Run integration tests and collect code coverage")
            .Produces(IntegrationTestsResultDirectory / "*.trx")
            .Produces(IntegrationTestsResultDirectory / "*.xml")
            .Produces(CoverageReportIntegrationTestsDirectory / "*.xml")
            .Executes(() =>
            {


                IEnumerable<Project> projects = Solution.GetProjects("*.IntegrationTests");
                IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

                testsProjects.ForEach(project => Info(project));

                DotNetTest(s => s
                    .SetConfiguration(Configuration)
                    .EnableCollectCoverage()
                    .SetNoBuild(InvokedTargets.Contains(Compile) || InvokedTargets.Contains(UnitTests))
                    .SetResultsDirectory(IntegrationTestsResultDirectory)
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                        .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                            .SetFramework(framework)
                            .SetLoggers($"trx;LogFileName={project.Name}.{framework}.trx")
                            .SetCollectCoverage(true)
                            .SetCoverletOutput(IntegrationTestsResultDirectory / $"{project.Name}.xml"))
                        )
                );

                // TODO Move this to a separate "coverage" target once https://github.com/nuke-build/nuke/issues/562 is solved !
                ReportGenerator(_ => _
                        .SetFramework("net5.0")
                        .SetReports(IntegrationTestsResultDirectory / "*.xml")
                        .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlChart, ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTargetDirectory(CoverageReportIntegrationTestsDirectory)
                        .SetHistoryDirectory(CoverageReportintegrationTestsHistoryDirectory)
                    );
            });

        public Target Tests => _ => _
            .DependsOn(UnitTests, IntegrationTests)
            .Description("Run all configured tests")
            .Executes(() =>
            {

            });

        public Target Pack => _ => _
            .DependsOn(Tests, Compile)
            .Consumes(Compile)
            .Produces(ArtifactsDirectory / "*.nupkg")
            .Produces(ArtifactsDirectory / "*.snupkg")
            .Executes(() =>
            {
                DotNetPack(s => s
                    .EnableIncludeSource()
                    .EnableIncludeSymbols()
                    .SetOutputDirectory(ArtifactsDirectory)
                    .SetProject(Solution)
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetFileVersion(GitVersion.AssemblySemFileVer)
                    .SetInformationalVersion(GitVersion.InformationalVersion)
                    .SetVersion(GitVersion.NuGetVersion)
                    .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                    .SetPackageReleaseNotes(GetNuGetReleaseNotes(ChangeLogFile, GitRepository))
                );
            });

        [Parameter("API key used to publish artifacts to Nuget.org")]
        public readonly string NugetApiKey;

        [Parameter(@"URI where packages should be published (default : ""https://api.nuget.org/v3/index.json""")]
        public string NugetPackageSource => "https://api.nuget.org/v3/index.json";

        public Target Publish => _ => _
            .Description($"Published packages (*.nupkg and *.snupkg) to the destination server set with {nameof(NugetPackageSource)} settings ")
            .DependsOn(UnitTests, Pack)
            .Consumes(Pack, ArtifactsDirectory / "*.nupkg", ArtifactsDirectory / "*.snupkg")
            .Requires(() => !NugetApiKey.IsNullOrEmpty())
            .Requires(() => GitHasCleanWorkingCopy())
            .Requires(() => GitRepository.Branch == MainBranchName
                            || GitRepository.IsOnReleaseBranch()
                            || GitRepository.IsOnDevelopBranch())
            .Requires(() => Configuration.Equals(Configuration.Release))
            .Executes(() =>
            {
                void PushPackages(IReadOnlyCollection<AbsolutePath> nupkgs)
                {
                    Info($"Publishing {nupkgs.Count} package{(nupkgs.Count > 1 ? "s" : string.Empty)}");
                    Info(string.Join(EnvironmentInfo.NewLine, nupkgs));

                    DotNetNuGetPush(s => s.SetApiKey(NugetApiKey)
                        .SetSource(NugetPackageSource)
                        .EnableSkipDuplicate()
                        .EnableNoSymbols()
                        .SetProcessLogTimestamp(true)
                        .CombineWith(nupkgs, (_, nupkg) => _
                                    .SetTargetPath(nupkg)),
                        degreeOfParallelism: 4,
                        completeOnFailure: true);
                }

                PushPackages(ArtifactsDirectory.GlobFiles("*.nupkg"));
                PushPackages(ArtifactsDirectory.GlobFiles("*.snupkg"));
            });

        public Target Run => _ => _
            .Requires(() => IsLocalBuild)
            .Description("Run services using Tye.")
            .DependsOn(Compile, Restore)
            .Executes(() =>
            {
                string command = string.Empty;
                if (Services.AtLeastOnce())
                {
                    string services = string.Join(' ', Services.Select(s => $"{s}"));
                    command = $"--tags {services.ToLowerInvariant()}";

                }

                string debug = null;
                if (DebugServices.AtLeastOnce())
                {
                    debug = $"--debug {string.Join(' ', DebugServices.Select(s => $"{s}")).ToLowerInvariant()}";
                }


                Tye($"run {debug} {command} --dashboard --dtrace zipkin=http://localhost:59411 --logs seq=http://localhost:55341 { (Watch ? "--watch" : string.Empty)}");
            });

        public Target StartMessageBus => _ => _
            .Description($"Starts message bus service by reading required configuration from '{TyeConfigurationFile}'")
            .Executes(() =>
            {
                string yaml = ReadAllText(TyeConfigurationFile);

                IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties()
                                                                      .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                                      .Build();

                TyeConfiguration tyeConfiguration = deserializer.Deserialize<TyeConfiguration>(yaml);

                Info($"Tye content : {tyeConfiguration.Jsonify()}");

                IEnumerable<TyeServiceConfiguration> services = tyeConfiguration.Services;
                var maybeMessageBus = services.SingleOrDefault(service => service.Name == "message-bus");

            });

        [PathExecutable]
        public readonly Tool Npx;
        public Target TypeScriptModels => _ => _
            .Description("Generates Typescript definition files")
            .Executes(() =>
            {
                Npx("swagger-typescript-api -p https://api-dev.devaktome.fr/swagger/v1/swagger.json --axios");
            });
    }
}