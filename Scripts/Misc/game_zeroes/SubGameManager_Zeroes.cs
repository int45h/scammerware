using Godot;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public partial class SubGameManager_Zeroes : Node
{
    [Export]
    public Microgame Game;

    [Export]
    public TextEdit TextBox;

    private Random m_rng;
    private string m_moneyText;
    private string m_moneyTextTrimmed;
    private SceneTreeTimer m_timer;
    private SceneTreeTimer m_spamTimer;
    
    private bool m_spamming = false;
    
    private bool CheckText()
    {
        string trimmed = Regex.Replace(TextBox.Text, "[^0-9]", "");
        if (trimmed.Equals(m_moneyTextTrimmed, StringComparison.InvariantCulture))
            return true;
        
        return false;
    }

    private void AddZeroes()
    {
        TextBox.Text += "0";
        TextBox.SetCaretColumn(TextBox.Text.Length);
    }

    private void AddZeroesSpam()
    {
        if (m_spamming || Game.CurrentDuration < 4.0f)
            return;
        
        var tween = this.GetTree().CreateTween();
        int lastTimestamp = -1;
        int spamCount = m_rng.Next(5,10);

        tween.TweenMethod(Callable.From<float>((f) => {
            if (lastTimestamp == (int)f || Game.CurrentDuration < 4.0f)
                return;
            
            lastTimestamp = (int)f;
            m_spamTimer = this.GetTree().CreateTimer(.23);
            m_spamTimer.Timeout += AddZeroes;
        }), spamCount, 0, .25*spamCount);

        tween.Finished += () => {
            m_spamming = false;
        };
    }

    private void AddZeroesSpamDelayed(float duration)
    {
        if (duration < 1.0f || (m_timer != null && m_timer.TimeLeft > 0.0)) 
            return;
        
        int fuckChance = m_rng.Next(0, 100);
        if (fuckChance < 50)
            return;

        m_timer = this.GetTree().CreateTimer(4.0);
        m_timer.Timeout += () => {
            AddZeroesSpam();
        };
    }

    private void AddZeroesBulk()
    {
        StringBuilder sb = new StringBuilder();
        int amt = m_rng.Next(30, 50);
        for (int i=0; i<amt; i++)
            sb.Append("0");
        
        TextBox.Text += sb.ToString();
        TextBox.SetCaretColumn(TextBox.Text.Length);
    }

    private void InitTextBox()
    {
        float money = m_rng.Next(1000, 100000);
        string c = money.ToString("C", new CultureInfo("en-US"));
        c = c.Remove(c.Length-3, 3);

        m_moneyText = c;
        m_moneyTextTrimmed = Regex.Replace(m_moneyText, "[^0-9]", "");

        TextBox.Text = string.Format("Refund amount: {0}", c);
        AddZeroesBulk();
    }

    public void GameHandler(float delta)
    {
        Game.CurrentDuration -= delta;
        if (Game.CurrentDuration <= 0.0f)
        {
            if (CheckText())
                Game.OnComplete();
            else
                Game.OnFailed();
        }
        else AddZeroesSpamDelayed(Game.CurrentDuration);
    }

    public override void _Ready()
    {
        if (Game == null)
            return;
        
        m_rng = new Random();
        Game.OnGameStart_ += () => {
            InitTextBox();
            TextBox.GrabFocus();
        };
        Game.GameRunnerOverride += GameHandler;
    }
}
