using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.DTO
{
    /// <summary>
    /// Custom claims that can be used throughout the application
    /// </summary>
    public class CustomClaimTypes
    {
        /// <summary>
        /// Name of the claim that holds the account id
        /// </summary>
        public static readonly string AccountId = "account-id";
    }
}
