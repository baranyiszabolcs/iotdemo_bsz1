using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace IoTConsole
{
    class IoTconsole
    {

        static RegistryManager registryManager;
        static string connectionString = "HostName=bsziothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=zhEb4IJwxkJptoepoguOGb/YLLJLTj42lvjff38wxPo=";
        //"{iot hub connection string}";

        static void Main(string[] args)
        {
            Console.WriteLine("Add location metadata tags to IoT device!");
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddTagsAndQuery().Wait();
            CreateIoTdevice().Wait();
            Console.WriteLine("delete device");
            RemoveIoTdevice();
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();



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
