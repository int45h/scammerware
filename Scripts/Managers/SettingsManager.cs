using Godot;
using System;

public partial class SettingsManager : Node
{
    public const string DEFAULT_CONFIG_PATH = "user://main_config.cfg";
    #region [ Default Sections ]
    public const string DEFAULT_AUDIO_SECTION = "Audio";
    public const string DEFAULT_GRAPHICS_SECTION = "Graphics";
    public const string DEFAULT_CONTROLS_SECTION = "Controls";
    #endregion
    #region [ Default Audio Fields ]
    public const string DEFAULT_AUDIO_MASTER_BUS = "master_bus_volume";
    public const string DEFAULT_AUDIO_MUSIC_BUS = "music_bus_volume";
    public const string DEFAULT_AUDIO_SFX_BUS = "sfx_bus_volume";
    public const string DEFAULT_AUDIO_DIALOGUE_BUS = "sfx_bus_volume";
    #endregion

    bool m_config_exists = false;
    ConfigFile m_config;

    public bool ExistingConfigFile 
    {
        get => m_config_exists;
    }

    #region [ Config Menu Interface ]
    public Variant GetValue(string section, string key, Variant _default = default) =>
        m_config.GetValue(section, key, _default);

    public void SetValue(string section, string key, Variant value) => 
        m_config.SetValue(section, key, value);

    public string[] GetSectionKeys(string section) =>
        m_config.GetSectionKeys(section);

    public void Save(string path = DEFAULT_CONFIG_PATH) =>
        m_config.Save(path);
    #endregion

    private void GenerateDefaults()
    {
        m_config.SetValue(DEFAULT_AUDIO_SECTION, DEFAULT_AUDIO_MASTER_BUS, -80.0f);
        m_config.SetValue(DEFAULT_AUDIO_SECTION, DEFAULT_AUDIO_MUSIC_BUS, -80.0f);
        m_config.SetValue(DEFAULT_AUDIO_SECTION, DEFAULT_AUDIO_SFX_BUS, -80.0f);
        m_config.SetValue(DEFAULT_AUDIO_SECTION, DEFAULT_AUDIO_DIALOGUE_BUS, -80.0f);

        m_config.Save(DEFAULT_CONFIG_PATH);
    }

    public Error LoadConfigFile(string path = DEFAULT_CONFIG_PATH)
    {
        Error result = m_config.Load(path);
        m_config_exists = (result == Error.Ok);

        return result;
    }

	public override void _Ready()
	{
        GD.Print(OS.GetDataDir());

        m_config = new ConfigFile();
        if (LoadConfigFile() != Error.Ok)
        {
            GD.PrintRich($"[color=yellow]Warning: Config file \"{DEFAULT_CONFIG_PATH}\" doesn't exist, making a new file now[/color]");
            GenerateDefaults();
        }
	}

	public override void _Process(double delta)
	{
        
	}
}
