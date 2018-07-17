using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.DTO
{
    public class BearerTokenInfo
    {
        public string Token { get; set; }

        public long Expires { get; set; }

    }
}
