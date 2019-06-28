﻿namespace FTServer.Monitor
{
    /// <summary>
    /// Implemented by metric types that observe individual events with specific values.
    /// </summary>
    public interface IObserver
    {
        /// <summary>
        /// Observes a single event with the given value.
        /// </summary>
        void Observe(double val);
    }
}