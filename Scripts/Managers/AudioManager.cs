using Godot;
using System;
using System.Collections.Generic;

public struct AudioProperties
{
    public int BPM = 0;
    public int BeatCount = 0;
    public double StartOffset = 0, EndOffset = 0;

    public AudioProperties( int _bpm = 0, 
                            int _beatCount = 0,
                            double _start = 0, 
                            double _end = 0)
    {
        BPM = _bpm;
        BeatCount = _beatCount;
        StartOffset = _start;
        EndOffset = _end;
    }
};

public struct AudioStream_Wrapper
{
    public AudioStream Stream;
    public AudioProperties Properties;
    public Type StreamType = typeof(AudioStream);
    public AudioStream_Wrapper(AudioStream _stream, int BPM, int BeatCount, double Start, double End, Type type)
    {
        Stream = _stream;
        Properties.BPM = BPM;
        Properties.BeatCount = BeatCount;
        Properties.StartOffset = Start;
        Properties.EndOffset = End;
    }
}

public partial class AudioManager : Node
{
	const float _INV_LOG_10 = 0.434294481903f;
	const string SFX_PATH 	= "res://Assets/Resources/SFX";
	const string MUSIC_PATH = "res://Assets/Resources/Music";

    public const string MASTER_BUS = "Master";
    public const string MUSIC_BUS = "Music";
    public const string SFX_BUS = "SFX";
    public const string DIALOGUE_BUS = "Dialogue";
	
    #region [ Track Names ]
    public const string GAME_INTRO              = "Intro";
    public const string GAME_TRANSITION         = "InBetween";
    public const string GAME_REDEEM             = "Redeem";
    public const string GAME_CAPTCHA            = "Captcha";
    public const string GAME_MAZE               = "Maze";
    public const string GAME_FORM               = "Form";
    public const string GAME_DELIVERY           = "Delivery";
    public const string GAME_CHAINSAW           = "Chainsaw";
    public const string GAME_CHAINSAW_GORDONIO  = "ChainsawGordon";
    public const string GAME_ALPHA              = "Alpha";

    public const string FREEPLAY = "Freeplay";
    #endregion

	private AudioStreamPlayer 	m_music_player_track1,
						        m_music_player_track2;

	private Dictionary<string, AudioStream_Wrapper> m_sfx,
									                m_music;

    private string m_activeTrack;

    #region [ Helper Functions ]
	public static float DB2Linear(float dB) => 
		Mathf.Pow(10, dB * 0.1f);

	public static float Linear2DB(float linear) =>
		10.0f * (float)Mathf.Log(Mathf.Clamp(linear, 1e-8, 1)) * _INV_LOG_10;

    public static double DurationToBeatCount(double duration, double BPM) =>
        duration*BPM/60.0;

    public static double BeatCountToDuration(double beatCount, double BPM) =>
        beatCount*60.0/BPM;

    public double DurationToBeatCount(double duration)
    {
        if (!m_music_player_track1.Playing)
            return 0;
        
        return DurationToBeatCount(duration, m_music[m_activeTrack].Properties.BPM);
    }

    public double BeatCountToDuration(double beatCount)
    {
        if (!m_music_player_track1.Playing)
            return 0;
        
        return BeatCountToDuration(beatCount, m_music[m_activeTrack].Properties.BPM);
    }
    
