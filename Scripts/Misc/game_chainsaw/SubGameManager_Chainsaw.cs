using Godot;
using System;

public partial class SubGameManager_Chainsaw : Node
{
    [Export]
    public Microgame Game;

    [Export]
    public ObjectFollowManager FollowManager;

    [Export]
    public Destructables Destructables;

    [Export]
    public ExplosionSpawner Spawner;

    bool m_isClicking = false;

    private void OnClick()
    {
        m_isClicking = true;

        var daddy = FollowManager.DragDaddy;
        daddy.Scale = Vector2.One * 1.2f;
        
        string daddy_class = daddy.GetClass().ToString();
        if (typeof(AnimatedSprite2D).ToString().Contains(daddy_class))
        {
            ((AnimatedSprite2D)daddy).SpeedScale = 1.0f;
        }
    }

    private void OnRelease()
    {
        m_isClicking = false;

        var daddy = FollowManager.DragDaddy;
        daddy.Scale = Vector2.One;
        
        string daddy_class = daddy.GetClass().ToString();
        if (typeof(AnimatedSprite2D).ToString().Contains(daddy_class))
        {
            ((AnimatedSprite2D)daddy).SpeedScale = 0.25f;
        }
        
    }

    private void InitDragDaddy()
    {
        var daddy = FollowManager.DragDaddy;
        string daddy_class = daddy.GetClass().ToString();
        if (typeof(AnimatedSprite2D).ToString().Contains(daddy_class))
        {
            ((AnimatedSprite2D)daddy).SpeedScale = 0.25f;
            ((AnimatedSprite2D)daddy).Play();
        }
    }

    private bool CollidesWithObject(Sprite2D obj)
    {
        var bounds = obj.GetRect();
        var position = FollowManager.DragDaddy.GlobalPosition;

        return !(
            position.X < (obj.GlobalPosition.X + bounds.Position.X) || 
            position.X > (obj.GlobalPosition.X + bounds.Position.X + bounds.Size.X) || 
            position.Y < (obj.GlobalPosition.Y + bounds.Position.Y) || 
            position.Y > (obj.GlobalPosition.Y + bounds.Position.Y + bounds.Size.Y)
        );
    }

    private void HandleCollisions()
    {
        foreach (var child in Destructables.GetChildren())
        {
            string child_class = child.GetClass();
            if (!typeof(Sprite2D).ToString().Contains(child_class))
                continue;
            
            if (CollidesWithObject((Sprite2D)child))
            {
                Spawner.SpawnExplosion(((Sprite2D)child).GlobalPosition);
                Destructables.RemoveChild(child);
                child.Dispose();
            }
        }
    }

    private void OnGameStart()
    {
        Destructables.GenerateCards();
    }

    public override void _Ready()
    {
        if (Game != null)
            Game.OnGameStart_ += OnGameStart;
        
        FollowManager.OnClickAction += OnClick;
        FollowManager.OnReleaseAction += OnRelease;

        InitDragDaddy();
    }

    public override void _Process(double delta)
    {
        if (!m_isClicking)
            return;
        
        HandleCollisions();
        if (Destructables.GetChildCount() < 1)
            Game?.OnComplete();
    }
}
