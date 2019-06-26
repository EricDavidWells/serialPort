using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

// Based on the following: https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport.datareceived?view=netframework-4.8
// https://blogs.msdn.microsoft.com/bclteam/2006/10/10/top-5-serialport-tips-kim-hamilton/
// https://github.com/MarkSherstan/MPU-6050-9250-I2C-CompFilter/blob/master/Visualizer/p5js/main/sketch.js


namespace cs_serialComs
{
    class Program
    {
        static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        static long prevtime = 0;
        static long curtime = 0;

        static void Main(string[] args)
        {
            Thread t = new Thread(new ThreadStart(serialThread));
            t.Start();
            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
            t.Join();
        }

        private static void serialThread()
        {
            SerialPort sp = new SerialPort("COM6");

            sp.BaudRate = 115200;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.DataBits = 8;
            sp.Handshake = Handshake.None;
            sp.Open();

            while (true)
            {
                // define header check values, and total bytes to be read
                byte headercheck1 = 0x9F;
                byte headercheck2 = 0x6E;
                byte footercheck1 = 0x7D;
                byte footercheck2 = 0x8E;
            
                // define arrays to hold header byte checks
                byte[] check1 = new byte[1];
                byte[] check2 = new byte[1];

                // check first header
                sp.Read(check1, 0, 1);
                if (check1[0] == headercheck1)
                {
                    // check second header
                    sp.Read(check2, 0, 1);
                    if (check2[0] == headercheck2)
                    {
                        // read the message size
                        sp.Read(check1, 0, 1);
                        int msgsize = (int)check1[0];

                        // read the data message
                        byte[] data = new byte[msgsize];
                        int readsizecheck = sp.Read(data, 0, msgsize);

                        //read the footer checks
                        sp.Read(check1, 0, 1);
                        sp.Read(check2, 0, 1);

                        if (check1[0] == footercheck1 && check2[0] == footercheck2)
                        {
                            // check that message is the anticipated size
                            if (readsizecheck == msgsize)
                            {
                                // Do Things
                                DisplayText(data, msgsize);
                            }
                        }  
                    }
                }
            }
        }

        private static void DisplayText(byte[] data_, int msgsize_)
        {
            prevtime = curtime;
            TimeSpan ts = sw.Elapsed;
            curtime = (ts.Hours * 60 * 60 * 1000 + ts.Minutes * 60 * 1000 + ts.Seconds * 1000 + ts.Milliseconds);

            Console.Write("Data Received: ");

            // print out values recieved
            for (int i = 0; i < msgsize_; i += 2)
            {
                Console.Write(BitConverter.ToInt16(data_, i));
                Console.Write(",");
            }

            Console.Write(" Elapsed Time (ms): ");
            Console.Write(curtime - prevtime);
            Console.WriteLine();
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
