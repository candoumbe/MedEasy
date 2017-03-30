using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Web.Models
{
    public class PatientListitemModel
    {
        public Guid Id { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTime BirthDate { get; set; }

        public string BirthPlace { get; set; }


    }
}
