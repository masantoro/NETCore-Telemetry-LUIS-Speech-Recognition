using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Entity.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Web.Models
{
    public class ResultModel<T> where T : BaseTableEntity<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Entity { get; set; }
    }
}
