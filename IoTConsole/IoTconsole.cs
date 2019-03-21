using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;

namespace IoTConsole
{
    class IoTconsole
    {

        static RegistryManager registryManager;
        static string connectionString = "HostName=bsziothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=zhEb4IJwxkJptoepoguOGb/YLLJLTj42lvjff38wxPo=";
        static string deviceConnectionString = "HostName=bsziothub.azure-devices.net;DeviceId=ikort;SharedAccessKey=gD0L99ctgo51nNKTkLR0ZMy2FCnK+w4W+7QoYRu28UA=";

        // Select one of the following transports used by DeviceClient to connect to IoT Hub.
        private static Microsoft.Azure.Devices.Client.TransportType s_transportType = Microsoft.Azure.Devices.Client.TransportType.Amqp;
        //private static TransportType s_transportType = TransportType.Mqtt;
        //private static TransportType s_transportType = TransportType.Http1;
        //private static TransportType s_transportType = TransportType.Amqp_WebSocket_Only;
        //private static TransportType s_transportType = TransportType.Mqtt_WebSocket_Only;

        static void Main(string[] args)
        {
            /*
             * Create a device  
             * Add a location info as a tag
             * Remove device
            Console.WriteLine("Add location metadata tags to IoT device!");
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddTagsAndQuery().Wait();
            CreateIoTdevice().Wait();
            Console.WriteLine("delete device");
            RemoveIoTdevice();
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
            */
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, s_transportType);

            if (deviceClient == null)
            {
                Console.WriteLine("Failed to create DeviceClient!");
                return ;
            }

            var sample = new MessageSample(deviceClient);
            sample.RunSampleAsync().GetAwaiter().GetResult();

            Console.ReadLine();
            Console.WriteLine("Done.\n");
            return ;



        }

        /// <summary>
        /// Using Iothub  owner?  
        /// 
        /// Also add tags to device  and query back
        /// </summary>
        /// <returns></returns>
        public static async Task AddTagsAndQuery()
        {
            var twin = await registryManager.GetTwinAsync("bszuwp");
            var patch =
                @"{
            tags: {
                location: {
                    region: 'US',
                    plant: 'Redmond43'
                }
            }
        }";
            await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);

            var query = registryManager.CreateQuery(
              "SELECT * FROM devices WHERE tags.location.plant = 'Redmond43'", 100);
            var twinsInRedmond43 = await query.GetNextAsTwinAsync();
            Console.WriteLine("Devices in Redmond43: {0}",
              string.Join(", ", twinsInRedmond43.Select(t => t.DeviceId)));

            query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.plant = 'Redmond43' AND properties.reported.connectivity.type = 'cellular'", 100);
            var twinsInRedmond43UsingCellular = await query.GetNextAsTwinAsync();
            Console.WriteLine("Devices in Redmond43 using cellular network: {0}",
              string.Join(", ", twinsInRedmond43UsingCellular.Select(t => t.DeviceId)));
        }


        /// create new IoT device
        public static async Task CreateIoTdevice()
        {
            try
            {
                registryManager.AddDeviceAsync(new Device("Szabolcs"));
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static async Task RemoveIoTdevice()
        {
            try
            {
                Device dev = await registryManager.GetDeviceAsync("Szabolcs");
                registryManager.RemoveDeviceAsync(dev);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}
