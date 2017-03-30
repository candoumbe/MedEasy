using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace MedEasy.Web.Controllers
{
    [Controller]
    public class PatientController
    {
        /// <summary>

        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                string json = await client.GetStringAsync("http://localhost:5000/api/Patients");
                
            }
            return new ViewResult();
        }
    }
}