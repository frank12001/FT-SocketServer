﻿namespace FTServer.Monitor
{
    public interface ICounter
    {
        void Inc(double increment = 1);
        double Value { get; }
    }
}
