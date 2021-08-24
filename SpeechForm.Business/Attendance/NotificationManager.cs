using SpeechForm.Business.API;
using SpeechForm.Models.Attendance;
using SpeechForm.Models.Attendance.Enum;
using SpeechForm.Repository.Entity.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SpeechForm.Models.Attendance.Enum.Speech;
using data = SpeechForm.Repository.FixedData;

namespace SpeechForm.Business.Attendance
{
    public class NotificationManager
    {
        private Send send;
        private CallManager callManager;
        public int FisrtSilenceNotificationTime = 30;

        public NotificationManager()
        {
            send = new Send();
            callManager = new CallManager();
        }

        private static object lockOverlay = new object();
        public void SendNotificationOverlay(Call call)
        {
            try
            {
                lock (lockOverlay)
                {
                    if (call.ChannelAtendente.speechRecognizeds.Exists(x => !x.SentOverlay) && call.ChannelCliente.speechRecognizeds.Exists(x => !x.SentOverlay))
                    {
                        foreach (var atendente in call.ChannelAtendente.speechRecognizeds.Where(x => !x.SentOverlay))
                        {
                            foreach (var cliente in call.ChannelCliente.speechRecognizeds.Where(x => !x.SentOverlay))
                            {
                                bool hasTrueOverlayAtendente = CheckTrueOverlayTime(atendente.StartRecognizing.Value, atendente.EndRecognizing.Value);
                                bool hasTrueOverlayCliente = CheckTrueOverlayTime(cliente.StartRecognizing.Value, cliente.EndRecognizing.Value);

                                if (atendente.StartRecognizing > cliente.StartRecognizing && atendente.StartRecognizing <= cliente.EndRecognizing)
                                {
                                    // Atendente sobrepos voz do cliente
                                    var notification = new NotificationEntity(call.ChannelAtendente.CallId);
                                    notification.SpeechId = atendente.SpeechId;
                                    notification.Type = Speech.NotificationType.Overlay.ToString();
                                    notification.ServiceOperationId = call.ServiceOperation.ServiceOperationId;
                                    notification.SupervisorId = call.ServiceOperation.SupervisorId;
                                    notification.AttendantId = call.ServiceOperation.AttendantId;
                                    notification.AverageSentimentLevel = GetAverageSentimentLevel(call.ChannelAtendente.speechRecognizeds).ToString();
                                    notification.AverageSentimentScore = GetAverageSentimentScore(call.ChannelAtendente.speechRecognizeds);
                                    notification.From = Speech.Channel.Atendente.ToString();
                                    notification.CallId = call.ChannelAtendente.CallId;
                                    notification.SpeechIdCliente = cliente.SpeechId;
                                    notification.SpeechIdAtendente = atendente.SpeechId;
                                    notification.OverlayTotalSeconds = GetOverlayTotalSeconds(atendente.StartRecognizing.Value, atendente.EndRecognizing.Value);
                                    notification.Message = $"Atendente sobrepos a voz do Cliente em {atendente.StartRecognizing}";
                                    var task = notification.InsertOrMergeEntityAsync();
                                    var continuation = task.ContinueWith(t =>
                                    {
                                        if (hasTrueOverlayAtendente)
                                        {
                                            Task.Run(() => send.Notification(t.Result));
                                                //Task.Run(() => callManager.UpdateKPI(call));
                                            }
                                    });
                                    continuation.Wait();

                                    cliente.SentOverlay = true;
                                    atendente.SentOverlay = true;

                                    Console.WriteLine(notification.Message);

                                    break;
                                }
                                else if (cliente.StartRecognizing > atendente.StartRecognizing && cliente.StartRecognizing <= atendente.EndRecognizing)
                                {
                                    // cliente sobrepos voz do atendente
                                    var notification = new NotificationEntity(call.ChannelCliente.CallId);
                                    notification.SpeechId = cliente.SpeechId;
                                    notification.Type = Speech.NotificationType.Overlay.ToString();
                                    notification.ServiceOperationId = call.ServiceOperation.ServiceOperationId;
                                    notification.SupervisorId = call.ServiceOperation.SupervisorId;
                                    notification.AttendantId = call.ServiceOperation.AttendantId;
                                    notification.From = Speech.Channel.Cliente.ToString();
                                    notification.AverageSentimentLevel = GetAverageSentimentLevel(call.ChannelCliente.speechRecognizeds).ToString();
                                    notification.AverageSentimentScore = GetAverageSentimentScore(call.ChannelCliente.speechRecognizeds);
                                    notification.SpeechIdCliente = cliente.SpeechId;
                                    notification.SpeechIdAtendente = atendente.SpeechId;
                                    notification.OverlayTotalSeconds = GetOverlayTotalSeconds(cliente.StartRecognizing.Value, cliente.EndRecognizing.Value);
                                    notification.Message = $"Cliente sobrepos a voz do Atendente em {cliente.StartRecognizing}";
                                    var task = notification.InsertOrMergeEntityAsync();
                                    var continuation = task.ContinueWith(t =>
                                    {
                                        if (hasTrueOverlayCliente)
                                        {
                                            Task.Run(() => send.Notification(t.Result));
                                        }
                                    });
                                    continuation.Wait();

                                    cliente.SentOverlay = true;
                                    atendente.SentOverlay = true;

                                    Console.WriteLine(notification.Message);

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task SaveNotificationSilence(Call call, string From, double SilenceTotalSeconds, SentimentLevel? AverageSentimentLevel , double? AverageSentimentScore)
        {
            if (CheckSilenceAttendantToNotify(From, SilenceTotalSeconds))
            {
                var notification = new NotificationEntity(call.CallId);
                notification.Type = Speech.NotificationType.SilenceAttendant.ToString();
                notification.ServiceOperationId = call.ServiceOperation.ServiceOperationId;
                notification.SupervisorId = call.ServiceOperation.SupervisorId;
                notification.AttendantId = call.ServiceOperation.AttendantId;
                notification.AverageSentimentLevel = AverageSentimentLevel.ToString();
                notification.AverageSentimentScore = AverageSentimentScore;
                notification.From = Speech.Channel.Atendente.ToString();
                notification.SilenceTotalSeconds = SilenceTotalSeconds;
                notification.Message = $"Atendente ficou em silêncio por {SilenceTotalSeconds} segundos";
                await notification.InsertOrMergeEntityAsync().ContinueWith((t) =>
                {
                    Task.Run(() => send.Notification(t.Result));
                });
            }
        }

        public void SaveNotificationSpeech(Call call, string From, SpeechRecognized speechRecognized)
        {
            if (CheckSpeedToNotify(From, speechRecognized.SpeechSpeed))
            {
                var notification = new NotificationEntity(call.CallId);
                notification.SpeechId = speechRecognized.SpeechId;
                notification.Type = Speech.NotificationType.SpeechSpeed.ToString();
                notification.ServiceOperationId = call.ServiceOperation.ServiceOperationId;
                notification.SupervisorId = call.ServiceOperation.SupervisorId;
                notification.AttendantId = call.ServiceOperation.AttendantId;
                notification.From = From;
                notification.AverageSentimentLevel = speechRecognized.AverageSentimentLevel.ToString();
                notification.AverageSentimentScore = speechRecognized.AverageSentimentScore;
                notification.SpeechSpeed = speechRecognized.SpeechSpeed.ToString();
                notification.Message = $"Velocidade da fala do {From}";
                var task = notification.InsertOrMergeEntityAsync();
                var continuation = task.ContinueWith(t =>
                {
                    Task.Run(() => send.Notification(t.Result));
                });
                continuation.Wait();
            }

            if ((speechRecognized.SentimentLevel != null && CheckSentimentLevelToNotify(speechRecognized.SentimentLevel.Value)))
            {
                var notification = new NotificationEntity(call.CallId);
                notification.SpeechId = speechRecognized.SpeechId;
                notification.Type = Speech.NotificationType.SentimentLevel.ToString();
                notification.ServiceOperationId = call.ServiceOperation.ServiceOperationId;
                notification.SupervisorId = call.ServiceOperation.SupervisorId;
                notification.AttendantId = call.ServiceOperation.AttendantId;
                notification.From = From;
                notification.AverageSentimentLevel = speechRecognized.AverageSentimentLevel.ToString();
                notification.AverageSentimentScore = speechRecognized.AverageSentimentScore;
                notification.SentimentLevel = speechRecognized.SentimentLevel.ToString();
                notification.SentimentScore = speechRecognized.SentimentScore;
                notification.Message = $"Sentimento da fala do {From}";
                var task = notification.InsertOrMergeEntityAsync();
                var continuation = task.ContinueWith(t =>
                {
                    Task.Run(() => send.Notification(t.Result));
                });
                continuation.Wait();
            }

            if (CheckCriticalIntentToNotify(speechRecognized.Intent, speechRecognized.IntentScore))
            {
                var notification = new NotificationEntity(call.CallId);
                notification.SpeechId = speechRecognized.SpeechId;
                notification.Type = Speech.NotificationType.CriticalIntent.ToString();
                notification.ServiceOperationId = call.ServiceOperation.ServiceOperationId;
                notification.SupervisorId = call.ServiceOperation.SupervisorId;
                notification.AttendantId = call.ServiceOperation.AttendantId;
                notification.From = From;
                notification.AverageSentimentLevel = speechRecognized.AverageSentimentLevel.ToString();
                notification.AverageSentimentScore = speechRecognized.AverageSentimentScore;
                notification.Intent = speechRecognized.Intent;
                notification.IntentScore = speechRecognized.IntentScore;
                notification.Message = $"Intenção muito negativa/critica (>= 60%) {From}";
                var task = notification.InsertOrMergeEntityAsync();
                var continuation = task.ContinueWith(t =>
                {
                    Task.Run(() => send.Notification(t.Result));
                });
                continuation.Wait();
            }

            if (speechRecognized.Backlist.Count > 0)
            {
                var notification = new NotificationEntity(call.CallId);
                notification.SpeechId = speechRecognized.SpeechId;
                notification.Type = Speech.NotificationType.Backlist.ToString();
                notification.ServiceOperationId = call.ServiceOperation.ServiceOperationId;
                notification.SupervisorId = call.ServiceOperation.SupervisorId;
                notification.AttendantId = call.ServiceOperation.AttendantId;
                notification.From = From;
                notification.AverageSentimentLevel = speechRecognized.AverageSentimentLevel.ToString();
                notification.AverageSentimentScore = speechRecognized.AverageSentimentScore;
                notification.Backlist = String.Join(", ", speechRecognized.Backlist);
                notification.Message = $"Palavras da backlist na fala do {From}";
                var task = notification.InsertOrMergeEntityAsync();
                var continuation = task.ContinueWith(t =>
                {
                    Task.Run(() => send.Notification(t.Result));
                });
                continuation.Wait();
            }
        }

        public List<string> GetBacklist(string text)
        {
            var result = new List<string>();
            return result;
        }

        private bool CheckBacklistToNotify(string text)
        {
            if(GetBacklist(text).Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool CheckSilenceAttendantToNotify(string From, double SilenceTotalSeconds)
        {
            if (From == Speech.Channel.Atendente.ToString() && SilenceTotalSeconds >= FisrtSilenceNotificationTime)
            {
                return true;
            }
            return false;
        }

        private bool CheckTrueOverlayTime(DateTime start, DateTime end)
        {
            return (end - start).TotalSeconds > 2;
        }

        private bool CheckSpeedToNotify(string From, Speech.SpeechSpeed speechSpeed)
        {
            if (From == Speech.Channel.Atendente.ToString() && (speechSpeed == Speech.SpeechSpeed.Rapido || speechSpeed == Speech.SpeechSpeed.Lento))
            {
                return true;
            }
            return false;
        }

        public Speech.SentimentLevel GetAverageSentimentLevel(List<SpeechRecognized> speechRecognizeds)
        {
            var average = (int)Speech.SentimentLevel.Positivo;
            if(speechRecognizeds.Exists(x => x.SentimentLevel != null))
            {
                average = Convert.ToInt32(speechRecognizeds.Where(x => x.SentimentLevel != null).Select(x => (int)x.SentimentLevel).Average());
            }
            return (Speech.SentimentLevel)average;
        }

        public double? GetAverageSentimentScore(List<SpeechRecognized> speechRecognizeds)
        {
            double? average = null;
            if (speechRecognizeds.Exists(x => x.SentimentScore != null))
            {
                average = speechRecognizeds.Where(x => x.SentimentScore != null).Select(x => x.SentimentScore).Average();
            }
            return average;
        }

        private bool CheckSentimentLevelToNotify(Speech.SentimentLevel sentimentLevel)
        {
            if (sentimentLevel == Speech.SentimentLevel.MuitoNegativo)
            {
                return true;
            }
            return false;
        }

        private bool CheckCriticalIntentToNotify(string Intent, double? IntentScore)
        {
            if (string.IsNullOrEmpty(Intent) || !data.Speech.IsCrititalIntent(Intent))
            {
                return false;
            }
            if (IntentScore == null || IntentScore < 0.6) // 60%
            {
                return false;
            }
            return true;
        }

        public Speech.SpeechSpeed GetSpeechSpeed(double speechSeconds, int NumberOfLetters)
        {
            var result = Speech.SpeechSpeed.Normal;
            var speed = speechSeconds / NumberOfLetters;
            if (speed <= 0.047)
            {
                result = Speech.SpeechSpeed.Rapido;
            }
            else if (speed >= 0.10)
            {
                result = Speech.SpeechSpeed.Lento;
            }
            else
            {
                result = Speech.SpeechSpeed.Normal;
            }

            return result;
        }

        public int GetSpeechSpeedScore(double speechSeconds, int NumberOfLetters)
        {
            var score = 4;
            var speed = speechSeconds / NumberOfLetters;
            if (speed <= 0.047)
            {
                score = 4; // MUITO RAPIDO
            }
            else if (speed <= 0.053)
            {
                score = 3; 
            }
            else if (speed <= 0.059)
            {
                score = 2;
            }
            else if (speed < 0.066)
            {
                score = 1;
            }
            else if (speed >= 0.066 && speed <= 0.081) //0,0735
            {
                score = 0; //NORMAL
            }
            else if (speed >= 0.10)
            {
                score = 4; // MUITO LENTO
            }
            else if (speed >= 0.094)
            {
                score = 3;
            }
            else if (speed >= 0.088)
            {
                score = 2;
            }
            else if (speed > 0.081)
            {
                score = 1;
            }
            
            return score;
        }

        public int GetSpeechSpeedScoreMultiple(double speechSeconds, int NumberOfLetters)
        {
            if (speechSeconds / NumberOfLetters > 0.081)
            {
                return -1;
            }
            return 1;
        }

        private double GetOverlayTotalSeconds(DateTime start, DateTime end)
        {
            return (end - start).TotalSeconds;
        }

        private double GetSilenceTotalSeconds(DateTime LastStartSilence, DateTime EndSilence1, DateTime EndSilence2)
        {
            if(EndSilence1 < EndSilence2)
            {
                return (EndSilence1 - LastStartSilence).TotalSeconds;
            } 
            else
            {
                return (EndSilence2 - LastStartSilence).TotalSeconds;
            }

        }
    }
}
