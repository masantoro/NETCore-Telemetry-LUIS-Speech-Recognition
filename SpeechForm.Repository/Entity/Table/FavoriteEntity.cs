using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class FavoriteEntity : BaseTableEntity<FavoriteEntity>
    {
        private static string _tableName = "favorite";
        
        public FavoriteEntity() : base(_tableName)
        {
            PartitionKey = "FAVORITE";
            RowKey = Guid.NewGuid().ToString();
        }

        public FavoriteEntity GetOne(string serviceOperationId, string supervisorId)
        {
            var query = GetTable().CreateQuery<FavoriteEntity>();
            var notifications = query
                .Where(x =>
                    x.ServiceOperationId == serviceOperationId &&
                    x.SupervisorId == supervisorId)
                .ToList().FirstOrDefault();
            return notifications;
        }


        public string ServiceOperationId { get; set; }
        public string SupervisorId { get; set; }
        public string AttendantId { get; set; }
    }
}
