// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetPackageVersionFetcher.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Update.MsBuild.NuGet
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Configuration;
    using global::NuGet.Protocol;
    using global::NuGet.Protocol.Core.Types;
    using global::NuGet.Versioning;
    using Sundew.Base.Collections;

    public class NuGetPackageVersionFetcher : INuGetPackageVersionFetcher
    {
        internal const string PackageSourcesText = "packageSources";
        private const string All = "All";

        public async Task<IEnumerable<NuGetVersion>> GetAllVersionsAsync(string rootDirectory, string? source, string packageId)
        {
            var logger = NullLogger.Instance;
            var cancellationToken = CancellationToken.None;
            var defaultSettings = Settings.LoadDefaultSettings(rootDirectory);
            var packageSourcesSection = defaultSettings.GetSection(PackageSourcesText);
            var packageSourceProvider = new PackageSourceProvider(defaultSettings);

            source = packageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == source)?.Value ??
                     source;
            source ??= packageSourceProvider.DefaultPushSource;
            var sources = source == All
                ? packageSourcesSection?.Items.OfType<AddItem>().Select(x => x.Value) ?? new[] { source }
                : new[] { source };

            return (await sources.SelectAsync(async x =>
                {
                    var sourceRepository = Repository.Factory.GetCoreV3(x);
                    var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken).ConfigureAwait(false);

                    return await resource.GetAllVersionsAsync(
                        packageId,
                        new SourceCacheContext { NoCache = true, RefreshMemoryCache = true },
                        logger,
                        cancellationToken).ConfigureAwait(false);
                }))
                .SelectMany(x => x);
        }
    }
}