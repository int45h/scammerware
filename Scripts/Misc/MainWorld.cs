using Godot;
using System;

public partial class MainWorld : Node2D
{
    [Export]
    public CanvasLayer TitleMenu;

    [Export]
    public UILayer UILayer;

    GameManager m_gameManager;
    
    public void OnGameComplete()
    {
        TitleMenu.Visible = true;
        UILayer.Visible = false;
        m_gameManager.SetState(GameState.TITLE_SCREEN);
    }

    public void OnPlayPressed()
    {
        TitleMenu.Visible = false;
        UILayer.Visible = true;
        m_gameManager.SetState(GameState.LOADING);
    }

    public void OnExitPressed()
    {
        this.GetTree().Quit();
    }

    public override void _Ready()
    {
        m_gameManager = this.GetNode<GameManager>("/root/GameManager");
        m_gameManager.SetState(GameState.TITLE_SCREEN);
        m_gameManager.OnGameComplete += OnGameComplete;
    }
}
