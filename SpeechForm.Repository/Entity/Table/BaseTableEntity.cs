using Microsoft.Azure.Cosmos.Table;
using SpeechForm.Repository.Enum;
using SpeechForm.Repository.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SpeechForm.Repository.Entity.Table
{
    public abstract class BaseTableEntity<T> : TableEntity
    {
        private string _tableName;

        private CloudTable Table;

        public BaseTableEntity(string tableName)
        {
            _tableName = tableName;
            connect();
        }

        public BaseTableEntity()
        {
            
        }

        public BaseTableEntity(string partitionKey, string rowKey, string tableName)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            _tableName = tableName;
            connect();
        }

        private void connect()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Resources.StorageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            Table = tableClient.GetTableReference(_tableName);
            Table.CreateIfNotExists();
        }

        public CloudTable GetTable()
        {
            return Table;
        }

        public List<T> Filtered<T>(Func<T, bool> _predicate) where T : BaseTableEntity<T>, new()
        {
            var result = Table.CreateQuery<T>()
            .Where(_predicate)
            .Select(x => x).ToList();
            return result;
        }

        public List<T> All<T>() where T : BaseTableEntity<T>, new()
        {
            var result = Table.CreateQuery<T>()
            .Select(x => x).ToList();
            return result;
        }

        public async Task<BaseTableEntity<T>> InsertOrMergeEntityAsync()
        {
            if (this == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(this);

                TableResult result = await Table.ExecuteAsync(insertOrMergeOperation);
                BaseTableEntity<T> inserted = result.Result as BaseTableEntity<T>;

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return inserted;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<BaseTableEntity<T>> DeleteEntityAsync()
        {
            if (this == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                TableOperation deleteOperation = TableOperation.Delete(this);

                TableResult result = await Table.ExecuteAsync(deleteOperation);
                BaseTableEntity<T> deleted = result.Result as BaseTableEntity<T>;

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Delete Operation: " + result.RequestCharge);
                }

                return deleted;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }


    }
}
