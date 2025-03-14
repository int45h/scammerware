using Godot;
using System;

public partial class Microgame : Node
{
    public const string SONG_LIST = 
        AudioManager.GAME_REDEEM + "," + 
        AudioManager.GAME_CAPTCHA + "," + 
        AudioManager.GAME_MAZE + "," + 
        AudioManager.GAME_FORM + "," + 
        AudioManager.GAME_DELIVERY + "," + 
        AudioManager.GAME_CHAINSAW + "," + 
        AudioManager.GAME_CHAINSAW_GORDONIO + "," + 
        AudioManager.GAME_ALPHA + "," + 
        AudioManager.GAME_ZEROES + "," +
        AudioManager.GAME_TRANSITION;

    [Export]
    public float Duration = 0.5f;

    [Export(PropertyHint.MultilineText)]
    public string StartText = "GAME START";

    [Export(PropertyHint.MultilineText)]
    public string EndText = "GAME END";

    [Export(PropertyHint.Enum, SONG_LIST)]
    public string GameTrack = AudioManager.GAME_TRANSITION;

    [Signal]
    public delegate void OnGameStart_EventHandler();

    public bool Running = false;
    public GameManager Manager;
    public float CurrentDuration = 0;

    private bool m_completed = false;
    public Action<float> GameRunnerOverride;

    public void OnComplete()
    {
        float timeRemaining = Mathf.Max(Duration - CurrentDuration, 0);

        m_completed = true;
        Running = false;

        Manager?.OnComplete(timeRemaining);
    }
    public void OnFailed()
    {
        Running = false;
        Manager?.OnFailed();
    }

    public void Start()
    {
        EmitSignal(SignalName.OnGameStart_);
        
        Running = true;
        CurrentDuration = Duration;
    }

    public override void _Process(double delta)
    {
        if (!Running)
            return;

        if (GameRunnerOverride == null)
        {
            CurrentDuration -= (float)delta;
            if (CurrentDuration <= 0.0f)
                OnFailed();
        }
        else GameRunnerOverride((float)delta);
    }
}
