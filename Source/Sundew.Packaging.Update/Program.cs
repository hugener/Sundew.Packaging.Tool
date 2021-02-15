// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Update
{
    using System;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Sundew.Base.Computation;
    using Sundew.CommandLine;
    using Sundew.Packaging.Update.Diagnostics;
    using Sundew.Packaging.Update.MsBuild.NuGet;

    public static class Program
    {
        public static async Task Main()
        {
            try
            {
                var commandLineParser = new CommandLineParser<int, int>();
                commandLineParser.WithArguments(new Arguments(), ExecuteArguments);
                var result = await commandLineParser.ParseAsync(Environment.CommandLine, 1);
                if (!result)
                {
                    result.WriteToConsole();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static async ValueTask<Result<int, ParserError<int>>> ExecuteArguments(Arguments arguments)
        {
            var consoleReporter = new ConsoleReporter(arguments.Verbose);
            var packageUpdaterFacade = new PackageUpdaterFacade(new FileSystem(), new NuGetPackageVersionFetcher(), new ProcessRunner(), consoleReporter, consoleReporter, consoleReporter, consoleReporter);
            await packageUpdaterFacade.UpdatePackagesInProjectsAsync(arguments);
            return Result.Success(0);
        }
    }
}
