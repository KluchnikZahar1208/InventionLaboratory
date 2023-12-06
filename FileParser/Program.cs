using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace FileParserService
{
    public class FileParserService
    {
        private const string QueueName = "DataProcessorQueue";
        private static readonly string XmlFolderPath = @"E:\TestTasks\Лаборатория изобретений\FileProcessor\Data\";

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console().CreateLogger();


            IConfigurationBuilder configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            IConfigurationRoot root = configuration.Build();


            var rabbitMQSettings = new RabbitMQSettings();
            root.GetSection("RabbitMQSettings").Bind(rabbitMQSettings);

            var factory = new ConnectionFactory() { HostName = rabbitMQSettings.HostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: rabbitMQSettings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var timer = new Timer(ProcessFile,
                                  channel,
                                  TimeSpan.Zero,
                                  TimeSpan.FromSeconds(1));

            Log.Information("FileParserService started. Press [enter] to exit.");
            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void ProcessFile(object state)
        {
            var channel = (IModel)state;
            var xmlFiles = Directory.GetFiles(XmlFolderPath, "*.xml");

            foreach (var xmlFilePath in xmlFiles)
            {
                try
                {
                    var xmlDoc = ReadXml(xmlFilePath);
                    UpdateModuleStates(xmlDoc);
                    var json = JsonConvert.SerializeXmlNode(xmlDoc);
                    SendJsonToRabbitMQ(channel, json);

                    Log.Information($"Processed and sent modules from {xmlFilePath} to DataProcessor");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error processing file: {xmlFilePath}");
                }
            }
        }

        private static XmlDocument ReadXml(string xmlFilePath)
        {
            XmlDocument xmlDoc = new();
            xmlDoc.Load(xmlFilePath);
            return xmlDoc;
        }

        private static void UpdateModuleStates(XmlDocument xml)
        {

            string[] statuses = { "Online", "Run", "NotReady", "Offline" };
            Random random = new();

            foreach (XmlNode row in xml.SelectNodes("//RapidControlStatus"))
            {
                string randomStatus = GetRandomStatus(statuses, random);
                Console.WriteLine("Randomly generated status: " + randomStatus);

                string pattern = @"<ModuleState>(.+)</ModuleState>";
                string replacement = $"<ModuleState>{randomStatus}</ModuleState>";
                string modifiedXmlString = Regex.Replace(row.InnerText, pattern, replacement);
                row.InnerText = modifiedXmlString;
            }
        }

        private static string GetRandomStatus(string[] statuses, Random random)
        {
            int index = random.Next(statuses.Length);
            return statuses[index];
        }

        private static void SendJsonToRabbitMQ(IModel channel, string json)
        {
            channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: Encoding.UTF8.GetBytes(json));
        }
    }
}