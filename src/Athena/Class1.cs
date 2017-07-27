using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Athena
{
    public static class StringExtensions
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Regex ParameterExpression =
            new Regex(@"^\{(?<name>[A-Za-z0-9]*)\}", RegexOptions.Compiled);

        public static async Task<Stream> ToStream(this string input)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(input).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;
            return stream;
        }

        public static bool IsParameterized(this string segment)
        {
            var parameterMatch =
                ParameterExpression.Match(segment);

            return parameterMatch.Success;
        }

        public static string GetParameterName(this string segment)
        {
            var nameMatch =
                ParameterExpression.Match(segment);

            if (nameMatch.Success)
            {
                return nameMatch.Groups["name"].Value;
            }

            throw new FormatException("");
        }
    }
}