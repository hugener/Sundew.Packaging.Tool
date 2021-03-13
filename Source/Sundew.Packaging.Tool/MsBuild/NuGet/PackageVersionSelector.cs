// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionSelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.MsBuild.NuGet
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::NuGet.Versioning;
    using Sundew.Base.Collections;

    public class PackageVersionSelector
    {
        private readonly INuGetPackageVersionFetcher nuGetPackageVersionFetcher;
        private readonly IPackageVersionSelectorReporter? packageVersionSelectorReporter;

        private readonly Dictionary<string, IReadOnlyList<NuGetVersion>> cache = new();

        public PackageVersionSelector(INuGetPackageVersionFetcher nuGetPackageVersionFetcher, IPackageVersionSelectorReporter? packageVersionSelectorReporter)
        {
            this.nuGetPackageVersionFetcher = nuGetPackageVersionFetcher;
            this.packageVersionSelectorReporter = packageVersionSelectorReporter;
        }

        public async Task<IEnumerable<PackageUpdate>> GetPackageVersions(IReadOnlyList<PackageUpdateSuggestion> possiblePackageUpdates, VersionMatcher? globalVersionMatcher, string rootDirectory, bool allowPrerelease, string? source)
        {
            return (await possiblePackageUpdates.SelectAsync(async x =>
                {
                    var actualVersionMatcher = x.VersionMatcher ?? globalVersionMatcher;
                    var newNuGetVersion = await this.GetLatestVersion(x.Id, actualVersionMatcher, rootDirectory, allowPrerelease, source);

                    if (x.NuGetVersion != newNuGetVersion)
                    {
                        this.packageVersionSelectorReporter?.PackageUpdateSelected(x.Id, x.NuGetVersion, newNuGetVersion);
                        return new PackageUpdate(x.Id, x.NuGetVersion, newNuGetVersion);
                    }

                    return default;
                }))
                .Where(x => x != null)
                .Cast<PackageUpdate>();
        }

        private async Task<NuGetVersion> GetLatestVersion(
            string packageId,
            VersionMatcher? versionMatcher,
            string rootDirectory,
            bool allowPrerelease,
            string? source)
        {
            if (!this.cache.TryGetValue(packageId, out var versions))
            {
                versions = (await this.nuGetPackageVersionFetcher.GetAllVersionsAsync(rootDirectory, source, packageId))
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToList();
                this.cache.Add(packageId, versions);
            }

            if (versionMatcher != null)
            {
                return versions.FirstOrDefault(x =>
                    versionMatcher.Regex.IsMatch(x.ToFullString())
                    && (!x.IsPrerelease || (allowPrerelease && x.IsPrerelease))) ?? throw new NuGetVersionNotFoundException(packageId, versionMatcher.Pattern, allowPrerelease, versions);
            }

            return versions.First(x => !x.IsPrerelease || (allowPrerelease && x.IsPrerelease));
        }
    }
}