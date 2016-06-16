using System;
using System.Runtime.Serialization;

namespace LeibingerAPI
{
    /// <summary>
    /// I have no idea what I am doing here. I just saw on MSDN that is is good practice to include this stuff...
    /// </summary>
    public class PrinterErrorException : Exception, ISerializable
    {
        public PrinterErrorException()
        {
        }
        public PrinterErrorException(string message)
            : base(message)
        {

        }
        public PrinterErrorException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
        protected PrinterErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }

}

