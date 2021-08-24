using SpeechForm.Models.Attendance;
using SpeechForm.Repository.Entity.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechForm.Models.KPI
{
    public class KPI
    {
        public string CallId { get; set; }
        public DateTime CallStart { get; set; }
        public ServiceOperation ServiceOperation { get; set; }
    }
}