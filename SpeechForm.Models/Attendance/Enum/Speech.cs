using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SpeechForm.Models.Attendance.Enum
{
    public class Speech
    {
        public enum Channel
        {
            Atendente,
            Cliente
        }

        public enum SentimentLevel
        {
            MuitoNegativo = 1,
            Negativo = 2,
            PoucoNegativo = 3,
            Neutro = 4,
            PoucoPositivo = 5,
            Positivo = 6,
            MuitoPositivo = 7
        }

        public enum SpeechSpeed
        {
            Lento,
            Normal,
            Rapido,
        }

        public enum NotificationType
        {
            None,
            Overlay,
            Backlist,
            Script,
            SilenceAttendant,
            SilenceBoth,
            SpeechSpeed,
            SentimentLevel,
            CriticalIntent
        }
    }
}
