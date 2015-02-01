using System;
using ActionStreetMap.Infrastructure.Diagnostic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> UnityConsole trace </summary>
    internal class UnityConsoleTrace: DefaultTrace
    {
        /// <inheritdoc />
        protected override void WriteRecord(RecordType type, string category, string message, Exception exception)
        {
            switch (type)
            {
                case RecordType.Error:
                    Debug.LogException(exception);
                    break;
                case RecordType.Warning:
                    Debug.LogWarning(String.Format("{0}:{1}", category, message));
                    break;
                default:
                    Debug.Log(String.Format("{0}:{1}", category, message));
                    break;
            }
        }

    }
}
