using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public class UserEntity : BaseTableEntity<UserEntity>
    {
        private static string _tableName = "user";
        
        public UserEntity() : base(_tableName)
        {
            var key = Guid.NewGuid().ToString();
            PartitionKey = key;
            RowKey = key;
            UserId = key;
        }

        public string UserId { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
