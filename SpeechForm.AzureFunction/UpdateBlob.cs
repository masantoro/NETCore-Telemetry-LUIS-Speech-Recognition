using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpeechForm.Models.KPI;
using SpeechForm.Business.KPI;
using SpeechForm.AzureFunction.Model;
using SpeechForm.Models.Attendance;

namespace SpeechForm.AzureFunction
{
    public static class UpdateBlob
    {
        [FunctionName("UpdateBlob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var result = new ResultModel();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var serviceOperation = JsonConvert.DeserializeObject<ServiceOperation>(requestBody);
                KPIManager.SaveBlob(serviceOperation);

                result.StatusCode = 200;
                result.Message = "Job successfully executed!";

                return new OkObjectResult(result);
            }
            catch(Exception ex)
            {
                result.StatusCode = 400;
                result.Message = ex.Message;
                return new BadRequestObjectResult(result);
            }
        }
    }
}
