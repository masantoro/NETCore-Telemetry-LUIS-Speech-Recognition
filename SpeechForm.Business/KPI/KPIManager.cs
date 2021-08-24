using SpeechForm.Models.KPI;
using SpeechForm.Repository.Entity.Table;
using SpeechForm.Repository.Entity.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using _enumTable = SpeechForm.Repository.Entity.Enum.Table;
using _enumBlob = SpeechForm.Repository.Entity.Enum.Blob;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Storage.Blob;
using SpeechForm.Business.Attendance;
using kpiModel = SpeechForm.Models.KPI;
using System.Diagnostics;
using SpeechForm.Models.Attendance;
using SpeechForm.Business.API;

namespace SpeechForm.Business.KPI
{
    public static class KPIManager
    {
        public static void SaveKPI(kpiModel.KPI kpi)
        {
            var notificationManager = new NotificationManager();
            var notification = new NotificationEntity();
            var speech = new SpeechEntity();

            var kpiEntity = (new KPIEntity()).GetByCall(kpi.CallId);
            if (kpiEntity == null)
            {
                kpiEntity = new KPIEntity(kpi.CallId);
            }

            kpiEntity.CallId = kpi.CallId;
            kpiEntity.CallStart = kpi.CallStart;
            kpiEntity.ServiceOperationId = kpi.ServiceOperation.ServiceOperationId;
            kpiEntity.ServiceOperationDescription = kpi.ServiceOperation.ServiceOperationDescription;
            kpiEntity.SupervisorId = kpi.ServiceOperation.SupervisorId;
            kpiEntity.SupervisorName = kpi.ServiceOperation.SupervisorName;
            kpiEntity.AttendantId = kpi.ServiceOperation.AttendantId;
            kpiEntity.AttendantName = kpi.ServiceOperation.AttendantName;
            
            // Recuperando falas de atendente e cliente
            var speechs = speech.GetByCall(kpi.CallId);
            var speechsAttendant = speechs != null ? speechs.Where(x => x.From == _enumTable.From.Atendente.ToString()).ToList() : null;
            var speechsCustomer = speechs != null ? speechs.Where(x => x.From == _enumTable.From.Cliente.ToString()).ToList() : null;

            // Recuperando notificacoes de atendente e cliente
            var notifications = notification.GetByCall(kpi.CallId);
            var notificationsAttendant = notifications != null ? notifications.Where(x => x.From == _enumTable.From.Atendente.ToString()).ToList() : null;
            var notificationsCustomer = notifications != null ? notifications.Where(x => x.From == _enumTable.From.Cliente.ToString()).ToList() : null;

            // ATENDENTE /////
            //////////////////
            // Niveis de sentimento do atendente
            SpeechEntity lastSpeechAttendant = null;
            if (speechsAttendant != null && speechsAttendant.Count > 0)
            {
                lastSpeechAttendant = speechsAttendant.First();
                kpiEntity.AverageSentimentScoreAttendant = lastSpeechAttendant.AverageSentimentScore;
                kpiEntity.SentimentScoreAttendant = lastSpeechAttendant.SentimentScore;
                kpiEntity.SpeechSpeedScoreAttendant = lastSpeechAttendant.SpeechSpeedScore;
                kpiEntity.SpeechSpeedScoreFullAttendant = lastSpeechAttendant.SpeechSpeedScore * notificationManager.GetSpeechSpeedScoreMultiple(lastSpeechAttendant.SpeechSeconds, lastSpeechAttendant.NumberOfLetters);
            }


            // Quantidade de notificacoes do atendente
            kpiEntity.NotificationCountAttendant = notificationsAttendant != null ? notificationsAttendant.Count : 0;

            // Quantidade de intencoes criticas do atendente
            var notificationsCriticalIntentAttendant = notificationsAttendant != null ? notificationsAttendant.Where(x => x.Type == _enumTable.NotificationType.CriticalIntent.ToString()).ToList() : null;
            kpiEntity.CriticalIntentCountAttendant = notificationsCriticalIntentAttendant != null ? notificationsCriticalIntentAttendant.Count : 0;

            // Tempo de sobreposicao de voz do atendente
            kpiEntity.OverlayTotalSecondsAttendant = 0;
            var notificationsOverlayAttendant = notificationsAttendant != null ? notificationsAttendant.Where(x => x.Type == _enumTable.NotificationType.Overlay.ToString()).ToList() : null;
            if (notificationsOverlayAttendant != null && notificationsOverlayAttendant.Count > 0)
            {
                kpiEntity.OverlayTotalSecondsAttendant = notificationsOverlayAttendant[0].OverlayTotalSeconds * (-1);
            }


            // CLIENTE /////
            //////////////////

            // Niveis de sentimento do cliente
            SpeechEntity lastSpeechCustomer = null;
            if (speechsCustomer != null && speechsCustomer.Count > 0)
            {
                lastSpeechCustomer = speechsCustomer.First();
                kpiEntity.AverageSentimentScoreCustomer = lastSpeechCustomer.AverageSentimentScore;
                kpiEntity.SentimentScoreCustomer = lastSpeechCustomer.SentimentScore;
                kpiEntity.SpeechSpeedScoreCustomer = lastSpeechCustomer.SpeechSpeedScore;
                kpiEntity.SpeechSpeedScoreFullCustomer = lastSpeechCustomer.SpeechSpeedScore * notificationManager.GetSpeechSpeedScoreMultiple(lastSpeechCustomer.SpeechSeconds, lastSpeechCustomer.NumberOfLetters);
            }

            // Quantidade de notificacoes do cliente
            kpiEntity.NotificationCountCustomer = notificationsCustomer != null ? notificationsCustomer.Count : 0;

            // Quantidade de intencoes criticas do cliente
            notificationsCustomer = notificationsCustomer != null ? notificationsCustomer.Where(x => x.Type == _enumTable.NotificationType.CriticalIntent.ToString()).ToList() : null;
            kpiEntity.CriticalIntentCountCustomer = notificationsCustomer != null ? notificationsCustomer.Count : 0;

            // Quantidade de falas rapidas e lentas do cliente

            // Tempo de sobreposicao de voz do cliente
            kpiEntity.OverlayTotalSecondsCustomer = 0;
            var notificationsOverlayCustomer = notificationsCustomer != null ? notificationsCustomer.Where(x => x.Type == _enumTable.NotificationType.Overlay.ToString()).ToList() : null;
            if (notificationsOverlayCustomer != null && notificationsOverlayCustomer.Count > 0)
            {
                kpiEntity.OverlayTotalSecondsCustomer = notificationsOverlayCustomer[0].OverlayTotalSeconds * (-1);
            }

            // Total em segundos de silencio de ambos ao mesmo tempo
            kpiEntity.BothSilenceTotalSeconds = 0;
            DateTime? endSpeechAttendant = kpi.CallStart;
            DateTime? endSpeechCustomer = kpi.CallStart;
            if (lastSpeechAttendant != null)
            {
                endSpeechAttendant = lastSpeechAttendant.EndRecognizing != null ? lastSpeechAttendant.EndRecognizing : null;
            }
            if (lastSpeechCustomer != null)
            {
                endSpeechCustomer = lastSpeechCustomer.EndRecognizing != null ? lastSpeechCustomer.EndRecognizing : null;
            }
            if (endSpeechAttendant != null && endSpeechCustomer != null)
            {
                DateTime? endSpeech = endSpeechAttendant > endSpeechCustomer ? endSpeechAttendant : endSpeechCustomer;
                kpiEntity.BothSilenceTotalSeconds = (DateTime.UtcNow - endSpeech.Value).TotalSeconds * (-1);
            }


            kpiEntity.InsertOrMergeEntityAsync().Wait();
            kpiEntity.DeleteOlderAsync(kpi.ServiceOperation.AttendantId, kpi.CallId).Wait();

            // Chamar azure function para salvar blob do KPI
            var send = new Send();
            Task.Run(() => send.UpdateBlob(kpi.ServiceOperation));
        }

