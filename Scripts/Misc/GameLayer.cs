using Godot;
using System;

public partial class GameLayer : CanvasLayer
{
    public Node ChildNode;
    private SubViewport m_subViewport;
    
    [Export]
    public UILayer UILayer;

    [Export]
    public ColorRect ScreenEffects;

    [Export]
    public TransLayer TransLayer;

    public void SetDisplayText(string text) => 
        UILayer?.SetDisplayText(text);

    public void DisplayStage(int maxStages, int stage) => 
        UILayer?.DisplayStage(maxStages, stage);
    
    public void DisplayLives(int maxLives, int lives) => 
        UILayer?.DisplayLives(maxLives, lives);

    public void DisplayProgress(float duration, float time) => 
        UILayer?.DisplayProgress(duration, time);

    public void SetChild(Node child)
    {
        var currentChildren = m_subViewport.GetChildren();
        if (currentChildren.Count >= 1)
        {
            foreach (var nchild in currentChildren)
                m_subViewport.RemoveChild(nchild);
        }

        if (child.GetParent() == null)
            m_subViewport.AddChild(child);
        else 
            child.Reparent(m_subViewport);
    }

    public void ShowScreen()
    {
        if (ScreenEffects == null)
            return;
        
        var shaderMaterial = (ShaderMaterial)ScreenEffects.Material;
        shaderMaterial.SetShaderParameter("show", 1);

        TransLayer.Visible = false;
    }

    public void HideScreen(bool success = true)
    {
        if (ScreenEffects == null)
            return;
        
        var shaderMaterial = (ShaderMaterial)ScreenEffects.Material;
        shaderMaterial.SetShaderParameter("show", 0);

        TransLayer.Visible = true;
        TransLayer.DoTransition(success);
    }

    public override void _Ready()
    {
        m_subViewport = this.GetChild(0).GetChild<SubViewport>(0);

        var gameManager = this.GetNode<GameManager>("/root/GameManager");
        gameManager?.SetGameLayer(this);

        if (UILayer == null)
            GD.Print("Warning: UILayer is not set.");
        
        if (ScreenEffects == null)
            GD.Print("Warning: ScreenEffects is not set.");
    }
}
