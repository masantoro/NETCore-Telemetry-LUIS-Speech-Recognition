using System;
using System.Threading.Tasks;
using SpeechForm.Models.Attendance;
using SpeechForm.Models.Attendance.Enum;
using SpeechForm.Repository;
using SpeechForm.Repository.Entity;
using SpeechForm.Repository.Entity.Table;

namespace SpeechForm.Business.Attendance
{
    public class ServiceManager
    {
        private ConfigurationEntity _configuration;

        private string speeechServiceRegion;
        private string speechSubscriptionKey;
        private string speechLanguage;
        private string luisKey;
        private string luisAppId;

        private SpeechService _speechService;

        public void Speech(Call call, Channel channel)
        {
            _configuration = Singleton.Configuration;

            speeechServiceRegion = _configuration.SpeeechServiceRegion;
            speechSubscriptionKey = _configuration.SpeechSubscriptionKey;
            speechLanguage = _configuration.SpeechLanguage;
            luisKey = _configuration.LuisKey;
            luisAppId = _configuration.LuisAppId;

            LUISService _LUISService = new LUISService(luisKey, luisAppId);
            _speechService = new SpeechService(subscriptionKey: speechSubscriptionKey, region: speeechServiceRegion,
                                                            speechLanguage: speechLanguage, LUISservice: _LUISService);

            _speechService.GetSpeechTranscriptionAndLUISIntentAndSentiment(call, channel).ContinueWith((Task task) => { Exception ex = task.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void StopService()
        {
            _speechService.StopService();
        }
    }
}