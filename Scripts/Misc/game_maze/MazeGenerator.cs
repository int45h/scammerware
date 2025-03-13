using Godot;
using System;

public class MazeGenerator
{
    private enum MazeWall { RIGHT = 1, LEFT = 2, BOTTOM = 4, TOP = 8, }
    private enum MazeWallDirection { RIGHT = 0, LEFT = 1, BOTTOM = 2, TOP = 3, }

    private System.Random m_rng = new System.Random();

    public int MazeSize = 7;
    public int[] MazeTiles;
    
    public MazeGenerator()
    {
        MazeTiles = new int[MazeSize*MazeSize];
    }

    public MazeGenerator(int mazeSize)
    {
        this.MazeSize = mazeSize;
        MazeTiles = new int[MazeSize*MazeSize];
    }

    public bool IsCollidingWithWall(int current, int next)
    {
        if (current == next) return false;
        if (current == int.MaxValue || next == int.MaxValue) return false;

        int cx = current % MazeSize;
        int cy = current / MazeSize;
        
        bool neighborFound = false;
        int wallDirection = int.MaxValue;

        for (int i=0; i<4; i++)
        {
            switch ((MazeWallDirection)i)
            {
                case MazeWallDirection.RIGHT: 
                    if (cx>=(MazeSize-1)) break;
                    neighborFound |= (cy*MazeSize+(cx+1)) == next;
                    wallDirection = (int)MazeWall.RIGHT;
                    break;
                case MazeWallDirection.LEFT: 
                    if (cx<=0) break;
                    neighborFound |= (cy*MazeSize+(cx-1)) == next;
                    wallDirection = (int)MazeWall.LEFT;
                    break;
                case MazeWallDirection.BOTTOM: 
                    if (cy>=(MazeSize-1)) break;
                    neighborFound |= ((cy+1)*MazeSize+cx) == next;
                    wallDirection = (int)MazeWall.BOTTOM;
                    break;
                case MazeWallDirection.TOP: 
                    if (cy<=0) break;
                    neighborFound |= ((cy-1)*MazeSize+cx) == next;
                    wallDirection = (int)MazeWall.TOP;
                    break;
            }

            if (neighborFound)
                break;
        }

        if (!neighborFound)
            return false;

        return (MazeTiles[current] & wallDirection) > 0;
    }

    bool IsVisited(int cell) => (MazeTiles[cell] & 16) > 0;
    void MarkVisited(int cell) => MazeTiles[cell] |= 16;
    void RemoveWall(MazeWall wall, int cell) => 
        MazeTiles[cell]=(MazeTiles[cell] & ((int)wall^0xFF)) & 0xFF;

    void RemoveWall(MazeWallDirection dir, int start, int end)
    {
        switch (dir)
        {
            case MazeWallDirection.RIGHT:   RemoveWall(MazeWall.RIGHT, start); RemoveWall(MazeWall.LEFT, end); break;
            case MazeWallDirection.LEFT:    RemoveWall(MazeWall.LEFT, start); RemoveWall(MazeWall.RIGHT, end); break;
            case MazeWallDirection.BOTTOM:  RemoveWall(MazeWall.BOTTOM, start); RemoveWall(MazeWall.TOP, end); break;
            case MazeWallDirection.TOP:     RemoveWall(MazeWall.TOP, start); RemoveWall(MazeWall.BOTTOM, end); break;
        }
    }

    public void ClearMaze()
    {
        for (int i=0; i<MazeTiles.Length; i++)
            MazeTiles[i] = 0xF;
    }

    public void BuildMaze(int current_cell)
    {
        MarkVisited(current_cell);
        
        int cx = current_cell % MazeSize;
        int cy = current_cell / MazeSize;
        int randomIndex = m_rng.Next(0,3);
        int nextIndex = 0;

        for (int i=0; i<4; i++)
        {
            randomIndex = (randomIndex+1)&3;
            switch ((MazeWallDirection)randomIndex)
            {
                case MazeWallDirection.RIGHT: 
                    if (cx>=(MazeSize-1)) break;
                    nextIndex = cy*MazeSize+(cx+1);
                    if (IsVisited(nextIndex)) break;
                    goto success;
                case MazeWallDirection.LEFT: 
                    if (cx<=0) break;
                    nextIndex = cy*MazeSize+(cx-1);
                    if (IsVisited(nextIndex)) break;
                    goto success;
                case MazeWallDirection.BOTTOM: 
                    if (cy>=(MazeSize-1)) break;
                    nextIndex = (cy+1)*MazeSize+cx;
                    if (IsVisited(nextIndex)) break;
                    goto success;
                case MazeWallDirection.TOP: 
                    if (cy<=0) break;
                    nextIndex = (cy-1)*MazeSize+cx;
                    if (IsVisited(nextIndex)) break;
                    goto success;
            }
            continue;
            success:
                RemoveWall((MazeWallDirection)randomIndex, current_cell, nextIndex);
                BuildMaze(nextIndex);
        }
    }

    public void GenerateMaze(int start_cell, int end_cell)
    {
        ClearMaze();
        BuildMaze(start_cell);
        RemoveWall(MazeWall.TOP, start_cell);
        RemoveWall(MazeWall.BOTTOM, end_cell);
    }
}
