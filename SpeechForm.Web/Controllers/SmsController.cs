using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpeechForm.Repository.Entity.Table;
using SpeechForm.Web.Models;

namespace SpeechForm.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        // GET: api/Sms
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Sms/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Sms
        [HttpPost]
        [EnableCors("AllowOrigin")]
        public IActionResult Post([FromBody] SmsModel value)
        {
            var _result = new ResultModel<SmsEntity>();

            try
            {
                //bool Solved;
                //if (!bool.TryParse(value.Solved.Value, out Solved))
                //{

                //}


                //if(string.IsNullOrEmpty(value.CallId) || value.Solved == null || value.Score == null)
                //{
                //    throw new Exception();
                //}
                
                var entity = new SmsEntity();

                entity.CallId = value.CallId;
                entity.Solved = value.Solved;
                entity.Score = value.Score;
                entity.Message = value.Message;
                entity.InsertOrMergeEntityAsync().Wait();

                _result.StatusCode = 200;
                _result.Message = "Resposta de SMS recebida com sucesso";
                _result.Entity = entity;

                return Ok(_result);
            } 
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                _result.StatusCode = 400;
                _result.Message = "Invalid body request";
                return BadRequest(_result);
            }
        }

        // PUT: api/Sms/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Sms/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
