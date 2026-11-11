using UnityEngine;

/// <summary>
/// Pure rules for a normal 3 x 3 tic-tac-toe board.
/// 0 = empty, 1 = X, 2 = O.
/// This class knows nothing about visuals, so the rules are easy to read and test.
/// </summary>
public class GameRules
{
    public const int Size = 3;

    // Cells are numbered 0..8 from the top-left corner, row by row:
    //   0 1 2
    //   3 4 5
    //   6 7 8
    // Board[x, y] uses x = column and y = row.
    private readonly int[,] board = new int[Size, Size];

    // The eight ways to win: three rows, three columns and two diagonals.
    private static readonly int[][] Lines =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 }, // rows
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, // columns
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 }                     // diagonals
    };

    public int Get(Vector2Int cell)
    {
        return board[cell.x, cell.y];
    }

    public bool IsEmpty(Vector2Int cell)
    {
        return IsInside(cell) && board[cell.x, cell.y] == 0;
    }

    public bool Place(Vector2Int cell, int player)
    {
        if (!IsEmpty(cell) || (player != 1 && player != 2))
            return false;

        board[cell.x, cell.y] = player;
        return true;
    }

    /// <summary>
    /// Returns the winning player (0 if nobody won) and the three cells that
    /// make up the winning line.
    /// </summary>
    public int CheckWinner(out Vector2Int[] winningLine)
    {
        foreach (int[] line in Lines)
        {
            Vector2Int a = ToCell(line[0]);
            Vector2Int b = ToCell(line[1]);
            Vector2Int c = ToCell(line[2]);

            int player = board[a.x, a.y];
            if (player != 0 &&
                board[b.x, b.y] == player &&
                board[c.x, c.y] == player)
            {
                winningLine = new[] { a, b, c };
                return player;
            }
        }

        winningLine = null;
        return 0;
    }

    public bool IsFull()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                if (board[x, y] == 0)
                    return false;
            }
        }

        return true;
    }

    public void Reset()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                board[x, y] = 0;
            }
        }
    }

    private static Vector2Int ToCell(int index)
    {
        return new Vector2Int(index % Size, index / Size);
    }

    private static bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Size &&
               cell.y >= 0 && cell.y < Size;
    }
}
