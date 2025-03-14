using Godot;
using System;

public partial class ScuffedExplosion : AnimatedSprite2D
{
    public override void _Ready()
    {
        this.Play();
        this.AnimationFinished += () => {
            this.GetParent().RemoveChild(this);
            this.Dispose();
        };
    }
}
