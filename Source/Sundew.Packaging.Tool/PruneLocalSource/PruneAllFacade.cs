// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PruneAllFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.PruneLocalSource
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using NuGet.Common;
    using Sundew.Packaging.Tool.MsBuild.NuGet;
    using Sundew.Packaging.Tool.RegularExpression;

    public class PruneAllFacade
    {
        private readonly INuGetSourceProvider nuGetSourceProvider;
        private readonly IFileSystem fileSystem;
        private readonly IPruneReporter purgerReporter;

        public PruneAllFacade(INuGetSourceProvider nuGetSourceProvider, IFileSystem fileSystem, IPruneReporter purgerReporter)
        {
            this.nuGetSourceProvider = nuGetSourceProvider;
            this.fileSystem = fileSystem;
            this.purgerReporter = purgerReporter;
        }

        public Task<int> PruneAsync(AllVerb allVerb)
        {
            var stopwatch = Stopwatch.StartNew();
            var source = this.nuGetSourceProvider.GetDefaultSource(allVerb.Source);
            this.purgerReporter.StartPruning(source);
            var numberDirectoriesPurged = 0;
            try
            {
                if (!UriUtility.TryCreateSourceUri(source, UriKind.Absolute).IsFile)
                {
                    throw new InvalidOperationException("Purge only works with local sources");
                }

                foreach (var packageId in allVerb.PackageIds)
                {
                    if (packageId == "*")
                    {
                        this.fileSystem.Directory.Delete(source, true);
                        this.fileSystem.Directory.CreateDirectory(source);
                        this.purgerReporter.Deleted(source);
                        numberDirectoriesPurged++;
                    }
                    else
                    {
                        var regex = new Regex($"^{RegexHelper.RewritePattern(packageId)}$");
                        var directories = this.fileSystem.Directory.GetDirectories(source)
                            .Where(x => regex.IsMatch(Path.GetFileName(x)));
                        foreach (var directory in directories)
                        {
                            this.fileSystem.Directory.Delete(directory, true);
                            this.purgerReporter.Deleted(directory);
                            numberDirectoriesPurged++;
                        }
                    }
                }

                this.purgerReporter.CompletedPruning(true, numberDirectoriesPurged, stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                this.purgerReporter.CompletedPruning(false, numberDirectoriesPurged, stopwatch.Elapsed);
                return Task.FromResult(-3);
            }
            catch (Exception e)
            {
                this.purgerReporter.Exception(e);
                return Task.FromResult(-1);
            }

            return Task.FromResult(0);
        }
    }
}