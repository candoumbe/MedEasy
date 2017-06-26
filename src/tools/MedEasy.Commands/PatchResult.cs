using System;
using System.Collections.Generic;
using System.Text;

namespace MedEasy.Commands
{
    public class PatchResult
    {

        public IEnumerable<(string code, string msg)> Errors { get; set; }


        public bool Success { get; set; }
    }
}
