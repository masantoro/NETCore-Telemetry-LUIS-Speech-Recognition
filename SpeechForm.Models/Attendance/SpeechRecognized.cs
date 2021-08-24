using System;
using System.Collections.Generic;
using System.Text;
using static SpeechForm.Models.Attendance.Enum.Speech;

namespace SpeechForm.Models.Attendance
{
    public class SpeechRecognized
    {
        public SpeechRecognized()
        {
            Backlist = new List<string>();
        }

        public string SpeechId { get; set; }
        public DateTime? StartRecognizing { get; set; }
        public DateTime? EndRecognizing { get; set; }
        public double SpeechSeconds { get; set; }
        public int NumberOfLetters { get; set; }
        public SpeechSpeed SpeechSpeed { get; set; }
        public int SpeechSpeedScore { get; set; }
        public string Intent { get; set; }
        public double? IntentScore { get; set; }
        public SentimentLevel? AverageSentimentLevel { get; set; }
        public double? AverageSentimentScore { get; set; }
        public SentimentLevel? SentimentLevel { get; set; }
        public double? SentimentScore { get; set; }
        public List<string> Backlist { get; set; }
        public bool SentOverlay { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
