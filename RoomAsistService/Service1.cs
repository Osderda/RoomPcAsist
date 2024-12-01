using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Lifetime;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using HidSharp;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;

namespace RoomAsistService
{
    public partial class Service1 : ServiceBase
    {
        private static bool stopEvent;
        public Service1()
        {
            InitializeComponent();
            stopEvent = false;
        }
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
                return externalIP+"  ";
            }
            catch (Exception)
            {
                return "NO CONNECTION! ";

            }
        }
        private void Systemserial()
        {
            Task.Run(async() =>
            {


                SerialPort serialPort = new SerialPort();
                //serialPort.DataReceived += SerialPort_DataReceived;
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

                DateTime? lastRunTime = null;

                while (!stopEvent)
                {
                    Thread.Sleep(1000);
                    DateTime currentTime = DateTime.Now;

                    if (lastRunTime.HasValue)
                    {
                        TimeSpan timeDifference = currentTime - lastRunTime.Value;
                         
                        if (timeDifference.TotalMinutes >= 5 || (publicIP.Contains("NO CONNECTION") && timeDifference.TotalSeconds >= 30))
                        {
                            publicIP = GetIP();
                            lastRunTime = currentTime;
                        }
                        else
                        {

                        }
                    }
                    else
                    { 

                        publicIP = GetIP();
                        lastRunTime = currentTime;
                    }
                    //com = new asistantcom
                    //{
                    //    system = 1,
                    //    value = 2,
                    //    value2 = 1,//row - asagı
                    //    clearlcd = false,
                    //    str = ""
                    //};
                    //jsonString = JsonSerializer.Serialize(com);
                    //serialPort.Write(jsonString + "\n");
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
            });
        }
        protected override void OnStart(string[] args)
        {
            Systemserial();
            return;
        }
        protected override void OnStop()
        {
            stopEvent = true;
            if (serialPort.IsOpen)
            {
                serialPort.Close(); 
            }
        }
    }
}
