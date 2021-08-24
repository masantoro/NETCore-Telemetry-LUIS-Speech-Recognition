using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class CallEntity : BaseTableEntity<CallEntity>
    {
        private static string _tableName = "call";

        public CallEntity() : base(_tableName)
        {

        }

        public CallEntity(string callId) : base(callId, callId, _tableName)
        {
            CallId = callId;
        }

        public string CallId { get; set; }
        public string ServiceOperationId { get; set; }
        public string AttendantId { get; set; }
        public string SupervisorId { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }
}
