namespace ActionStreetMap.Infrastructure.Diagnostic
{
    /// <summary>  Defines trace record types. </summary>
    public enum RecordType
    {
        /// <summary> Normal. </summary>
        Normal,
        /// <summary> Warning. </summary>
        Warning,
        /// <summary> Error. </summary>
        Error,
        /// <summary> System. </summary>
        System,
        /// <summary> Input. </summary>
        Input,
        /// <summary> Output. </summary>
        Output
    }
}