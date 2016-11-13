using System.Linq;
using System.Text;

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
            StringBuilder sbResult = new StringBuilder(input?.Length ?? 0);

            if (input?.Any() ?? false)
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

            return sbResult.ToString();
        }
    }
}
