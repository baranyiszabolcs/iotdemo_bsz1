using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace ReportConnectivity
{
    class DeviceTwinReport
    {

        static string DeviceConnectionString = "HostName=bsziothub.azure-devices.net;DeviceId=bszuwp;SharedAccessKey=SJXEKntewdKYI/oRIAVGr4qoaky/qC0FyJMkIiK5gOs=";
static  DeviceClient Client = null;

       
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                DeviceTwinHandler dth = new DeviceTwinHandler();
                InitClient();

                Client.OpenAsync().Wait(); //.
                Client.SetDesiredPropertyUpdateCallbackAsync(dth.OnDesiredPropertyUpdate, null);
                Client.SetMethodHandlerAsync("MyMethodAsync", dth.MyMethodAsync, null);
                int i = 0;
                while (Console.ReadKey().KeyChar != 'e')
                {
                    GetTwin();
                    ReportConnectivity();
                    ReportRepcount(i++);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }



        public static void InitClient()
        {
            try
            {
                Console.WriteLine("Connecting to hub");
                Client = DeviceClient.CreateFromConnectionString(DeviceConnectionString,
                  TransportType.Mqtt);
  
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        public static async void GetTwin()
        {
            try
            {
                Console.WriteLine("Retrieving twin");
               Twin mytwin= await Client.GetTwinAsync();
                StringBuilder sb = new StringBuilder("TWIN:");
               sb.AppendFormat( "ID: {0}",mytwin.DeviceId)
                    .AppendLine(" twin reported:"+mytwin.Properties.Reported.ToJson())
                    .AppendLine(" twin desired :"+ mytwin.Properties.Desired.ToJson());
                Console.WriteLine(sb);

            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }


        public static async void ReportConnectivity()
        {
            try
            {
                Console.WriteLine("Sending connectivity data as reported property");

                TwinCollection reportedProperties, connectivity;
                reportedProperties = new TwinCollection();
                connectivity = new TwinCollection();
                connectivity["type"] = "cellular";
                reportedProperties["connectivity"] = connectivity;
                await Client.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        public static async void ReportRepcount( int i)
        {
            try
            {            
                TwinCollection reportedProperties, repcount;
                reportedProperties = new TwinCollection();
                repcount = new TwinCollection();
                repcount["cnt"] = i;
                reportedProperties["repcount"] = repcount;
                await Client.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

    }

    public class DeviceTwinHandler
    {
        public Task OnDesiredPropertyUpdate(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("Desired property:");
            Console.WriteLine(desiredProperties.ToJson());
  
            return null;
        }


        public Task<MethodResponse> MyMethodAsync(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("Method CAll");
            Console.WriteLine("Method Name:" + methodRequest.Name);
            Console.WriteLine( methodRequest.DataAsJson );
            return null;
        }
    }
}
