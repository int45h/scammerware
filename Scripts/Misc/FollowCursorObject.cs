using Godot;
using System;

public partial class FollowCursorObject : Sprite2D
{
    public virtual void OnClick()
    {
        this.Scale = Vector2.One*1.2f;
    }

    public virtual void OnRelease()
    {
        this.Scale = Vector2.One;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.GetType() == typeof(InputEventMouseButton))
        {
            var mouseEv = (InputEventMouseButton)@event;
            if ((mouseEv.ButtonMask & MouseButtonMask.Left) != 0)
            {
                OnClick();
            }
            else
            {
                OnRelease();
            }
        }
    }

    public override void _Ready()
    {
        this.YSortEnabled = true;
    }

    public override void _Process(double delta)
    {
        this.GlobalPosition = this.GetTree().Root.GetMousePosition();
    }
}
