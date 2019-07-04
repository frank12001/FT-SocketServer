namespace FTServer.Monitor
{
    public interface IGauge
    {
        void Inc(double increment = 1);
        void Set(double val);
        void Dec(double decrement = 1);
        double Value { get; }
    }
}
