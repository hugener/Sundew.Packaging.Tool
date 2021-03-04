﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateVerbTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Tests
{
    using FluentAssertions;
    using NuGet.Versioning;
    using NUnit.Framework;
    using Sundew.Base.Computation;
    using Sundew.CommandLine;
    using Sundew.Packaging.Tool.MsBuild.NuGet;

    [TestFixture]
    public class UpdateVerbTests
    {
        [TestCase(@"u -id Sundew.Base", "Sundew.Base", null, null)]
        [TestCase(@"u -id Sundew.Base.6.0.0", "Sundew.Base", "6.0.0", false)]
        [TestCase(@"u -id ""Sundew.Base 6.0.0""", "Sundew.Base", "6.0.0", false)]
        [TestCase(@"u -id ""Sundew.Base 6.0.0-pre""", "Sundew.Base", "6.0.0-pre", false)]
        [TestCase(@"u -id WithNumber.6.6.0.0-pre", "WithNumber", "6.6.0.0-pre", false)]
        [TestCase(@"u -id ""WithIllegalNumber.6 6.0.0-pre""", "WithIllegalNumber.6", "6.0.0-pre", false)]
        [TestCase(@"u -id WithIllegal.6.Number.6.0.0-pre", "WithIllegal.6.Number", "6.0.0-pre", false)]
        [TestCase(@"u -id WithIllegal.6.Number.16.0.0-pre", "WithIllegal.6.Number", "16.0.0-pre", false)]
        [TestCase(@"u -id WithIllegal.6.Number.6.10.0-pre", "WithIllegal.6.Number", "6.10.0-pre", false)]
        [TestCase(@"u -id WithIllegal.6.Number.6.0.10-pre", "WithIllegal.6.Number", "6.0.10-pre", false)]
        [TestCase(@"u -id Sundew.Base.6.0", "Sundew.Base", "6.0.0", true)]
        [TestCase(@"u -id ""Sundew.Base 6.0""", "Sundew.Base", "6.0.0", true)]
        [TestCase(@"u -id ""Sundew.Base 6.0-pre""", "Sundew.Base", "6.0.0-pre", true)]
        public void Parse_When_PackageIdIsSpecifiedWithVersion_Then_VersionShouldBeParsedSuccessfully(string input, string expectedId, string? expectedVersion, bool? useMajorMinorSearchMode)
        {
            var commandLineParser = new CommandLineParser<int, int>();
            var arguments = commandLineParser.AddVerb(new UpdateVerb(), updateVerb => Result.Success(0));

            commandLineParser.Parse(input);

            arguments.PackageIds.Should().Equal(new[] { new PackageId(expectedId, expectedVersion != null ? NuGetVersion.Parse(expectedVersion) : null, useMajorMinorSearchMode) });
        }
    }
}