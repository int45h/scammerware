using Godot;
using System;

public partial class Boga : Node3D
{
    [Export]
    public MeshInstance3D MeshFront;
    [Export]
    public MeshInstance3D MeshBack;
    [Export]
    public bool FivetailMode{
        get => m_fivetailMode;
        set {
            m_fivetailMode = value;
            SetFivetailMode();
        }
    }

    public float SquashDuration = .25f;
    public float JumpDuration   = .5f;
    public float RevertDuration = .45f;
    
    private bool m_fivetailMode = false;

    private ShaderMaterial m_shaderMatFront;
    private ShaderMaterial m_shaderMatBack;

    private Vector3 m_oldPosition;
    private Vector3 m_oldRotation;

    public Action OnAnimationComplete;

    public enum BogaState
    {
        IDLE = 0,
        SQUASHING,
        JUMPING,
        REVERTING
    }
    private BogaState m_state = BogaState.IDLE;

    private void DoSquash(float duration)
    {
        var tween = this.GetTree().CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Spring);
        tween.TweenMethod(Callable.From<float>((f) => {
            m_shaderMatFront.SetShaderParameter("squashFactor", f);
            m_shaderMatBack.SetShaderParameter("squashFactor", f);
        }), 1.0f, 0.8f, duration);

        tween.Finished += () => {
            m_shaderMatFront.SetShaderParameter("squashFactor", 1.0f);
            m_shaderMatBack.SetShaderParameter("squashFactor", 1.0f);
            SetState(BogaState.JUMPING);
        };
    }

    private void DoSpinAndJump(float duration)
    {
        float jumpHeight = 1.0f;
        m_oldPosition = this.Position;
        m_oldRotation = this.Rotation;

        var tween = this.GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenMethod(Callable.From<float>((f) => {
            this.Position = m_oldPosition.Lerp(
                m_oldPosition + (Vector3.Up*jumpHeight), 
                Mathf.Sin(Mathf.Pi*(f))
            );
            this.Rotation = m_oldRotation.Lerp(
                m_oldRotation + (Vector3.Up*Mathf.Pi), 
                f
            );
        }), 0.0f, 1.0f, duration);

        tween.Finished += () => {
            this.Position = m_oldPosition;
            
            SetState(BogaState.REVERTING);
        };
    }

    private void RevertState(float duration)
    {
        var timer = this.GetTree().CreateTimer(duration);
        timer.Timeout += () => {
            this.Rotation = m_oldRotation;
            OnAnimationComplete?.Invoke();
            SetState(BogaState.IDLE);
        };
    }

    public void SetState(BogaState newState)
    {
        if (newState == m_state)
            return;
        
        switch (newState)
        {
            case BogaState.IDLE:
                if ((Input.GetMouseButtonMask() & MouseButtonMask.Right) != 0)
                {
                    m_state = BogaState.JUMPING;
                }
                break;
            case BogaState.SQUASHING:
                DoSquash(SquashDuration);
                break;
            case BogaState.JUMPING:
                DoSpinAndJump(JumpDuration);
                break;
            case BogaState.REVERTING:
                RevertState(RevertDuration);
                break;
            default:
                m_state = BogaState.IDLE;
                break;
        }

        m_state = newState;
    }

    public void SetFivetailMode()
    {
        int mode = FivetailMode ? 1 : 0;
        m_shaderMatFront?.SetShaderParameter("fivetailMode", mode);
        m_shaderMatBack?.SetShaderParameter("fivetailMode", mode);
    }

    public override void _Ready()
    {
        if (MeshFront == null)
        {
            GD.PrintErr("MeshFront not set in inspector");
            return;
        }

        if (MeshBack == null)
        {
            GD.PrintErr("MeshBack not set in inspector");
            return;
        }

        m_shaderMatFront = (ShaderMaterial)MeshFront.MaterialOverride;
        m_shaderMatBack = (ShaderMaterial)MeshBack.MaterialOverride;
    }

    public override void _Process(double delta)
    {
        switch (m_state)
        {
            case BogaState.IDLE:
            case BogaState.SQUASHING:
            case BogaState.JUMPING:
            case BogaState.REVERTING:
            default:
                SetState(BogaState.IDLE);
                break;
        }
    }
}
