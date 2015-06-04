using System;

namespace ActionStreetMap.Infrastructure.Diagnostic
{
    /// <summary> Default trace. Provides method to override. </summary>
    public class DefaultTrace : ITrace
    {
        #region ITrace implementation
       
        /// <inheritdoc />
        public int Level { get; set; }

        /// <inheritdoc />
        public void Debug(string category, string message)
        {
            if (Level == 0) 
                WriteRecord(RecordType.Debug, category, message, null);
        }

        /// <inheritdoc />
        public void Debug(string category, string format, string arg1)
        {
            if (Level == 0)
                WriteRecord(RecordType.Debug, category, String.Format(format, arg1), null);
        }

        /// <inheritdoc />
        public void Debug(string category, string format, string arg1, string arg2)
        {
            if (Level == 0)
                WriteRecord(RecordType.Debug, category, String.Format(format, arg1, arg2), null);
        }

        /// <inheritdoc />
        public void Info(string category, string message)
        {
            if (Level <= 1) 
                WriteRecord(RecordType.Info, category, message, null);
        }

        /// <inheritdoc />
        public void Info(string category, string format, string arg1)
        {
            if (Level <= 1) 
                WriteRecord(RecordType.Info, category, String.Format(format, arg1), null);
        }

        /// <inheritdoc />
        public void Info(string category, string format, string arg1, string arg2)
        {
            if (Level <= 1)
                WriteRecord(RecordType.Info, category, String.Format(format, arg1, arg2), null);
        }

        /// <inheritdoc />
        public void Warn(string category, string message)
        {
            if (Level <= 2) 
                WriteRecord(RecordType.Warn, category, message, null);
        }

        /// <inheritdoc />
        public void Warn(string category, string format, string arg1)
        {
            if (Level <= 2) 
                WriteRecord(RecordType.Warn, category, String.Format(format, arg1), null);
        }

        /// <inheritdoc />
        public void Warn(string category, string format, string arg1, string arg2)
        {
            if (Level <= 2)
                WriteRecord(RecordType.Warn, category, String.Format(format, arg1, arg2), null);
        }

        /// <inheritdoc />
        public void Error(string category, Exception ex, string message)
        {
            WriteRecord(RecordType.Error, category, message, ex);
        }

        /// <inheritdoc />
        public void Error(string category, Exception ex, string format, string arg1)
        {
            WriteRecord(RecordType.Error, category, String.Format(format, arg1), ex);
        }

        /// <inheritdoc />
        public void Error(string category, Exception ex, string format, string arg1, string arg2)
        {
            WriteRecord(RecordType.Error, category, String.Format(format, arg1, arg2), ex);
        }

        #endregion

        /// <summary> Writes record to trace. </summary>
        /// <param name="type">Record type.</param>
        /// <param name="category">Category.</param>
        /// <param name="message">Message.</param>
        /// <param name="exception">Exception.</param>
        protected virtual void WriteRecord(RecordType type, string category, string message, Exception exception) { }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing) { }

        /// <summary>  Defines trace record types. </summary>
        public enum RecordType
        {
            /// <summary> Debug. </summary>
            Debug,
            /// <summary> Info. </summary>
            Info,
            /// <summary> Warn. </summary>
            Warn,
            /// <summary> Error. </summary>
            Error,
        }
    }
}
