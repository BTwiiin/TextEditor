using System.Configuration;

namespace TextEditor.Utilities
{
    public class ConfigManager
    {
        public string LoadConfig(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        public void SaveConfig(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}