using System;

namespace ActionStreetMap.Infrastructure.Diagnostic
{
    /// <summary> Represents a tracer for tracing subsystem. </summary>
    public interface ITrace : IDisposable
    {
        /// <summary> Level of tracing. </summary>
        int Level { get; set; }

        /// <summary> Writes message to trace using default tracer category. </summary>
        void Normal(string message);

        /// <summary> Writes message to trace using category provided. </summary>
        void Normal(string category, string message);

        /// <summary> Writes message to trace using default tracer category. </summary>
        void Output(string message);

        /// <summary> Writes message to trace using category provided. </summary>
        void Output(string category, string message);

        /// <summary> Writes message to trace using default tracer category. </summary>
        void Input(string message);

        /// <summary> Writes message to trace using category provided. </summary>
        void Input(string category, string message);

        /// <summary> Writes message to trace using default tracer category. </summary>
        void System(string message);

        /// <summary> Writes message to trace using category provided. </summary>
        void System(string category, string message);

        /// <summary> Writes message to trace using default tracer category. </summary>
        void Warn(string message);

        /// <summary> Writes message to trace using category provided. </summary>
        void Warn(string category, string message);

        /// <summary> Writes message to trace using default tracer category. </summary>
        void Error(string message, Exception exception);

        /// <summary> Writes message to trace. </summary>
        void Error(string category, string message, Exception exception);
    }
}