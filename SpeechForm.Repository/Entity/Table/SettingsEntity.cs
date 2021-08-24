using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class SettingsEntity : BaseTableEntity<SettingsEntity>
    {
        private static string _tableName = "settings";
        
        public SettingsEntity() : base(_tableName)
        {
            
        }


        public SettingsEntity(NotificationEnum.To to) : base(to.ToString(), to.ToString(), _tableName)
        {

        }

        public List<SettingsEntity> All()
        {
            var result = base.All<SettingsEntity>();
            if(result.Count == 0)
            {
                var settings = new SettingsEntity(NotificationEnum.To.Supervisor);
                settings.InsertOrMergeEntityAsync().Wait();

                settings = new SettingsEntity(NotificationEnum.To.Atendente);
                settings.InsertOrMergeEntityAsync().Wait();
                
                result = base.All<SettingsEntity>();
            }

            return result;
        }

        
        public int TempoSilencio { get; set; }
        public bool SobreposicaoVoz { get; set; }
        public bool SentimentoNegativoCliente { get; set; }
        public bool SentimentoNegativoAtendente { get; set; }

        public Double NegativoInicio { get; set; }
        public Double NegativoFim { get; set; }
        public Double PositivoInicio { get; set; }
        public Double PositivoFim { get; set; }
        public Double NeutroInicio { get; set; }
        public Double NeutroFim { get; set; }
    }
}
