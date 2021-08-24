using SpeechForm.Models.Attendance.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SpeechForm.Models.Attendance
{
    public delegate void ChannelTextEventHandler(string Text);
    public delegate void ChannelTextBackColorEventHandler(string Text, Color BackColor);
    public class Channel
    {
        public Channel()
        {
            speechRecognizeds = new List<SpeechRecognized>();
        }

        //public TextBox txtSentimentoScore { get; set; }
        //public TextBox txtSentimento { get; set; }
        //public TextBox txtIntentScore { get; set; }
        //public TextBox txtIntent { get; set; }
        //public TextBox txtTranscricao { get; set; }
        public Speech.Channel channel { get; set; } 
        public string CallId { get; set; }

        public DateTime CallStart { get; set; }
        public DateTime? CallEnd { get; set; }
        public List<SpeechRecognized> speechRecognizeds { get; set; }

        public event ChannelTextBackColorEventHandler SentimentoScore_Changed;
        public event ChannelTextEventHandler Sentimento_Changed;
        public event ChannelTextEventHandler IntentScore_Changed;
        public event ChannelTextEventHandler Intent_Changed;
        public event ChannelTextEventHandler Transcricao_Changed;
        
        public virtual void OnSentimentoScore_Changed(string Text, Color BackColor)
        {
            SentimentoScore_Changed(Text, BackColor);
        }

        public virtual void OnSentimento_Changed(string Text)
        {
            Sentimento_Changed(Text);
        }

        public virtual void OnIntentScore_Changed(string Text)
        {
            IntentScore_Changed(Text);
        }

        public virtual void OnIntent_Changed(string Text)
        {
            Intent_Changed(Text);
        }

        public virtual void OnTranscricao_Changed(string Text)
        {
            Transcricao_Changed(Text);
        }
    }
}
