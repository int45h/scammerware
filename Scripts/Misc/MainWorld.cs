using Godot;
using System;

public partial class MainWorld : Node2D
{
    GameManager m_gameManager;
    public override void _Ready()
    {
        m_gameManager = this.GetNode<GameManager>("/root/GameManager");
        m_gameManager.SetState(GameState.LOADING);
    }
}
