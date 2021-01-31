// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Update
{
    using System;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Sundew.Base.Computation;
    using Sundew.Build.Update.MsBuild.NuGet;
    using Sundew.CommandLine;

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
            var packageUpdaterFacade = new PackageUpdaterFacade(new FileSystem(), new NuGetPackageVersionFetcher(), consoleReporter, consoleReporter, consoleReporter);
            await packageUpdaterFacade.UpdatePackagesInProjectsAsync(arguments);
            return Result.Success(0);
        }
    }
}
