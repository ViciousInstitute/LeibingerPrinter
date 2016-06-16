using System;
using System.Collections.Generic;
using System.Linq;
using LeibingerAPI;
using System.Diagnostics;

namespace LeibingerTestApp
{
    class Program
    {
        static Leibinger printer;
        static LeibingerStatus printerStatus;
        static void Main(string[] args)
        {
            string printerIP = "10.10.5.62"; //IP of the TCP server (the printer)
            int port = 3000; // Server port to attach to
            printer = new Leibinger(printerIP, port);
            printerStatus = new LeibingerStatus(printerIP, port);
            bool getCommand = true;
            PrintCommands command;

            while (getCommand)
            {
                #region Menu Items
                Console.Clear();
                Console.WriteLine("Choose a command.");
                Console.WriteLine("PowerOn");
                Console.WriteLine("PowerOff");
                Console.WriteLine("OpenNozzle");
                Console.WriteLine("CloseNozzle");
                Console.WriteLine("LoadJobNBBS");
                Console.WriteLine("LoadJobDefend");
                Console.WriteLine("Status");
                Console.WriteLine("ClearError");
                Console.WriteLine("SendTimeTest");
                Console.WriteLine("PrintRoutine");
                Console.WriteLine("ErrorTesting");
                Console.WriteLine("LastExternalText");
                Console.WriteLine("CurrentCount");
                Console.WriteLine("GetErrorCode");
                Console.WriteLine("TestAllProperties");
                Console.WriteLine("TerminalMode");
                Console.WriteLine("SetDelay");
                Console.WriteLine("EncoderTest");
                Console.WriteLine("Listener");
                Console.WriteLine("Exit");
                #endregion

                string userInput = Console.ReadLine();
                if (Enum.TryParse(userInput, true, out command))
                {
                    switch (command)
                    {
                        #region Power On
                        case PrintCommands.POWERON:
                            Console.WriteLine("Booting printer...");
                            printer.PowerOn();
                            bool status = printerStatus.IsBooted;
                            while (!status)
                            {
                                status = printerStatus.IsBooted;
                                System.Threading.Thread.Sleep(1000);
                            }
                            Console.WriteLine("The printer is ready to receive commands.");
                            break;
                        #endregion //powers up and polls status
                        #region Power Off
                        case PrintCommands.POWEROFF:
                            Console.WriteLine("Shutting printer down...");
                            printer.PowerOff();
                            break;
                        #endregion
                        #region Open Nozzle
                        case PrintCommands.OPENNOZZLE:
                            Console.WriteLine("Opening nozzle...");
                            printer.OpenNozzle();
                            bool nozzleOpen = false;
                            while (!nozzleOpen)
                            {
                                nozzleOpen = printerStatus.NozzleOpen;
                                System.Threading.Thread.Sleep(1000);
                            }
                            Console.WriteLine("The nozzle is open.");
                            break;
                        #endregion
                        #region Close Nozzle
                        case PrintCommands.CLOSENOZZLE:
                            Console.WriteLine("Closing nozzle...");
                            printer.CloseNozzle();
                            break;
                        #endregion
                        #region Load NBBS Template
                        case PrintCommands.LOADJOBNBBS:
                            Console.WriteLine("Loading NBBS barcode template...");
                            printer.LoadJob(PrintJobs.NBBS);
                            Console.WriteLine("Querying printer for current job...");
                            Console.WriteLine(printerStatus.CurrentJob);
                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        #endregion
                        #region Load Defend Template
                        case PrintCommands.LOADJOBDEFEND:
                            Console.WriteLine("Loading DEFEND barcode template...");
                            printer.LoadJob(PrintJobs.DEFEND);
                            Console.WriteLine("Querying printer for current job...");
                            Console.WriteLine(printerStatus.CurrentJob);
                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        #endregion
                        #region Status
                        case PrintCommands.STATUS:
                            Console.WriteLine("Getting status...");
                            Console.WriteLine(printerStatus.Status);
                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        #endregion
                        #region Current Job
                        case PrintCommands.CURRENTJOB:
                            Console.WriteLine("Getting current job...");
                            Console.WriteLine(printerStatus.CurrentJob);
                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        #endregion
                        #region Clear Error
                        case PrintCommands.CLEARERROR:
                            Console.WriteLine("Clearing Error...");
                            printerStatus.ClearError();
                            break;
                        #endregion
                        #region Last External Text
                        case PrintCommands.LASTEXTERNALTEXT:
                            Console.WriteLine("The last string sent to the printer is:");
                            Console.WriteLine(printerStatus.LastExternalText);
                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        #endregion
                        #region Send Time Test
                        case PrintCommands.SENDTIMETEST:
                            Console.WriteLine("Sending 1000 test numbers and averaging time...");
                            SendTimeTest();
                            break;
                        #endregion
                        #region Print Routine
                        case PrintCommands.PRINTROUTINE:
                            Console.WriteLine("Demonstrating a typical print routine.");
                            PrintRoutine();
                            break;
                        #endregion
                        #region Error Testing
                        case PrintCommands.ERRORTESTING:
                            Console.WriteLine("Test status parse.");
                            ErrorTesting();
                            Console.ReadLine();
                            break;
                        #endregion
                        #region CurrentCount
                        case PrintCommands.CURRENTCOUNT:
                            Console.WriteLine(printerStatus.Count);
                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        #endregion
                        #region Get Error Code
                        case PrintCommands.GETERRORCODE:
                            Console.WriteLine(printerStatus.ErrorCode);
                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        #endregion
                        #region Test All Properties
                        case PrintCommands.TESTALLPROPERTIES:
                            TestAllProperties();
                            break;
                        #endregion
                        #region TerminalMode
                        case PrintCommands.TERMINALMODE:
                            TerminalMode();
                            break;
                        #endregion
                        case PrintCommands.LISTENER:
                            ListenerServiceTest();
                            break;
                        #region Set Delay
                        case PrintCommands.SETDELAY:
                            int delayTime;
                            Console.WriteLine("Enter the delay between prints in milliseconds.");
                            string msInput = Console.ReadLine();
                            if (Int32.TryParse(msInput, out delayTime))
                            {
                                printer.Delay = delayTime;
                            }
                            else
                            {
                                Console.WriteLine("Invalid entry. Delay set to default 500ms.");
                                printer.Delay = 500;
                            }

                            Console.WriteLine("\nPress enter.");
                            Console.ReadLine();
                            break;
                        case PrintCommands.ENCODERTEST:
                            EncoderTest();
                            break;
                        #endregion

                        #region Exit
                        case PrintCommands.EXIT:
                            getCommand = false;
                            break;
                        #endregion
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command, \nPress enter to try again.");
                    Console.ReadLine();
                    Console.Clear();
                }

            }

        }
        public static void SendTimeTest()
        {
            /* ****************************************
             * Sends 1000 test numbers to the printer 
             * and gives the average time when done. 
             *****************************************/

            const string nbBarcode = "NBBS.job"; // The name of the print job template
            string testNumber; // Test number to be printed
            string expirationDate; // Expiration date to be printed
            int nbTestNumGen = 10000001; // for testing
            
            Stopwatch sendTimer = new Stopwatch(); // for timing test        
            List<long> sendTimes = new List<long>(); // for timing test

            sendTimer.Start();
            printer.LoadJob(nbBarcode); // make sure a template is loaded

            for (int i = 0; i < 1000; i++)
            {
                sendTimer.Restart();
                testNumber = nbTestNumGen.ToString();
                expirationDate = "31-MAR-2017";
                printer.SendTestNumber(testNumber, expirationDate);
                nbTestNumGen++;
                sendTimes.Add(sendTimer.ElapsedMilliseconds);
               
            }
            Console.WriteLine("The average send time was {0} ms.", sendTimes.Average());
            Console.ReadLine();
        }
        public static void PrintRoutine()
        {
           
            string testNumber; // Test number to be printed
            string expirationDate; // Expiration date to be printed
            int nbTestNumGen = 10000001; // for testing
            int delay = 750; //polling delay

            printer.PowerOn();
            bool ready = printerStatus.IsBooted;
            while (!ready)
            {
                ready = printerStatus.IsBooted;
                System.Threading.Thread.Sleep(delay);
            }
            printerStatus.ClearError();
            printer.LoadJob(PrintJobs.NBBS);
            printer.OpenNozzle();
            printerStatus.ClearError();
            bool nozzleOpen = printerStatus.NozzleOpen;
            while (!nozzleOpen)
            {
                nozzleOpen = printerStatus.NozzleOpen;
                System.Threading.Thread.Sleep(delay);
            }

            ready = printerStatus.PrintReady;
            while (!ready)
            {
                ready = printerStatus.PrintReady;
                System.Threading.Thread.Sleep(delay);
            }


            Console.WriteLine("Ready to print. \nPress enter to continue.");
            Console.ReadLine();
            printer.StartPrint();
            printer.Delay = 500; // this sets the delay between sending test info

            for (int i = 0; i < 20; i++)
            {
                nbTestNumGen++;
                testNumber = nbTestNumGen.ToString();
                expirationDate = "31-MAR-2017";
                printer.SendTestNumber(testNumber, expirationDate);

            }
            printer.StopPrint();

            System.Threading.Thread.Sleep(delay);
            printer.PowerOff();
        }
        public static void ErrorTesting()
        {
            Console.WriteLine(printerStatus.Status);
            Console.WriteLine(printerStatus.NozzleState);
            Console.WriteLine(printerStatus.MachineState);
            Console.WriteLine(printerStatus.ErrorCode);
            Console.WriteLine(printerStatus.HeadCover);
            Console.WriteLine(printerStatus.InternalEncoderSpeed);
            Console.WriteLine("\nPress enter.");
            Console.ReadLine();
        }
        public static void TestAllProperties()
        {
            try
            {
                //printerStatus.ErrorCheck();
                Console.WriteLine();
                Console.WriteLine("Internal Encoder value: {0} m/min", printerStatus.InternalEncoderSpeed);
                Console.WriteLine("Count value: {0}", printerStatus.Count);
                Console.WriteLine("Current Job value: {0}", printerStatus.CurrentJob);
                Console.WriteLine("Error code value: {0}", printerStatus.ErrorCode);
                Console.WriteLine("Head Cover value: {0}", printerStatus.HeadCover);
                Console.WriteLine("Last External Text value: {0}", printerStatus.LastExternalText);
                Console.WriteLine("Machine State value: {0}", printerStatus.MachineState);
                Console.WriteLine("Nozzle State value: {0}", printerStatus.NozzleState);
                Console.WriteLine("Status value: {0}", printerStatus.Status);
                Console.WriteLine("IsBooted value: {0}", printerStatus.IsBooted);
                Console.WriteLine("NozzleOpen value: {0}", printerStatus.NozzleOpen);
                Console.WriteLine("BinaryError value: {0}", printerStatus.BinaryError);
                Console.WriteLine("HasError value: {0}", printerStatus.HasError);
                Console.WriteLine("Delay value: {0}", printer.Delay);
                Console.WriteLine("\nPress enter.");
                Console.ReadLine();
            }
            catch (PrinterErrorException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                printerStatus.ClearError();

            }


        }
        public static void TerminalMode()
        {

            Console.WriteLine("Press ESC to stop");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {

                string userInput = Console.ReadLine();
                string messageCMD = userInput + '\r';
                printer.SendCMD(messageCMD);

            }

        }
        public static void EncoderTest()
        {
            Console.WriteLine("The internal encoder speed is {0}", printerStatus.InternalEncoderSpeed);
            Console.WriteLine("Enter a value.");
            string userInput = Console.ReadLine();
            double encoderSpeed;
            if(Double.TryParse(userInput, out encoderSpeed))
            {
                printerStatus.InternalEncoderSpeed = encoderSpeed;
            }
            Console.ReadLine();
            Console.WriteLine("The internal encoder speed is {0}", printerStatus.InternalEncoderSpeed);
            Console.ReadLine();
        }
        public static void ListenerServiceTest()
        {
            PrinterSpeedListener speedListener = new PrinterSpeedListener();
            speedListener.Listen();
            
        }
        public enum PrintCommands
        {
            POWERON,
            POWEROFF,
            OPENNOZZLE,
            CLOSENOZZLE,
            LOADJOBNBBS,
            LOADJOBDEFEND,
            STATUS,
            CURRENTJOB,
            CLEARERROR,
            LASTEXTERNALTEXT,
            SENDTIMETEST,
            PRINTROUTINE,
            ERRORTESTING,
            CURRENTCOUNT,
            GETERRORCODE,
            TESTALLPROPERTIES,
            TERMINALMODE,
            SETDELAY,
            ENCODERTEST,
            LISTENER,
            EXIT

        }

    }
}
