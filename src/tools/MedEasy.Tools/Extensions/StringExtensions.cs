using System.Globalization;
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
            if (input?.ToCharArray()?.AtLeastOnce() ?? false)
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
        /// Converts the <paramref name="input"/> to its camelCase equivalent
        /// </summary>
        /// <param name="input">the string to convert</param>
        /// <returns>the string converted to Title case</returns>
        /// <example><c>"cyrille-alexandre".<see cref="ToTitleCase()"/></c> returns <c>"cyrilleAlexandre"</c></example>
        public static string ToCamelCase(this string input)
        {
            StringBuilder sbResult = null;
            if (input?.ToCharArray()?.AtLeastOnce() ?? false)
            {
                sbResult = new StringBuilder(input);
                if (char.IsLetter(sbResult[0]))
                {
                    sbResult[0] = char.ToLower(sbResult[0]);
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
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the string to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <c>null</c>.</exception>
        public static bool Like(this string input, string pattern) => input.Like(pattern, ignoreCase: true);

        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the string to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <c>null</c>.</exception>
        public static bool Like(this string input, string pattern, bool ignoreCase)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            RegexOptions regexOptions = RegexOptions.Singleline;
            if (ignoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            pattern = pattern.Replace("?", ".")
                .Replace("*", ".*");

            return Regex.IsMatch(input, $"{pattern}$", regexOptions);
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

        /// <summary>
        /// Decodes a <see cref="string"/> converted using <see cref="GuidExtensions.Encode{string}"/> back to <see cref="Guid"/>. 
        /// </summary>
        /// <remarks>
        /// See http://madskristensen.net/post/A-shorter-and-URL-friendly-GUID for more details.
        /// </remarks>
        /// <param name="encoded">the encoded <see cref="Guid"/> string</param>
        /// <returns>The original <see cref="Guid"/></returns>
        public static Guid Decode(this string encoded)
        {
            encoded = encoded.Replace("_", "/");
            encoded = encoded.Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(encoded + "==");
            return new Guid(buffer);
        }

        /// <summary>
        /// Converts <see cref="input"/> to its lower kebab representation
        /// 
        /// </summary>
        /// <param name="input">The string to transform</param>
        /// <returns>The lower-kebab-cased string</returns>
        public static string ToLowerKebabCase(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), $"{nameof(input)} cannot be null");
            }

            StringBuilder sb = new StringBuilder(input.Length * 2);
            foreach (char character in input)
            {
                if (char.IsUpper(character) && sb.Length > 0)
                {
                    sb.Append("-");
                }
                sb.Append(char.ToLower(character));
            }

            return sb.ToString();
        }

#if !NETSTANDARD1_0 && !NETSTANDARD1_1
        /// <summary>
        /// Removes diacritics from <paramref name="input"/>
        /// </summary>
        /// <param name="input">where to remove diacritics</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

#endif

    }
}
