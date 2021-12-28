namespace MedEasy.ContinuousIntegration
{

    using Nuke.Common;
    using Nuke.Common.ProjectModel;
    using Nuke.Common.Tools.Coverlet;
    using Nuke.Common.Tools.DotNet;
    using Nuke.Common.Tools.EntityFramework;
    using Nuke.Common.Tools.GitVersion;
    using Nuke.Common.Tooling;

    using static Nuke.Common.Logger;
    using static Nuke.Common.Tools.EntityFramework.EntityFrameworkTasks;
    using System.Collections.Generic;
    using System;

    public partial class Build : NukeBuild
    {
        [Parameter("Store engine to target (default : sqlite, postgres)")]
        public readonly DataStoreProvider[] Providers = { DataStoreProvider.Sqlite, DataStoreProvider.Postgres };

        [Parameter("Defines the service which an operation will be run for")]
        public readonly MedEasyServices Service;

        public Target MigrationAdd => _ => _
            .Description("Adds a EF Core migration for the specified service")
            .DependsOn(Restore)
            .OnlyWhenStatic(() => Providers.Length > 0 && Service != null)
            .Requires(() => !string.IsNullOrWhiteSpace(Name))
            .Executes(() =>
            {
                string startupProject = Solution.GetProject($"{Service}.API");

                Providers.ForEach((provider, index) =>
                {
                    string migrationProject = Solution.GetProject($"{Service}.DataStores.{provider}");

                    Info($"Adding migration for {startupProject} using {migrationProject}");

                    EntityFrameworkMigrationsAdd(s => s
                        .SetStartupProject(startupProject)
                        .SetProject(migrationProject)
                        .SetName(Name.ToPascalCase())
                        .SetNoBuild(index > 1 || InvokedTargets.Contains(Compile))
                        .SetProcessArgumentConfigurator(args => args.Add("-- --provider {0}", provider.ToString(), customValue: true)));
                });
            });

        public Target MigrationScript => _ => _
            .Description("Generates idempotent scripts for the specified service's datastore")
            .After(Compile)
            .OnlyWhenStatic(() => Providers.Length > 0 && Service != null)
            .Requires(() => !string.IsNullOrWhiteSpace(Name))
            .Produces(SqlScriptsDirectory / "*.sql")
            .Executes(() =>
            {
                string currentDateTime = $"{DateTime.Now:yyyyMMddhhmmss}";
                string startupProject = Solution.GetProject($"{Service}.API");

                Providers.ForEach((provider, index) =>
                {
                    string migrationProject = Solution.GetProject($"{Service}.DataStores.{provider}");

                    Info($"Generating idempotent script for {startupProject} using {migrationProject}");

                    EntityFrameworkDbContextScript(s => s
                        .SetStartupProject(startupProject)
                        .SetProject(migrationProject)
                        .SetNoBuild(index > 1 || InvokedTargets.Contains(Compile))
                        .SetOutput($"{SqlScriptsDirectory / Service / provider}/{currentDateTime}_{Name.ToPascalCase()}.sql")
                        .SetProcessArgumentConfigurator(args => args.Add("-- --provider {0}", provider.ToString(), customValue: true)));

                });
            });
    }
}
