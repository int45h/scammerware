using Godot;
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

    private Random m_rng;
    private string m_codeString;

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

    public override void _Ready()
    {
        m_rng = new Random();
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
    }
}
