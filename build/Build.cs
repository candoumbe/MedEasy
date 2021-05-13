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
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitVersion.GitVersionTasks;
using static Nuke.Common.Tools.EntityFramework.EntityFrameworkTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using Nuke.Common.Tools.EntityFramework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MedEasy.ContinuousIntegration
{

    [GitHubActions(
        "continuous",
        GitHubActionsImage.WindowsLatest,
        OnPushBranchesIgnore = new[] { MainBranchName, ReleaseBranchPrefix + "/*" },
        OnPullRequestBranches = new[] { DevelopBranch },
        PublishArtifacts = true,
        InvokedTargets = new[] { nameof(UnitTests), nameof(IntegrationTests) }
    )]
    [GitHubActions(
        "deployment",
        GitHubActionsImage.WindowsLatest,
        OnPushBranches = new[] { MainBranchName, ReleaseBranchPrefix + "/*" },
        InvokedTargets = new[] { nameof(Publish) },
        ImportGitHubTokenAs = nameof(GitHubToken),
        ImportSecrets =
            new[]
            {
            nameof(NugetApiKey),
            })]
    [AzurePipelines(
        suffix: "release",
        AzurePipelinesImage.WindowsLatest,
        InvokedTargets = new[] { nameof(Pack) },
        NonEntryTargets = new[] { nameof(Restore), nameof(Changelog) },
        ExcludedTargets = new[] { nameof(Clean) },
        PullRequestsAutoCancel = true,
        TriggerBranchesInclude = new[] { ReleaseBranchPrefix + "/*" },
        TriggerPathsExclude = new[]
        {
        "docs/*",
        "README.md",
        "CHANGELOG.md"
        }
    )]
    [AzurePipelines(
        suffix: "pull-request",
        AzurePipelinesImage.WindowsLatest,
        InvokedTargets = new[] { nameof(UnitTests) },
        NonEntryTargets = new[] { nameof(Restore), nameof(Changelog) },
        ExcludedTargets = new[] { nameof(Clean) },
        PullRequestsAutoCancel = true,
        PullRequestsBranchesInclude = new[] { MainBranchName },
        TriggerBranchesInclude = new[] {
        FeatureBranchPrefix + "/*",
        HotfixBranchPrefix + "/*"
        },
        TriggerPathsExclude = new[]
        {
        "docs/*",
        "README.md",
        "CHANGELOG.md"
        }
    )]
    [AzurePipelines(
        AzurePipelinesImage.WindowsLatest,
        InvokedTargets = new[] { nameof(Pack) },
        NonEntryTargets = new[] { nameof(Restore), nameof(Changelog) },
        ExcludedTargets = new[] { nameof(Clean) },
        PullRequestsAutoCancel = true,
        TriggerBranchesInclude = new[] { MainBranchName },
        TriggerPathsExclude = new[]
        {
        "docs/*",
        "README.md",
        "CHANGELOG.md"
        }
    )]
    [CheckBuildProjectConfigurations]
    [UnsetVisualStudioEnvironmentVariables]
    public class Build : NukeBuild
    {
        public static int Main() => Execute<Build>(x => x.Compile);

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Parameter("Indicates wheter to restore nuget in interactive mode - Default is false")]
        public readonly bool Interactive = false;

        [Required] [Solution] public readonly Solution Solution;
        [Required] [GitRepository] public readonly GitRepository GitRepository;
        [Required] [GitVersion(Framework = "net5.0")] public readonly GitVersion GitVersion;
        [CI] public readonly AzurePipelines AzurePipelines;

        [Partition(3)] public readonly Partition TestPartition;

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

        public AbsolutePath CoverageReportHistoryDirectory => OutputDirectory / "coverage-history";

        public AbsolutePath CoverageReportUnitTestsHistoryDirectory => CoverageReportHistoryDirectory / "unit-tests";

        public AbsolutePath CoverageReportintegrationTestsHistoryDirectory => CoverageReportHistoryDirectory / "integration-tests";

        /// <summary>
        /// Path to the folder that contains databases used by integration tests.
        /// </summary>
        public AbsolutePath DatabaseFolder => RootDirectory / "databases";

        public const string MainBranchName = "main";

        public const string DevelopBranch = "develop";

        public const string FeatureBranchPrefix = "feature";

        public const string HotfixBranchPrefix = "hotfix";

        public const string ReleaseBranchPrefix = "release";

        [Parameter("Indicates if any changes should be stashed automatically prior to switching branch (Default : true)")]
        public readonly bool AutoStash = true;

        [PathExecutable]
        public readonly Tool Tye;

        [Parameter("Token required when publishing artifacts to GitHub")]
        public readonly string GitHubToken;

        [Parameter(@"Defines which services should start when running using Tye tool")]
        public readonly MedEasyService[] Services =
        {
            MedEasyService.Agenda,
            MedEasyService.Documents,
            MedEasyService.Identity,
            MedEasyService.Measures,
            MedEasyService.Patients
        };

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
                    .EnableUseSourceLink()
                    .SetNoBuild(InvokedTargets.Contains(Compile))
                    .SetResultsDirectory(UnitTestsResultDirectory)
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                        .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                            .SetFramework(framework)
                            .SetLogger($"trx;LogFileName={project.Name}.{framework}.trx")
                            .SetCollectCoverage(true)
                            .SetCoverletOutput(UnitTestsResultDirectory / $"{project.Name}.xml"))
                        )
                );

                UnitTestsResultDirectory.GlobFiles("*.trx")
                                   .ForEach(testFileResult => AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                                                                 title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                                                                 files: new string[] { testFileResult })
                );

                // TODO Move this to a separate "coverage" target once https://github.com/nuke-build/nuke/issues/562 is solved !
                ReportGenerator(_ => _
                        .SetFramework("net5.0")
                        .SetReports(UnitTestsResultDirectory / "*.xml")
                        .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlChart, ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTargetDirectory(CoverageReportUnitTestsDirectory)
                        .SetHistoryDirectory(CoverageReportUnitTestsHistoryDirectory)
                    );

                UnitTestsResultDirectory.GlobFiles("*.xml")
                                               .ForEach(file => AzurePipelines?.PublishCodeCoverage(coverageTool: AzurePipelinesCodeCoverageToolType.Cobertura,
                                                                                        summaryFile: file,
                                                                                        reportDirectory: CoverageReportUnitTestsDirectory));
            });

        public Target CleanDatabaseFolder => _ => _
            .Unlisted()
            .Description($"Cleans '{DatabaseFolder}'")
            .Executes(() =>
            {
                EnsureCleanDirectory(OutputDirectory / "databases");
            });

        [Parameter("Indicates if the connection strings should be updated in appsettings.integrationtest.json file (Default = true)")]
        public readonly bool UpdateConnectionString = true;

        public Target UpdateDatabases => _ => _
            .Description("Applies any pending migrations on databases")
            .DependsOn(Compile, CleanDatabaseFolder)
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
                    .EnableUseSourceLink()
                    .SetNoBuild(InvokedTargets.Contains(Compile) || InvokedTargets.Contains(UnitTests))
                    .SetResultsDirectory(IntegrationTestsResultDirectory)
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                        .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                            .SetFramework(framework)
                            .SetLogger($"trx;LogFileName={project.Name}.{framework}.trx")
                            .SetCollectCoverage(true)
                            .SetCoverletOutput(IntegrationTestsResultDirectory / $"{project.Name}.xml"))
                        )
                );

                IntegrationTestsResultDirectory.GlobFiles("*.trx")
                                   .ForEach(testFileResult => AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                                                                 title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                                                                 files: new string[] { testFileResult })
                );

                // TODO Move this to a separate "coverage" target once https://github.com/nuke-build/nuke/issues/562 is solved !
                ReportGenerator(_ => _
                        .SetFramework("net5.0")
                        .SetReports(IntegrationTestsResultDirectory / "*.xml")
                        .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlChart, ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTargetDirectory(CoverageReportIntegrationTestsDirectory)
                        .SetHistoryDirectory(CoverageReportintegrationTestsHistoryDirectory)
                    );

                IntegrationTestsResultDirectory.GlobFiles("*.xml")
                                   .ForEach(file => AzurePipelines?.PublishCodeCoverage(coverageTool: AzurePipelinesCodeCoverageToolType.Cobertura,
                                                                                        summaryFile: file,
                                                                                        reportDirectory: CoverageReportIntegrationTestsDirectory));
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

        private AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";

        #region Git flow section

        public Target Changelog => _ => _
            .Requires(() => IsLocalBuild)
            .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch() || GitRepository.IsOnHotfixBranch())
            .Description("Finalizes the change log so that its up to date for the release. ")
            .Executes(() =>
            {
                FinalizeChangelog(ChangeLogFile, GitVersion.MajorMinorPatch, GitRepository);
                Info($"Please review CHANGELOG.md ({ChangeLogFile}) and press 'Y' to validate (any other key will cancel changes)...");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Y)
                {
                    Git($"add {ChangeLogFile}");
                    Git($"commit -m \"Finalize {Path.GetFileName(ChangeLogFile)} for {GitVersion.MajorMinorPatch}\"");
                }
            });

        public Target Feature => _ => _
            .Description($"Starts a new feature development by creating the associated branch {FeatureBranchPrefix}/{{feature-name}} from {DevelopBranch}")
            .Requires(() => IsLocalBuild)
            .Requires(() => !GitRepository.IsOnFeatureBranch() || GitHasCleanWorkingCopy())
            .Executes(() =>
            {
                if (!GitRepository.IsOnFeatureBranch())
                {
                    Info("Enter the name of the feature. It will be used as the name of the feature/branch (leave empty to exit) :");
                    string featureName;
                    bool exitCreatingFeature = false;
                    do
                    {
                        featureName = (Console.ReadLine() ?? string.Empty).Trim()
                                                                        .Trim('/');

                        switch (featureName)
                        {
                            case string name when !string.IsNullOrWhiteSpace(name):
                                {
                                    string branchName = $"{FeatureBranchPrefix}/{featureName.Slugify()}";
                                    Info($"{Environment.NewLine}The branch '{branchName}' will be created.{Environment.NewLine}Confirm ? (Y/N) ");

                                    switch (Console.ReadKey().Key)
                                    {
                                        case ConsoleKey.Y:
                                            Info($"{Environment.NewLine}Checking out branch '{branchName}' from '{DevelopBranch}'");
                                            Checkout(branchName, start: DevelopBranch);
                                            Info($"{Environment.NewLine}'{branchName}' created successfully");
                                            exitCreatingFeature = true;
                                            break;

                                        default:
                                            Info($"{Environment.NewLine}Exiting {nameof(Feature)} task.");
                                            exitCreatingFeature = true;
                                            break;
                                    }
                                }
                                break;
                            default:
                                Info($"Exiting {nameof(Feature)} task.");
                                exitCreatingFeature = true;
                                break;
                        }

                    } while (string.IsNullOrWhiteSpace(featureName) && !exitCreatingFeature);

                    Info($"{EnvironmentInfo.NewLine}Good bye !");
                }
                else
                {
                    FinishFeature();
                }
            });

        public Target Release => _ => _
            .DependsOn(Changelog)
            .Description($"Starts a new {ReleaseBranchPrefix}/{{version}} from {DevelopBranch}")
            .Requires(() => !GitRepository.IsOnReleaseBranch() || GitHasCleanWorkingCopy())
            .Executes(() =>
            {
                if (!GitRepository.IsOnReleaseBranch())
                {
                    Checkout($"{ReleaseBranchPrefix}/{GitVersion.MajorMinorPatch}", start: DevelopBranch);
                }
                else
                {
                    FinishReleaseOrHotfix();
                }
            });

        public Target Hotfix => _ => _
            .DependsOn(Changelog)
            .Description($"Starts a new hotfix branch '{HotfixBranchPrefix}/*' from {MainBranchName}")
            .Requires(() => !GitRepository.IsOnHotfixBranch() || GitHasCleanWorkingCopy())
            .Executes(() =>
            {
                (GitVersion mainBranchVersion, IReadOnlyCollection<Output> _) = GitVersion(s => s
                    .SetFramework("net5.0")
                    .SetUrl(RootDirectory)
                    .SetBranch(MainBranchName)
                    .EnableNoFetch()
                    .DisableProcessLogOutput());

                if (!GitRepository.IsOnHotfixBranch())
                {
                    Checkout($"{HotfixBranchPrefix}/{mainBranchVersion.Major}.{mainBranchVersion.Minor}.{mainBranchVersion.Patch + 1}", start: MainBranchName);
                }
                else
                {
                    FinishReleaseOrHotfix();
                }
            });

        private void Checkout(string branch, string start)
        {
            bool hasCleanWorkingCopy = GitHasCleanWorkingCopy();

            if (!hasCleanWorkingCopy && AutoStash)
            {
                Git("stash");
            }
            Git($"checkout -b {branch} {start}");

            if (!hasCleanWorkingCopy && AutoStash)
            {
                Git("stash apply");
            }
        }

        private string MajorMinorPatchVersion => GitVersion.MajorMinorPatch;

        private void FinishReleaseOrHotfix()
        {
            Git($"checkout {MainBranchName}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");
            Git($"tag {MajorMinorPatchVersion}");

            Git($"checkout {DevelopBranch}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");

            Git($"branch -D {GitRepository.Branch}");

            Git($"push origin --follow-tags {MainBranchName} {DevelopBranch} {MajorMinorPatchVersion}");
        }

        private void FinishFeature()
        {
            Git($"rebase {DevelopBranch}");
            Git($"checkout {DevelopBranch}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");

            Git($"branch -D {GitRepository.Branch}");
            Git($"push origin {DevelopBranch}");
        }

        #endregion

        [Parameter("API key used to publish artifacts to Nuget.org")]
        public readonly string NugetApiKey;

        [Parameter(@"URI where packages should be published (default : ""https://api.nuget.org/v3/index.json""")]
        public string NugetPackageSource => "https://api.nuget.org/v3/index.json";

        public Target Publish => _ => _
            .Description($"Published packages (*.nupkg and *.snupkg) to the destination server set with {nameof(NugetPackageSource)} settings ")
            .DependsOn(Clean, UnitTests, Pack)
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

        public Target TyeInstall => _ => _
            .Requires(() => IsLocalBuild)
            .Description("Install/Updates Tye tool globally")
            .Executes(() =>
            {
                try
                {
                    if (new Ping().Send("https://www.google.fr").Status == IPStatus.Success)
                    {
                        IReadOnlyCollection<Output> outputs = DotNet(arguments: "tool list -g");
                        const string tyePackageId = "microsoft.tye";
                        if (!outputs.AtLeastOnce(output => output.Type == OutputType.Std && output.Text.Like($"*{tyePackageId}*", true)))
                        {
                            Info($"Installing {tyePackageId}");
                            DotNetToolInstall(s => s.SetPackageName(tyePackageId)
                                                    .SetGlobal(true)
                                                    .SetVersion("0.6.0-alpha.21070.5")
                            );
                        }
                        else
                        {
                            Info($"Updating {tyePackageId}");
                            DotNetToolUpdate(s => s.SetPackageName(tyePackageId)
                                                    .SetGlobal(true)
                                                    .SetVersion("0.6.0-alpha.21070.5")
                            );
                        }
                    }
                }
                catch (PingException)
                {
                    Info("No internet connexion available ...");
                    Info("Skipping installing/updating");
                }
            });


        public Target Run => _ => _
            .Requires(() => IsLocalBuild)
            .Description("Run all services using Tye")
            .DependsOn(Compile, TyeInstall)
            .Executes(() =>
            {
                

                Tye("run --dashboard --logs seq=http://localhost:55340");
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