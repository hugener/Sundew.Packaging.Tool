// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionUpdater.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.MsBuild
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Sundew.Base.Primitives.Computation;
    using Sundew.Packaging.Tool.MsBuild.NuGet;
    using Sundew.Packaging.Tool.RegularExpression;

    public class PackageVersionUpdater
    {
        private const string PrefixGroupName = "Prefix";
        private const string PostfixGroupName = "Postfix";
        private readonly IPackageVersionUpdaterReporter packageVersionUpdaterReporter;

        public PackageVersionUpdater(IPackageVersionUpdaterReporter packageVersionUpdaterReporter)
        {
            this.packageVersionUpdaterReporter = packageVersionUpdaterReporter;
        }

        public Result.IfSuccess<MsBuildProject> TryUpdateAsync(MsBuildProject msBuildProject, IEnumerable<PackageUpdate> packageUpdates)
        {
            var fileContent = msBuildProject.ProjectContent;
            var wasModified = false;
            foreach (var packageId in packageUpdates)
            {
                var regex = new Regex(string.Format(MsBuildProjectPackagesParser.PackageReferenceRegex, GlobRegex.ConvertToRegexPattern(packageId.Id).Expression));
                fileContent = regex.Replace(
                    fileContent,
                    m =>
                    {
                        wasModified = true;
                        return m.Groups[PrefixGroupName].Value + packageId.UpdatedNuGetVersion.ToFullString() + m.Groups[PostfixGroupName].Value;
                    });
            }

            this.packageVersionUpdaterReporter.ProcessedProject(msBuildProject.Path, wasModified);
            return Result.FromValue(wasModified, msBuildProject with { ProjectContent = fileContent });
        }
    }
}