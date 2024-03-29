using System;

namespace Fisco
{
    internal class Notify
    {
        public Guid ID { get; private set; } = Guid.NewGuid();
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
