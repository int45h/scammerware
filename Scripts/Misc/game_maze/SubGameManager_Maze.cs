using Godot;
using System;
using System.Text;
using System.Text.RegularExpressions;

public partial class SubGameManager_Maze : Node
{
    [Export]
    public Microgame Game;

    [Export]
    public MazeRenderer MazeRenderer;

    private int m_prevCell = int.MaxValue;
    private int m_currentCell = int.MaxValue;

    private int m_startCell = 0;
    private int m_endCell = MazeRenderer.MAZE_SIZE*MazeRenderer.MAZE_SIZE-1;

    private bool m_startReached = false;


    public void OnButtonPressed() => Game?.OnComplete();
    private void OnGameStart()
    {
        MazeRenderer.GenerateMaze(m_startCell, m_endCell);
    }

    public override void _Ready()
    {
        Game.OnGameStart_ += OnGameStart;
    }

    public override void _Process(double delta)
    {
        Vector2 cellPos = MazeRenderer.GetCellPositionFromCursor();
        int next = MazeRenderer.GetCellIndexFromPosition(cellPos);
        if (next != int.MaxValue)
        {
            m_prevCell = m_currentCell;
            m_currentCell = next;

            if((m_currentCell == m_startCell) && !m_startReached)
                m_startReached = true;

            if (MazeRenderer.IsCollidingWithWall(m_prevCell, m_currentCell))
            {
                GD.Print($"THUNK (Prev: {m_prevCell}, Next: {m_currentCell})");
                m_startReached = false;
            }

            if ((m_currentCell == m_endCell) && m_startReached)
            {
                Game.OnComplete();
            }
        }
    }
}
