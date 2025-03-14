using Godot;
using System;

public partial class TransitionScene : Node3D
{
    [Export]
    public Boga BogaGuy;

    [Export]
    public CameraController3D Camera;
    public enum TransitionState
    {
        IDLE = 0,
        ZOOM_OUT,
        BOGA,
        ZOOM_IN
    };

    private AudioManager m_audioManager;
    private TransitionState m_transState = TransitionState.IDLE;
    private bool m_gameWin = true;

    // TO-DO: make these actually sync up with the beat
    public void DoCameraZoomOut()
    {
        float duration = (float)m_audioManager.BeatCountToDuration(2);
        Camera.TransformTransitionOut(duration);
    }

    // TO-DO: make these actually sync up with the beat
    public void DoCameraZoomIn()
    {
        float duration = (float)m_audioManager.BeatCountToDuration(2);
        Camera.TransformTransitionIn(duration);
        var timer = this.GetTree().CreateTimer(duration);
        timer.Timeout += () => {
            SetState(TransitionState.IDLE);
        };
    }

    public void DoBoga()
    {
        BogaGuy.SetState(Boga.BogaState.SQUASHING);
    }

    public void DoTransition(bool gameWin = true)
    {
        m_gameWin = gameWin;
        SetState(TransitionState.ZOOM_OUT);
    }

    public void SetState(TransitionState newState)
    {
        if (newState == m_transState)
            return;
        
        switch (newState)
        {
            case TransitionState.IDLE:
                break;
            case TransitionState.ZOOM_OUT:
                DoCameraZoomOut();
                break;
            case TransitionState.BOGA:
                DoBoga();
                break;
            case TransitionState.ZOOM_IN:
                DoCameraZoomIn();
                break;
            default:
                break;
        }

        m_transState = newState;
    }

    public override void _Ready()
    {
        m_audioManager = this.GetNode<AudioManager>("/root/AudioManager");

        BogaGuy.SquashDuration = (float)AudioManager.BeatCountToDuration(.4, 170);
        BogaGuy.JumpDuration = (float)AudioManager.BeatCountToDuration(1.2, 170);
        BogaGuy.RevertDuration = (float)AudioManager.BeatCountToDuration(.9, 170);
    }

    public override void _Process(double delta)
    {
        double currentBeat = 0;
        switch (m_transState)
        {
            case TransitionState.IDLE:
                break;
            case TransitionState.ZOOM_OUT:
                currentBeat = m_audioManager.GetCurrentBeat();
                if (m_gameWin)
                {
                    if (currentBeat >= 1.35)
                        SetState(TransitionState.BOGA);
                }
                else
                {
                    if (Mathf.Floor(currentBeat) >= 12)
                    {
                        GD.Print(currentBeat);
                        SetState(TransitionState.ZOOM_IN);
                    }
                }
                break;
            case TransitionState.BOGA:
                currentBeat = m_audioManager.GetCurrentBeat();
                if (Mathf.Floor(currentBeat) >= 12)
                {
                    GD.Print(currentBeat);
                    SetState(TransitionState.ZOOM_IN);
                }
                break;
            case TransitionState.ZOOM_IN:
                break;
            default:
                m_transState = TransitionState.IDLE;
                break;
        }
    }
}
