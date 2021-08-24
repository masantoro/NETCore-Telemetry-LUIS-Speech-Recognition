using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpeechForm.Web.Models;
using SpeechForm.Repository.Entity.Table;
using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Web.Models.Enum;

namespace SpeechForm.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            
            return View();
        }

        

        public IActionResult Configuracao()
        {
            var settings = (new SettingsEntity()).All();
            return View(settings);
        }

        [HttpPost]
        public IActionResult SaveSettings(List<SettingsEntity> model)
        {
            try
            {
                foreach (var item in model)
                {
                    item.InsertOrMergeEntityAsync().Wait();
                }
                return Ok(new { errou = false });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = true });
            }
            
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
