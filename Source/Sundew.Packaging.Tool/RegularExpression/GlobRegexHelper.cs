// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobRegexHelper.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.RegularExpression
{
    using System.Text.RegularExpressions;

    public static class GlobRegexHelper
    {
        private const string Dot = ".";
        private const string RegexDot = @"\.";
        private const string WildcardAnyWithBackslash = @"**\";
        private const string WildcardAnyWithSlash = "**/";
        private const string WildcardAny = "**";
        private const string RegexWildcardAny = ".*(?:\\|/)";
        private const string WildcardAnyWithoutBackslash = "*";
        private const string RegexWildcardAnyWithoutBackslash = @"[^\\/]*";
        private const string WildcardOne = "?";
        private const string RegexWildcardOne = @"[^\\/]";
        private const string Backslash = @"\";
        private const string RegexBackslash = @"\\";
        private const string GlobPattern = @"\*\*\\|\*\*/|\*\*|(?<NegativeRange>\[\!\.+\])|\.|\*|\?|\\";
        private const string NegativeRange = "NegativeRange";
        private const string Exclamation = "!";
        private const string Hat = "^";

        public static string RewritePattern(string globText)
        {
            return Regex.Replace(globText, GlobPattern, match =>
            {
                if (match.Groups[NegativeRange].Success)
                {
                    return match.Value.Replace(Exclamation, Hat);
                }

                return match.Value switch
                {
                    WildcardAny => RegexWildcardAny,
                    WildcardAnyWithBackslash => ".*",
                    WildcardAnyWithSlash => ".*",
                    WildcardAnyWithoutBackslash => RegexWildcardAnyWithoutBackslash,
                    WildcardOne => RegexWildcardOne,
                    Dot => RegexDot,
                    Backslash => RegexBackslash,
                    _ => match.Value,
                };
            });
        }

        public static Regex CreateRegex(string globText, RegexOptions regexOptions = RegexOptions.None)
        {
            return new($"^{RewritePattern(globText)}$", regexOptions);
        }
    }
}