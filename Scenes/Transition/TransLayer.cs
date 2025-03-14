using Godot;
using System;

public partial class TransLayer : CanvasLayer
{
    [Export]
    public TransitionScene TransitionScene;

    public void DoTransition() =>
        TransitionScene.DoTransition();
}
