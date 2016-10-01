﻿using System.Linq;
using System.Text;

namespace System
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
                    if (char.IsWhiteSpace(sbResult[i - 1]))
                    {
                        sbResult[i] = char.ToUpper(sbResult[i]);
                    }
                }
            }

            return sbResult.ToString();
        }
    }
}