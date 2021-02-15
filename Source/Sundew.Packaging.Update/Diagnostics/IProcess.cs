﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcess.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Update.Diagnostics
{
    using System;
    using System.IO;

    public interface IProcess
    {
        public int ExitCode { get; }

        public bool HasExited { get; }

        public DateTime StartTime { get; }

        public DateTime ExitTime { get; }

        public int Id { get; }

        public string MachineName { get; }

        public StreamReader StandardOutput { get; }

        public StreamReader StandardError { get; }

        public StreamWriter StandardInput { get; }
    }
}