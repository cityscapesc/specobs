using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Spectrum.Storage.Queue;
using Microsoft.Spectrum.Storage.Queue.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Spectrum.Common;
using Microsoft.Spectrum.Common.Azure;
using Newtonsoft.Json;
using System.Threading;

namespace MessageQueueClient
{
    class Program
    {
        static void Main(string[] args)
        {
            PopulateQueue();
            //var masterCloudStorageAccount = CloudStorageAccount.Parse(ConnectionStringsUtility.StorageAccountConnectionString);
            //AzureMessageQueue msgQueue = new AzureMessageQueue("test-queue", masterCloudStorageAccount, false);

            //WorkerQueueMessage queueStationAvailabilityMessage = new WorkerQueueMessage(MessageType.SpectrumObservatoriesHealthMonitoring, string.Empty, string.Empty, true);
            //JsonSerializerSettings jss = new JsonSerializerSettings();
            //jss.TypeNameHandling = TypeNameHandling.All;

            //string message = JsonConvert.SerializeObject(queueStationAvailabilityMessage, jss);

            //try
            //{
            //    while (true)
            //    {
            //        //Console.WriteLine("{0} | Inserting a message into the queue- {1}", DateTime.Now.ToString(), "test-queue");

            //        msgQueue.PutMessage(message);

            //        Console.WriteLine("{0} | Inserted a message successfuly to the queue - {1}", DateTime.Now.ToString(), "test- queue");

            //        Thread.Sleep(60 * 1000);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error occurred! couldn't able to insert message into the queue {0}", ex.ToString());
            //}

            //Console.WriteLine("Enter any key to exit");
            //Console.ReadLine();

            //PopulateQueue();
        }

        public static void PopulateQueue()
        {
            string measurementStationAccessId = "12037188-276d-467a-81dd-6f2daff3d9b7";
            var masterCloudStorageAccount = CloudStorageAccount.Parse(ConnectionStringsUtility.StorageAccountConnectionString);
            AzureMessageQueue msgQueue = new AzureMessageQueue("test-queue", masterCloudStorageAccount, false);

            List<string> urls = new List<string>();

            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-04T2200.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-05T1100.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-04T1700.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-03T2100.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-04T0200.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-04T1500.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-05T0400.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-04T1400.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-04T0100.bin.dsox");
            urls.Add("https://so10stor20161201t0759.blob.core.windows.net/12037188-276d-467a-81dd-6f2daff3d9b7/2017-04-03T2200.bin.dsox");


            foreach (var item in urls)
            {
                try
                {
                    Uri blob = new Uri(item);
                    Guid measurementStation = Guid.Parse(measurementStationAccessId);

                    // Queue up this file for processing
                    WorkerQueueMessage queueMessage = new WorkerQueueMessage(MessageType.FileProcessing, measurementStationAccessId, item, true);

                    JsonSerializerSettings jss = new JsonSerializerSettings();
                    jss.TypeNameHandling = TypeNameHandling.All;
                    msgQueue.PutMessage(JsonConvert.SerializeObject(queueMessage, jss));
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error occurred! couldn't able to insert message into the queue {0}", ex.ToString());
                }
            }            
        }
    }
}
