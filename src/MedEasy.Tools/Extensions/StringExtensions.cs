using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using static System.Collections.Generic.EnumerableExtensions;
using static System.Linq.Expressions.Expression;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="string"/> type
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the <paramref name="input"/> to its Title Case equivalent
        /// </summary>
        /// <param name="input">the string to convert</param>
        /// <returns>the string converted to Title case</returns>
        /// <example><c>"cyrille-alexandre".<see cref="ToTitleCase()"/></c> returns <c>"Cyrille-Alexandre"</c></example>
        public static string ToTitleCase(this string input)
        {
            StringBuilder sbResult = null;
            if ((input?.ToCharArray()?.AtLeastOnce() ?? false))
            {
                sbResult = new StringBuilder(input);
                if (char.IsLetter(sbResult[0]))
                {
                    sbResult[0] = char.ToUpper(sbResult[0]);
                }

                for (int i = 1; i < sbResult.Length; i++)
                {
                    if (char.IsWhiteSpace(sbResult[i - 1]) || sbResult[i - 1] == '-')
                    {
                        sbResult[i] = char.ToUpper(sbResult[i]);
                    }
                }
            }

            return sbResult?.ToString() ?? string.Empty;
        }


        /// <summary>
        /// Perfom
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static bool Like(this string input, string pattern, bool ignoreCase = true)
        {
            RegexOptions regexOptions = RegexOptions.Singleline;
            if (ignoreCase)
            {
                regexOptions = regexOptions | RegexOptions.IgnoreCase;
            }
            pattern = pattern.Replace("?", ".")
                .Replace("*", ".*");
            //return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(pattern, ch => @"\" + ch)
            //    .Replace('?', '.')
            //    .Replace("*", ".*")
            //    + @"\z", regexOptions).IsMatch(input);

            return Regex.IsMatch(input, pattern, regexOptions);
        }


        /// <summary>
        /// Converts <paramref name="source"/> to its <see cref="LambdaExpression"/> equivalent
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <c>null</c>.</exception>
        public static LambdaExpression ToLambda<TSource>(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ParameterExpression pe = Parameter(typeof(TSource), "x");
            string[] fields = source.Split(new[] { '.' });
            MemberExpression property = null;
            foreach (string field in fields)
            {
                property = property == null
                    ? Property(pe, field)
                    : Property(property, field);
            }


            return Lambda(property, pe);
        }

    }
}
