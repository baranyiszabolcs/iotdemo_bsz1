// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTConsole
{
    public class MessageSample
    {
        private const int delay = 500;
        private const int MessageCount = 2000;
        private const int TemperatureThreshold = 30;
        private static Random s_randomGenerator = new Random();
        private float _temperature;
        private float _humidity;
        DateTime _timeCreated;
        private DeviceClient _deviceClient;

        public MessageSample(DeviceClient deviceClient)
        {
            _deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
        }

        public async Task RunSampleAsync()
        {
            await SendEvent().ConfigureAwait(false);
            await ReceiveCommands().ConfigureAwait(false);
        }

        private async Task SendEvent()
        {
            string dataBuffer;

            Console.WriteLine("Device sending {0} messages to IoTHub...\n", MessageCount);

            for (int count = 0; count < MessageCount; count++)
            {
                _timeCreated = DateTime.Now.AddMinutes(count-MessageCount);
                _temperature = s_randomGenerator.Next(20, 35);
                _humidity = s_randomGenerator.Next(60, 80);
                // Add Spikes and dip
                if(count %50 ==1)
                    _temperature = _temperature + 30;               
                if (count % 50 == 2)   
                    _temperature = _temperature + 50;
                if (count % 50 == 3)
                    _temperature = _temperature + 30;


                if (count % 50 == 47)
                    _temperature = _temperature - 30;
                if (count % 50 == 48)
                    _temperature = _temperature - 50;
                if (count % 50 == 49)
                    _temperature = _temperature - 30;


                //dataBuffer = $"{{\"messageId\":{count},\"temperature\":{_temperature},\"humidity\":{_humidity},\"timeCreated\":{_timeCreated}}}";
                //Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));

                var telemetryDataPoint = new
                {
                    messageId = count,
                    temperature = _temperature,
                    humidity = _humidity,
                    timeCreated = _timeCreated
                };
                dataBuffer = JsonConvert.SerializeObject(telemetryDataPoint);
                Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(dataBuffer));

                eventMessage.Properties.Add("temperatureAlert", (_temperature > TemperatureThreshold) ? "true" : "false");
                Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count, dataBuffer);

                await _deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
                Thread.Sleep(delay);
            }
        }

        private async Task ReceiveCommands()
        {
            Console.WriteLine("\nDevice waiting for commands from IoTHub...\n");
            Console.WriteLine("Use the IoT Hub Azure Portal to send a message to this device.\n");

            Message receivedMessage;
            string messageData;

            receivedMessage = await _deviceClient.ReceiveAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            if (receivedMessage != null)
            {
                messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                int propCount = 0;
                foreach (var prop in receivedMessage.Properties)
                {
                    Console.WriteLine("\t\tProperty[{0}> Key={1} : Value={2}", propCount++, prop.Key, prop.Value);
                }

                await _deviceClient.CompleteAsync(receivedMessage).ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine("\t{0}> Timed out", DateTime.Now.ToLocalTime());
            }
        }
    }
}
