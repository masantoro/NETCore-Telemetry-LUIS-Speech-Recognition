using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class ServiceOperationAttendantEntity : BaseTableEntity<ServiceOperationAttendantEntity>
    {
        private static string _tableName = "serviceoperationattendant";
        
        public ServiceOperationAttendantEntity() : base(_tableName)
        {
            var key = Guid.NewGuid().ToString();
            PartitionKey = key;
            RowKey = key;
            ServiceOperationAttendantId = key;
        }

        public string ServiceOperationAttendantId { get; set; }
        public string ServiceOperationId { get; set; }
        public bool Active { get; set; }
        public string AttendantId { get; set; }
    }
}
