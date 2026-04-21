#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace MCPTools.RTW.Editor
{
    public partial class Tool_RTW
    {
        [McpPluginTool("rtw-query", Title = "Real Time Weather Pro / Query")]
        [Description(@"Reports the RealTimeWeatherManager state.
Reports: location (lat/lon), active weather/water systems, request modes, auto-update settings.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                var mgr = GetManager();
                var sb = new StringBuilder();

                sb.AppendLine("RealTimeWeatherManager:");
                sb.AppendLine($"  Latitude: {Get(mgr, "Latitude")}");
                sb.AppendLine($"  Longitude: {Get(mgr, "Longitude")}");
                sb.AppendLine($"  SelectedWeatherSystem: {Get(mgr, "SelectedWeatherSystem")}");
                sb.AppendLine($"  SelectedWaterSystem: {Get(mgr, "SelectedWaterSystem")}");
                sb.AppendLine($"  WeatherDataRequestMode: {Get(mgr, "WeatherDataRequestMode")}");
                sb.AppendLine($"  MaritimeDataRequestMode: {Get(mgr, "MaritimeDataRequestMode")}");
                sb.AppendLine($"  LastChosenRequestMode: {Get(mgr, "LastChosenRequestMode")}");
                sb.AppendLine($"  IsAutoWeatherEnabled: {Get(mgr, "IsAutoWeatherEnabled")}");
                sb.AppendLine($"  AutoWeatherUpdateRate: {Get(mgr, "AutoWeatherUpdateRate")}");

                try
                {
                    var forecast = Call(mgr, "IsForecastModeEnabled");
                    sb.AppendLine($"  ForecastModeEnabled: {forecast}");
                }
                catch { /* method may not exist or fail in edit mode */ }

                return sb.ToString();
            });
        }
    }
}
