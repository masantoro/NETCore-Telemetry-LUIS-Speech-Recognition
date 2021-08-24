using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpeechForm.Repository.Entity.Table;
using SpeechForm.Web.Models;
using SpeechForm.Models.KPI;
using SpeechForm.Business.KPI;
using SpeechForm.Models.Attendance;

namespace SpeechForm.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateBlobController : ControllerBase
    {
        // GET: api/KPI
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/KPI/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/KPI
        [HttpPost]
        [EnableCors("AllowOrigin")]
        public IActionResult PostAsync([FromBody] ServiceOperation value)
        {
            var _result = new ResultModel<KPIEntity>();

            try
            {
                KPIManager.SaveBlob(value);

                _result.StatusCode = 200;
                _result.Message = "Job successfully executed!";

                return Ok(_result);
            }
            catch (Exception ex)
            {
                _result.StatusCode = 400;
                _result.Message = ex.Message;
                return BadRequest(_result);
            }
        }

        // PUT: api/KPI/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/KPI/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
