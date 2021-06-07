namespace MedEasy.ContinuousIntegration
{

    using Nuke.Common;
    using Nuke.Common.CI;
    using Nuke.Common.ProjectModel;
    using Nuke.Common.Tools.Coverlet;
    using Nuke.Common.Tools.DotNet;
    using Nuke.Common.Tools.EntityFramework;
    using Nuke.Common.Tools.GitVersion;
    using Nuke.Common.Tooling;

    using static Nuke.Common.Logger;
    using static Nuke.Common.Tools.EntityFramework.EntityFrameworkTasks;

    public partial class Build : NukeBuild
    {
        [Parameter("Database engine to build an operationwill be build for (default : )")]
        public readonly DataStoreProvider Provider = DataStoreProvider.Sqlite;

        [Parameter("Defines the service which an operation will be run for")]
        public readonly MedEasyServices Service;

        public Target MigrationAdd => _ => _
            .Description("Adds a EF Core migration for the specified service")
            .OnlyWhenStatic(() => Provider != null && Service != null)
            .Requires(() => ! string.IsNullOrWhiteSpace(Name))
            .Executes(() =>
            {
                string migrationProject = Solution.GetProject($"{Service}.DataStores.{Provider}");
                string startupProject = Solution.GetProject($"{Service}.API");

                Info($"Generating idempotent script for {startupProject} using {migrationProject}");

                EntityFrameworkMigrationsAdd(s => s
                    .SetStartupProject(startupProject)
                    .SetProject(migrationProject)
                    .SetName(Name)
                    .SetProcessArgumentConfigurator(args => args.Add("-- --provider {0}", Provider.ToString(), customValue: true)));
            });

        public Target MigrationScript => _ => _
            .Description("Generates idempotent script for the specified service's datastore and provider and output the ")
            .OnlyWhenStatic(() => Provider != null && Service != null)
            .Produces(SqlScriptsDirectory / "*.sql")
            .Executes(() =>
            {
                string migrationProject = Solution.GetProject($"{Service}.DataStores.{Provider}");
                string startupProject = Solution.GetProject($"{Service}.API");

                Info($"Generating idempotent script for {startupProject} using {migrationProject}");

                EntityFrameworkDbContextScript(s => s
                    .SetStartupProject(startupProject)
                    .SetProject(migrationProject)
                    .SetOutput($"{SqlScriptsDirectory / Service / Provider}/idempotent_script.sql")
                    .SetProcessArgumentConfigurator(args => args.Add("-- --provider {0}", Provider.ToString(), customValue: true)));
            });

    }
}
