namespace MedEasy.ContinuousIntegration
{
    using Nuke.Common;
    using Nuke.Common.Git;
    using Nuke.Common.IO;
    using Nuke.Common.Tools.GitVersion;

    using System;
    using System.Collections.Generic;

    using static Nuke.Common.Tools.GitVersion.GitVersionTasks;
    using static Nuke.Common.ChangeLog.ChangelogTasks;
    using static Nuke.Common.Logger;
    using static Nuke.Common.Tools.Git.GitTasks;
    using System.IO;
    using Nuke.Common.Tooling;

    public partial class Build
    {

        private AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";

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
                    AskBranchNameAndSwitchToIt(FeatureBranchPrefix, DevelopBranch);
                    Info($"{EnvironmentInfo.NewLine}Good bye !");
                }
                else
                {
                    FinishFeature();
                }
            });

        /// <summary>
        /// Asks the user for a branch name
        /// </summary>
        /// <param name="branchNamePrefix">A prefix to preprend in front of the user branch name</param>
        /// <param name="sourceBranch">Branch from which a new branch will be created</param>
        private void AskBranchNameAndSwitchToIt(string branchNamePrefix, string sourceBranch)
        {
            string featureName;
            bool exitCreatingFeature;
            do
            {
                featureName = (Name ?? Console.ReadLine() ?? string.Empty).Trim()
                                                                .Trim('/');

                switch (featureName)
                {
                    case string name when !string.IsNullOrWhiteSpace(name):
                        {
                            string branchName = $"{branchNamePrefix}/{featureName.Slugify()}";
                            Info($"{Environment.NewLine}The branch '{branchName}' will be created.{Environment.NewLine}Confirm ? (Y/N) ");

                            switch (Console.ReadKey().Key)
                            {
                                case ConsoleKey.Y:
                                    Info($"{Environment.NewLine}Checking out branch '{branchName}' from '{sourceBranch}'");

                                    Checkout(branchName, start: sourceBranch);

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
                        Info($"Exiting task.");
                        exitCreatingFeature = true;
                        break;
                }
            } while (string.IsNullOrWhiteSpace(featureName) && !exitCreatingFeature);
        }

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

        public Target Coldfix => _ => _
            .Description($"Starts a new coldfix development by creating the associated '{ColdfixBranchPrefix}/{{name}}' from {DevelopBranch}")
            .Requires(() => IsLocalBuild)
            .Requires(() => !GitRepository.Branch.Like($"{ColdfixBranchPrefix}/*", true) || GitHasCleanWorkingCopy())
            .Executes(() =>
            {
                if (!GitRepository.Branch.Like($"{ColdfixBranchPrefix}/*"))
                {
                    Info("Enter the name of the coldfix. It will be used as the name of the coldfix/branch (leave empty to exit) :");
                    AskBranchNameAndSwitchToIt(ColdfixBranchPrefix, DevelopBranch);
                    Info($"{EnvironmentInfo.NewLine}Good bye !");
                }
                else
                {
                    FinishColdfix();
                }
            });

        /// <summary>
        /// Merge a coldfix/* branch back to the develop branch
        /// </summary>
        private void FinishColdfix() => FinishFeature();

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
    }
}
