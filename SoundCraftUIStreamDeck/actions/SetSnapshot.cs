using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UIsocket;

namespace SoundCraftUIStreamDeck
{
    [PluginActionId("org.m-a-b.soundcraftuisnap")]
    public class SetSnapshot : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.ShowName = string.Empty;
                instance.SnapshotName = string.Empty;
                instance.CueName = string.Empty;
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
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
           
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public async override void KeyPressed(KeyPayload payload)
        {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }
            if (settings.SnapshotName != "" && settings.CueName != "") {
                ConMan.client.ChangeSnapshot(settings.ShowName, settings.SnapshotName);
                ConMan.client.ChangeCue(settings.ShowName, settings.CueName);
            } else if (settings.SnapshotName != "")
            {
                ConMan.client.ChangeSnapshot(settings.ShowName, settings.SnapshotName);
            } else if (settings.CueName != "")
            {
                ConMan.client.ChangeCue(settings.ShowName, settings.CueName);
            } else
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, "Key Pressed but now Show is set");
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
        }

        public override void KeyReleased(KeyPayload payload) { }

        public async override void OnTick() {
            if (!ConMan.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }


            //if (ConMan.client.CheckState(settings.ShowName, settings.SnapshotName, settings.CueName))
            //{
            //    await Connection.SetStateAsync(0);

            //} else
            //{
            //    await Connection.SetStateAsync(1);
            //}
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

        #endregion
    }
}