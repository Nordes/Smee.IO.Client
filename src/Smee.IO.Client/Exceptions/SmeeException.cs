using System;

namespace Smee.IO.Client.Exceptions
{
    public class SmeeException : Exception
    {
        public SmeeException(string message) : base(message)
        {
        }
    }
}