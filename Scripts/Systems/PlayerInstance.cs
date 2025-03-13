using Godot;
using System;

public partial class PlayerInstance : Node
{
    PlayerState m_state;
    GameManager m_manager;

    private void AddToScore(float score) => m_state.Score += score;
    private void SubtractLife() 
    {
        m_state.Lives--;
        m_manager.PlayerLives = m_state.Lives;
        
        if (m_state.Lives <= 0)
        {
            m_manager.OnPlayerDeath();
        }
    }
    private void ResetState()
    {
        m_state.Score = 0;
        m_state.Lives = 3;

        m_manager.Lives = 3;
        m_manager.PlayerLives = 3;
    }

    public override void _Ready()
    {
        m_state = new PlayerState();
        m_manager = this.GetNode<GameManager>("/root/GameManager");

        m_manager.ScoreEvent_ += AddToScore;
        m_manager.LifeEvent_ += SubtractLife;
        m_manager.ResetPlayer_ += ResetState;

        ResetState();
    }

    public override void _Process(double delta)
    {
        
    }
}
