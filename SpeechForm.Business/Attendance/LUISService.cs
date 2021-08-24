using Newtonsoft.Json;
using SpeechForm.Models.Attendance;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using SpeechForm.Models.Attendance.Enum;
using System.Drawing;
using System.Linq;

namespace SpeechForm.Business.Attendance
{
    public class LUISService
    {
        private readonly string _LUISKey;
        private readonly string _LUISAppId;
        public LUISService(string LUISKey, string LUISAppId)
        {
            _LUISKey = LUISKey;
            _LUISAppId = LUISAppId;
        }

        //Using LUIS
        public async Task GetIntentAndSentimentAnalysis(string recognizedText, Channel channel, SpeechRecognized speechRecognized)
        {
            var strResponseContent = string.Empty;

            try
            {
                var client = new HttpClient();
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                queryString["q"] = recognizedText;

                var endpointUri = String.Format($"https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{_LUISAppId}?verbose=true&timezoneOffset=0&subscription-key={_LUISKey}&{queryString}");

                var response = await client.GetAsync(endpointUri);

                strResponseContent = await response.Content.ReadAsStringAsync();

                IntentAndSentimentAnalyzeResult result = JsonConvert.DeserializeObject<IntentAndSentimentAnalyzeResult>(strResponseContent.ToString());
                // Console.WriteLine($"Intent score: {result.topScoringIntent.score}");

                
                double? intentTopScore = null;
                var intentTop = string.Empty;
                if (result != null && result.topScoringIntent != null)
                {
                    intentTopScore = result.topScoringIntent.score;
                    intentTop = result.topScoringIntent.intent;
                }
                
                var backColorSentiment = Color.Gray;
                Speech.SentimentLevel? sentimentLevel = null;
                double? sentimentScore = null;

                if (result != null && result.sentimentAnalysis != null) 
                {
                    sentimentScore = result.sentimentAnalysis.score;
                    
                    if (sentimentScore >= 0.8)
                    {
                        backColorSentiment = Color.DarkGreen;
                        sentimentLevel = Speech.SentimentLevel.MuitoPositivo;
                    }
                    else if (sentimentScore >= 0.6)
                    {
                        backColorSentiment = Color.Green;
                        sentimentLevel = Speech.SentimentLevel.Positivo;
                    }
                    else if (sentimentScore >= 0.5)
                    {
                        backColorSentiment = Color.LightGreen;
                        sentimentLevel = Speech.SentimentLevel.PoucoPositivo;
                    }
                    else if (sentimentScore >= 0.4)
                    {
                        backColorSentiment = Color.Gray;
                        sentimentLevel = Speech.SentimentLevel.Neutro;
                    }
                    else if (sentimentScore <= 0.09)
                    {
                        backColorSentiment = Color.DarkRed;
                        sentimentLevel = Speech.SentimentLevel.MuitoNegativo;
                    }
                    else if (sentimentScore <= 0.2)
                    {
                        backColorSentiment = Color.Red;
                        sentimentLevel = Speech.SentimentLevel.Negativo;
                    }
                    else if (sentimentScore < 0.4)
                    {
                        backColorSentiment = Color.OrangeRed;
                        sentimentLevel = Speech.SentimentLevel.PoucoNegativo;
                    }
                }


                if(intentTopScore != null)
                {
                    int intentPercent = Convert.ToInt32(intentTopScore * 100);

                    channel.OnIntentScore_Changed($"{intentPercent}%");
                    channel.OnIntent_Changed(intentTop);
                    channel.OnTranscricao_Changed(Environment.NewLine + $"INTENÇÃO: [{intentPercent}%] {intentTop.ToString().ToUpper()}");
                }
                else
                {
                    channel.OnIntentScore_Changed(string.Empty);
                    channel.OnIntent_Changed("[Não Reconhecida]");
                    channel.OnTranscricao_Changed(Environment.NewLine + $"INTENÇÃO: [NÃO RECONHECIDA]");
                }


                if (sentimentScore != null)
                {
                    int sentimentPercent = Convert.ToInt32(sentimentScore * 100);

                    channel.OnSentimento_Changed(sentimentLevel.ToString());
                    channel.OnSentimentoScore_Changed($"{sentimentPercent}%", backColorSentiment);
                    channel.OnTranscricao_Changed(Environment.NewLine + $"SENTIMENTO: [{sentimentPercent}%] {sentimentLevel.ToString().ToUpper()}");
                }
                else
                {

                    channel.OnSentimento_Changed("[Não Reconhecido]");
                    channel.OnSentimentoScore_Changed(string.Empty, Color.Gray);
                    channel.OnTranscricao_Changed(Environment.NewLine + $"SENTIMENTO: [NÃO RECONHECIDO]");
                }



                //Adicionar KIPs de sentimento da fala
                speechRecognized.Intent = intentTop;
                speechRecognized.IntentScore = intentTopScore;
                speechRecognized.SentimentLevel = sentimentLevel;
                speechRecognized.SentimentScore = sentimentScore;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



            
        }
    }
}