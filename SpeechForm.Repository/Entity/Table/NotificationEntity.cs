using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using _enum = SpeechForm.Repository.Entity.Enum.Table;
using data = SpeechForm.Repository.FixedData;

namespace SpeechForm.Repository.Entity.Table
{
    public class NotificationEntity: BaseTableEntity<NotificationEntity>
    {
        private static string _tableName = "notification";
        
        public NotificationEntity() : base(_tableName)
        {

        }

        public NotificationEntity(string callId) : base(callId, callId, _tableName)
        {
            var _notificationId = Guid.NewGuid().ToString();
            RowKey = _notificationId;

            CallId = callId;
            NotificationId = _notificationId;
        }

        public string GetLastOverlayFrom(string CallId)
        {
            var from = string.Empty;
            var query = GetTable().CreateQuery<NotificationEntity>();
            var result = query
                .Where(x => 
                    x.CallId == CallId 
                    && x.Type == _enum.NotificationType.Overlay.ToString())
                .ToList()
                .OrderByDescending(x=> x.Timestamp)
                .Take(1)
                .FirstOrDefault();
            if(result != null)
            {
                from = result.From;
            }
            return from;
        }

        public List<NotificationEntity> GetByCall(string CallId)
        {
            var query = GetTable().CreateQuery<NotificationEntity>();
            var notifications = query
                .Where(x => 
                    x.CallId == CallId)
                .ToList()
                .OrderByDescending(x => x.Timestamp)
                .ToList();
            return notifications;
        }

        public NotificationEntity InsertOrMergeNotification()
        {
            var task = base.InsertOrMergeEntityAsync();
            var inserted = new NotificationEntity();
            task.ContinueWith(x => inserted = (NotificationEntity)x.Result);
            base.InsertOrMergeEntityAsync().Wait();
            return inserted;
        }


        public string CallId { get; set; }
        public string SpeechId { get; set; }
        public string NotificationId { get; set; }
        public string ServiceOperationId { get; set; }
        public string AttendantId { get; set; }
        public string SupervisorId { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string SpeechSpeed { get; set; }
        public string AverageSentimentLevel { get; set; }
        public double? AverageSentimentScore { get; set; }
        public string SentimentLevel { get; set; }
        public double? SentimentScore { get; set; }
        public string Intent { get; set; }
        public double? IntentScore { get; set; }
        public string Backlist { get; set; }
        public string Message { get; set; }
        public string SpeechIdCliente { get; set; }
        public string SpeechIdAtendente { get; set; }
        public bool SilenceEnded { get; set; }
        public double? SilenceTotalSeconds { get; set; }
        public string SilenceIdCliente { get; set; }
        public string SilenceIdAtendente { get; set; }
        public double OverlayTotalSeconds { get; set; }
    }
}
