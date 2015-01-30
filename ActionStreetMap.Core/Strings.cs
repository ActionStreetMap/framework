namespace ActionStreetMap.Core
{
    internal static class Strings
    {
        // errors
        public static string StyleDeclarationNotFound = "Declaration '{0}' not found for '{1}'";
        public static string RuleNotApplicable = "Rule isn't applicable!";
        public static string TileDeactivationBug = "Tile was destroyed, but still present in active tiles collection. Suspect an issue in activation/deactivation logic";
        public static string StyleVisitNullTree = "Cannot visit style: tree is null!";
    }
}
