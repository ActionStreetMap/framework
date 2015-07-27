namespace ActionStreetMap.Explorer
{
    internal static class Strings
    {
        public static string CannotRegisterPluginForCompletedBootstrapping = "Plugin cannot be installed if bootstrapping is completed.";
        public static string CannotGetBuildingStyle = "Can't get building style - unknown building type: {0}. " +
                                                      "Try to check your current mapcss and theme files";       
        public static string InvalidPolyline = "Attempt to render polyline with less than 2 points";
        public static string InvalidUvMappingDefinition = "Cannot read uv mapping: '{0}'. Something is wrong with theme files?";
        public static string CannotChangeRelativeNullPoint = "You cannot change relative null point dynamically!";

        public static string CannotFindRoofBuilder ="Cannot find roof builder which can build roof of given building: {0} - suspect wrong theme definition";
        public static string CannotClipPolygon = "The polygons passed in must have at least 3 MapPoints: subject={0}, clip={1}";

        public static string GabledRoofGenFailed = "Gabled roof generation algorithm is faled for {0}";

        public static string InvalidGradientString = "Invalid gradient string: {0}";

        public static string CommandIsNotRegistered = "Command is not registered: {0}";

        public static string MapIndexBuildOutputDirectoryMismatch = "Output directory is different than expected map path.";

        #region Commands

        public static string SearchCommand = "Searches for OSM objects.";
        public static string LocateCommand = "Shows position of given object.";
        public static string GeocodeCommand = "Performs reverse geocoding.";

        #endregion
    }
}
