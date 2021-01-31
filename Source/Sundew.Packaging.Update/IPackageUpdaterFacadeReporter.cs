// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageUpdaterFacadeReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Update
{
    using System;
    using System.Collections.Generic;
    using Sundew.Build.Update.MsBuild;

    public interface IPackageUpdaterFacadeReporter
    {
        void StartingPackageUpdate(string rootDirectory);

        void UpdatingProject(string project);

        void CompletedPackageUpdate(List<MsBuildProject> msBuildProjects, TimeSpan totalTime);

        void Exception(Exception exception);
    }
}