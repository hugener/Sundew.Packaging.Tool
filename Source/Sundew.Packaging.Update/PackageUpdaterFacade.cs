// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageUpdaterFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Update
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using Sundew.Build.Update.MsBuild;
    using Sundew.Build.Update.MsBuild.NuGet;

    public sealed class PackageUpdaterFacade
    {
        private readonly IFileSystem fileSystem;
        private readonly IPackageUpdaterFacadeReporter packageUpdaterFacadeReporter;
        private readonly PackageVersionUpdater packageVersionUpdater;
        private readonly PackageVersionSelector packageVersionSelector;
        private readonly MsBuildProjectPackagesParser msBuildProjectPackagesParser;
        private readonly MsBuildProjectFileSearcher msBuildProjectFileSearcher;

        public PackageUpdaterFacade(IFileSystem fileSystem, INuGetPackageVersionFetcher nuGetPackageVersionFetcher, IPackageUpdaterFacadeReporter packageUpdaterFacadeReporter, IPackageVersionUpdaterReporter packageVersionUpdaterReporter, IPackageVersionSelectorReporter packageVersionSelectorReporter)
        {
            this.fileSystem = fileSystem;
            this.packageUpdaterFacadeReporter = packageUpdaterFacadeReporter;
            this.packageVersionUpdater = new PackageVersionUpdater(packageVersionUpdaterReporter);
            this.packageVersionSelector = new PackageVersionSelector(nuGetPackageVersionFetcher, packageVersionSelectorReporter);
            this.msBuildProjectPackagesParser = new MsBuildProjectPackagesParser(fileSystem.File);
            this.msBuildProjectFileSearcher = new MsBuildProjectFileSearcher(fileSystem.Directory);
        }

        public async Task UpdatePackagesInProjectsAsync(Arguments arguments)
        {
            var rootDirectory = arguments.RootDirectory ?? this.fileSystem.Directory.GetCurrentDirectory();
            this.packageUpdaterFacadeReporter.StartingPackageUpdate(rootDirectory);
            var changedProjects = new List<MsBuildProject>();
            var stopwatch = Stopwatch.StartNew();
            try
            {
                foreach (var project in this.msBuildProjectFileSearcher.GetProjects(rootDirectory, arguments.Projects).ToList())
                {
                    this.packageUpdaterFacadeReporter.UpdatingProject(project);

                    var msBuildProject = await this.msBuildProjectPackagesParser.GetPackages(project, arguments.PackageIds);
                    var packageUpdates = await this.packageVersionSelector.GetPackageVersions(msBuildProject.PossiblePackageUpdates, arguments.NuGetVersion, rootDirectory, arguments.AllowPrerelease, arguments.Source);
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
            }
            catch (Exception e)
            {
                this.packageUpdaterFacadeReporter.Exception(e);
                return;
            }

            this.packageUpdaterFacadeReporter.CompletedPackageUpdate(changedProjects, stopwatch.Elapsed);
        }
    }
}