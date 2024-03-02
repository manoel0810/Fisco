using System;

namespace Fisco.Exceptions.Table.Cells
{
    internal class CellException : Exception
    {
        public CellException(string message) : base(message)
        {
            //nothing
        }
    }
}
