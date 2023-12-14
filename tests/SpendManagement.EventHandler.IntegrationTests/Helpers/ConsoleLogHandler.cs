﻿using KafkaFlow;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SpendManagement.EventHandler.IntegrationTests.Helpers
{
    public class ConsoleLogHandler : ILogHandler
    {
        public void Error(string message, Exception ex, object data)
        {
            Console.WriteLine($"[KafkaBus] - Error: {message} | Exception: {JsonConvert.SerializeObject(ex)}");
        }

        public void Info(string message, object data)
        {
            Debug.WriteLine($"[KafkaBus] - Info: {message} | Data: {JsonConvert.SerializeObject(data)}");
        }

        public void Verbose(string message, object data)
        {
            Debug.WriteLine($"[KafkaBus] - Verbose: {message} | Data: {JsonConvert.SerializeObject(data)}");
        }

        public void Warning(string message, object data)
        {
            Debug.WriteLine($"[KafkaBus] - Warning: {message} | Data: {JsonConvert.SerializeObject(data)}");
        }
    }
}
