using System.Collections.Generic;

namespace SpeechForm.Models.Attendance
{
    public class IntentAndSentimentAnalyzeResult
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public List<Intent> intents { get; set; }
        public List<object> entities { get; set; }
        public SentimentAnalysis sentimentAnalysis { get; set; }
    }
}