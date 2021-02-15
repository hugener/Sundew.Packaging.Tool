// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Update
{
    using System;
    using System.Collections.Generic;
    using NuGet.Versioning;
    using Sundew.Packaging.Update.MsBuild;
    using Sundew.Packaging.Update.MsBuild.NuGet;

    public class ConsoleReporter : IPackageVersionUpdaterReporter, IPackageUpdaterFacadeReporter, IPackageVersionSelectorReporter, IPackageRestorerReporter
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
            if (this.verbose)
            {
                Console.WriteLine($" {(wasModified ? ModifiedVerbose : UnmodifiedVerbose)} {projectPath}");
            }
            else
            {
                Console.WriteLine($" {(wasModified ? Modified : Unmodified)} {projectPath}");
            }
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
            var backgroundColor = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(exception.ToString());
            Console.BackgroundColor = backgroundColor;
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
    }
}