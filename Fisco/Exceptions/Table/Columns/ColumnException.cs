using System;

namespace Fisco.Exceptions.Table.Columns
{
    internal class ColumnException : Exception
    {
        public ColumnException(string message) : base(message)
        {
            //nothing
        }
    }
}
