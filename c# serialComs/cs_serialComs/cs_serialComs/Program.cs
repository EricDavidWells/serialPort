﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

// Based on the following: https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport.datareceived?view=netframework-4.8
// 

namespace cs_serialComs
{
    class Program
    {
        static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        static long prevtime = 0;
        static long curtime = 0;

        static void Main(string[] args)
        {
            SerialPort mySerialPort = new SerialPort("COM6");

            mySerialPort.BaudRate = 115200;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.ReadTimeout = -1;

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            mySerialPort.Open();
            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
            mySerialPort.Close();
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            byte headercheck1 = 0x9F;
            byte headercheck2 = 0x6E;
            int msgsize = 2;

            SerialPort sp = (SerialPort)sender;
            //int indata = sp.ReadByte();
            //Console.WriteLine("Data Received:");
            //Console.WriteLine(indata);

            byte[] check1 = new byte[1];
            byte[] check2 = new byte[1];

            if (sp.BytesToRead > (msgsize + 2))
            {
                sp.Read(check1, 0, 1);
                if (check1[0] == headercheck1)
                {
                    sp.Read(check2, 0, 1);
                    if (check2[0] == headercheck2)
                    {
                        byte[] data = new byte[msgsize];
                        int temp = sp.Read(data, 0, data.Length);

                        if (temp == data.Length)
                        {
                            prevtime = curtime;
                            curtime = sw.ElapsedMilliseconds;

                            Console.Write("Data Received: ");
                            Console.Write(System.Text.Encoding.Default.GetString(data, 0, data.Length));
                            Console.Write(" Elapsed Time (ms): ");
                            Console.Write(curtime-prevtime);
                            //Console.Write(" Frequency: ");
                            //Console.Write(1000 / (curtime - prevtime));
                            Console.WriteLine();
                        }  
                    }
                }
            }
        }
    }
}
