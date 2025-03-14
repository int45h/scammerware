using Godot;
using System;

public partial class ObjectFollowManager : Node
{
    [Export]
    public Node2D DragDaddy;

    public Action OnClickAction;
    public Action OnReleaseAction;

    public virtual void OnClick()
    {
        OnClickAction?.Invoke();
    }

    public virtual void OnRelease()
    {
        OnReleaseAction?.Invoke();
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
        DragDaddy.YSortEnabled = true;        
    }

    public override void _Process(double delta)
    {
        DragDaddy.GlobalPosition = this.GetTree().Root.GetMousePosition();
    }
}
