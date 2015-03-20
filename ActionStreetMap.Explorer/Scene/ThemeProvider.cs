using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Explorer.Geometry.Primitives;
using ActionStreetMap.Explorer.Scene.Buildings;
using ActionStreetMap.Explorer.Scene.Buildings.Facades;
using ActionStreetMap.Explorer.Scene.Buildings.Roofs;
using ActionStreetMap.Explorer.Scene.Infos;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;
using UnityEngine;
using Rect = ActionStreetMap.Explorer.Geometry.Primitives.Rect;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Defines theme provider logic. </summary>
    public interface IThemeProvider
    {
        /// <summary> Gets theme. </summary>
        /// <returns>Theme.</returns>
        Theme Get();
    }

    /// <summary> Default theme provider which uses json files with style definitions. </summary>
    internal class ThemeProvider : IThemeProvider, IConfigurable
    {
        private const string BuildingsThemeFile = @"buildings";
        private const string InfosThemeFile = @"infos";

        private readonly IFileSystemService _fileSystemService;
        private readonly IEnumerable<IFacadeBuilder> _facadeBuilders;
        private readonly IEnumerable<IRoofBuilder> _roofBuilders;

        private Theme _theme;

        /// <summary> Creates ThemeProvider. </summary>
        /// <param name="fileSystemService">File system service.</param>
        /// <param name="facadeBuilders">Facade builders.</param>
        /// <param name="roofBuilders">Roof builders.</param>
        [Dependency]
        public ThemeProvider(IFileSystemService fileSystemService,
            IEnumerable<IFacadeBuilder> facadeBuilders,
            IEnumerable<IRoofBuilder> roofBuilders)
        {
            _fileSystemService = fileSystemService;
            _facadeBuilders = facadeBuilders.ToArray();
            _roofBuilders = roofBuilders.ToArray();
        }

        /// <inheritdoc />
        public Theme Get()
        {
            return _theme;
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            var buildingStyleProvider = GetBuildingStyleProvider(configSection);
            var infoStyleProvider = GetInfoStyleProvider(configSection);
            _theme = new Theme(buildingStyleProvider, infoStyleProvider);
        }

        #region Buildings

        private IBuildingStyleProvider GetBuildingStyleProvider(IConfigSection configSection)
        {
            var facadeStyleMapping = new Dictionary<string, List<BuildingStyle.FacadeStyle>>();
            var roofStyleMapping = new Dictionary<string, List<BuildingStyle.RoofStyle>>();
            foreach (var buildThemeConfig in configSection.GetSections(BuildingsThemeFile))
            {
                var path = buildThemeConfig.GetString("path", null);

                var jsonStr = _fileSystemService.ReadText(path);
                var json = JSON.Parse(jsonStr);

                var facadeStyles = GetFacadeStyles(json);
                var roofStyles = GetRoofStyles(json);

                var types = json["name"].AsArray.Childs.Select(t => t.Value);
                foreach (var type in types)
                {
                    facadeStyleMapping.Add(type, facadeStyles);
                    roofStyleMapping.Add(type, roofStyles);
                }

            }
            return new BuildingStyleProvider(facadeStyleMapping, roofStyleMapping);
        }

        private List<BuildingStyle.FacadeStyle> GetFacadeStyles(JSONNode json)
        {
            var facadeStyles = new List<BuildingStyle.FacadeStyle>();
            foreach (JSONNode node in json["facades"].AsArray)
            {
                var builders = node["builders"].AsArray.Childs
                    .Select(t => _facadeBuilders.Single(b => b.Name == t.Value)).ToArray();
                var path = node["path"].Value;
                var size = new Size(node["size"]["width"].AsInt, node["size"]["height"].AsInt);
                foreach (JSONNode textureNode in node["textures"].AsArray)
                {
                    var map = textureNode["map"];
                    facadeStyles.Add(new BuildingStyle.FacadeStyle
                    {
                        Height = textureNode["height"].AsInt,
                        Width = textureNode["width"].AsInt,
                        Material = String.Intern(textureNode["material"].Value),
                        Color = ColorUtility.FromUnknown(textureNode["color"].Value),
                        Builders = builders,
                        Path = path,
                        FrontUvMap = GetUvMap(map["front"], size),
                        BackUvMap = GetUvMap(map["back"], size),
                        SideUvMap = GetUvMap(map["side"], size)
                    });
                }
            }
            return facadeStyles;
        }

        private List<BuildingStyle.RoofStyle> GetRoofStyles(JSONNode json)
        {
            var roofStyles = new List<BuildingStyle.RoofStyle>();
            foreach (JSONNode node in json["roofs"].AsArray)
            {
                var builders = node["builders"].AsArray.Childs
                    .Select(t => _roofBuilders.Single(b => b.Name == t.Value)).ToArray();
                var path = node["path"].Value;
                var size = new Size(node["size"]["width"].AsInt, node["size"]["height"].AsInt);
                foreach (JSONNode textureNode in node["textures"].AsArray)
                {
                    var map = textureNode["map"];
                    roofStyles.Add(new BuildingStyle.RoofStyle
                    {
                        Type = String.Intern(textureNode["type"].Value),
                        Height = textureNode["height"].AsInt,
                        Material = String.Intern(textureNode["material"].Value),
                        Color = ColorUtility.FromUnknown(textureNode["color"].Value),
                        Builders = builders,
                        Path = path,
                        FrontUvMap = GetUvMap(map["front"], size),
                        SideUvMap = GetUvMap(map["side"], size),
                    });
                }
            }

            return roofStyles;
        }

        #endregion


        private IInfoStyleProvider GetInfoStyleProvider(IConfigSection configSection)
        {
            // NOTE ignore name of style pack - just use one collection
            var infoStyleMap = new Dictionary<string, InfoStyle>();
            foreach (var infoThemeConfig in configSection.GetSections(InfosThemeFile))
            {
                var path = infoThemeConfig.GetString("path", null);

                var jsonStr = _fileSystemService.ReadText(path);
                var json = JSON.Parse(jsonStr);
                FillInfoStyleList(json, infoStyleMap);
            }
            return new InfoStyleProvider(infoStyleMap);
        }

        private void FillInfoStyleList(JSONNode json, Dictionary<string, InfoStyle> infoStyleMap)
        {
            foreach (JSONNode node in json["infos"].AsArray)
            {
                var path = node["path"].Value;
                var size = new Size(node["size"]["width"].AsInt, node["size"]["height"].AsInt);
                foreach (JSONNode textureNode in node["textures"].AsArray)
                {
                    var map = textureNode["map"];
                    infoStyleMap.Add(textureNode["key"].Value, new InfoStyle
                    {
                        Path = path,
                        UvMap = GetUvMap(map["main"], size),
                    });
                }
            }
        }

        private Rect GetUvMap(string value, Size size)
        {
            // expect x,y,width,height and (0,0) is left bottom corner
            if (value == null)
                return null;

            var values = value.Split(',');
            if (values.Length != 4)
                throw new InvalidOperationException(String.Format(Strings.InvalidUvMappingDefinition, value));

            var width = (float)int.Parse(values[2]);
            var height = (float)int.Parse(values[3]);

            var offset = int.Parse(values[1]);
            var x = (float)int.Parse(values[0]);
            var y = Math.Abs( (offset + height) - size.Height);

            var leftBottom = new Vector2(x / size.Width, y / size.Height);
            var rightUpper = new Vector2((x + width) / size.Width, (y + height) / size.Height);

            return new Rect(leftBottom, rightUpper);
        }
    }
}
