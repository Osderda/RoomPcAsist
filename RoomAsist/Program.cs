using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using HidSharp;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
///Manuel Uygulama
namespace RoomAsist
{
    internal class Program
    {
        private static float totalWatt = 0;
        private static string publicIP = "";
        public static WebClient webclient = new WebClient();
        private static SerialPort serialPort = new SerialPort();

        public class asistantcom
        {
            public int system { get; set; }
            public int value { get; set; }
            public int value2 { get; set; }
            public bool clearlcd { get; set; }
            public string str { get; set; }
        }

        public static string GetIP()
        {
            try
            {
                string externalIP = "";
                externalIP = webclient.DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                                               .Matches(externalIP)[0].ToString();
                return externalIP;
            }
            catch (Exception)
            {
                return "NO CONNECT";

            }
        }
        static void Main(string[] args)
        {
            SerialPort serialPort = new SerialPort();
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.PortName = "COM4";
            if (!serialPort.IsOpen)
            {
                serialPort.Open();
            }

            var com = new asistantcom
            {
                system = 0,
                str = ""
            };
            string jsonString = JsonSerializer.Serialize(com);
            serialPort.Write(jsonString + "\n");
            com = new asistantcom
            {
                system = 2,
                value = 80
            };
            jsonString = JsonSerializer.Serialize(com);
            serialPort.Write(jsonString + "\n");

            publicIP = GetIP();
            while (true)
            {
                Thread.Sleep(1000);
            
                totalWatt = 0;
                Computer computer = new Computer()
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMotherboardEnabled = true,
                    IsMemoryEnabled = true,
                    IsPsuEnabled = true,
                    IsStorageEnabled = true,
                    IsBatteryEnabled = true,
                    IsNetworkEnabled = true,
                    IsControllerEnabled = true,

                };

                computer.Open();

                foreach (IHardware hardware in computer.Hardware)
                {
                    //Console.WriteLine($"Donanım: {hardware.Name}");
                    hardware.Update();

                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Power)
                        {
                            totalWatt += (float)sensor.Value;
                            Console.WriteLine($"\t{sensor.Name}: {sensor.Value} Watt");
                        }
                    }
                }
                //com = new asistantcom
                //{
                //    system = 1,
                //    value = 6,
                //    value2 = 0,//row - asagı
                //    clearlcd = false,
                //    str = "       "
                //};
                //jsonString = JsonSerializer.Serialize(com);
                //serialPort.Write(jsonString + "\n");
                //Console.WriteLine($"\nTotal Watt usage: {Math.Round(totalWatt)}");
                //Console.WriteLine($"\nPublic IP: {publicIP.Substring(6, 7)}");
                com = new asistantcom
                {
                    system = 1,
                    value = 4,
                    value2 = 0,//row - asagı
                    clearlcd = false,
                    str = Math.Round(totalWatt).ToString() + " WATT "
                };
                jsonString = JsonSerializer.Serialize(com);
                //Console.WriteLine(jsonString);
                serialPort.Write(jsonString + "\n");

                com = new asistantcom
                {
                    system = 1,
                    value = 1,
                    value2 = 1,//row - asagı
                    clearlcd = false,
                    str = publicIP
                };
                jsonString = JsonSerializer.Serialize(com);
                serialPort.Write(jsonString + "\n");

            }

        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen)
            {

                var data = (char)serialPort.ReadChar();
                Console.WriteLine($"ReadedData: {data}\n");
            }
        }
    }
}
