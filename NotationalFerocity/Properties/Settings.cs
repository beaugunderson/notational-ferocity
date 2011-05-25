namespace NotationalFerocity.Properties
{
    public sealed partial class Settings
    {
        public Settings()
        {
            PropertyChanged += Settings_PropertyChanged;
            SettingChanging += Settings_SettingChanging;

            SettingsSaving += Settings_SettingsSaving;
            SettingsLoaded += Settings_SettingsLoaded;
        }

        void Settings_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        void Settings_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            
        }

        void Settings_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            
        }

        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }
    }
}