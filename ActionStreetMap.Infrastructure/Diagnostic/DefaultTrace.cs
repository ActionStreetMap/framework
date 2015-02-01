using System;

namespace ActionStreetMap.Infrastructure.Diagnostic
{
    /// <summary> Empty trace which simplifies logging via inheritance. </summary>
    public class DefaultTrace : ITrace
    {
        #region ITrace implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {

        }
        
        /// <inheritdoc />
        public int Level { get; set; }

        /// <inheritdoc />
        public void Normal(string message)
        {
            WriteRecord(RecordType.Normal, "", message, null);
        }

        /// <inheritdoc />
        public void Normal(string category, string message)
        {
            WriteRecord(RecordType.Normal, category, message, null);
        }

        /// <inheritdoc />
        public void Output(string message)
        {
            WriteRecord(RecordType.Output, "", message, null);
        }

        /// <inheritdoc />
        public void Output(string category, string message)
        {
            WriteRecord(RecordType.Normal, category, message, null);
        }

        /// <inheritdoc />
        public void Input(string message)
        {
            WriteRecord(RecordType.Input, "", message, null);
        }

        /// <inheritdoc />
        public void Input(string category, string message)
        {
            WriteRecord(RecordType.Normal, category, message, null);
        }

        /// <inheritdoc />
        public void System(string message)
        {
            WriteRecord(RecordType.System, "", message, null);
        }

        /// <inheritdoc />
        public void System(string category, string message)
        {
            WriteRecord(RecordType.Normal, category, message, null);
        }

        /// <inheritdoc />
        public void Warn(string message)
        {
            WriteRecord(RecordType.Warning, "", message, null);
        }

        /// <inheritdoc />
        public void Warn(string category, string message)
        {
            WriteRecord(RecordType.Normal, category, message, null);
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception)
        {
            WriteRecord(RecordType.Error, "", message, exception);
        }

        /// <inheritdoc />
        public void Error(string category, string message, Exception exception)
        {
            WriteRecord(RecordType.Normal, category, message, exception);
        }

        #endregion

        /// <summary> Writes record to trace. </summary>
        /// <param name="type">Record type.</param>
        /// <param name="category">Category.</param>
        /// <param name="message">Message.</param>
        /// <param name="exception">Exception.</param>
        protected virtual void WriteRecord(RecordType type, string category, string message, Exception exception)
        {

        }
    }
}
