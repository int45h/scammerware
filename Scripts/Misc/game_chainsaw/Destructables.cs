using Godot;
using System;

public partial class Destructables : Node
{
    [Export(PropertyHint.File)]
    public string CardPrefabPath;
    
    [Export]
    public Vector2 ScreenMargin = Vector2.Zero;

    Random m_rng;
    PackedScene m_cardPrefab;

    private void RemoveChildren()
    {
        foreach (var child in this.GetChildren())
        {
            this.RemoveChild(child);
            child.Dispose();
        }
    }

    private void LoadCardPrefab()
    {
        if (string.IsNullOrWhiteSpace(CardPrefabPath))
        {
            GD.Print("CardPrefabPath was not set in the inspector");
            return;
        }

        var resource = GD.Load<PackedScene>(CardPrefabPath);
        if (resource == null)
        {
            GD.Print($"Failed to load card prefab from path \"{CardPrefabPath}\"");
            return;
        }

        m_cardPrefab = resource;
    }

    private Vector2 GenerateRandomPointInScreen()
    {
        var bounds = this.GetViewport().GetVisibleRect().Size;
        return new Vector2(
            Mathf.Clamp(m_rng.Next(0, (int)bounds.X), ScreenMargin.X, bounds.X - ScreenMargin.X),
            Mathf.Clamp(m_rng.Next(0, (int)bounds.Y), ScreenMargin.Y, bounds.Y - ScreenMargin.Y)
        );
    }

    public void GenerateCards()
    {
        if (this.GetChildCount() > 0)
            RemoveChildren();
        
        for (int i=0; i<5; i++)
        {
            var instance = m_cardPrefab.Instantiate<Sprite2D>();
            this.AddChild(instance);
            instance.GlobalPosition = GenerateRandomPointInScreen();
        }
    }

    public override void _Ready()
    {
        m_rng = new Random();
        LoadCardPrefab();
    }
}
