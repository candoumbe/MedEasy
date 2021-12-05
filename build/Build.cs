
namespace MedEasy.ContinuousIntegration
{
    using Newtonsoft.Json;

    using Nuke.Common;
    using Nuke.Common.CI;
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
    using Nuke.Common.Tools.Npm;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    using static Nuke.Common.ChangeLog.ChangelogTasks;
    using static Nuke.Common.IO.FileSystemTasks;
    using static Nuke.Common.IO.PathConstruction;
    using static Nuke.Common.IO.TextTasks;
    using static Nuke.Common.Logger;
    using static Nuke.Common.Tools.DotNet.DotNetTasks;
    using static Nuke.Common.Tools.Npm.NpmTasks;
    using static Nuke.Common.Tools.EntityFramework.EntityFrameworkTasks;
    using static Nuke.Common.Tools.Git.GitTasks;
    using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

    [GitHubActions(
        "continuous",
        GitHubActionsImage.UbuntuLatest,
        OnPushBranchesIgnore = new[] { MainBranchName },
        OnPullRequestBranches = new[] { DevelopBranch },
        PublishArtifacts = true,
        InvokedTargets = new[] { nameof(Compile), nameof(IntegrationTests) },
        CacheKeyFiles = new[] { "global.json", "nuget.config", ".config/dotnet-tools.json" },
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
        GitHubActionsImage.UbuntuLatest,
        OnPushBranches = new[] { MainBranchName, ReleaseBranchPrefix + "/*" },
        InvokedTargets = new[] { nameof(Compile), nameof(Tests), nameof(Publish) },
        CacheKeyFiles = new[] { "global.json", "nuget.config", ".config/dotnet-tools.json", "**/*.csproj" },
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
        private Solution _ciSolution;
        [Required] [GitRepository] public readonly GitRepository GitRepository;
        [Required] [GitVersion(Framework = "net5.0")] public readonly GitVersion GitVersion;


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

        public AbsolutePath ServicesDirectory => SourceDirectory / "services";

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

        public AbsolutePath ConnectionsFile => OutputDirectory / "connections.dat";

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
                    .EnableIgnoreFailedSources()
                    .SetDisableParallel(false)
                    .SetProjectFile(Solution)
                );

                DotNetToolRestore(s => s
                    .SetIgnoreFailedSources(true));
            });

        /// <summary>
        /// This target will generates a sln file that only contains "buildable"
        /// csproj files
        /// </summary>
        public Target GenerateGlobalSolution => _ => _
              .Before(Compile)
              .Unlisted()
              .Executes(() =>
              {
                  _ciSolution = ProjectModelTasks.CreateSolution($"{ Solution.Directory / Solution.Name}.CI.sln", Solution);
                  IEnumerable<Project> projectsToRemove = _ciSolution.AllProjects
                                                                    .Where(proj => !proj.Is(ProjectType.CSharpProject));

                  projectsToRemove.ForEach(proj => _ciSolution.RemoveProject(proj));

                  _ciSolution.Save();
              });


        public Target Compile => _ => _
            .DependsOn(Restore, GenerateGlobalSolution)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetNoRestore(SucceededTargets.Contains(Restore))
                    .SetConfiguration(Configuration)
                    .SetProjectFile(_ciSolution));
            });

        public Target UnitTests => _ => _
            .DependsOn(Compile)
            .Partition(5)
            .Description("Run unit tests and collect code coverage")
            .Produces(UnitTestsResultDirectory / "*.trx")
            .Produces(UnitTestsResultDirectory / "*.xml")
            .Produces(CoverageReportUnitTestsDirectory / "*.xml")
            .Executes(() =>
            {
                IEnumerable<Project> projects = Solution.GetProjects("*.UnitTests");
                IEnumerable<Project> testsProjects = Partition.GetCurrent(projects);

                testsProjects.ForEach(project => Info(project));

                DotNetTest(s => s
                    .SetConfiguration(Configuration)
                    .EnableCollectCoverage()
                    .SetNoBuild(SucceededTargets.Contains(Compile))
                    .SetNoRestore(SucceededTargets.Contains(Compile) || SucceededTargets.Contains(Restore))
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

        [Parameter("Indicates if the connection strings should be updated in appsettings.Integrationtest.json file (Default = true)")]
        public readonly bool UpdateConnectionString = IsServerBuild;

        public Target UpdateDatabases => _ => _
            .Description("Applies any pending migrations on databases")
            .Consumes(Compile)
            .DependsOn(Compile, CleanDatabaseFolder)
            .Produces(DatabaseFolder / "*.db",
                      ConnectionsFile)
            .Executes(() =>
            {
                Project[] datastoresProjects = Solution.AllProjects
                                                       .Where(project => project.Name.Like("*.datastores", true)
                                                                         || project.Name.Like("*.context", true))
                                                       .OrderBy(project => project.Name)
                                                       .ToArray();

                IList<(string service, string connectionString)> connections = new List<(string, string)>(datastoresProjects.Length);

                foreach (Project datastoreProject in datastoresProjects)
                {
                    Info($"Updating database associated with '{datastoreProject.Name}'");
                    string databaseName = datastoreProject.Name.Replace(".Context", string.Empty)
                                                               .Replace(".DataStores", string.Empty);

                    string apiProjectName = datastoreProject.Name.Replace(".Context", ".API")
                                                                 .Replace(".DataStores", ".API");

                    string sqliteConnectionString = DatabaseFolder / $"{databaseName}.db".ToLowerInvariant();
                    Project apiProject = Solution.GetProject(apiProjectName);
                    Info($"API project is '{apiProjectName}' ({apiProject.Path})");
                    string dataSource = string.Empty;
                    if (IsServerBuild)
                    {
                        dataSource = @$"DataSource=""{sqliteConnectionString}""";

                        connections.Add((apiProjectName.Replace(".API", string.Empty, StringComparison.OrdinalIgnoreCase), connectionString: dataSource));

                        Info("Pending migrations : ");
                        EntityFrameworkMigrationsList(_ => _
                            .SetProject(datastoreProject)
                            .SetStartupProject(apiProject)
                            .SetProcessToolPath(DotNetPath)
                            .SetProcessArgumentConfigurator(args => args.Add($@"-- --connectionstrings:{databaseName}=""{dataSource}"""))
                            .SetProcessEnvironmentVariable("DOTNET_ENVIRONMENT", "IntegrationTest")
                        );

                        Info($"Updating '{databaseName}' database");

                        EntityFrameworkDatabaseUpdate(_ => _
                            .SetStartupProject(apiProject)
                            .SetProject(datastoreProject)
                            .SetProcessWorkingDirectory(datastoreProject.Path.Parent)
                            .ToggleJson()
                            .SetProcessToolPath(DotNetPath)
                            .SetProcessArgumentConfigurator(args => args.Add($@"-- --connectionstrings:{databaseName}=""{dataSource}"""))
                            .SetProcessEnvironmentVariable("DOTNET_ENVIRONMENT", "IntegrationTest")
                        );

                        Info($"'{databaseName}' database updated");

                    }

                }

                string[] lines = connections.Select(item => $"{item.service}|{item.connectionString}")
                                                       .ToArray();
                WriteAllLines(ConnectionsFile, lines);

                connections.ForEach(line => Info($"Connection string -> '{line}'"));
            });

        public Target IntegrationTests => _ => _
            .DependsOn(Compile, UpdateDatabases)
            .Consumes(UpdateDatabases, ConnectionsFile)
            .Description("Run integration tests and collect code coverage")
            .Produces(IntegrationTestsResultDirectory / "*.trx")
            .Produces(IntegrationTestsResultDirectory / "*.xml")
            .Produces(CoverageReportIntegrationTestsDirectory / "*.xml")
            .Executes(() =>
            {
                IEnumerable<Project> projects = Solution.GetProjects("*.IntegrationTests");
                IEnumerable<Project> testsProjects = Partition.GetCurrent(projects);

                testsProjects.ForEach(project => Info(project));
                IEnumerable<(string service, string connectionString)> connections = Enumerable.Empty<(string, string)>();

                if (FileExists(ConnectionsFile))
                {
                    connections = ReadAllLines(ConnectionsFile)
                        .Select(line => line.Split('|', StringSplitOptions.RemoveEmptyEntries))
                        .Where(line => line.Exactly(2))
                        .Select(line => (service: line[0], connectionString: line[1]));
                }

                DotNetTest(s => s
                    .SetConfiguration(Configuration)
                    .EnableCollectCoverage()
                    .SetNoBuild(SucceededTargets.Contains(Compile) || SucceededTargets.Contains(UnitTests))
                    .SetResultsDirectory(IntegrationTestsResultDirectory)
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                        .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                            .SetFramework(framework)
                            .SetLoggers($"trx;LogFileName={project.Name}.{framework}.trx")
                            .SetCollectCoverage(true)
                            .SetCoverletOutput(IntegrationTestsResultDirectory / $"{project.Name}.xml"))
                            .CombineWith(connections, (setting, connection) => setting.AddProcessEnvironmentVariable($"CONNECTION_STRINGS_{connection.service}", connection.connectionString))
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
            .DependsOn(Tests)
            .Consumes(Compile)
            .Produces(ArtifactsDirectory / "*.nupkg")
            .Produces(ArtifactsDirectory / "*.snupkg")
            .Executes(() =>
            {
                DotNetPack(s => s
                    .EnableIncludeSource()
                    .EnableIncludeSymbols()
                    .SetNoRestore(SucceededTargets.Contains(Compile)
                                  || SucceededTargets.Contains(Restore)
                                  || SucceededTargets.Contains(Tests))
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


        protected override void OnBuildFinished()
        {
            DeleteFile(_ciSolution);
        }

        //[LocalExecutable]
        //public readonly Tool Npx;
        //public Target TypeScriptModels => _ => _
        //    .Description("Generates Typescript definition files")
        //    .Executes(() =>
        //    {
        //        NpmInstall(s => s.AddPackages("npx")
        //                         .EnableGlobal());

        //        Npm("npx swagger-typescript-api -p https://api-dev.devaktome.fr/swagger/v1/swagger.json --axios");
        //    });
    }
}