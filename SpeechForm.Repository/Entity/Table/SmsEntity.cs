using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class SmsEntity : BaseTableEntity<SmsEntity>
    {
        private static string _tableName = "sms";
        
        public SmsEntity() : base(_tableName)
        {
            PartitionKey = "SMS";
            RowKey = Guid.NewGuid().ToString();
        }

        public string CallId { get; set; }
        public string Solved { get; set; }
        public string Score { get; set; }
        public string Message { get; set; }
    }
}
