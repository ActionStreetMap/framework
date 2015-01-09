namespace ActionStreetMap.Explorer
{
    internal static class Strings
    {
        public static string CannotGetBuildingStyle = "Can't get building style - unknown building type: {0}. " +
                                                      "Try to check your current mapcss and theme files";       
        public static string InvalidPolyline = "Attempt to render polyline with less than 2 points";
        public static string InvalidUvMappingDefinition = "Cannot read uv mapping: '{0}'. Something is wrong with theme files?";
        public static string CannotChangeRelativeNullPoint = "You cannot change relative null point dynamically!";

        #region Commands

        public static string TagCommand = "Tag search";
        public static string LocateCommand = "Shows position of given object";

        #endregion
    }
}
