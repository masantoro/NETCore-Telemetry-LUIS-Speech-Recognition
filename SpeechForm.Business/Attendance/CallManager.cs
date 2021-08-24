using System;
using System.Linq;
using SpeechForm.Models.Attendance;
using kpiModel = SpeechForm.Models.KPI;
using SpeechForm.Models.Attendance.Enum;
using SpeechForm.Repository.Entity.Table;
using SpeechForm.Models.KPI;
using System.Threading.Tasks;
using SpeechForm.Business.KPI;
using SpeechForm.Business.API;

namespace SpeechForm.Business.Attendance
{
    public class CallManager
    {
        private CallEntity call;

        public async Task SaveCall(Call Call, ServiceOperation ServiceOperation)
        {
            call = new CallEntity(Call.CallId);
            call.ServiceOperationId = ServiceOperation.ServiceOperationId;
            call.SupervisorId = ServiceOperation.SupervisorId;
            call.AttendantId = ServiceOperation.AttendantId;
            call.Start = Call.CallStart;
            await call.InsertOrMergeEntityAsync();
        }

        public async Task EndCall()
        {
            call.End = DateTime.UtcNow;
            await call.InsertOrMergeEntityAsync();
        }
        public void SaveSpeechStart(string callId, string from, string SpeechId, DateTime StartRecognizing)
        {
            var speech = new SpeechEntity(callId);
            speech.CallId = callId;
            speech.From = from;
            speech.SpeechId = SpeechId;
            speech.StartRecognizing = StartRecognizing;
            speech.EndRecognizing = null;
            speech.InsertOrMergeEntityAsync().Wait();
        }

        public void SaveSpeechEnd(string callId, string from, SpeechRecognized speechRecognized)
        {
            var speech = (new SpeechEntity()).GetBySpeech(speechRecognized.SpeechId);
            if (speech != null)
            {
                speech.CallId = callId;
                speech.From = from;
                speech.SpeechId = speechRecognized.SpeechId;
                speech.StartRecognizing = speechRecognized.StartRecognizing;
                speech.EndRecognizing = speechRecognized.EndRecognizing;
                speech.SpeechSeconds = speechRecognized.SpeechSeconds;
                speech.NumberOfLetters = speechRecognized.NumberOfLetters;
                speech.SpeechSpeed = speechRecognized.SpeechSpeed.ToString();
                speech.SpeechSpeedScore = speechRecognized.SpeechSpeedScore;
                speech.Intent = speechRecognized.Intent;
                speech.IntentScore = speechRecognized.IntentScore;
                speech.AverageSentimentLevel = speechRecognized.AverageSentimentLevel.ToString();
                speech.AverageSentimentScore = speechRecognized.AverageSentimentScore;
                speech.SentimentLevel = speechRecognized.SentimentLevel.ToString();
                speech.SentimentScore = speechRecognized.SentimentScore;
                speech.InsertOrMergeEntityAsync().Wait();
            }
        }

        public void UpdateKPI(Call call)
        {
            var kpi = new kpiModel.KPI();
            kpi.CallId = call.CallId;
            kpi.CallStart = call.CallStart;
            kpi.ServiceOperation = call.ServiceOperation;

            var send = new Send();
            send.UpdateKPI(kpi).Wait();
        }
    }
}