        public static void SaveBlob(ServiceOperation serviceOperation)
        {
            string blobContent = null;
            var favoriteTop3 = false;
            var favorite = (new FavoriteEntity()).GetOne(serviceOperation.ServiceOperationId, serviceOperation.SupervisorId);
            var KPIs = (new KPIEntity()).GetLast3BySupervisor(serviceOperation.ServiceOperationId, serviceOperation.SupervisorId);

            if(KPIs != null && KPIs.Count > 0)
            {
                for(var i = 0; i < KPIs.Count; i++)
                {
                    KPIs[i].Ranking = i + 1;
                    KPIs[i].Favorite = false;
                    if (favorite != null && KPIs[i].AttendantId == favorite.AttendantId)
                    {
                        KPIs[i].Favorite = true;
                        favoriteTop3 = true;
                    }
                    KPIs[i].CallStartFormatted = $"{KPIs[i].CallStart.ToString("o").Substring(0, 23)}Z"; // GAMBIARRA PARA ATENDER LIMITACAO DO POWER BI
                    KPIs[i].Updated = $"{DateTime.UtcNow.ToString("o").Substring(0, 23)}Z"; // GAMBIARRA PARA ATENDER LIMITACAO DO POWER BI
                    blobContent += JsonConvert.SerializeObject(KPIs[i]);
                    blobContent += (i + 1) < KPIs.Count ? "," : string.Empty;
                }
            }

            if (!favoriteTop3 && favorite != null)
            {
                var favoriteKPI = (new KPIEntity()).GetByAttendant(favorite.AttendantId);
                if (favoriteKPI != null)
                {
                    blobContent += blobContent != null ? "," : string.Empty;
                    favoriteKPI.Ranking = 0;
                    favoriteKPI.Favorite = true;
                    favoriteKPI.CallStartFormatted = $"{favoriteKPI.CallStart.ToString("o").Substring(0, 19)}.000Z"; // GAMBIARRA PARA ATENDER LIMITACAO DO POWER BI
                    blobContent += JsonConvert.SerializeObject(favoriteKPI);
                }
            }

            // SALVAR BLOB
            if (blobContent != null)
            {
                var blob = new BlobEntity(_enumBlob.Container.kpi);

                // DELETAR BLOBS MAIS ANTIGOS DESSA OEPRACAO + SUPERVISOR
                var fileName = $"{serviceOperation.ServiceOperationId}_{serviceOperation.SupervisorId}";
                blob.DeleteOlder(fileName);

                // NOVO BLOB
                fileName += $"_{Guid.NewGuid().ToString()}"; 
                Task.Run(() => blob.UploadAsync(fileName, blobContent));
            }
        }
    }
}
