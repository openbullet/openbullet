namespace OpenBullet.ViewModels
{
    public class OBSettingsViewModel
    {
        public OBSettingsGeneral General { get; set; } = new OBSettingsGeneral();
        public OBSettingsSounds Sounds { get; set; } = new OBSettingsSounds();
        public OBSettingsThemes Themes { get; set; } = new OBSettingsThemes();
    }
}
