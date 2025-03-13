using Godot;
using System;
using System.Text;

public partial class CaptchaScreen : Node2D
{
    [Export]
    public Label CaptchaLabel;

    public string CaptchaString;

    public void SetCaptchaString(string code)
    {
        CaptchaString = code;
        CaptchaLabel.Text = code;
    }

    public override void _Ready()
    {

    }
}
