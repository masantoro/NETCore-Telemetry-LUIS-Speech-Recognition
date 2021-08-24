using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _enum = SpeechForm.Repository.Entity.Enum.Table;

namespace SpeechForm.Repository.Entity.Table
{
    public class SpeechEntity : BaseTableEntity<SpeechEntity>
    {
        private static string _tableName = "speech";

        public SpeechEntity() : base(_tableName)
        {
            
        }

        public SpeechEntity(string callId) : base(callId, Guid.NewGuid().ToString(), _tableName)
        {
            CallId = callId;
        }

        public List<SpeechEntity> GetByCall(string CallId)
        {
            var query = GetTable().CreateQuery<SpeechEntity>();
            var speechs = query.Where(x => x.CallId == CallId).ToList().OrderByDescending(x => x.Timestamp).ToList();
            return speechs;
        }

        public SpeechEntity GetBySpeech(string SpeechId)
        {
            var query = GetTable().CreateQuery<SpeechEntity>();
            var speech = query.Where(x => x.SpeechId == SpeechId).ToList().FirstOrDefault();
            return speech;
        }

        public string CallId { get; set; }
        public string SpeechId { get; set; }
        public string From { get; set; }
        public DateTime? StartRecognizing { get; set; }
        public DateTime? EndRecognizing { get; set; }
        public double SpeechSeconds { get; set; }
        public int NumberOfLetters { get; set; }
        public string SpeechSpeed { get; set; }
        public int SpeechSpeedScore { get; set; }
        public string Intent { get; set; }
        public double? IntentScore { get; set; }
        public string AverageSentimentLevel { get; set; }
        public double? AverageSentimentScore { get; set; }
        public string SentimentLevel { get; set; }
        public double? SentimentScore { get; set; }
    }
}
