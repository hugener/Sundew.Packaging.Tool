// --------------------------------------------------------------------------------------------------------------------
// <copyright file="describe_package_updater_facade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable 8602
namespace Sundew.Build.Update.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using global::NuGet.Versioning;
    using Moq;
    using Sundew.Base.Collections;
    using Sundew.Base.Text;
    using Sundew.Build.Update.MsBuild;
    using Sundew.Build.Update.MsBuild.NuGet;

    public class describe_package_updater_facade : nspec
    {
        void when_updating_packages()
        {
            this.beforeEach = () =>
             {
                 this.fileSystem = New.Mock<IFileSystem>().SetDefaultValue(DefaultValue.Mock);
                 this.nuGetPackageVersionFetcher = New.Mock<INuGetPackageVersionFetcher>();
                 this.packageVersionSelectorReporter = New.Mock<IPackageVersionSelectorReporter>();
                 this.packageUpdaterFacade = new PackageUpdaterFacade(
                     this.fileSystem,
                     this.nuGetPackageVersionFetcher,
                     New.Mock<IPackageUpdaterFacadeReporter>(),
                     New.Mock<IPackageVersionUpdaterReporter>(),
                     this.packageVersionSelectorReporter);

                 TestData.GetPackages().ForEach(x =>
                 {
                     this.nuGetPackageVersionFetcher.Setup(n => n.GetAllVersionsAsync(
                             It.IsAny<string>(),
                             It.IsAny<string>(),
                             x.Id))
                         .ReturnsAsync(x.Versions);
                 });
             };

            this.actAsync = () => this.packageUpdaterFacade?.UpdatePackagesInProjectsAsync(this.arguments!);

            this.context[$"given projects: {TestData.GetProjects().AggregateToStringBuilder((builder, data) => builder.Append(data.Path).Append(',').Append(' '), builder => builder.ToStringFromEnd(2))}"] = () =>
            {
                this.beforeEach = () =>
                {
                    this.fileSystem!.Directory.Setup(x => x.GetCurrentDirectory()).Returns(TestData.RootDirectory);
                    this.fileSystem.Directory.Setup(x =>
                        x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories)).Returns(
                        TestData.GetProjects().Select(x => x.Path));
                    TestData.GetProjects().ForEach(x =>
                    {
                        this.fileSystem.File.Setup(f => f.ReadAllTextAsync(x.Path, CancellationToken.None)).ReturnsAsync(x.Source);
                    });
                };

                this.context["and arguments: package id: Sundew*"] = () =>
                {
                    this.beforeEach = () => this.arguments = new Arguments(new List<PackageId> { new("Sundew*") }, new List<string>());

                    this.context["and arguments: projects: Sundew*"] = () =>
                    {
                        this.beforeEach = () => this.arguments = new Arguments(this.arguments.PackageIds.ToList(), new List<string> { "Sundew*" });

                        this.context["and do not allow update to prerelease"] = () =>
                        {
                            TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedNonPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                            TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                            TestData.TransparentMoqProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                            TestData.SundewBasePackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            TestData.SundewBuildPublishPackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            this.it["should not update any other packages"] = () =>
                                this.packageVersionSelectorReporter.Verify(
                                    r => r.PackageUpdateSelected(
                                        It.Is<string>(x =>
                                            x != TestData.SundewBasePackage.Id &&
                                            x != TestData.SundewBuildPublishPackage.Id),
                                        It.IsAny<NuGetVersion>(),
                                        It.IsAny<NuGetVersion>()),
                                    Times.Never);
                        };

                        this.context["and does allow update to prerelease"] = () =>
                        {
                            this.beforeEach = () => this.arguments = new Arguments(this.arguments.PackageIds.ToList(), this.arguments.Projects.ToList(), allowPrerelease: true);

                            TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                            TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                            TestData.TransparentMoqProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                            TestData.SundewBasePrereleasePackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            TestData.SundewBuildPublishPackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            this.it["should not update any other packages"] = () =>
                                this.packageVersionSelectorReporter.Verify(
                                    r => r.PackageUpdateSelected(
                                        It.Is<string>(x =>
                                            x != TestData.SundewBasePackage.Id &&
                                            x != TestData.SundewBuildPublishPackage.Id),
                                        It.IsAny<NuGetVersion>(),
                                        It.IsAny<NuGetVersion>()),
                                    Times.Never);
                        };

                        this.context["and pins Sundew.Base version to 6.0.0"] = () =>
                        {
                            this.beforeEach = () => this.arguments = new Arguments(new List<PackageId> { new("Sundew.Base", NuGetVersion.Parse("6.0.0")) }, this.arguments.Projects.ToList());

                            TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                    f => f.WriteAllTextAsync(x.Path, TestData.SundewCommandLineData.PinnedSundewBaseUpdatedSource, CancellationToken.None),
                                    Times.Once));

                            TestData.TransparentMoqProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                        f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None),
                                        Times.Never));

                            TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                        f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None),
                                        Times.Never));

                            TestData.SundewBasePackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(
                                            r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion),
                                            Times.Once()));

                            this.it["should not update any other packages"] = () =>
                                this.packageVersionSelectorReporter.Verify(
                                    r => r.PackageUpdateSelected(
                                        It.Is<string>(x => x != TestData.SundewBasePackage.Id),
                                        It.IsAny<NuGetVersion>(),
                                        It.IsAny<NuGetVersion>()),
                                    Times.Never);
                        };
                    };

                    TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                                    () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedNonPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                    TestData.TransparentMoqProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                        () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedNonPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                    TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                        () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                    TestData.SundewBasePackageUpdateForSundewCommandLine.Assert(x =>
                        this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                            () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                    TestData.SundewBuildPublishPackageUpdateForSundewCommandLine.Assert(x =>
                        this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                            () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                    this.it["should not update any other packages"] = () =>
                        this.packageVersionSelectorReporter.Verify(
                            r => r.PackageUpdateSelected(
                                It.Is<string>(x =>
                                    x != TestData.SundewBasePackage.Id &&
                                    x != TestData.SundewBuildPublishPackage.Id),
                                It.IsAny<NuGetVersion>(),
                                It.IsAny<NuGetVersion>()),
                            Times.Never);
                };
            };
        }

        PackageUpdaterFacade? packageUpdaterFacade;
        IFileSystem? fileSystem;
        Arguments? arguments;
        INuGetPackageVersionFetcher? nuGetPackageVersionFetcher;
        IPackageVersionSelectorReporter? packageVersionSelectorReporter;
    }
}