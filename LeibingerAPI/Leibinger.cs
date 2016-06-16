using System;
using System.Net.Sockets;
using NLog;

namespace LeibingerAPI
{

    public class Leibinger
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string _printString;
        private string _job;
        private int _delay;

        private readonly string externalTextCMD = "^0=ET";
        private readonly string powerOnCMD = "^0!PO\r";
        private readonly string powerOffCMD = "^0!PF\r";
        private readonly string openNozzleCMD = "^0!NO\r";
        private readonly string closeNozzleCMD = "^0!NC\r";
        private readonly string printReadyCMD = "^0!GO\r";
        private readonly string stopPrintReadyCMD = "^0!ST\r";
        private readonly string jobLoadCMD = "^0=JL\\FFSDISK\\JOBS\\";

        public Leibinger(string printerIP, int port)
        {
            IPAddress = printerIP;
            Port = port;
            _delay = 0; //default printer delay value

        } //constructor

        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }


        public string SendCMD(string message) //handles socket interactions
        {
            try
            {

                TcpClient client = new TcpClient(IPAddress, Port);

                byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                data = new byte[256];
                string responseData = "";
                client.ReceiveTimeout = 50; //timeout if nothing received after 50 ms
                int bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                stream.Close();
                client.Close();
                return responseData;
            }
            catch (ArgumentNullException e)
            {
                //Console.WriteLine("ArgumentNullException: {0}", e);
                logger.Error(e.Message);
                return e.Message;

            }
            catch (ObjectDisposedException e)
            {
                // Console.WriteLine("ObjectDisposedException: {0}", e);
                logger.Error(e.Message);
                return e.Message;

            }
            catch (System.IO.IOException e) // exception if nothing is returned from the socket 
            {

                //Console.WriteLine("IOException: {0}", e);
                logger.Error(e.Message);
                return e.Message;

            }
            catch (SocketException e)
            {
                //Console.WriteLine("Socket Exception: {0}", e);
                logger.Error(e.Message);
                return e.Message;
            }
        }

        public void PowerOn()
        {
            SendCMD(powerOnCMD);
        }
        public void PowerOff()
        {
            SendCMD(powerOffCMD);
        } // this will automatically close the nozzle if it is open before shutting down.
        public void OpenNozzle()
        {
            SendCMD(openNozzleCMD);
        }
        public void CloseNozzle()
        {
            SendCMD(closeNozzleCMD);
        }
        public void StartPrint()
        {
            SendCMD(printReadyCMD);
        } //this command will start the printer printing when its photoeye is triggered
        public void StopPrint()
        {
            SendCMD(stopPrintReadyCMD);
        }
        public void LoadJob(string job)
        {
            _job = job;
            SendCMD(string.Concat(jobLoadCMD, _job, "\r"));
            //System.Threading.Thread.Sleep(100); // give it time to load
        }

        /// <summary>
        /// This is what you call to send the info to be printed
        /// </summary>
        /// <param name="testNumber"></param>
        /// <param name="expiration"></param>
        public void SendTestNumber(string testNumber, string expiration)
        {
            _printString = string.Concat(externalTextCMD, testNumber, expiration, "\r");
            SendCMD(_printString);
            System.Threading.Thread.Sleep(_delay);
        }

        
        ////These additional CMD strings aren't implemented, but they are available if needed in the future
        //private readonly string restartPrinterCMD = "^0!RM\r"; //Software reset, only while standby
        //private readonly string factoryResetCMD = "^0!FA\r";
        //private readonly string resetCounterCMD = "^0!RC\r"; // this could be implemented with parameters to reset counters for specific jobs,
        //private readonly string displayOnCMD = "^0!W1\r"; //turns the display on the printer on
        //private readonly string displayOffCMD = "^0!W0\r";
        //private readonly string flushBufferCMD = "^0!FF\r"; // if in mailing mode, this flushes the FIFO buffer, probably not needed.
        //private readonly string printParameterCMD = "^0=PR"; //make sure to append \r after constructing the rest of the command
        




    }

}

