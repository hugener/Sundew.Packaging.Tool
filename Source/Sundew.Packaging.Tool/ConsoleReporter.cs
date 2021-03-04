// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool
{
    using System;
    using System.Collections.Generic;
    using NuGet.Versioning;
    using Sundew.Packaging.Tool.AwaitPublish;
    using Sundew.Packaging.Tool.MsBuild;
    using Sundew.Packaging.Tool.MsBuild.NuGet;
    using Sundew.Packaging.Tool.PruneLocalSource;
    using Sundew.Packaging.Tool.Update;

    public class ConsoleReporter : IPackageVersionUpdaterReporter, IPackageUpdaterFacadeReporter, IPackageVersionSelectorReporter, IPackageRestorerReporter, IAwaitPublishFacadeReporter, IPruneReporter
    {
        private const string ModifiedVerbose = "Updated";
        private const string Modified = "*";
        private const string UnmodifiedVerbose = "No changes for";
        private const string Unmodified = " ";
        private readonly bool verbose;

        public ConsoleReporter(bool verbose)
        {
            this.verbose = verbose;
        }

        public void ProcessedProject(string projectPath, bool wasModified)
        {
            Console.WriteLine(this.verbose
                ? $" {(wasModified ? ModifiedVerbose : UnmodifiedVerbose)} {projectPath}"
                : $" {(wasModified ? Modified : Unmodified)} {projectPath}");
        }

        public void StartingPackageUpdate(string rootDirectory)
        {
            Console.WriteLine($"Starting package update in: {rootDirectory}");
        }

        public void UpdatingProject(string project)
        {
            if (this.verbose)
            {
                Console.WriteLine($" Updating packages for: {project}");
            }
        }

        public void CompletedPackageUpdate(List<MsBuildProject> msBuildProjects, bool skippedRestore, TimeSpan totalTime)
        {
            Console.WriteLine($"Completed updating{(skippedRestore ? string.Empty : " and restored")}: {msBuildProjects.Count} projects in: {totalTime}");
        }

        public void Exception(Exception exception)
        {
            var backgroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(exception.ToString());
            Console.ForegroundColor = backgroundColor;
        }

        public void PackageExistsResult(PackageIdAndVersion packageIdAndVersion, bool packageExists)
        {
            if (this.verbose)
            {
                Console.WriteLine($"Package: {packageIdAndVersion.Id}.{packageIdAndVersion.NuGetVersion} was {(packageExists ? string.Empty : "not ")}found");
            }
        }

        public void StartWaitingForPackage(PackageIdAndVersion packageIdAndVersion, string source)
        {
            Console.WriteLine($"Start waiting for: {packageIdAndVersion.Id}.{packageIdAndVersion.NuGetVersion} in {source}");
        }

        public void CompletedWaitingForPackage(PackageIdAndVersion packageIdAndVersion, bool packageExists, TimeSpan stopwatchElapsed)
        {
            Console.WriteLine($"Completed waiting for: {packageIdAndVersion.Id}.{packageIdAndVersion.NuGetVersion} {(packageExists ? "succeeded" : "timed out")}");
        }

        public void ReportMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void PackageUpdateSelected(string packageId, NuGetVersion? oldNuGetVersion, NuGetVersion newNuGetVersion)
        {
            if (this.verbose)
            {
                Console.WriteLine($"  Updated {packageId} from {oldNuGetVersion} to {newNuGetVersion}");
            }
        }

        public void StartPruning(string source)
        {
            Console.WriteLine($"Start purging in: {source}");
        }

        public void Deleted(string directory)
        {
            if (this.verbose)
            {
                Console.WriteLine($"  Deleted {directory}");
            }
        }

        public void CompletedPruning(bool wasSuccessful, int numberDirectoriesPurged, TimeSpan stopwatchElapsed)
        {
            if (wasSuccessful)
            {
                Console.WriteLine($"Completed purging {numberDirectoriesPurged} directories");
                return;
            }

            Console.WriteLine($"Purging canceled ({numberDirectoriesPurged} directory deleted)");
        }
    }
}