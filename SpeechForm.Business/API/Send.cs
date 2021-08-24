using Newtonsoft.Json;
using SpeechForm.Repository.Entity.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using kpiModel = SpeechForm.Models.KPI;
using System.Net.Http;
using System.Net.Http.Formatting;
using SpeechForm.Models.Attendance;
using SpeechForm.Repository;

namespace SpeechForm.Business.API
{
    public class Send
    {
        private ConfigurationEntity _configuration;

        public Send()
        {
            _configuration = Singleton.Configuration;

        }

        public async Task<string> Notification(BaseTableEntity<NotificationEntity> Notification)
        {
            string apiResponse = string.Empty;
            using (var httpClient = new HttpClient())
            {

                using (var response = await httpClient.PostAsJsonAsync(_configuration.APISendNotificationURL, Notification))
                {
                    apiResponse = await response.Content.ReadAsStringAsync();

                }
            }
            return apiResponse;
        }

        public async Task<string> UpdateKPI(kpiModel.KPI kpi)
        {
            string apiResponse = string.Empty;
            using (var httpClient = new HttpClient())
            {

                using (var response = await httpClient.PostAsJsonAsync(_configuration.APISendUpdateKPIURL, kpi))
                {
                    apiResponse = await response.Content.ReadAsStringAsync();

                }
            }
            return apiResponse;
        }

        public async Task<string> UpdateBlob(ServiceOperation serviceOperation)
        {
            string apiResponse = string.Empty;
            using (var httpClient = new HttpClient())
            {

                using (var response = await httpClient.PostAsJsonAsync(_configuration.APISendUpdateBlobURL, serviceOperation))
                {
                    apiResponse = await response.Content.ReadAsStringAsync();

                }
            }
            return apiResponse;
        }
    }
}
