using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class ConfigurationEntity : BaseTableEntity<ConfigurationEntity>
    {
        private static string _tableName = "configuration";

        public ConfigurationEntity() : base(_tableName)
        {

        }

        public ConfigurationEntity Get()
        {
            var query = GetTable().CreateQuery<ConfigurationEntity>();
            var config = query.FirstOrDefault();
            return config;
        }
        
        public string APISendNotificationURL { get; set; }
        public string APISendUpdateBlobURL { get; set; }
        public string APISendUpdateKPIURL { get; set; }
        public string LuisAppId { get; set; }
        public string LuisKey { get; set; }
        public string SpeechLanguage { get; set; }
        public string SpeechSubscriptionKey { get; set; }
        public string SpeeechServiceRegion { get; set; }
        public string StorageConnectionString { get; set; }
    }
}
