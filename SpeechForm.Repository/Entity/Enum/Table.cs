using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechForm.Repository.Entity.Enum
{
    public class Table
    {
        public enum NotificationType
        {
            None,
            Overlay,
            Backlist,
            Script,
            Silence,
            SpeechSpeed,
            SentimentLevel,
            CriticalIntent
        }

        public enum SpeechSpeed
        {
            Lento,
            Normal,
            Rapido,
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

        public enum From
        {
            Cliente,
            Atendente
        }
    }
}
