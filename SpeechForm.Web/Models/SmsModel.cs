using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Web.Models
{
    public class SmsModel
    {
        public string Id { get; set; }
        public string CallId { get; set; }
        public string Solved { get; set; }
        public string Score { get; set; }
        public string Message { get; set; }
    }
}
