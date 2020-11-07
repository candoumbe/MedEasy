using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;

using System.Collections.Generic;
using System.Linq;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using System;
using System.IO;

[AzurePipelines(
    AzurePipelinesImage.UbuntuLatest,
    AzurePipelinesImage.WindowsLatest,
    InvokedTargets = new[] {nameof(UnitTests), nameof(IntegrationTests)},
    NonEntryTargets = new [] {nameof(Restore) },
    ExcludedTargets = new [] {nameof(Clean)},
    PullRequestsAutoCancel = true,
    PullRequestsBranchesInclude = new[] { "main" },
    TriggerBranchesInclude = new[] {
        "main",
        "feature/*",
        "fix/*"
    },
    TriggerPathsExclude = new[]
    {
        "docs/*",
        "README.md"
    }
    )]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
public class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Parameter("Indicates wheter to restore nuget in interactive mode - Default is false")]
    public readonly bool Interactive = false;

    [Solution] public readonly Solution Solution;
    [GitRepository] public readonly GitRepository GitRepository;

    [CI] public readonly AzurePipelines AzurePipelines;

    [Partition(10)] public readonly Partition TestPartition;

    public AbsolutePath SourceDirectory => RootDirectory / "src";
    public AbsolutePath TestDirectory => RootDirectory / "test";

    public AbsolutePath OutputDirectory => RootDirectory / "output";

    public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

    public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

    private const string CsProjGlobFilesPattern = "**/*.csproj";

    public Target Clean => _ => _
        .Before(Restore)
        .Executes(() => {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    public Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            IEnumerable<AbsolutePath> projects = SourceDirectory.GlobFiles(CsProjGlobFilesPattern)
                                                                .Concat(TestDirectory.GlobFiles(CsProjGlobFilesPattern))
                                                                .Where(path => !path.ToString().Like("*.SPA.csproj"));

            Trace($"Projects : {string.Join(NewLine, projects)}");

            DotNetRestore(s => s
                .SetConfigFile("nuget.config")
                .SetIgnoreFailedSources(true)
                .When(IsLocalBuild && Interactive, _ => _.SetProperty("NugetInteractive", IsLocalBuild && Interactive))
                .CombineWith(projects, (setting, project) => setting.SetProjectFile(project)
                                                                    .SetVerbosity(DotNetVerbosity.Minimal))
            );
        });

    public Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                );
        });

    public Target UnitTests => _ => _
        .DependsOn(Compile)
        .Description("Run unit tests and collect code")
        .Partition(() => TestPartition)
        .Produces(TestResultDirectory / "*.trx")
        .Produces(TestResultDirectory / "*.xml")
        .Executes(() =>
        {
            Info("Start executing unit tests");
            IEnumerable<Project> projects = Solution.GetProjects("*.UnitTests");
            IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

            DotNetTest(s => s
                 .SetConfiguration(Configuration)
                 .ResetVerbosity()
                 .EnableCollectCoverage()
                 .SetNoBuild(InvokedTargets.Contains(Compile))
                 .SetResultsDirectory(TestResultDirectory)
                 .When(IsServerBuild, _ => _
                     .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                     .AddProperty("ExcludeByAttribute", "Obsolete")
                     .EnableUseSourceLink()
                 )
                 .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                    .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                        .SetFramework(framework)
                        .SetLogger($"trx;LogFileName={project.Name}-unit-test.{framework}.trx")
                        .When(InvokedTargets.Contains(Coverage) || IsServerBuild, _ => _
                            .SetCoverletOutput(TestResultDirectory / $"{project.Name}.{framework}.xml")))));

            TestResultDirectory.GlobFiles("*-unit-test.*.trx").ForEach(testFileResult =>
                AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                   title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                   files: new string[] { testFileResult }));
        });

    public Target IntegrationTests => _ => _
        .DependsOn(Compile)
        .Description("Run integration tests and collect code coverage")
        .Partition(() => TestPartition)
        .Produces(TestResultDirectory / "*.trx")
        .Produces(TestResultDirectory / "*.xml")
        .Executes(() =>
        {
            IEnumerable<Project> projects = Solution.GetProjects("*.IntegrationTests");
            IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .ResetVerbosity()
                .EnableCollectCoverage()
                .SetNoBuild(InvokedTargets.Contains(Compile))
                .SetResultsDirectory(TestResultDirectory)
                .When(IsServerBuild, _ => _.SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                                           .AddProperty("ExcludeByAttribute", "Obsolete")
                                           .EnableUseSourceLink()
                )
                .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                    .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                        .SetFramework(framework)
                        .SetLogger($"trx;LogFileName={project.Name}-integration.{framework}.trx")
                        .When(InvokedTargets.Contains(Coverage) || IsServerBuild, _ => _
                            .SetCoverletOutput(TestResultDirectory / $"{project.Name}.{framework}.xml")))));

            TestResultDirectory.GlobFiles("-integration.*.trx").ForEach(testFileResult =>
                AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                   title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                   files: new string[] { testFileResult }));
        });

    public Target Tests => _ => _
        .DependsOn(UnitTests, IntegrationTests)
        .Executes(() =>
        {
        });

    public Target Coverage => _ => _
        .DependsOn(UnitTests, IntegrationTests)
        .Executes(() =>
        {
        });

    public Target Publish => _ => _
        .DependsOn(UnitTests, IntegrationTests)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetSelfContained(true)
                .SetNoBuild(InvokedTargets.Contains(Compile))
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetOutput(OutputDirectory / "publish")
            );
        })
    ;

    protected override void OnTargetStart(string target)
    {
        Info($"Starting '{target}' task");
    }

    protected override void OnTargetExecuted(string target)
    {
        Info($"'{target}' task finished");
    }

    protected override void OnBuildInitialized()
    {
        Info($"{nameof(BuildProjectDirectory)} : {BuildProjectDirectory}");
    }

}
