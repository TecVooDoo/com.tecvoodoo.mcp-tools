#if HAS_MALBERS_QUESTFORGE
#nullable enable
using System.ComponentModel;
using System.IO;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using MalbersAnimations.QuestForge;
using UnityEditor;
using UnityEngine;

namespace MCPTools.QuestForge.Editor
{
    public partial class Tool_QuestForge
    {
        [McpPluginTool("qf-create-poi", Title = "Quest Forge / Create POI")]
        [Description(@"Creates a new PointOfInterest ScriptableObject asset for the minimap/compass system.
POIs mark locations on the minimap and can be quest objectives, NPCs, merchants, etc.
Categories: QuestObjective, QuestGiver, Waypoint, Location, Enemy, NPC, Item, Merchant, FastTravel, Custom.")]
        public CreatePOIResponse CreatePOI(
            [Description("Asset path for the new POI SO (e.g. 'Assets/_Sandbox/_AQS/Data/POI/POI_SwampEntrance.asset').")]
            string assetPath,
            [Description("Display name of the point of interest.")]
            string poiName,
            [Description("POI category: 'QuestObjective', 'QuestGiver', 'Waypoint', 'Location', 'Enemy', 'NPC', 'Item', 'Merchant', 'FastTravel', 'Custom'. Default 'Waypoint'.")]
            string category = "Waypoint",
            [Description("Location ID string (e.g. 'swamp_entrance'). Used for quest objective matching.")]
            string? locationId = null,
            [Description("World X position. Default 0.")]
            float posX = 0f,
            [Description("World Y position. Default 0.")]
            float posY = 0f,
            [Description("World Z position. Default 0.")]
            float posZ = 0f,
            [Description("Location radius for proximity detection. Default 5.")]
            float radius = 5f,
            [Description("Show on minimap. Default true.")]
            bool showOnMinimap = true,
            [Description("Show on compass. Default true.")]
            bool showOnCompass = true,
            [Description("Enable fast travel to this location. Default false.")]
            bool enableFastTravel = false,
            [Description("Hide until player discovers this location. Default false.")]
            bool hideUntilDiscovered = false,
            [Description("POI description text.")]
            string? description = null,
            [Description("Priority for display ordering (higher = more important). Default 0.")]
            int priority = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                PointOfInterest poi = ScriptableObject.CreateInstance<PointOfInterest>();

                AssetDatabase.CreateAsset(poi, assetPath);

                // Use SerializedObject to set private [SerializeField] fields
                SerializedObject so = new SerializedObject(poi);
                so.FindProperty("poiName").stringValue = poiName;
                so.FindProperty("showOnMinimap").boolValue = showOnMinimap;
                so.FindProperty("showOnCompass").boolValue = showOnCompass;
                so.FindProperty("enableFastTravel").boolValue = enableFastTravel;
                so.FindProperty("hideUntilDiscovered").boolValue = hideUntilDiscovered;
                so.FindProperty("priority").intValue = priority;

                if (System.Enum.TryParse<POICategory>(category, true, out var parsedCategory))
                    so.FindProperty("category").enumValueIndex = (int)parsedCategory;

                if (!string.IsNullOrEmpty(description))
                    so.FindProperty("description").stringValue = description;

                so.ApplyModifiedPropertiesWithoutUndo();

                // Use public setter methods for location data
                poi.SetWorldPosition(new Vector3(posX, posY, posZ));
                poi.SetLocationRadius(radius);

                if (!string.IsNullOrEmpty(locationId))
                    poi.SetLocationId(locationId);

                EditorUtility.SetDirty(poi);
                AssetDatabase.SaveAssets();

                return new CreatePOIResponse
                {
                    assetPath = assetPath,
                    poiName = poi.POIName,
                    category = poi.Category.ToString(),
                    locationId = poi.LocationId ?? "",
                    worldPosition = $"({posX:F1}, {posY:F1}, {posZ:F1})",
                    radius = radius,
                    showOnMinimap = poi.ShowOnMinimap,
                    showOnCompass = poi.ShowOnCompass
                };
            });
        }

        [McpPluginTool("qf-query-pois", Title = "Quest Forge / Query POIs")]
        [Description(@"Lists all PointOfInterest ScriptableObject assets in the project.
Shows name, category, location ID, position, and display settings for each POI.")]
        public QueryPOIsResponse QueryPOIs(
            [Description("Folder path to search (e.g. 'Assets/_Sandbox/_AQS/Data/POI'). Null to search entire project.")]
            string? searchFolder = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string[] guids;
                if (!string.IsNullOrEmpty(searchFolder))
                    guids = AssetDatabase.FindAssets("t:PointOfInterest", new[] { searchFolder });
                else
                    guids = AssetDatabase.FindAssets("t:PointOfInterest");

                StringBuilder sb = new StringBuilder();
                int count = 0;

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    PointOfInterest poi = AssetDatabase.LoadAssetAtPath<PointOfInterest>(path);
                    if (poi == null) continue;

                    count++;
                    Vector3 pos = poi.WorldPosition;
                    sb.AppendLine($"  {poi.POIName} | Category: {poi.Category} | LocationID: {poi.LocationId ?? "none"} | Pos: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1}) | Radius: {poi.LocationRadius:F1}");
                    sb.AppendLine($"    Minimap: {poi.ShowOnMinimap} | Compass: {poi.ShowOnCompass} | FastTravel: {poi.EnableFastTravel} | Hidden: {poi.HideUntilDiscovered} | Priority: {poi.Priority}");
                    sb.AppendLine($"    Path: {path}");
                }

                return new QueryPOIsResponse
                {
                    poiCount = count,
                    details = sb.ToString()
                };
            });
        }

        public class CreatePOIResponse
        {
            [Description("Asset path where POI was saved")] public string assetPath = "";
            [Description("POI display name")] public string poiName = "";
            [Description("POI category")] public string category = "";
            [Description("Location ID")] public string locationId = "";
            [Description("World position")] public string worldPosition = "";
            [Description("Location radius")] public float radius;
            [Description("Shown on minimap")] public bool showOnMinimap;
            [Description("Shown on compass")] public bool showOnCompass;
        }

        public class QueryPOIsResponse
        {
            [Description("Number of POIs found")] public int poiCount;
            [Description("Detailed POI listing")] public string details = "";
        }
    }
}
#endif
