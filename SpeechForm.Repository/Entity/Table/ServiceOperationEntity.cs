using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class ServiceOperationEntity : BaseTableEntity<ServiceOperationEntity>
    {
        private static string _tableName = "serviceoperation";
        
        public ServiceOperationEntity() : base(_tableName)
        {
            var key = Guid.NewGuid().ToString();
            PartitionKey = key;
            RowKey = key;
            ServiceOperationId = key;
        }

        public string ServiceOperationId { get; set; }
        public bool Active { get; set; }
        public string SupervisorId { get; set; }
        public string Description { get; set; }
    }
}
