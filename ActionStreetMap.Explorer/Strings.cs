namespace ActionStreetMap.Explorer
{
    internal static class Strings
    {
        public static string CannotRunGameWithoutPrerequesites = "Fatal: GameRunner cannot be instatiated because if missing service registrations inside container.";
        public static string CannotReadMainConfig = "Fatal: cannot read configuration from {0}";
        public static string CannotRunGameTwice = "Cannot call RunGame method second time!";
        public static string CannotRegisterPluginForActiveGame = "Plugin cannot be installed while game is run";
        public static string CannotGetBuildingStyle = "Can't get building style - unknown building type: {0}. " +
                                                      "Try to check your current mapcss and theme files";       
        public static string InvalidPolyline = "Attempt to render polyline with less than 2 points";
        public static string InvalidUvMappingDefinition = "Cannot read uv mapping: '{0}'. Something is wrong with theme files?";
        public static string CannotChangeRelativeNullPoint = "You cannot change relative null point dynamically!";

        public static string CannotFindRoofBuilder ="Cannot find roof builder which can build roof of given building: {0} - suspect wrong theme definition";
        public static string CannotClipPolygon = "The polygons passed in must have at least 3 MapPoints: subject={0}, clip={1}";
        public static string BugInPolygonOrderAlgorithm = "Bug in polygon order algorithm!";
        public static string GabledRoofGenFailed = "Gabled roof generation algorithm is faled for {0}";

        public static string TerrainScanLineAlgorithmBug =
            "Bug in algorithm! We're expecting to have even number of intersection _pointsBuffer: (_pointsBuffer.Count % 2 != 0)";

        #region Commands

        public static string TagCommand = "Tag search";
        public static string LocateCommand = "Shows position of given object";

        #endregion
    }
}
