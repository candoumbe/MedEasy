using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.API.Features.Authentication
{
    public class JwtOptions
    {
        /// <summary>
        /// Validaty in minutes
        /// </summary>
        public int Validity { get; set; }

        public string Key { get; set; }


        public string Issuer { get; set; }

        public IEnumerable<string> Audiences { get; set; }
    }
}
