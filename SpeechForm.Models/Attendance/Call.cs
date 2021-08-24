using SpeechForm.Repository.Entity.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechForm.Models.Attendance
{
    public class Call
    {
        public Call()
        {
            ServiceOperation = new ServiceOperation();
            ChannelAtendente = new Channel();
            ChannelCliente = new Channel();
        }

        public string CallId { get; set; }
        public DateTime CallStart { get; set; }

        public ServiceOperation ServiceOperation { get; set; }
        public Channel ChannelAtendente { get; set; }
        public Channel ChannelCliente { get; set; }
    }
}
