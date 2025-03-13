using Godot;
using System;

public partial class Draggable : Sprite2D
{
    private bool m_isDragging = false;

    public bool IsDragging { get => m_isDragging; }

    public virtual void OnDragging() 
    {
        this.Scale = new Vector2(1.2f, 1.2f); 
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
            if ((mouseEv.ButtonMask & MouseButtonMask.Left) == 0)
            {
                m_isDragging = false;
                this.ZIndex = 0;

                // Do release events here
                OnRelease();
                return;
            }

            // Check if the mouse is intersecting with the sprite's bounding rectangle
            var bounds = this.GetRect();
            var position = this.GetTree().Root.GetMousePosition();
                
            m_isDragging = !(
                position.X < (this.GlobalPosition.X + bounds.Position.X) || 
                position.X > (this.GlobalPosition.X + bounds.Position.X + bounds.Size.X) || 
                position.Y < (this.GlobalPosition.Y + bounds.Position.Y) || 
                position.Y > (this.GlobalPosition.Y + bounds.Position.Y + bounds.Size.Y)
            );

            // Do dragging events here
            if (m_isDragging)
            {
                this.ZIndex = 1;
                OnDragging();
            }
        }
    }

    public override void _Ready()
    {
        this.YSortEnabled = true;
    }

    public override void _Process(double delta)
    {
        if (!m_isDragging)
            return;
        
        this.GlobalPosition = this.GetTree().Root.GetMousePosition();
    }
}
