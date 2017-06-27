using Microsoft.Spectrum.Common;
using Microsoft.Spectrum.Storage.Table.Azure;
using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Spectrum.RawIQPolicyDataUploadClient
{
    class Program
    {
        const string StorageConnectionKey = "StorageAccountConnectionString";
        static string rawIQPolicyFilepath;
        static AzureTableOperations<RawIQScanPolicy> rawIQScanPolicyTableOperations;
        static ILogger logger;


        static void Main(string[] args)
        {
            logger = new ConsoleLogger();

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[StorageConnectionKey]);
                rawIQScanPolicyTableOperations = new AzureTableOperations<RawIQScanPolicy>(storageAccount);
            }
            catch (Exception ex)
            {
                logger.Log(TraceEventType.Error, LoggingMessageId.RawIQPolicyUploadClient, string.Format("Cloudn't able to read Cloud Storage Connection string: Exception:{0}", ex.ToString()));
            }

            if (args.Length == 0)
            {
                logger.Log(TraceEventType.Error, LoggingMessageId.RawIQPolicyUploadClient, "Please input RawIQPolicy csv file path");
            }

            if (args.Length > 1)
            {
                logger.Log(TraceEventType.Error, LoggingMessageId.RawIQPolicyUploadClient, "Application doesn't accept more than 1 arguments");
            }

            rawIQPolicyFilepath = args[0];

            if (rawIQScanPolicyTableOperations != null)
            {
                if (!File.Exists(rawIQPolicyFilepath))
                {
                    logger.Log(TraceEventType.Error, LoggingMessageId.RawIQPolicyUploadClient, string.Format("Input filepath {0} doesn't exist", rawIQPolicyFilepath));
                }
                else
                {
                    List<RawIQScanPolicy> rawIQPolicies = ReadRawIQPolicyDataFromFile(rawIQPolicyFilepath);

                    try
                    {
                        UploadFileDataToCloud(rawIQPolicies);
                        logger.Log(TraceEventType.Information, LoggingMessageId.RawIQPolicyUploadClient, "Data uploaded successfully");
                    }
                    catch (Exception ex)
                    {
                        logger.Log(TraceEventType.Information, LoggingMessageId.RawIQPolicyUploadClient, string.Format("Unable to upload RawIQScanPolicy data to cloud, Error Details :{0}", ex.ToString()));
                    }
                }
            }

            logger.Log(TraceEventType.Stop, LoggingMessageId.RawIQPolicyUploadClient, "Enter any key exit");
            Console.ReadLine();
        }

        private static List<RawIQScanPolicy> ReadRawIQPolicyDataFromFile(string filepath)
        {
            List<RawIQScanPolicy> rawIQScanPolicies = new List<RawIQScanPolicy>();

            using (Stream fileStream = File.OpenRead(filepath))
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                int lineCount = 1;

                while (!streamReader.EndOfStream)
                {
                    string result = streamReader.ReadLine();

                    if (lineCount == 1)
                    {
                        lineCount++;
                        continue;
                    }

                    try
                    {
                        RawIQScanPolicy iqScanPolicy = ConvertCommaSeparatedStringToRawIQScanPolicy(result);
                        rawIQScanPolicies.Add(iqScanPolicy);

                        lineCount++;
                    }
                    catch (InvalidCastException)
                    {
                        logger.Log(TraceEventType.Stop, LoggingMessageId.RawIQPolicyUploadClient, string.Format("Could not able read row no.{0} because contain invalid format data:{1}", lineCount, result));

                        if (rawIQScanPolicies.Any())
                        {
                            rawIQScanPolicies.Clear();
                        }

                        break;
                    }
                }
            }

            return rawIQScanPolicies;
        }

        private static RawIQScanPolicy ConvertCommaSeparatedStringToRawIQScanPolicy(string input)
        {
            string[] iqScanPolicyValues = input.Split(',');

            int bandPriority = int.Parse(iqScanPolicyValues[0]);
            string category = iqScanPolicyValues[1];
            int dutycycleOnTimeInMilliSec = int.Parse(iqScanPolicyValues[2]);
            int dutycycleTimeInMilliSec = int.Parse(iqScanPolicyValues[3]);
            int fileDurationInSec = int.Parse(iqScanPolicyValues[4]);
            string policyDetails = iqScanPolicyValues[5];
            int retentionTimeInSec = int.Parse(iqScanPolicyValues[6]);
            long startFrequency = long.Parse(iqScanPolicyValues[7]);
            long stopFrequency = long.Parse(iqScanPolicyValues[8]);

            return new RawIQScanPolicy(startFrequency, stopFrequency, category)
            {
                BandPriority = bandPriority,
                Category = category,
                DutycycleOnTimeInMilliSec = dutycycleOnTimeInMilliSec,
                DutycycleTimeInMilliSec = dutycycleTimeInMilliSec,
                FileDurationInSec = fileDurationInSec,
                PolicyDetails = policyDetails,
                RetentionTimeInSec = retentionTimeInSec,
                StartFrequency = startFrequency,
                StopFrequency = stopFrequency
            };
        }

        private static void UploadFileDataToCloud(List<RawIQScanPolicy> rawIQScanPolycies)
        {
            if (rawIQScanPolycies.Any())
            {
                rawIQScanPolicyTableOperations.GetTableReference(AzureTableHelper.RawIQScanPolicyTable);

                foreach (var rawIQScanPolicy in rawIQScanPolycies)
                {
                    rawIQScanPolicyTableOperations.InsertOrReplaceEntity(rawIQScanPolicy, true);
                    logger.Log(TraceEventType.Information, LoggingMessageId.RawIQPolicyUploadClient, string.Format("RawIQPolicy for Start Frequency - {0} | Stop Frequency - {1} , having Category :{2} uploaded successfully to cloud", rawIQScanPolicy.StartFrequency, rawIQScanPolicy.StopFrequency, rawIQScanPolicy.Category));
                }
            }
        }
    }
}
