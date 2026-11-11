using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum GameState
{
    Playing,
    Won,
    Draw
}

/// <summary>
/// Owns the turn flow, reads mouse input and updates the HUD.
/// The rules live in GameRules and all visuals live in BoardView.
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("Scene references")]
    public BoardView board;
    public Camera viewCamera;

    [Header("HUD references")]
    public TMP_Text turnText;
    public TMP_Text scoreText;
    public TMP_Text hintText;
    public Button newRoundButton;

    [Header("Player colours")]
    public Color xColor = new Color(1f, 0.54f, 0.24f);
    public Color oColor = new Color(0.22f, 0.83f, 1f);

    private const string DefaultHint =
        "Hover a square to inspect it   ·   left-click to place your mark";

    private readonly GameRules rules = new GameRules();

    private GameState state;
    private int currentPlayer;
    private int startingPlayer = 1;
    private int xScore;
    private int oScore;
    private BoardCell hoveredCell;

    public GameState State => state;
    public int CurrentPlayer => currentPlayer;
    public int XScore => xScore;
    public int OScore => oScore;

    private void Start()
    {
        if (newRoundButton != null)
            newRoundButton.onClick.AddListener(NewRound);

        NewRound();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            NewRound();

        if (state != GameState.Playing)
        {
            ClearHover();
            return;
        }

        UpdateHover();

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            hoveredCell != null)
        {
            TryPlace(hoveredCell.Coord);
        }
    }

    private void UpdateHover()
    {
        if (viewCamera == null || Mouse.current == null || PointerIsOverUI())
        {
            ClearHover();
            return;
        }

        Ray ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            BoardCell cell = hit.collider.GetComponent<BoardCell>();
            if (cell != null && rules.IsEmpty(cell.Coord))
            {
                SetHover(cell);
                return;
            }
        }

        ClearHover();
    }

    private static bool PointerIsOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void SetHover(BoardCell cell)
    {
        if (hoveredCell == cell)
            return;

        ClearHover();
        hoveredCell = cell;
        hoveredCell.SetHover(true);

        hintText.text = "Row " + (hoveredCell.Coord.y + 1) +
                        "   ·   Column " + (hoveredCell.Coord.x + 1);
    }

    private void ClearHover()
    {
        if (hoveredCell == null)
            return;

        hoveredCell.SetHover(false);
        hoveredCell = null;

        if (state == GameState.Playing)
            hintText.text = DefaultHint;
    }

    /// <summary>
    /// Places the current player's mark if the move is valid.
    /// Public so it can also be driven by tests or a future AI.
    /// </summary>
    public bool TryPlace(Vector2Int coord)
    {
        if (state != GameState.Playing)
            return false;

        if (!rules.Place(coord, currentPlayer))
            return false;

        ClearHover();
        board.ShowMark(coord, currentPlayer);

        int winner = rules.CheckWinner(out Vector2Int[] winningLine);

        if (winner != 0)
        {
            state = GameState.Won;

            if (winner == 1)
                xScore++;
            else
                oScore++;

            board.HighlightWin(winningLine);
            turnText.text = winner == 1 ? "X WINS!" : "O WINS!";
            turnText.color = winner == 1 ? xColor : oColor;
            hintText.text = "Three in a row! Click New Round or press R to play again.";
            UpdateScoreText();
            return true;
        }

        if (rules.IsFull())
        {
            state = GameState.Draw;
            turnText.text = "DRAW";
            turnText.color = Color.white;
            hintText.text = "The board is full. Click New Round or press R to play again.";
            UpdateScoreText();
            return true;
        }

        currentPlayer = 3 - currentPlayer;
        UpdateTurnText();
        return true;
    }

    public void NewRound()
    {
        rules.Reset();
        board.ClearBoard();

        hoveredCell = null;
        state = GameState.Playing;

        // Alternating the starter keeps the first-move advantage fair over
        // several rounds. X starts the very first round.
        currentPlayer = startingPlayer;
        startingPlayer = 3 - startingPlayer;

        hintText.text = DefaultHint;
        UpdateTurnText();
    }

    private void UpdateTurnText()
    {
        turnText.text = currentPlayer == 1 ? "X'S TURN" : "O'S TURN";
        turnText.color = currentPlayer == 1 ? xColor : oColor;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        string xHex = ColorUtility.ToHtmlStringRGB(xColor);
        string oHex = ColorUtility.ToHtmlStringRGB(oColor);
        scoreText.text = "<color=#" + xHex + ">X  " + xScore + "</color>" +
                         "     " +
                         "<color=#" + oHex + ">O  " + oScore + "</color>";
    }
}
