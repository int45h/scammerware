using Godot;
using System;

public partial class TransLayer : CanvasLayer
{
    [Export]
    public TransitionScene TransitionScene;

    public void DoTransition(bool success = true) =>
        TransitionScene.DoTransition(success);
}
