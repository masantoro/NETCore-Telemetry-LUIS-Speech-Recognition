using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SpeechForm.Models.Attendance;
using SpeechForm.Models.Attendance.Enum;
using SpeechForm.Repository.Entity.Table;
using data = SpeechForm.Repository.FixedData;

namespace SpeechForm.Business.Attendance
{
    public class SpeechService
    {
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly string _speechLanguage;
        private readonly LUISService _LUISService;
        public string _audioUri = null;

        private SpeechConfig config;
        private string authorizationToken;

        private string SpeechId = null;
        private DateTime? startRecognizing;
        private DateTime? endRecognizing;
        private DateTime? startSilence = null;

        private bool stop = false;
        private bool stopped = false;

        // Authorization token expires every 10 minutes. Renew it every 9 minutes.
        private TimeSpan RefreshTokenDuration = TimeSpan.FromMinutes(9);

        private Channel _channel;
        private Call _call;

        private NotificationManager notificationManager;
        private CallManager callManager;

        private Speech.SentimentLevel averageSentimentLevel;
        private double? averageSentimentScore;

        private int _lastNotificationTime = 0;


        public SpeechService(string subscriptionKey, string region, string speechLanguage, LUISService LUISservice)
        {
            _subscriptionKey = subscriptionKey;
            _region = region;
            _speechLanguage = speechLanguage;
            _LUISService = LUISservice;

            notificationManager = new NotificationManager();
            callManager = new CallManager();
            averageSentimentLevel = Speech.SentimentLevel.Neutro;
        }

        public void StopService()
        {
            stop = true;
            while (!stopped)
            {

            }
        }

        public async Task GetSpeechTranscriptionAndLUISIntentAndSentiment(Call call, Channel channel)
        {
            _lastNotificationTime = notificationManager.FisrtSilenceNotificationTime;

            _channel = channel;
            _call = call;

            startSilence = DateTime.UtcNow.AddSeconds(-1); // Há um delay de 1 segundo para iniciar o tempo do silencio;

            authorizationToken = await GetToken(_subscriptionKey, _region);
            config = SpeechConfig.FromAuthorizationToken(authorizationToken, _region);

            if(_channel.channel == Models.Attendance.Enum.Speech.Channel.Cliente)
            {
                var micId = "";
                var enumerator = new MMDeviceEnumerator();
                foreach (var endpoint in
                         enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                {
                    if (endpoint.FriendlyName.Contains("CABLE Output (VB-Audio Virtual Cable)"))
                    {
                        micId = endpoint.ID;
                        break;
                    }
                }

                using (var audioInput = AudioConfig.FromMicrophoneInput(micId))
                {
                    config.SpeechRecognitionLanguage = _speechLanguage;
                    SpeechRecognizer speechRecognizer = new SpeechRecognizer(config, _speechLanguage, audioInput);
                    await ContinuousRecognitionWithAuthorizationTokenAsync(speechRecognizer);
                }
            } else
            {
                using (var audioInput = AudioConfig.FromDefaultMicrophoneInput())
                {
                    config.SpeechRecognitionLanguage = _speechLanguage;
                    SpeechRecognizer speechRecognizer = new SpeechRecognizer(config, _speechLanguage, audioInput);
                    await ContinuousRecognitionWithAuthorizationTokenAsync(speechRecognizer);
                }
            }
            
        }

        private async Task ContinuousRecognitionWithAuthorizationTokenAsync(SpeechRecognizer speechRecognizer)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            using (var recognizer = speechRecognizer)
            {
                var tokenRenewTask = StartTokenRenewTask(source.Token, recognizer);

                recognizer.Recognizing += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizingSpeech)
                    {
                        // Recuperar inicio e fim (TEMPO) da fala
                        if (startRecognizing == null)
                        {
                            //Finalizar silêncio
                            _lastNotificationTime = notificationManager.FisrtSilenceNotificationTime;
                            startSilence = null;

                            // Salvar o inicio da fala
                            SpeechId = Guid.NewGuid().ToString();
                            startRecognizing = DateTime.UtcNow.AddSeconds(-1); // Há um delay de 1 segundo para iniciar o tempo da fala
                            callManager.SaveSpeechStart(_channel.CallId, _channel.channel.ToString(), SpeechId, startRecognizing.Value);
                        }
                        endRecognizing = DateTime.UtcNow.AddSeconds(-1); // Há um delay de 1 segundo para finalizar o tempo da fala
                    }
                };

