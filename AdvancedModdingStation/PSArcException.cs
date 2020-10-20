using System;

namespace AdvancedModdingStation
{
    public class PSArcException : Exception
    {
        public PSArcException() : base()
        {

        }

        public PSArcException(string msg) : base(msg)
        {

        }

        public PSArcException(string msg, Exception innerException) : base(msg, innerException)
        {

        }
    }
}
