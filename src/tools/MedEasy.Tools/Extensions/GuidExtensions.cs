using System;

namespace MedEasy.Tools.Extensions
{
    /// <summary>
    /// Extensions methods for Guid
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Encodes the <paramref name="guid"/> to a 22 characters length, url safe  <see cref="string"/>.
        /// </summary>
        /// <remarks>The encoded string </remarks>
        /// <param name="guid">The <see cref="Guid"/>to encode</param>
        /// <returns>The encoded <see cref="string"/></returns>
        public static string Encode(this Guid guid)
        {
            string enc = Convert.ToBase64String(guid.ToByteArray());
            enc = enc.Replace("/", "_");
            enc = enc.Replace("+", "-");
            return enc.Substring(0, 22);
        }
    }
}
