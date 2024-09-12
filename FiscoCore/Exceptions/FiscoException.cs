namespace Fisco.Exceptions
{
    internal class FiscoException : Exception
    {
        public FiscoException(string message) : base(message)
        {

        }

        public FiscoException(string message, Exception exception) : base(message, exception)
        {

        }

        public FiscoException()
        {

        }
    }
}
