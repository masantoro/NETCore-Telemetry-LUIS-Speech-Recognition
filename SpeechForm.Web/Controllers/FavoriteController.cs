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
    public class FavoriteController : ControllerBase
    {
        // GET: api/Favorite
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Favorite/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Favorite
        [HttpPost]
        [EnableCors("AllowOrigin")]
        public IActionResult Post([FromBody] FavoriteModel value)
        {
            var _result = new ResultModel<FavoriteEntity>();

            try
            {
                if (string.IsNullOrEmpty(value.ServiceOperationId))
                {
                    throw new Exception("[ServiceOperationId] ID da Operação é obrigatório");
                }

                if (string.IsNullOrEmpty(value.SupervisorId))
                {
                    throw new Exception("[SupervisorId] ID do(a) Supervisor(a) é obrigatório");
                }

                if (string.IsNullOrEmpty(value.AttendantId))
                {
                    throw new Exception("[AttendantId] ID do(a) Atendente é obrigatório");
                }

                try
                {
                    var entity = (new FavoriteEntity()).GetOne(value.ServiceOperationId, value.SupervisorId);
                    if (entity == null)
                    {
                        entity = new FavoriteEntity();
                    }

                    entity.ServiceOperationId = value.ServiceOperationId;
                    entity.SupervisorId = value.SupervisorId;
                    entity.AttendantId = value.AttendantId;
                    entity.InsertOrMergeEntityAsync().Wait();

                    _result.StatusCode = 200;
                    _result.Message = "Atendente favorito salvo com sucesso!";
                    _result.Entity = entity;
                } 
                catch(Exception ex)
                {
                    _result.StatusCode = 400;
                    _result.Message = "[API-FAV-001] Erro técnico, por favor contate o administrador do sistema!";
                    Console.WriteLine($"{_result.Message} | {ex.Message}");
                    return BadRequest(_result);
                }

                return Ok(_result);
            } 
            catch(Exception ex)
            {
                _result.StatusCode = 400;
                _result.Message = $"INVALID BODY REQUEST - {ex.Message}";
                return BadRequest(_result);
            }
        }

        // PUT: api/Favorite/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Favorite/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
