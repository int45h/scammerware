using Godot;
using System;

public partial class CameraController3D : Camera3D
{
    [ExportGroup("Transform A")]
    [Export]
    public Vector3 Position_A;
    [Export]
    public Vector3 Rotation_A;
    
    [ExportGroup("Transform B")]
    [Export]
    public Vector3 Position_B;
    [Export]
    public Vector3 Rotation_B;

    private Transform3D m_activeTransform;
    private Transform3D m_targetTransform;

    private Tween m_tween;
    private bool m_translating = false;

    public bool Translating
    {
        get => m_translating;
    }

    public Action<float> OnTranslating;
    public Action OnTranslatingFinish;

    public void TransformTransitionIn(float duration)
    {
        m_tween = this.GetTree().CreateTween();
        m_tween.SetEase(Tween.EaseType.In);
        m_tween.SetTrans(Tween.TransitionType.Cubic);
        m_tween.TweenMethod(Callable.From<float>((f) => {
            OnTranslating?.Invoke(f);
            this.Transform = m_activeTransform.InterpolateWith(m_targetTransform, f);
        }), 0.0f, 1.0f, duration);

        m_tween.Finished += () => {
            GD.Print("Transition finished");
            m_translating = false;
            
            var tmp = m_activeTransform;
            m_activeTransform = m_targetTransform;
            m_targetTransform = tmp;
            
            OnTranslatingFinish?.Invoke();
        };
    }

    public void TransformTransitionOut(float duration)
    {
        m_tween = this.GetTree().CreateTween();
        m_tween.SetEase(Tween.EaseType.Out);
        m_tween.SetTrans(Tween.TransitionType.Cubic);
        m_tween.TweenMethod(Callable.From<float>((f) => {
            OnTranslating?.Invoke(f);
            this.Transform = m_activeTransform.InterpolateWith(m_targetTransform, f);
        }), 0.0f, 1.0f, duration);

        m_tween.Finished += () => {
            GD.Print("Transition finished");
            m_translating = false;
            
            var tmp = m_activeTransform;
            m_activeTransform = m_targetTransform;
            m_targetTransform = tmp;
            
            OnTranslatingFinish?.Invoke();
        };
    }

    public override void _Ready()
    {
        m_activeTransform = new Transform3D(
            Basis.FromEuler(Rotation_B*Mathf.Pi/180.0f, EulerOrder.Xyz), 
            Position_B
        );
        m_targetTransform = new Transform3D(
            Basis.FromEuler(Rotation_A*Mathf.Pi/180.0f, EulerOrder.Xyz), 
            Position_A
        );
        this.Transform = m_activeTransform;
    }

    public override void _Process(double delta)
    {

    }
}
