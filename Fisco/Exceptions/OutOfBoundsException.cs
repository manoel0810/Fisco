using System;

namespace Fisco.Exceptions
{
    internal class OutOfBoundsException : Exception
    {
        public OutOfBoundsException(string message) : base(message)
        {

        }
    }
}
