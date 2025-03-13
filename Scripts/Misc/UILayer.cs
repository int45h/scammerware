using Godot;
using System;

public partial class UILayer : CanvasLayer
{
    [Export]
    public GameText Text;

    private Sprite2D m_timerBar;
    private ShaderMaterial m_timerMaterial;

    private Label   m_labelLives,
                    m_labelStage;

    public void SetDisplayText(string text) =>
        this.Text?.SetText(text);

    public void DisplayStage(int maxStages, int stage)
    {
        m_labelStage.Text = $"Stage: {stage}/{maxStages}";
    }

    public void DisplayLives(int maxLives, int lives)
    {
        m_labelLives.Text = $"Lives: {lives}/{maxLives}";
    }

    public void DisplayProgress(float duration, float time)
    {
        float progress = Mathf.Clamp(1.0f - ((duration - time) / duration), 0, 1);
        m_timerMaterial.SetShaderParameter("progress", progress);
    }

    public override void _Ready()
    {
        m_timerBar = this.GetChild(1).GetChild<Sprite2D>(0);
        m_timerMaterial = (ShaderMaterial)m_timerBar.Material;

        var hContainer = this.GetChild(0);
        m_labelLives = hContainer.GetChild<Label>(0);
        m_labelStage = hContainer.GetChild<Label>(1);
    }

    public override void _Process(double delta)
    {
        
    }
}
