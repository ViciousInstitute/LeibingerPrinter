using System;
using System.Collections.Specialized;

namespace LeibingerAPI
{
    public class LeibingerStatus : Leibinger
    {
        /// <summary>
        /// The following information is all located in the document titled "Leibinger-JET2neo Interface Protocol" v 1.7.4
        /// This document is poorly translated from German so it is unclear what some of these states mean...
        /// When issued a Status() command, the printer responds with a string of tab delimited 
        /// parameters, for example: 0^=RS4\t1\t0\t0\t303
        /// The first portion "0^=RS" is the printer indicating which command it is respinding to.
        /// The first digit after this (4 in the above example) is a parameter indicating the status of the nozzle.
        /// The following is a list of possible nozzle states and the value for each:
        /// 0 = invalid (printer in standby or initialization)
        /// 1 = is opening
        /// 2 = is opened
        /// 3 = is closing
        /// 4 = is closed
        /// 5 = is inbetween open and closed (???)
        /// 
        /// The second parameter is the state of the machine:
        /// 1 = Standby
        /// 2 = Initialization phase (booting up, bleeding ink)
        /// 3 = Interval or service panel (???)
        /// 4 = Ready for Action (can send instructions like OpenNozzle())
        /// 5 = Ready for StartPrint() (nozzle is open)
        /// 
        /// Some of the status commands will not respond if the printer is not booted. In this case the socket 
        /// connection times out after a wait period (set in the Connect() method) and throws an IO exception.
        /// There can also be issues with some of them if the printer is in an error state while it is booting up
        /// (for example if it has exceeded its service interval), which can also result in an IO exception.
        /// </summary>
        /// 

       public LeibingerStatus(string printerIP, int port)
            : base(printerIP, port)
        {
            IPAddress = printerIP;
            Port = port;

        } //constructor

       private readonly string statusCMD = "^0?RS\r"; //get a string of tab delimited parameters indicating status of the printer. Most Methods in this class just parse the data returned by this command
        private readonly string currentJobCMD = "^0?JL\r"; //check which job is currently loaded
        private readonly string queryExtTextCMD = "^0?ET\r";
        private readonly string currentCountCMD = "^0?CC\r";
        private readonly string clearErrorCMD = "^0!EQ\r";
        private readonly string setEncoderCMD = "^0=PR5"; //dont forget to concat \r


        private int _currentCount;
        private int _errorCode;
        private bool _headCover;
        private int _nozzleState;
        private int _machineState;
        private double _actSpeed;
        private string _status;
        private string _lastExtText;
        private string _currentJob;
        private bool _booted;
        private bool _nozzleOpen;
        private bool _printReady;
        private bool _ready;
        private bool _hasError;
        private BitVector32 _errorBits;
        //private string _errorBits;

