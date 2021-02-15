// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arguments.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Update
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using NuGet.Versioning;
    using Sundew.CommandLine;
    using Sundew.Packaging.Update.MsBuild.NuGet;

    public class Arguments : IArguments
    {
        private const string VersionGroupName = "Version";
        private const string LocalSundewName = "Local-Sundew";
        private const string PatchGroupName = "Patch";
        private static readonly Regex PackageIdAndVersionRegex = new(@"(?: |\.)(?<Version>(?:\d+\.\d+(?<Patch>\.\d+)?).*)");
        private static readonly Regex VersionRegex = new(@"(?<Version>(?:\d+\.\d+(?<Patch>\.\d+)?).*)");
        private readonly List<PackageId> packageIds;
        private readonly List<string> projects;
        private bool useLocalSource;

        public Arguments()
        : this(new List<PackageId> { new("*") }, new List<string> { "*" })
        {
        }

        public Arguments(List<PackageId> packageIds, List<string> projects, string? source = null, PinnedNuGetVersion? pinnedNuGetVersion = null, string? rootDirectory = null, bool allowPrerelease = false, bool verbose = false, bool useLocalSource = false, bool skipRestore = false)
        {
            this.packageIds = packageIds;
            this.projects = projects;
            this.Source = source;
            this.PinnedNuGetVersion = pinnedNuGetVersion;
            this.RootDirectory = rootDirectory;
            this.AllowPrerelease = allowPrerelease;
            this.Verbose = verbose;
            this.UseLocalSource = useLocalSource;
            this.SkipRestore = skipRestore;
        }

        public IReadOnlyList<PackageId> PackageIds => this.packageIds;

        public IReadOnlyList<string> Projects => this.projects;

        public string? Source { get; private set; }

        public PinnedNuGetVersion? PinnedNuGetVersion { get; private set; }

        public string? RootDirectory { get; private set; }

        public bool AllowPrerelease { get; private set; }

        public bool Verbose { get; private set; }

        public bool UseLocalSource
        {
            get => this.useLocalSource;
            private set
            {
                this.useLocalSource = value;
                if (this.UseLocalSource)
                {
                    this.Source = LocalSundewName;
                }
            }
        }

        public bool SkipRestore { get; private set; }

        public void Configure(IArgumentsBuilder argumentsBuilder)
        {
            argumentsBuilder.AddOptionalList("id", "package-ids", this.packageIds, this.SerializePackageId, this.DeserializePackageId, @$"The package(s) to update. (* Wildcards supported){Environment.NewLine}Format: Id[.Version] or ""Id[ Version]"" (Pinning version is optional)");
            argumentsBuilder.AddOptionalList("p", "projects", this.projects, "The project(s) to update (* Wildcards supported)");
            argumentsBuilder.AddOptional("s", "source", () => this.Source, s => this.Source = s, @"The source or source name to search for packages (""All"" supported)", defaultValueText: "NuGet.config: defaultPushSource");
            argumentsBuilder.AddOptional(null, "version", () => this.SerializeVersion(this.PinnedNuGetVersion), s => this.PinnedNuGetVersion = this.DeserializeVersion(s), "Pins the NuGet package version.", defaultValueText: "Latest version");
            argumentsBuilder.AddOptional("d", "root-directory", () => this.RootDirectory, s => this.RootDirectory = s, "The directory to search for projects", true, defaultValueText: "Current directory");
            argumentsBuilder.AddSwitch("pr", "prerelease", this.AllowPrerelease, b => this.AllowPrerelease = b, "Allow updating to latest prerelease version");
            argumentsBuilder.AddSwitch("v", "verbose", this.Verbose, b => this.Verbose = b, "Verbose");
            argumentsBuilder.AddSwitch("l", "local", this.UseLocalSource, b => this.UseLocalSource = b, $@"Forces the source to ""{LocalSundewName}""");
            argumentsBuilder.AddSwitch("sr", "skip-restore", this.SkipRestore, b => this.SkipRestore = b, "Skips a dotnet restore command after package update.");
        }

        private static string SerializeVersion(NuGetVersion pinnedNuGetVersion, bool? useMajorMinorSearchMode)
        {
            if (useMajorMinorSearchMode.GetValueOrDefault(false))
            {
                return $"{pinnedNuGetVersion.Major}.{pinnedNuGetVersion.Minor}{GetRelease(pinnedNuGetVersion)}";
            }

            return pinnedNuGetVersion.ToString();
        }

        private static string GetRelease(NuGetVersion pinnedNuGetVersion)
        {
            return string.IsNullOrEmpty(pinnedNuGetVersion.Release) ? string.Empty : $"-{pinnedNuGetVersion.Release}";
        }

        private string? SerializeVersion(PinnedNuGetVersion? pinnedNuGetVersion)
        {
            if (pinnedNuGetVersion == null)
            {
                return null;
            }

            return SerializeVersion(pinnedNuGetVersion.NuGetVersion, pinnedNuGetVersion.UseMajorMinorSearchMode);
        }

        private PinnedNuGetVersion DeserializeVersion(string pinnedNuGetVersion)
        {
            var match = VersionRegex.Match(pinnedNuGetVersion);
            if (match.Success)
            {
                return new PinnedNuGetVersion(NuGetVersion.Parse(pinnedNuGetVersion), !match.Groups[PatchGroupName].Success);
            }

            throw new ArgumentException($"Invalid version: {pinnedNuGetVersion}", nameof(pinnedNuGetVersion));
        }

        private string SerializePackageId(PackageId id, CultureInfo cultureInfo)
        {
            if (id.NuGetVersion != null)
            {
                return $"{id.Id}.{SerializeVersion(id.NuGetVersion, id.UseMajorMinorSearchMode)}";
            }

            return id.Id;
        }

        private PackageId DeserializePackageId(string id, CultureInfo cultureInfo)
        {
            var match = PackageIdAndVersionRegex.Match(id);
            if (match.Success)
            {
                var versionGroup = match.Groups[VersionGroupName];
                if (versionGroup.Success)
                {
                    return new PackageId(id.Substring(0, versionGroup.Index - 1), NuGetVersion.Parse(versionGroup.Value), !match.Groups[PatchGroupName].Success);
                }
            }

            return new PackageId(id);
        }
    }
}