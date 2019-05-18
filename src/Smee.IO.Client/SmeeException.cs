using System;

namespace Smee.IO.Client
{
    public class SmeeException : Exception
    {
        public SmeeException(string message) : base(message)
        {
        }
    }
}