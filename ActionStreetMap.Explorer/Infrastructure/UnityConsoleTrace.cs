using System;
using ActionStreetMap.Infrastructure.Diagnostic;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> UnityConsole trace </summary>
    public class UnityConsoleTrace: DefaultTrace
    {
        /// <inheritdoc />
        protected override void WriteRecord(RecordType type, string category, string message, Exception exception)
        {
            switch (type)
            {
                case RecordType.Error:
                    UnityEngine.Debug.LogException(exception);
                    break;
                case RecordType.Warn:
                    UnityEngine.Debug.LogWarning(String.Format("{0}:{1}", category, message));
                    break;
                default:
                    UnityEngine.Debug.Log(String.Format("{0}:{1}", category, message));
                    break;
            }
        }

    }
}
