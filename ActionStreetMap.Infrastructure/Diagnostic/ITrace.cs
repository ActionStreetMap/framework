using System;

namespace ActionStreetMap.Infrastructure.Diagnostic
{
    /// <summary> Represents a tracer for tracing subsystem. </summary>
    public interface ITrace : IDisposable
    {
        /// <summary> Level of tracing. </summary>
        int Level { get; set; }

        /// <summary> Writes debug message. </summary>
        void Debug(string category, string message);

        /// <summary> Writes debug message. </summary>
        void Debug(string category, string format, string arg1);

        /// <summary> Writes debug message. </summary>
        void Debug(string category, string format, string arg1, string arg2);

        /// <summary> Writes info message. </summary>
        void Info(string category, string message);

        /// <summary> Writes info message. </summary>
        void Info(string category, string format, string arg1);

        /// <summary> Writes info message. </summary>
        void Info(string category, string format, string arg1, string arg2);

        /// <summary> Writes warn message. </summary>
        void Warn(string category, string message);

        /// <summary> Writes warn message. </summary>
        void Warn(string category, string format, string arg1);

        /// <summary> Writes warn message. </summary>
        void Warn(string category, string format, string arg1, string arg2);

        /// <summary> Writes exception message. </summary>
        void Error(string category, Exception ex, string message);

        /// <summary> Writes exception message. </summary>
        void Error(string category, Exception ex, string format, string arg1);

        /// <summary> Writes exception message. </summary>
        void Error(string category, Exception ex, string format, string arg1, string arg2);
    }
}