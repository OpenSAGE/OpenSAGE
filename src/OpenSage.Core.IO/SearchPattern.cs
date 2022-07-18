// This file is from Zio - https://github.com/xoofx/zio
// and is licenced under the BSD 2-Clause "Simplified" License:
//
// Copyright (c) 2017-2021, Alexandre Mutel
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification
// , are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this 
//    list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice, 
//    this list of conditions and the following disclaimer in the documentation 
//    and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenSage.IO
{
    internal readonly struct SearchPattern
    {
        private static readonly char[] SpecialChars = { '?', '*' };

        private readonly string _exactMatch;
        private readonly Regex _regexMatch;

        /// <summary>
        /// Tries to match the specified path with this instance.
        /// </summary>
        /// <param name="name">The path to match.</param>
        /// <returns><c>true</c> if the path was matched, <c>false</c> otherwise.</returns>
        public bool Match(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            // if _execMatch is null and _regexMatch is null, we have a * match
            return _exactMatch != null ? _exactMatch == name : _regexMatch is null || _regexMatch.IsMatch(name);
        }

        public SearchPattern(string searchPattern)
        {
            if (searchPattern is null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            _exactMatch = null;
            _regexMatch = null;

            // Optimized path, most common case
            if (searchPattern is "*")
            {
                return;
            }

            searchPattern = searchPattern.Replace('\\', '/');
            if (searchPattern.Contains('/'))
            {
                throw new NotSupportedException();
            }

            int startIndex = 0;
            int nextIndex;
            StringBuilder builder = null;
            while ((nextIndex = searchPattern.IndexOfAny(SpecialChars, startIndex)) >= 0)
            {
                if (builder is null)
                {
                    builder = new StringBuilder();
                    builder.Append('^');
                }

                var lengthToEscape = nextIndex - startIndex;
                if (lengthToEscape > 0)
                {
                    var toEscape = Regex.Escape(searchPattern.Substring(startIndex, lengthToEscape));
                    builder.Append(toEscape);
                }

                var c = searchPattern[nextIndex];
                var regexPatternPart = c == '*' ? "[^/]*" : "[^/]";
                builder.Append(regexPatternPart);

                startIndex = nextIndex + 1;
            }
            if (builder is null)
            {
                _exactMatch = searchPattern;
            }
            else
            {
                var length = searchPattern.Length - startIndex;
                if (length > 0)
                {
                    var toEscape = Regex.Escape(searchPattern.Substring(startIndex, length));
                    builder.Append(toEscape);
                }

                builder.Append('$');

                var regexPattern = builder.ToString();
                _regexMatch = new Regex(regexPattern, RegexOptions.IgnoreCase);
            }
        }
    }
}
