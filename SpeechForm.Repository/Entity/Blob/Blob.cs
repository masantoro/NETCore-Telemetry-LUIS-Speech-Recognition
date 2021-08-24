using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Blob.Protocol;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using SpeechForm.Repository.Enum;
using SpeechForm.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Enum = SpeechForm.Repository.Entity.Enum;
using SpeechForm.Repository.Entity.Table;
using SpeechForm.Repository.Properties;

namespace SpeechForm.Repository.Entity.Blob
{
    public class BlobEntity
    {
        private string _containerName;
        private CloudBlobContainer _container;

        public BlobEntity(Enum.Blob.Container container)
        {
            _containerName = container.ToString();
            connect();
        }

        private void connect()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Resources.StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(_containerName);
            _container.CreateIfNotExists();
        }

        public void DeleteOlder(string fileName)
        {
            var blobs = _container.ListBlobs().OfType<CloudBlockBlob>().Where(x => x.Name.Contains(fileName));
            if(blobs != null)
            {
                foreach(var blob in blobs)
                {
                    if((DateTime.UtcNow - blob.Properties.Created.Value).TotalSeconds > 60)
                    {
                        blob.DeleteIfExistsAsync();
                    }
                }
            }
            
        }


        public void UploadAsync(string blobName, string Content)
        {
            var blob = _container.GetBlockBlobReference(blobName);
            blob.DeleteIfExists();
            blob.UploadText(Content);
            blob.Properties.ContentType = "application/json";
            blob.SetProperties();
        }
    }
}
