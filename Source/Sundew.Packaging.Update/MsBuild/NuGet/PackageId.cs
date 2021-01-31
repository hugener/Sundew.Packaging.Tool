// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageId.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Update.MsBuild.NuGet
{
    using global::NuGet.Versioning;

    public record PackageId(string Id, NuGetVersion? NuGetVersion = null);

    public record PackageIdAndVersion(string Id, NuGetVersion NuGetVersion);

    public record PackageUpdateSuggestion(string Id, NuGetVersion NuGetVersion, NuGetVersion? PinnedNuGetVersion) : PackageIdAndVersion(Id, NuGetVersion);

    public record PackageUpdate(string Id, NuGetVersion NuGetVersion, NuGetVersion UpdatedNuGetVersion) : PackageIdAndVersion(Id, NuGetVersion);
}