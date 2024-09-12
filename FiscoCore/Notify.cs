namespace Fisco
{
    internal class Notify
    {
        public Guid ID { get; private set; } = Guid.NewGuid();
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }
}
