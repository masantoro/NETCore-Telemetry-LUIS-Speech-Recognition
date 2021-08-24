using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class KPIEntity : BaseTableEntity<KPIEntity>
    {
        private static string _tableName = "kpi";

        public KPIEntity() : base(_tableName)
        {

        }

        public KPIEntity(string callId) : base(callId, callId, _tableName)
        {
            CallId = callId;
        }

        public KPIEntity GetByCall(string CallId)
        {
            var query = GetTable().CreateQuery<KPIEntity>();
            var kpi = query
                .Where(x => x.CallId == CallId)
                .ToList().FirstOrDefault();
            return kpi;
        }

        public List<KPIEntity> GetLast3BySupervisor(string ServiceOperationId, string SupervisorId)
        {
            var query = GetTable().CreateQuery<KPIEntity>();
            var kpis = new List<KPIEntity>();

            var attendantIds = query
                .Where(x => x.ServiceOperationId == ServiceOperationId &&
                            x.SupervisorId == SupervisorId)
                .ToList().OrderByDescending(x => x.Timestamp).Select(x => x.AttendantId).Distinct().Take(3);

            foreach(var attendantId in attendantIds)
            {
                var kpi = query
                .Where(x => x.AttendantId == attendantId)
                .ToList().OrderByDescending(x => x.Timestamp).FirstOrDefault();
                if(kpi != null)
                {
                    kpis.Add(kpi);
                }
            }
            return kpis;
        }

        public KPIEntity GetByAttendant(string AttendantId)
        {
            var query = GetTable().CreateQuery<KPIEntity>();
            var kpi = query
                .Where(x => x.AttendantId == AttendantId)
                .ToList().OrderByDescending(x => x.Timestamp).FirstOrDefault();
            return kpi;
        }

        public async Task DeleteOlderAsync(string AttendantId, string CallId)
        {
            var query = GetTable().CreateQuery<KPIEntity>();
            var kpis = query
                .Where(x => x.AttendantId == AttendantId &&
                            x.CallId != CallId);
            
            foreach(var kpi in kpis)
            {
                await kpi.DeleteEntityAsync();
            }
        }

        public string Updated { get; set; }
        public string CallId { get; set; }
        public DateTime CallStart { get; set; }
        public string ServiceOperationId { get; set; }
        public string ServiceOperationDescription { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorId { get; set; }
        public string AttendantName { get; set; }
        public string AttendantId { get; set; }

        public double? AverageSentimentScoreAttendant { get; set; }
        public double? SentimentScoreAttendant { get; set; }
        public int CriticalIntentCountAttendant { get; set; }
        public int NotificationCountAttendant { get; set; }
        public int SpeechSpeedScoreAttendant { get; set; }
        public int SpeechSpeedScoreFullAttendant { get; set; }
        public double OverlayTotalSecondsAttendant { get; set; }

        public double? AverageSentimentScoreCustomer { get; set; }
        public double? SentimentScoreCustomer { get; set; }
        public int CriticalIntentCountCustomer { get; set; }
        public int NotificationCountCustomer { get; set; }
        public int SpeechSpeedScoreCustomer { get; set; }
        public int SpeechSpeedScoreFullCustomer { get; set; }
        public double OverlayTotalSecondsCustomer { get; set; }
        public double BothSilenceTotalSeconds { get; set; }

        // Utilizado no azure function para salvar o Blob
        public int Ranking { get; set; }
        public bool Favorite { get; set; }
        public string CallStartFormatted { get; set; }
    }
}
