namespace ActionStreetMap.Core
{
    internal static class Strings
    {
        // errors
        public static string StyleDeclarationNotFound = "Declaration '{0}' not found for '{1}'";
        public static string RuleNotApplicable = "Rule isn't applicable!";
        public static string TileStateException = "Unexpected state ({0}) of tile ({1})";
        public static string StyleVisitNullTree = "Cannot visit style: tree is null!";
        public static string CannotAddTagCollection = "Cannot add tag to collection as it is completed.";
        public static string CannotSearchTagCollection = "Cannot search inside tag collection as it's not completed!";
    }
}
