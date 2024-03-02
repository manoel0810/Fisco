using System;

namespace Fisco.Exceptions
{
    internal class NoDeterministicsException : Exception
    {
        public NoDeterministicsException(string message) : base(message)
        {

        }
    }
}
