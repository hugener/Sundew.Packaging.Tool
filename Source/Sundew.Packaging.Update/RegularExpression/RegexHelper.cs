// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegexHelper.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Update.RegularExpression
{
    public static class RegexHelper
    {
        private const string Dot = ".";
        private const string RegexWildcard = ".*";
        private const string EscapedDot = @"\.";
        private const string Wildcard = "*";

        public static string RewritePattern(string wildcardText)
        {
            return wildcardText.Replace(Dot, EscapedDot).Replace(Wildcard, RegexWildcard);
        }
    }
}