                recognizer.Recognized += (s, e) =>
                {                    
                    var _textSession = string.Empty;
                    var _text = string.Empty;

                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        // Iniciar Silencio
                        startSilence = DateTime.UtcNow.AddSeconds(-1); // Há um delay de 1 segundo para iniciar o tempo do silencio;
                        
                        // Recuperar fala reconhecida
                        _text = e.Result.Text;

                        // Criar KPIs da fala
                        var speechRecognized = new SpeechRecognized();
                        if (startRecognizing != null && endRecognizing != null)
                        {
                            _textSession = $"INICIO {startRecognizing} - FIM {endRecognizing}";

                            speechRecognized.SpeechId = SpeechId;
                            speechRecognized.StartRecognizing = startRecognizing;
                            speechRecognized.EndRecognizing = endRecognizing;
                            speechRecognized.SpeechSeconds = (endRecognizing - startRecognizing).Value.TotalSeconds;
                            speechRecognized.NumberOfLetters = Regex.Replace(_text, @"\s+", string.Empty).Length;
                            speechRecognized.SpeechSpeed = notificationManager.GetSpeechSpeed(speechRecognized.SpeechSeconds, speechRecognized.NumberOfLetters);
                            speechRecognized.SpeechSpeedScore = notificationManager.GetSpeechSpeedScore(speechRecognized.SpeechSeconds, speechRecognized.NumberOfLetters);
                            speechRecognized.Backlist = notificationManager.GetBacklist(_text);
                            speechRecognized.SentOverlay = false;
                            speechRecognized.NotificationType = Speech.NotificationType.None;

                            SpeechId = null;
                            startRecognizing = null;
                            endRecognizing = null;
                        }

                        // Incluir espaço para nova fala
                        _channel.OnTranscricao_Changed(Environment.NewLine + Environment.NewLine);
                        
                        // Incluir inicio e fim (TEMPO) no textbox da transcrição (cliente ou atendente)
                        if (!string.IsNullOrEmpty(_textSession))
                        {
                            _channel.OnTranscricao_Changed(_textSession);
                        }
                        // LUIS => Reconhecimento do Sentimento e Intencão
                        _LUISService.GetIntentAndSentimentAnalysis(_text, _channel, speechRecognized).Wait();

                        // Adicionar a fala no textbox da transcrição
                        if (!string.IsNullOrEmpty(_text))
                        {
                            _channel.OnTranscricao_Changed(Environment.NewLine + " - " + _text);
                        }

                        // NOTIFICAR
                        averageSentimentLevel = notificationManager.GetAverageSentimentLevel(_channel.speechRecognizeds);
                        averageSentimentScore = notificationManager.GetAverageSentimentScore(_channel.speechRecognizeds);
                        speechRecognized.AverageSentimentLevel = averageSentimentLevel;
                        speechRecognized.AverageSentimentScore = averageSentimentScore;
                        notificationManager.SaveNotificationSpeech(_call, _channel.channel.ToString(), speechRecognized);

                        // Adicionar todos os reconhecimentos da fala na lista da ligação (atendente ou cliente)
                        _channel.speechRecognizeds.Add(speechRecognized);

                        //Checar se existe sobreposicao de voz e enviar a notificacao
                        notificationManager.SendNotificationOverlay(_call);

                        Console.WriteLine($"Salvou fala do {_channel.channel}");

                        // Salvar o final da fala
                        callManager.SaveSpeechEnd(_channel.CallId, _channel.channel.ToString(), speechRecognized);
                        
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        //_text = "NOMATCH: Speech could not be recognized.";
                    }

                    
                };

                recognizer.Canceled += (s, e) =>
                {
                    var _error = string.Empty;

                    _error = $"CANCELED: Reason={e.Reason}";

                    if (e.Reason == CancellationReason.Error)
                    {
                        _error += $"CANCELED: ErrorCode={e.ErrorCode}";
                        _error += $"CANCELED: ErrorDetails={e.ErrorDetails}";
                        _error += "CANCELED: Did you update the subscription info?";
                    }

                    try
                    {
                        _channel.OnTranscricao_Changed(Environment.NewLine + Environment.NewLine + _error);
                    }
                    catch { }
                };

                // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                if (_audioUri == null)
                {
                _channel.OnTranscricao_Changed("Transcrição iniciada...");
                }
                
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);


                while(!stop)
                {
                    try
                    {
                        // Verificar envio de notificao de silencio
                        SendSaveSilence();

                        // Atualizar os KPIS no BLOB
                        UpdateKPIAsync();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    Thread.Sleep(2000);
                }

                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                source.Cancel();
                stopped = true;
            }
        }

        private void StartSpeech()
        {

        }

        private void UpdateKPIAsync()
        {
            Task.Run(() => callManager.UpdateKPI(_call));
        }

        private void SendSaveSilence()
        {
            if (startSilence != null)
            {
                Task.Run(() => SaveSilence(startSilence.Value, DateTime.UtcNow));
            }
        }

        private async Task SaveSilence(DateTime _startSilence, DateTime _endSilence)
        {
            var SilenceTotalSeconds = (_endSilence - _startSilence).TotalSeconds;

            if (_lastNotificationTime < SilenceTotalSeconds)
            {
                _lastNotificationTime += 10; // Notificar de novo somente daqui 10 segundos
                await notificationManager.SaveNotificationSilence(_call,
                                                        _channel.channel.ToString(),
                                                        SilenceTotalSeconds,
                                                        averageSentimentLevel,
                                                        averageSentimentScore);
            }
        }

        private bool IsCriticalIntentScore(string Intent, double? IntentScore)
        {
            if (!data.Speech.IsCrititalIntent(Intent))
            {
                return false;
            }
            if(IntentScore == null || IntentScore < 0.6) // 60%
            {
                return false;
            }
            return true;
        }

        // Renews authorization token periodically until cancellationToken is cancelled.
        private Task StartTokenRenewTask(CancellationToken cancellationToken, SpeechRecognizer recognizer)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(RefreshTokenDuration, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        recognizer.AuthorizationToken = await GetToken(_subscriptionKey, _region);
                    }
                }
            });
        }

        // Gets an authorization token by sending a POST request to the token service.
        private async Task<string> GetToken(string subscriptionKey, string region)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                UriBuilder uriBuilder = new UriBuilder("https://" + region + ".api.cognitive.microsoft.com/sts/v1.0/issueToken");

                using (var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null))
                {
                    //Console.WriteLine("Token Uri: {0}", uriBuilder.Uri.AbsoluteUri);
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new HttpRequestException($"Cannot get token from {uriBuilder.ToString()}. Error: {result.StatusCode}");
                    }
                }
            }
        }
    }
}