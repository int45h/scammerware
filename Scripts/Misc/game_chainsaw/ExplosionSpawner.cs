using Godot;
using System;

public partial class ExplosionSpawner : Node
{
    [Export(PropertyHint.File)]
    public string ExplosionPrefabPath;

    private AudioManager m_audioManager;
    private PackedScene m_explosionPrefab;

    public void SpawnExplosion(Vector2 position)
    {
        var instance = m_explosionPrefab.Instantiate<ScuffedExplosion>();
        instance.GlobalPosition = position;
        this.AddChild(instance);

        m_audioManager.PlayAudio(AudioManager.SFX_EXPLODE, "SFX");
    }

    private void LoadExplosionPrefab()
    {
        if (string.IsNullOrWhiteSpace(ExplosionPrefabPath))
        {
            GD.Print("CardPrefabPath was not set in the inspector");
            return;
        }

        var resource = GD.Load<PackedScene>(ExplosionPrefabPath);
        if (resource == null)
        {
            GD.Print($"Failed to load card prefab from path \"{ExplosionPrefabPath}\"");
            return;
        }

        m_explosionPrefab = resource;
    }

    public override void _Ready()
    {
        m_audioManager = this.GetNode<AudioManager>("/root/AudioManager");
        LoadExplosionPrefab();
    }
}
