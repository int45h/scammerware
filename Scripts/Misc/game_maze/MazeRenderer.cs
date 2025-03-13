using Godot;
using System;

public partial class MazeRenderer : MultiMeshInstance2D
{
    public const int MAZE_SIZE = 9;
    private int m_instanceSize = MAZE_SIZE*MAZE_SIZE;
    private float m_tileSize = 32;
    
    private MazeGenerator m_mazeGenerator;
    private ShaderMaterial m_tileMaterial;
    
    private void InitMazeGenerator()
    {
        m_mazeGenerator = new MazeGenerator(MAZE_SIZE);
    }

    private void InitRenderer()
    {
        this.Multimesh.InstanceCount = m_instanceSize;
        this.Multimesh.VisibleInstanceCount = m_instanceSize;
        
        for (int i=0; i<m_instanceSize; i++)
        {
            int x = i%MAZE_SIZE;
            int y = i/MAZE_SIZE;

            this.Multimesh.SetInstanceTransform2D(i, new Transform2D(
                0, 
                new Vector2(x-MAZE_SIZE/2,y-MAZE_SIZE/2)*m_tileSize)
            );
        }

        m_tileMaterial = (ShaderMaterial)this.Material;
    }

    public void GenerateMaze(int start, int end)
    {
        m_mazeGenerator.GenerateMaze(start, end);
        m_tileMaterial.SetShaderParameter("tiles", m_mazeGenerator.MazeTiles);
    }

    public Vector2 GetCellPositionFromCursor()
    {
        Vector2 bounds = new Vector2(
            MAZE_SIZE*m_tileSize, 
            MAZE_SIZE*m_tileSize
        );
        Vector2 mouseCursor = ((
            this.GetGlobalMousePosition()-this.GlobalPosition
        ) + (bounds/2)) / m_tileSize;
        
        if (mouseCursor.X < 0 || mouseCursor.X > MAZE_SIZE || 
            mouseCursor.Y < 0 || mouseCursor.Y > MAZE_SIZE)
            return new Vector2(-1, -1);
        
        mouseCursor.X = Mathf.Floor(mouseCursor.X);
        mouseCursor.Y = Mathf.Floor(mouseCursor.Y);
        
        return mouseCursor;
    }

    public int GetCellIndexFromPosition(Vector2 pos)
    {
        if (pos.X < 0 || pos.Y < 0)
            return int.MaxValue;
        
        return (int)(pos.Y*MAZE_SIZE+pos.X);
    }

    public bool IsCollidingWithWall(int current, int next) =>
        m_mazeGenerator.IsCollidingWithWall(current, next);

    public override void _Ready()
    {
        InitMazeGenerator();
        InitRenderer();
        
        this.GetTree().Root.SizeChanged += () => {
            GD.Print($"Position: {this.GlobalPosition}");
        };
    }

    public override void _Process(double delta)
    {
        
    }
}
