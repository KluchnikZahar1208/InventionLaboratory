using System;
using System.Data.SqlTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DataProcessor.Models;
using DataProcessor.Data;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;


namespace DataProcessor
{
    public class DataProcessorService
    {
        private static IConnection? connection;
        private static IModel? channel;
        static void Main()
        {
            // Configure Serilog for logging to the console
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();

                channel.QueueDeclare(queue: "DataProcessorQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Log.Information("Received information from FileProcessor");

                    var xmlDoc = JsonConvert.DeserializeXmlNode(message);
                    AddToDataBase(xmlDoc);
                };

                channel.BasicConsume(queue: "DataProcessorQueue", autoAck: true, consumer: consumer);

                Console.WriteLine("Data Processor Service is running. Press [Enter] to exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the Data Processor Service");
            }
            finally
            {
                connection.Close();
            }
        }
        private static void AddToDataBase(XmlDocument document)
        {
            XmlNodeList deviceStatusNode = document.SelectNodes("//DeviceStatus");
          
            foreach (XmlNode deviceStatus in deviceStatusNode)
            {
                string patternModuleCategoryID = @"<ModuleCategoryID>(.*?)<\/ModuleCategoryID>";
                Match matchModuleCategoryID = Regex.Match(deviceStatus.OuterXml, patternModuleCategoryID);

                if (matchModuleCategoryID.Success)
                {
                    string moduleCategoryID = matchModuleCategoryID.Groups[1].Value;

                    XmlNode? rapidControlStatus = deviceStatus.SelectSingleNode("RapidControlStatus");

                    string patternRapidControlStatus = @"<ModuleState>(.*?)<\/ModuleState>";
                    Match matchRapidControlStatus = Regex.Match(rapidControlStatus.InnerText,
                                                                patternRapidControlStatus);

                    if (matchRapidControlStatus.Success)
                    {
                        string moduleStateValue = matchRapidControlStatus.Groups[1].Value;

                        Log.Information($"{moduleCategoryID}: {moduleStateValue}");
                        ProcessAndSaveToSQLite(moduleCategoryID, moduleStateValue);

                    }
                    else
                    {
                        Log.Warning("ModuleState not found in the XML.");
                    }
                }
                else
                {
                    Log.Warning("ModuleCategoryID not found in the XML.");
                }
            }
            

        }
        private static void ProcessAndSaveToSQLite(string moduleCategoryId, string moduleState)
        {
            try
            {
                using var dbContext = new Data.AppContext();
                dbContext.Database.EnsureCreated();

                var existingModule = dbContext.Modules.SingleOrDefault(md => md.ModuleCategoryID == moduleCategoryId);

                if (existingModule != null)
                {
                    existingModule.ModuleState = moduleState;
                    Log.Information("Processed and updated to SQLite database.");
                }
                else
                {
                    var newModule = new Module
                    {
                        ModuleCategoryID = moduleCategoryId,
                        ModuleState = moduleState
                    };

                    dbContext.Modules.Add(newModule);
                    Log.Information("Processed and added to SQLite database.");
                }

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing and saving to SQLite database.");
            }
        }

    }

}