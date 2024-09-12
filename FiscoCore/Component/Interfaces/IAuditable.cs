namespace Fisco.Component.Interfaces
{
    internal interface IAuditable
    {
        IList<Notify> Notifies { get; }
        void AddNotify(Notify notify);
    }
}
