using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public enum GameState
{
    TITLE_SCREEN = 0,
    LOADING,
    GAME_START,
    GAME_RUNNING,
    GAME_END,
    COMPLETE,
    PAUSED
}

public enum GameStatus
{
    UNDECIDED = 0,
    SUCCESS,
    FAILURE
}

public partial class GameManager : Node
{
    #region [Player Callbacks]
    [Signal]
    public delegate void ScoreEvent_EventHandler(float score);

    [Signal]
    public delegate void LifeEvent_EventHandler();
    
    [Signal]
    public delegate void ResetPlayer_EventHandler();
    
    public void OnPlayerDeath()
    {
        GD.Print("GAME OVER");
        SetState(GameState.COMPLETE);
    }
    #endregion
    
    #region [Microgame Callbacks]
    public void OnComplete(float timeRemaining)
    {
        GD.Print("Success");
        m_status = GameStatus.SUCCESS;

        EmitSignal(SignalName.ScoreEvent_, timeRemaining * 100.0f);
    }
    
    public void OnFailed()
    {
        GD.Print("Failure");
        m_status = GameStatus.FAILURE;

        EmitSignal(SignalName.LifeEvent_);
    }
    #endregion

    #region [Audio Manager]
    private AudioManager m_audioManager;
    #endregion

    #region [Game State Variables]
    [Export]
    public int Stages = 10;

    [Export]
    public int Lives = 3;

    public int PlayerLives = 0;

    private bool m_delayed = false;
    private GameState m_state;
    private GameStatus m_status;
    private Dictionary<GameState, Action> m_stateActions;
    
    private System.Random m_rng;
    private int m_currentGameIndexPtr = 0;
    private int m_currentGameIndex = 0;
    
    private GameLayer m_layer;
    private int m_currentGameStage = 0;
    #endregion

    #region [Gameplay Variables]
    [Export(PropertyHint.Dir)]
    public string MicrogamePath = "res://Scenes/Microgames/";
    
    private List<Microgame> m_gameList;
    private List<int> m_gameIndices;
    #endregion

    #region [AudioManager Getter]
    public AudioManager AudioManager
    {
        get
        {
            if (m_audioManager == null)
            {
                m_audioManager = this.GetNode<AudioManager>("/root/AudioManager");
                if (m_audioManager == null){
                    GD.PrintErr("Failed to get audio manager");
                    return null;
                }
            }

            return m_audioManager;
        }
    }
    #endregion

    #region [Game State Methods]
    public void SetState(GameState newState) =>
        HandleStateChange(newState);

    private void HandleStateChange(GameState newState)
    {
        switch (m_state)
        {
            case GameState.COMPLETE:
                m_state = newState;
                AudioManager.PlayMusic(AudioManager.GAME_INTRO);
                break;
            case GameState.GAME_START:

                m_state = newState;
                break;
            default:
                m_state = newState;
                break;
        }
    }

    private void HandleState()
    {
        if (m_delayed)
            return;
        
        if (!m_stateActions.ContainsKey(m_state))
        {
            GD.Print($"State \"{m_state.ToString()}\" doesn't have an action associated with it.");
            return;
        }
        m_stateActions[m_state]();
    }
    
    private void SetStateDeferred(GameState newState, float time, Action callback = null)
    {
        m_delayed = true;
        var timer = this.GetTree().CreateTimer(time);
        timer.Timeout += () => {
            SetState(newState);
            m_delayed = false;

            callback?.Invoke();
        };
    }
    
    // TO-DO: Make a version that will wait until the next beat to actually start the beat timer
    private void SetStateDeferredBeat(GameState newState, double beatCount, Action callback = null)
    {
        m_delayed = true;
        // Get offset between current duration and the next beat
        double currentBeat = AudioManager.GetCurrentBeat();
        double beatOffset = AudioManager.BeatCountToDuration(currentBeat-Mathf.Ceil(currentBeat));
        double beatDuration = AudioManager.BeatCountToDuration(beatOffset + beatCount);

        var timer = this.GetTree().CreateTimer(beatDuration);
        timer.Timeout += () => {
            SetState(newState);
            m_delayed = false;
        
            callback?.Invoke();
        };
    }
    
    private void SetStateTween()
    {
        var tween = this.GetTree().CreateTween();
        tween.TweenMethod(Callable.From<float>((v) => {
            
        }), 10.0f, 0.0f, 1.0f);
    }

    #endregion

    #region [Gameplay Stuff]
    private void ShuffleGames()
    {
        // Fisher-Yates shuffle algorithm
        int startLength = m_gameList.Count;
        int endLength = 0;

        while (endLength != m_gameList.Count)
        {
            int rndIndex = m_rng.Next(0, startLength-1);
            startLength--;
            
            int tmp = m_gameIndices[startLength];
            m_gameIndices[startLength] = m_gameIndices[rndIndex];
            m_gameIndices[rndIndex] = tmp;
            
            endLength++;
        }
    }

