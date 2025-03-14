using Godot;
using System;
using System.Collections.Generic;

public partial class GameText : HBoxContainer
{
    private Label m_label;
    private Vector2 m_originalPosition;
    private Vector2 m_oldScreenSize;

    private int m_stepCount = 16;
    private float m_duration = .8f;
    private float m_startRadius = 200.0f;
    private List<float> offsets; 

    private System.Random m_rng;

    public void SetText(string text)
    {
        this.m_label.Text = text;
        this.Visible = true;
    }

    private void HideDelay()
    {
        var timer = this.GetTree().CreateTimer(.5f);
        timer.Timeout += () => 
        {
            this.Visible = false;
        };
    }

    private void OnScreenResized()
    {
        //var newScreenSize = this.GetViewportRect().Size;
        //Vector2 offset = m_label.GetMinimumSize() / 2.0f;
        //m_originalPosition = (m_originalPosition / m_oldScreenSize * newScreenSize) - offset;
        //m_oldScreenSize = newScreenSize;
    }

    private void OnShow()
    {
        var tween = this.GetTree().CreateTween();
        tween.Finished += HideDelay;
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.Out);

        tween.TweenMethod(Callable.From<float>((f) => {
            int idx = (int)Mathf.Floor(f / m_startRadius * m_stepCount);
            float angle = offsets[idx];

            Vector2 offset = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            );
            m_label.Position = m_originalPosition + offset * idx;
        }), m_startRadius, 0.0f, m_duration);
    }

    public override void _Ready()
    {
        m_label = this.GetChild<Label>(0);
        m_originalPosition = m_label.Position;

        m_rng = new System.Random();
        offsets = new List<float>();

        for (int i=0; i<m_stepCount; i++)
            offsets.Add(m_rng.NextSingle()*2.0f*Mathf.Pi - Mathf.Pi);

        this.VisibilityChanged += () => {
            if (this.Visible)
                OnShow();
        };

        this.Visible = false;
        this.m_oldScreenSize = this.GetViewportRect().Size;
        this.GetTree().Root.SizeChanged += OnScreenResized;
    }
}
