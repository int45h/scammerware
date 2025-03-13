using Godot;
using System;
using System.Text;
using System.Text.RegularExpressions;

public partial class SubGameManager_Captcha : Node
{
    [Export]
    public Microgame Game;

    [Export]
    public CaptchaScreen Captcha;

    [Export]
    public TextEdit TextBox;

    private System.Random m_rng;
    private string m_captchaString = "";
    
    public void OnButtonPressed() => Game?.OnComplete();
    public void SubmitCode()
    {
        if (ParseCodeString(TextBox.Text))
            Game?.OnComplete();
        else GD.Print($"Invalid code ({m_captchaString} != {Captcha.CaptchaString})");
    }

    public void GenerateCaptcha()
    {
        StringBuilder sb = new StringBuilder();
        string cipher = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        for (int i=0; i<4; i++)
            sb.Append(cipher[m_rng.Next(0, cipher.Length - 1)]);

        m_captchaString = sb.ToString();
        Captcha.SetCaptchaString(m_captchaString);
    }
    
    public bool IsValidCodeString(string code)
    {
        bool valid = true;
        if (code.Length != 4)
            return false;

        foreach (char c in code)
        {
            if (c >= '0' && c <= '9')
                continue;
            
            char c_ = char.ToUpper(c);
            if (c_ >= 'A' && c_ <= 'Z')
                continue;

            valid = false;    
            break;
        }
        
        return valid;
    }
    
    public bool ParseCodeString(string code)
    {
        // Check for From Eden mentioned
        string heft = "The great heft of the man";
        if (code.Contains(heft, StringComparison.InvariantCultureIgnoreCase))
        {
            GD.Print("FROM EDEN FUCKING MENTIONED???");
            return false;
        }

        // Trim the string
        string codeTrimmed = Regex.Replace(code, @"[^\d\w]", "");
        if (!IsValidCodeString(codeTrimmed))
            return false;
        
        return Captcha.CaptchaString.Equals(
            codeTrimmed, 
            StringComparison.InvariantCulture
        );
    }

    public void OnGameStart()
    {
        TextBox.Text = "";
        GenerateCaptcha();
    }

    public override void _Ready()
    {
        if (Game == null)
        {
            GD.PrintErr("Game not set in the inspector");
            return;
        }

        Game.OnGameStart_ += OnGameStart;
        m_rng = new System.Random();

        TextBox.TextChanged += () => {
            if (TextBox.Text.Contains('\n'))
            {
                TextBox.Text = TextBox.Text.Replace("\n", "");
                SubmitCode();
            }
        };
    }
}
