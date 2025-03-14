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
    public const string GAME_ZEROES             = "MickeysCockBlower";

    public const string FREEPLAY = "Freeplay";
    #endregion

    #region [ Sound Effect Names ]
    public const string SFX_EXPLODE = "ScuffedExplosion";
    public const string SFX_CHAINSAW = "Chainsaw";

    public const string SFX_REDEEM1 = "Redeem1";
    public const string SFX_REDEEM2 = "Redeem2";
    public const string SFX_REDEEM3 = "Redeem3";
    public const string SFX_REDEEM4 = "Redeem4";
    public const string SFX_REDEEM5 = "Redeem5";
    public const string SFX_REDEEM6 = "Redeem6";
    public const string SFX_REDEEM7 = "Redeem7";
    public const string SFX_REDEEM8 = "Redeem8";
    
    #endregion

	private AudioStreamPlayer[] m_musicPlayerTrackList;
    private int m_activeMusicStreamPlayer = 0;
    private AudioStreamPlayer m_musicPlayer;
    
    private AudioStreamPlayer m_musicPlayerTrack
    {
        get => (m_musicPlayerTrackList[m_activeMusicStreamPlayer]);
    }

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
        if (!m_musicPlayerTrack.Playing)
            return 0;
        
        return DurationToBeatCount(duration, m_music[m_activeTrack].Properties.BPM);
    }

    public double BeatCountToDuration(double beatCount)
    {
        if (!m_musicPlayerTrack.Playing)
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

	public void PlayAudio(string name, string bus, float offset = 0.0f)
	{
		AudioStreamPlayer player = new AudioStreamPlayer();
		this.AddChild(player);

		player.Stream = m_sfx[name].Stream;
		player.Bus = bus;

		player.Finished += player.QueueFree;
		player.Play();
        player.Seek(offset);
	}

	public void PlayMusic(string name)
	{
		m_musicPlayerTrack.Stop();
		m_musicPlayerTrack.Stream = m_music[name].Stream;
		m_musicPlayerTrack.Bus = "Music";

        m_musicPlayerTrack.Play();
        m_activeTrack = name;
	}

    public void StopAllMusic()
    {
        foreach (var player in m_musicPlayerTrackList)
            player?.Stop();
    }

    public void PlayMusicTransition(string newTrack, double endBeat, double transLength = 4)
    {
        double offset = BeatCountToDuration(endBeat);
        int newIdx = (m_activeMusicStreamPlayer + 1) % m_musicPlayerTrackList.Length;

        // Increase the beat count temporarily (to accomodate the transition length)
        string oldTrack = m_activeTrack;
        var oldTrackMP3 = (AudioStreamMP3)m_music[oldTrack].Stream;
        int oldBeatCount = oldTrackMP3.BeatCount;
        oldTrackMP3.BeatCount = (int)(oldTrackMP3.BeatCount + transLength);
        int oldIdx = m_activeMusicStreamPlayer;

        // Set the playhead of the old track
        m_musicPlayerTrack.Seek((float)offset);

        // Wait until the transition is done, then stop the old track
        var timer = this.GetTree().CreateTimer(BeatCountToDuration(transLength));
        timer.Timeout += () => {
            // Stop the old music player and set the new one as the active track
            m_musicPlayerTrackList[oldIdx].Stop();
            GD.Print($"Stopping track \"{oldTrack}\"");
            
            // Set the beat count back to the way it was before
            oldTrackMP3.BeatCount = oldBeatCount;
        };
        
        // Play the track in the second stream player
        m_musicPlayerTrackList[newIdx].Stop();
		m_musicPlayerTrackList[newIdx].Stream = m_music[newTrack].Stream;
		m_musicPlayerTrackList[newIdx].Bus = "Music";
        m_musicPlayerTrackList[newIdx].Play();
        GD.Print($"Playing track {newTrack}");

        m_activeTrack = newTrack;
        m_activeMusicStreamPlayer = newIdx;
    }

    public double GetPlayheadPosition() =>
        m_musicPlayerTrack.GetPlaybackPosition();

    public double GetCurrentBeat() =>
        DurationToBeatCount(GetPlayheadPosition(), m_music[m_activeTrack].Properties.BPM);

    public bool IsMusicPlaying() =>
        m_musicPlayerTrack.Playing;

    private void InitAudioStreamPlayers()
    {
        m_musicPlayerTrackList = new AudioStreamPlayer[2]{
            new AudioStreamPlayer(),
            new AudioStreamPlayer()
        };
        for (int i=0; i<m_musicPlayerTrackList.Length; i++)
        {
            this.AddChild(m_musicPlayerTrackList[i]);
        }
        m_activeMusicStreamPlayer = 0;
    }

	private void LoadSFX()
	{
		AddAudio(SFX_EXPLODE, SFX_PATH + "/deltarune_explosion.mp3");
        AddAudio(SFX_CHAINSAW, SFX_PATH + "/chainsaw/chainsaw.mp3");

        AddAudio(SFX_REDEEM1, SFX_PATH + "/redeem/redeem1.ogg");
        AddAudio(SFX_REDEEM2, SFX_PATH + "/redeem/redeem2.ogg");
        AddAudio(SFX_REDEEM3, SFX_PATH + "/redeem/redeem3.ogg");
        AddAudio(SFX_REDEEM4, SFX_PATH + "/redeem/redeem4.ogg");
        AddAudio(SFX_REDEEM5, SFX_PATH + "/redeem/redeem5.ogg");
        AddAudio(SFX_REDEEM6, SFX_PATH + "/redeem/redeem6.ogg");
        AddAudio(SFX_REDEEM7, SFX_PATH + "/redeem/redeem7.ogg");
        AddAudio(SFX_REDEEM8, SFX_PATH + "/redeem/redeem8.ogg");
	}

	private void LoadMusic()
	{
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
        AddMusic(GAME_ZEROES, MUSIC_PATH + "/HOLY SHIT IT'S MICKEYS COCK BLOWER CAN WE GO RIDE IT.mp3");

        AddMusic(FREEPLAY, MUSIC_PATH + "/freeplay theing.mp3");
	}

	public override void _Ready()
	{
		m_sfx = new Dictionary<string, AudioStream_Wrapper>();
		m_music = new Dictionary<string, AudioStream_Wrapper>();

        InitAudioStreamPlayers();
		LoadMusic();
		LoadSFX();
	}

	public override void _Process(double delta)
	{
        if (m_musicPlayerTrack.Playing)
        {
            var _as = ((AudioStreamMP3)m_musicPlayerTrack.Stream);
            float t = m_musicPlayerTrack.GetPlaybackPosition() / (float)m_music[m_activeTrack].Properties.EndOffset;
            double currentBeat = Mathf.Floor(GetCurrentBeat());

            //GD.Print($"Name: {_as._GetStreamName()}");
            //GD.Print($"Beat count: {_as.BeatCount}");
            //GD.Print($"Current beat: {currentBeat+1}");
        }
	}
}