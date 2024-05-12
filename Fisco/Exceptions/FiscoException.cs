using System;

namespace Fisco.Exceptions
{
    internal class FiscoException : Exception
    {
        public FiscoException(string message) : base(message)
        {

        }
    }
}
