using Godot;
using System;

public partial class SubGameManager : Node
{
    [Export]
    public Microgame Game;

    public void OnButtonPressed()
    {
        Game?.OnComplete();
    }
}