        public bool Ready
        {
            get
            {
                string printerStatus = SendCMD(statusCMD);
                char[] stateOfMachine = printerStatus.ToCharArray();
                if (stateOfMachine[7] == '6')
                {
                    _ready = true;
                }
                else
                {
                    _ready = false;
                }
                return _ready;
            }

        } //The printer will print when optical sensor triggers.
        public bool PrintReady
        {
            get
            {
                string printerStatus = SendCMD(statusCMD);
                char[] stateOfMachine = printerStatus.ToCharArray();
                if (stateOfMachine[7] == '5')
                {
                    _printReady = true;
                }
                else
                {
                    _printReady = false;
                }
                return _printReady;
            }

        } //ready for Print Go (big green button on HMI)
        public bool NozzleOpen
        {
            get
            {
                string printerStatus = SendCMD(statusCMD);
                char[] stateOfMachine = printerStatus.ToCharArray();
                if (stateOfMachine[5] == '2')
                {
                    _nozzleOpen = true;
                }
                else
                {
                    _nozzleOpen = false;
                }
                return _nozzleOpen;
            }

        } // indicates if the printer nozzle is open (true) or closed (false)
        public bool IsBooted
        {
            get
            {

                string printerStatus = SendCMD(statusCMD);
                char[] stateOfMachine = printerStatus.ToCharArray();
                if (stateOfMachine[7] == '4' || stateOfMachine[7] == '5')
                {
                    _booted = true;
                }
                else
                {
                    _booted = false;
                }
                return _booted;
            }

        } // indicates printer is powered on and ready to accept commands
        public string CurrentJob //returns the current print job loaded on the printer
        {
            get
            {
                _currentJob = SendCMD(currentJobCMD);
                return _currentJob;
            }

        }
        public string LastExternalText
        {
            get
            {
                _lastExtText = SendCMD(queryExtTextCMD);
                return _lastExtText;
            }

        }  // get the current contents of external text buffer on printer
        public string Status
        {
            get
            {
                _status = SendCMD(statusCMD);
                return _status;
            }

        }
        public double InternalEncoderSpeed // gets or sets the speed setting of the internal software encoder in the printer, measured in meters/minute.
        {
            get
            {
                
                double returnedVal;
                string printerStatus = SendCMD(statusCMD);
                string[] parsedStatus = printerStatus.Split('\t');
                returnedVal = double.Parse(parsedStatus[4]);
                _actSpeed = returnedVal / 10;
                return _actSpeed;
            }
            set
            {
                _actSpeed = value;
                SendCMD(string.Concat(setEncoderCMD, '\t', _actSpeed, "\r"));
            }

        }
        public int MachineState
        {
            get
            {

                string printerStatus = SendCMD(statusCMD);
                string[] parsedStatus = printerStatus.Split('\t');
                _machineState = int.Parse(parsedStatus[1]);
                return _machineState;
            }

        }
        public int NozzleState
        {
            get
            {

                string printerStatus = SendCMD(statusCMD);
                string[] parsedStatus = printerStatus.Split('\t');
                char[] statusPrefix = { '^', '0', '=', 'R', 'S' };
                string nozzleParam = parsedStatus[0].Trim(statusPrefix);
                Int32.TryParse(nozzleParam, out _nozzleState);
                return _nozzleState;
            }

        }
        public int ErrorCode
        {
            get
            {
                string printerStatus = SendCMD(statusCMD);
                string[] parsedStatus = printerStatus.Split('\t');
                Int32.TryParse(parsedStatus[2], out _errorCode);
                return _errorCode;

            }

        }
        public int Count
        {
            get
            {
                string countString = SendCMD(currentCountCMD);
                string[] parsedCount = countString.Split('\t');
                char[] statusPrefix = { '^', '0', '=', 'C', 'C' };
                string countVal = parsedCount[0].Trim(statusPrefix);
                Int32.TryParse(countVal, out _currentCount);
                return _currentCount;
            }

        } //throws IO exception if the printer is not on
        public bool HeadCover
        {
            get
            {
                string printerStatus = SendCMD(statusCMD);
                string[] parsedStatus = printerStatus.Split('\t');
                Boolean.TryParse(parsedStatus[3], out _headCover);
                return _headCover;
            }

        } //false is closed, true is open
        public bool HasError
        {
            get
            {
                if (ErrorCode != 0)
                {
                    _hasError = true;

                }
                else
                    _hasError = false;
                return _hasError;
            }

        }


        // This will return 32 bits representing error code bitmask. 
        // This information is on page 16 of the interface protocol manual.
        public BitVector32 BinaryError
        {
            get
            {
                _errorBits = new BitVector32(ErrorCode);
                return _errorBits;
            }

        }

        //public string BinaryError
        //{
        //    get
        //    {
        //        _errorBits = Convert.ToString(ErrorCode, 2);
        //        return _errorBits;
        //    }

        //} //this version returns a string


        /// <summary>
        /// If this is called and there is an error, it will throw an exception
        /// </summary>
        public void ErrorCheck()
        {
            if (ErrorCode != 0)
            {
                throw new PrinterErrorException(ErrorCode.ToString());
            }
        }

        /// <summary>
        /// Use this to clear an error
        /// </summary>
        public void ClearError()
        {
            SendCMD(clearErrorCMD);
        }

    }
}

