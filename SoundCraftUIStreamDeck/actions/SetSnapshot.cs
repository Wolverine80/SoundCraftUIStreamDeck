using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Threading;
using System.Threading.Tasks;
using UIControl;

namespace SoundCraftUIStreamDeck
{
    [PluginActionId("org.m-a-b.soundcraftuisnap")]
    public class SetSnapshot : KeypadBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    ShowName = string.Empty,
                    SnapshotName = string.Empty,
                    CueName = string.Empty
                };
                return instance;
            }
            [JsonProperty(PropertyName = "showName")]
            public string ShowName { get; set; }
            [JsonProperty(PropertyName = "snapshotName")]
            public string SnapshotName { get; set; }
            [JsonProperty(PropertyName = "cueName")]
            public string CueName { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public SetSnapshot(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings is null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            ConMan.ClientActive(true);
        }

        public override void Dispose()
        {
            ConMan.ClientActive(false);
        }

        public async override void KeyPressed(KeyPayload payload)
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            if (settings.SnapshotName != "" && settings.CueName != "")
            {
                ConMan.client.ChangeSnapshot(settings.ShowName, settings.SnapshotName);
                Task.Delay(100).Wait();
                ConMan.client.ChangeCue(settings.ShowName, settings.CueName);
            }
            else if (settings.SnapshotName != "")
            {
                ConMan.client.ChangeSnapshot(settings.ShowName, settings.SnapshotName);
            }
            else if (settings.CueName != "")
            {
                ConMan.client.ChangeCue(settings.ShowName, settings.CueName);
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, "Key Pressed but now Show is set");
            }

        }

        public override void KeyReleased(KeyPayload payload) { }

        public async override void OnTick()
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                //  Logger.Instance.LogMessage(TracingLevel.ERROR, "Active Clients: "+ConMan.ActiveClients +"Connected: "+ConMan.Instance.IsConnected);
                return;
            }

            if (CheckState())
            {
                await Connection.SetStateAsync(0);
            }
            else
            {
                await Connection.SetStateAsync(1);
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private bool CheckState()
        {
            if (settings.ShowName == ConnectionClass.Show && settings.SnapshotName == ConnectionClass.Snapshot && settings.CueName == ConnectionClass.Cue) return true;
            else if (settings.ShowName == ConnectionClass.Show && settings.SnapshotName == ConnectionClass.Snapshot && settings.CueName == "") return true;
            else if (settings.ShowName == ConnectionClass.Show && settings.CueName == ConnectionClass.Cue && settings.SnapshotName == "") return true;
            else return false;
        }

        #endregion
    }
}