    private bool IsStreamClassOf<T>(AudioStream stream) where T : AudioStream
    {
        string strA = stream.GetClass();
        string strB = typeof(T).ToString();
        return strB.Contains(strA, StringComparison.InvariantCultureIgnoreCase);
    }
    #endregion
    #region [ Audio Server Interface ]
	public static void SetBusVolume(string bus, float dB) => 
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(bus), dB);

	public static void SetBusVolumeSlider(string bus, float slider) => 
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(bus), Linear2DB(slider));

	public static float GetBusVolume(string bus) =>
		AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(bus));

	public static float GetBusVolumeSlider(string bus) =>
		DB2Linear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(bus)));
    #endregion
    #region [ Config File Reader/Writer ]
    public static void ReadVolumeFromConfig(SettingsManager config)
    {
        var audio_section = SettingsManager.DEFAULT_AUDIO_SECTION;
        var master_vol      = config.GetValue(audio_section, SettingsManager.DEFAULT_AUDIO_MASTER_BUS);
        var music_vol       = config.GetValue(audio_section, SettingsManager.DEFAULT_AUDIO_MUSIC_BUS);
        var sfx_vol         = config.GetValue(audio_section, SettingsManager.DEFAULT_AUDIO_SFX_BUS);
        var dialogue_vol    = config.GetValue(audio_section, SettingsManager.DEFAULT_AUDIO_DIALOGUE_BUS);
        
        SetBusVolume(MASTER_BUS, (float)master_vol);
        SetBusVolume(MUSIC_BUS, (float)music_vol);
        SetBusVolume(SFX_BUS, (float)sfx_vol);
        SetBusVolume(DIALOGUE_BUS, (float)dialogue_vol);
    }

    public static void WriteVolumeToConfig(SettingsManager config)
    {
        var audio_section = SettingsManager.DEFAULT_AUDIO_SECTION;
        var master_vol      = GetBusVolume(MASTER_BUS);
        var music_vol       = GetBusVolume(MUSIC_BUS);
        var sfx_vol         = GetBusVolume(SFX_BUS);
        var dialogue_vol    = GetBusVolume(DIALOGUE_BUS);
        
        config.SetValue(audio_section, SettingsManager.DEFAULT_AUDIO_MASTER_BUS, master_vol);
        config.SetValue(audio_section, SettingsManager.DEFAULT_AUDIO_MUSIC_BUS, music_vol);
        config.SetValue(audio_section, SettingsManager.DEFAULT_AUDIO_SFX_BUS, sfx_vol);
        config.SetValue(audio_section, SettingsManager.DEFAULT_AUDIO_DIALOGUE_BUS, dialogue_vol);

        config.Save();
    }
    #endregion

	public void AddAudio(string name, string filepath) 
    {
        var track = GD.Load<AudioStream>(filepath);
		m_sfx.Add(name, new AudioStream_Wrapper(track, 0, 0, 0, 0, typeof(AudioStream)));
    }

	public void AddMusic(string name, string filepath) 
    {
        var track = GD.Load<AudioStream>(filepath);
        Type type = typeof(AudioStream);
        if (!IsStreamClassOf<AudioStreamMP3>(track))
        {
            GD.PrintErr($"Unsupported audio type: {track.GetClass()}");
            return;
        }

        var mp3 = ((AudioStreamMP3)track);
        double _end = (mp3.BeatCount != 0) 
            ? BeatCountToDuration(mp3.BeatCount, mp3.Bpm) 
            :  mp3.GetLength();

        m_music.Add(name, new AudioStream_Wrapper(
            track, 
            (int)mp3.Bpm,
            mp3.BeatCount, 
            mp3.LoopOffset, 
            _end, 
            type
        ));
    }

	public void PlayAudio(string name, string bus)
	{
		AudioStreamPlayer player = new AudioStreamPlayer();
		this.AddChild(player);

		player.Stream = m_sfx[name].Stream;
		player.Bus = bus;

		player.Finished += player.QueueFree;
		player.Play();
	}

	public void PlayMusic(string name)
	{
		m_music_player_track1.Stop();
		m_music_player_track1.Stream = m_music[name].Stream;
		m_music_player_track1.Bus = "Music";

        m_music_player_track1.Play();
        m_activeTrack = name;
	}

    public double GetPlayheadPosition() =>
        m_music_player_track1.GetPlaybackPosition();

    public double GetCurrentBeat() =>
        DurationToBeatCount(GetPlayheadPosition(), m_music[m_activeTrack].Properties.BPM);

    public bool IsMusicPlaying() =>
        m_music_player_track1.Playing;

	private void LoadSFX()
	{
		//AddAudio("click_sound",		SFX_PATH + "/guns/cg1.wav");
	}

	private void LoadMusic()
	{
		m_music_player_track1 = new AudioStreamPlayer();
		this.AddChild(m_music_player_track1);

        AddMusic(GAME_INTRO, MUSIC_PATH + "/intro.mp3");
        AddMusic(GAME_TRANSITION, MUSIC_PATH + "/the inbetween.mp3");
        AddMusic(GAME_REDEEM, MUSIC_PATH + "/breakcore.mp3");
        AddMusic(GAME_CAPTCHA, MUSIC_PATH + "/mysterio but hes made of music.mp3");
        AddMusic(GAME_MAZE, MUSIC_PATH + "/Weird maze thingy.mp3");
        AddMusic(GAME_FORM, MUSIC_PATH + "/ticking.mp3");
        AddMusic(GAME_DELIVERY, MUSIC_PATH + "/Placeholder Song.mp3");
        AddMusic(GAME_CHAINSAW, MUSIC_PATH + "/Not mickgordoned.mp3");
        AddMusic(GAME_CHAINSAW_GORDONIO, MUSIC_PATH + "/Holy Mick Gordon.mp3");
        AddMusic(GAME_ALPHA, MUSIC_PATH + "/phonk only.mp3");

        AddMusic(FREEPLAY, MUSIC_PATH + "/freeplay theing.mp3");
	}

	public override void _Ready()
	{
		m_sfx = new Dictionary<string, AudioStream_Wrapper>();
		m_music = new Dictionary<string, AudioStream_Wrapper>();

		LoadMusic();
		LoadSFX();
	}

	public override void _Process(double delta)
	{
        if (m_music_player_track1.Playing)
        {
            var _as = ((AudioStreamMP3)m_music_player_track1.Stream);
            float t = m_music_player_track1.GetPlaybackPosition() / (float)m_music[m_activeTrack].Properties.EndOffset;
            double currentBeat = Mathf.Floor(GetCurrentBeat());

            //GD.Print($"Name: {_as._GetStreamName()}");
            //GD.Print($"Beat count: {_as.BeatCount}");
            //GD.Print($"Current beat: {currentBeat+1}");
        }
	}
}