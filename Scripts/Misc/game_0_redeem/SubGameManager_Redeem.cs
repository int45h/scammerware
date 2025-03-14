using Godot;
using Microsoft.VisualBasic;
using System;
using System.Text;
using System.Text.RegularExpressions;

public partial class SubGameManager_Redeem : Node
{
    [Export]
    public Microgame Game;

    [Export]
    public Label CodeLabel;

    [Export]
    public TextEdit TextBox;

    private AudioManager m_audioManager;
    private Random m_rng;
    private string m_codeString;

    private bool m_playingSound = false;

    public void OnButtonPressed() => SubmitCode();
    public void SubmitCode()
    {
        if (TextBox == null)
        {
            GD.PrintErr("Text Box was not set in the inspector");
            return;
        }

        if (ParseCodeString(TextBox.Text))
            Game?.OnComplete();
        else GD.Print("Invalid code");
    }

    public void GenerateCodeString()
    {
        StringBuilder sb = new StringBuilder();
        string cipher = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        for (int i=0; i<16; i++)
            sb.Append(cipher[m_rng.Next(0, cipher.Length - 1)]);

        m_codeString = sb.ToString();
    }
    
    public bool IsValidCodeString(string code)
    {
        bool valid = true;
        if (code.Length != 16)
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
        return IsValidCodeString(codeTrimmed);
    }

    private void SetCodeStringLabel()
    {
        if (CodeLabel == null)
        {
            GD.PrintErr("Code label was not set in the inspector");
            return;
        }

        CodeLabel.Text = string.Format(
            "Code: {0}-{1}-{2}-{3}",
            m_codeString.Substring(0,4),
            m_codeString.Substring(4,4),
            m_codeString.Substring(8,4),
            m_codeString.Substring(12,4)
        );
    }

    private void ClearTextbox()
    {
        if (TextBox == null)
        {
            GD.PrintErr("Text Box was not set in the inspector");
            return;
        }
        TextBox.Text = "";
    }

    private void OnGameStart()
    {
        GenerateCodeString();
        SetCodeStringLabel();
        ClearTextbox();
    }

    private void OnGameRunning(float delta)
    {
        if (!Game.Running)
            return;

        Game.CurrentDuration -= (float)delta;
        if (Game.CurrentDuration >= 2.0f && !m_playingSound)
        {
            m_playingSound = true;
            PullRandomRedeemSound();
            
            var timer = this.GetTree().CreateTimer(4.0f);
            timer.Timeout += () => {
                m_playingSound = false;
            };
        }

        if (Game.CurrentDuration <= 0.0f)
            Game.OnFailed();
    }

    private void PullRandomRedeemSound()
    {
        int index = m_rng.Next(0,24)%8;
        string sfx = AudioManager.SFX_REDEEM1;
        switch (index)
        {
            case 0: sfx = AudioManager.SFX_REDEEM1; break;
            case 1: sfx = AudioManager.SFX_REDEEM2; break;
            case 2: sfx = AudioManager.SFX_REDEEM3; break;
            case 3: sfx = AudioManager.SFX_REDEEM4; break;
            case 4: sfx = AudioManager.SFX_REDEEM5; break;
            case 5: sfx = AudioManager.SFX_REDEEM6; break;
            case 6: sfx = AudioManager.SFX_REDEEM7; break;
            case 7: sfx = AudioManager.SFX_REDEEM8; break;
        }

        m_audioManager.PlayAudio(sfx, AudioManager.SFX_BUS);
    }

    public override void _Ready()
    {
        m_rng = new Random();
        m_audioManager = this.GetNode<AudioManager>("/root/AudioManager");

        if (Game == null)
        {
            GD.PrintErr("Game not set in the inspector");
            return;
        }

        Game.OnGameStart_ += OnGameStart;
        if (TextBox == null)
        {
            GD.PrintErr("Text Box was not set in the inspector");
            return;
        }

        TextBox.TextChanged += () => {
            if (TextBox.Text.Contains('\n'))
            {
                TextBox.Text = TextBox.Text.Replace("\n", "");
                SubmitCode();
            }
        };

        Game.GameRunnerOverride += OnGameRunning;
    }
}