    private void LoadMicrogames()
    {
        // Init game list
        if (m_gameList == null)
            m_gameList = new List<Microgame>();
        else
            m_gameList.Clear();
        
        // Init game indices
        if (m_gameIndices == null)
            m_gameIndices = new List<int>();
        else
            m_gameList.Clear();

        if (string.IsNullOrEmpty(MicrogamePath))
        {
            GD.PrintErr("Microgame path has not been set.");
            return;
        }

        var directory = DirAccess.GetFilesAt(MicrogamePath);
        if (directory.Length < 1)
        {
            GD.PrintErr("Error: there are no microgames present in this directory");
            return;
        }

        foreach (var file in directory)
        {
            LoadMicrogame(string.Format("{0}/{1}", MicrogamePath, file));
        }

        ShuffleGames();
    }

    private void LoadMicrogame(string path)
    {
        var game = ResourceLoader.Load<PackedScene>(path);
        if (game == null)
        {
            GD.PrintErr($"Failed to load Microgame from path \"{path}\"");
            return;
        }
        
        var gameInstance = game.Instantiate<Microgame>();
        if (gameInstance == null)
        {
            GD.PrintErr($"Failed to instantiate Microgame from path \"{path}\"");
            return;
        }
        
        gameInstance.Manager = this;
        m_gameIndices.Add(m_gameList.Count);
        m_gameList.Add(gameInstance);
    }
    
    private void SelectMicrogame()
    {
        int nextIndex = m_gameIndices[m_currentGameIndexPtr++];
        if (m_currentGameIndexPtr >= m_gameList.Count)
        {
            m_currentGameIndexPtr = 0;
            ShuffleGames();
        }

        m_currentGameIndex = nextIndex;
        m_layer?.SetChild(m_gameList[m_currentGameIndex]);
    }

    private void StartGameHandler()
    {
        AudioManager.PlayMusic(AudioManager.GAME_TRANSITION);
        m_layer?.HideScreen();
        SelectMicrogame();
        
        m_layer?.DisplayStage(Stages, m_currentGameStage);

        SetStateDeferredBeat(GameState.GAME_RUNNING, 4, () => {
            m_layer?.ShowScreen();
            m_layer?.SetDisplayText(m_gameList[m_currentGameIndex].StartText);
            AudioManager.PlayMusic(m_gameList[m_currentGameIndex].GameTrack);
            m_gameList[m_currentGameIndex].Start();
        });
    }

    private void RunGameHandler()
    {
        if (m_gameList[m_currentGameIndex].Running)
        {
            m_layer?.DisplayProgress(
                m_gameList[m_currentGameIndex].Duration, 
                m_gameList[m_currentGameIndex].CurrentDuration
            );
        } 
        else 
        {
            SetState(GameState.GAME_END);
        }
    }

    private void EndGameHandler()
    {
        GD.Print($"Status: {m_status.ToString()}");
        m_layer?.DisplayLives(Lives, PlayerLives);
        switch (m_status)
        {
            case GameStatus.SUCCESS:
                EmitSignal(SignalName.ScoreEvent_);
                m_layer?.SetDisplayText(m_gameList[m_currentGameIndex].EndText);
                if (m_currentGameStage++ >= Stages)
                {
                    GD.Print("YOU WIN");
                    SetState(GameState.COMPLETE);
                }
                else SetState(GameState.GAME_START);
                break;
            case GameStatus.FAILURE:
                SetState(GameState.GAME_START);
                break;
            default:
                SetState(GameState.GAME_START);
                break;
        }
    }
    
    private void CompleteHandler()
    {
        EmitSignal(SignalName.ResetPlayer_);
        SetState(GameState.LOADING);
    }
    #endregion

    #region [Miscellanious]
    public void SetGameLayer(GameLayer layer) 
    {
        this.m_layer = layer;
        LoadMicrogames();
    }
    #endregion

    public override void _Ready()
    {
        m_state = GameState.TITLE_SCREEN;
        m_stateActions = new Dictionary<GameState, Action>()
        {
            {GameState.TITLE_SCREEN, () => {}},
            {GameState.LOADING, () => {
                LoadMicrogames();
                m_status = GameStatus.UNDECIDED;
                m_currentGameStage = 0;

                m_layer?.DisplayLives(Lives, PlayerLives);
                SetState(GameState.GAME_START);
            }},
            {GameState.GAME_START, StartGameHandler},
            {GameState.GAME_RUNNING, RunGameHandler},
            {GameState.GAME_END, EndGameHandler},
            {GameState.COMPLETE, CompleteHandler},
            {GameState.PAUSED, () => {}}
        };

        m_rng = new System.Random();
    }

    public override void _Process(double delta)
    {
        HandleState();
    }
}
