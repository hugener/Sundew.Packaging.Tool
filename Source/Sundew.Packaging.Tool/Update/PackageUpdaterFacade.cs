// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageUpdaterFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using Sundew.Packaging.Tool.Diagnostics;
    using Sundew.Packaging.Tool.MsBuild;
    using Sundew.Packaging.Tool.MsBuild.NuGet;
    using Sundew.Packaging.Tool.RegularExpression;

    public sealed class PackageUpdaterFacade
    {
        private readonly IFileSystem fileSystem;
        private readonly PackageRestorer packageRestorer;
        private readonly IPackageUpdaterFacadeReporter packageUpdaterFacadeReporter;
        private readonly PackageVersionUpdater packageVersionUpdater;
        private readonly PackageVersionSelector packageVersionSelector;
        private readonly MsBuildProjectPackagesParser msBuildProjectPackagesParser;
        private readonly MsBuildProjectFileSearcher msBuildProjectFileSearcher;

        public PackageUpdaterFacade(
            IFileSystem fileSystem,
            INuGetPackageVersionFetcher nuGetPackageVersionFetcher,
            IProcessRunner processRunner,
            IPackageUpdaterFacadeReporter packageUpdaterFacadeReporter,
            IPackageVersionUpdaterReporter packageVersionUpdaterReporter,
            IPackageVersionSelectorReporter packageVersionSelectorReporter,
            IPackageRestorerReporter packageRestorerReporter)
        {
            this.fileSystem = fileSystem;
            this.packageRestorer = new PackageRestorer(processRunner, packageRestorerReporter);
            this.packageUpdaterFacadeReporter = packageUpdaterFacadeReporter;
            this.packageVersionUpdater = new PackageVersionUpdater(packageVersionUpdaterReporter);
            this.packageVersionSelector = new PackageVersionSelector(nuGetPackageVersionFetcher, packageVersionSelectorReporter);
            this.msBuildProjectPackagesParser = new MsBuildProjectPackagesParser(fileSystem.File);
            this.msBuildProjectFileSearcher = new MsBuildProjectFileSearcher(fileSystem.Directory);
        }

        public async Task UpdatePackagesInProjectsAsync(UpdateVerb updateVerb)
        {
            var rootDirectory = updateVerb.RootDirectory ?? this.fileSystem.Directory.GetCurrentDirectory();
            this.packageUpdaterFacadeReporter.StartingPackageUpdate(rootDirectory);
            var changedProjects = new List<MsBuildProject>();
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var globalVersionMatcher = string.IsNullOrEmpty(updateVerb.VersionPattern)
                    ? null
                    : new VersionMatcher(GlobRegexHelper.CreateRegex(updateVerb.VersionPattern), updateVerb.VersionPattern);
                foreach (var project in this.msBuildProjectFileSearcher.GetProjects(rootDirectory, updateVerb.Projects).ToList())
                {
                    this.packageUpdaterFacadeReporter.UpdatingProject(project);

                    var msBuildProject = await this.msBuildProjectPackagesParser.GetPackages(project, updateVerb.PackageIds);
                    var packageUpdates = await this.packageVersionSelector.GetPackageVersions(msBuildProject.PossiblePackageUpdates, globalVersionMatcher, rootDirectory, updateVerb.AllowPrerelease, updateVerb.Source);
                    var result = this.packageVersionUpdater.TryUpdateAsync(msBuildProject, packageUpdates);
                    if (result)
                    {
                        changedProjects.Add(result.Value);
                    }
                }

                foreach (var changedProject in changedProjects)
                {
                    await this.fileSystem.File.WriteAllTextAsync(changedProject.Path, changedProject.ProjectContent);
                }

                if (changedProjects.Count > 0 && !updateVerb.SkipRestore)
                {
                    await this.packageRestorer.RestoreAsync(rootDirectory, updateVerb.Verbose);
                }
            }
            catch (Exception e)
            {
                this.packageUpdaterFacadeReporter.Exception(e);
                return;
            }

            this.packageUpdaterFacadeReporter.CompletedPackageUpdate(changedProjects, updateVerb.SkipRestore, stopwatch.Elapsed);
        }
    }
}