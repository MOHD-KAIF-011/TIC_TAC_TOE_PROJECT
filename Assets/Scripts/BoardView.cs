using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds and animates the flat 3 x 3 board and its 3D marks.
/// It does not know the rules; the GameController tells it when to show a
/// mark or highlight a win.
/// </summary>
public class BoardView : MonoBehaviour
{
    [Header("Board layout")]
    public float cellSpacing = 1.4f;
    public float cellSize = 1.08f;

    [Header("Materials")]
    public Material slotMaterial;
    public Material frameMaterial;
    public Material floorMaterial;
    public Material playerXMaterial;
    public Material playerOMaterial;
    public Material winMaterial;

    [Header("Colours")]
    public Color slotColor = new Color(0.34f, 0.45f, 0.60f);
    public Color slotHoverColor = new Color(0.48f, 0.74f, 1f);
    public Color frameColor = new Color(0.45f, 0.58f, 0.76f);
    public Color winColor = new Color(1f, 0.78f, 0.3f);

    private readonly BoardCell[,] cells = new BoardCell[GameRules.Size, GameRules.Size];
    private readonly List<GameObject> marks = new List<GameObject>();

    private const float MarkHeight = 0.12f;
    private const float LineThickness = 0.09f;

    private GameObject winLine;
    private Material winLineMaterial;

    private void Awake()
    {
        BuildFloor();
        BuildBoardBase();
        BuildCells();

        if (winMaterial != null)
        {
            winLineMaterial = new Material(winMaterial);
            winLineMaterial.name = "Win Line (runtime)";
            winLineMaterial.EnableKeyword("_EMISSION");
        }
    }

    private void BuildFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(transform, false);
        floor.transform.localPosition = new Vector3(0f, -1.2f, 0f);
        floor.transform.localScale = new Vector3(8f, 1f, 8f);

        Collider floorCollider = floor.GetComponent<Collider>();
        if (floorCollider != null)
        {
            floorCollider.enabled = false;
            Destroy(floorCollider);
        }

        if (floorMaterial != null)
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
    }

    private void BuildBoardBase()
    {
        float halfSpan = (GameRules.Size - 1) * cellSpacing * 0.5f + cellSize * 0.5f;
        float baseSize = halfSpan * 2f + 0.5f;

        Material baseMaterial = new Material(frameMaterial);
        baseMaterial.SetColor("_BaseColor", frameColor * 0.42f);
        baseMaterial.name = "Board Base (runtime)";

        // Thick slab under the tiles gives the board its 3D body.
        // Its top sits slightly below the tile tops to avoid z-fighting.
        CreateCube(
            "Board Base",
            transform,
            new Vector3(0f, -0.30f, 0f),
            new Vector3(baseSize, 0.50f, baseSize),
            baseMaterial);

        // Raised rim around the play area.
        const float rimThickness = 0.12f;
        float rimOffset = halfSpan + 0.08f;
        float rimLength = (rimOffset + rimThickness * 0.5f) * 2f;
        float rimY = 0f;
        CreateCube("Rim North", transform, new Vector3(0f, rimY, rimOffset),
            new Vector3(rimLength, 0.1f, rimThickness), frameMaterial);
        CreateCube("Rim South", transform, new Vector3(0f, rimY, -rimOffset),
            new Vector3(rimLength, 0.1f, rimThickness), frameMaterial);
        CreateCube("Rim East", transform, new Vector3(rimOffset, rimY, 0f),
            new Vector3(rimThickness, 0.1f, rimLength), frameMaterial);
        CreateCube("Rim West", transform, new Vector3(-rimOffset, rimY, 0f),
            new Vector3(rimThickness, 0.1f, rimLength), frameMaterial);

        // The classic # grid lines between the tiles.
        float gridLength = (GameRules.Size - 1) * cellSpacing + cellSize;
        for (int i = 1; i < GameRules.Size; i++)
        {
            float offset = (i - GameRules.Size * 0.5f) * cellSpacing;
            CreateCube("Grid Vertical " + i, transform,
                new Vector3(offset, 0.055f, 0f),
                new Vector3(0.07f, 0.05f, gridLength), frameMaterial);
            CreateCube("Grid Horizontal " + i, transform,
                new Vector3(0f, 0.055f, offset),
                new Vector3(gridLength, 0.05f, 0.07f), frameMaterial);
        }
    }

    private void BuildCells()
    {
        for (int row = 0; row < GameRules.Size; row++)
        {
            for (int column = 0; column < GameRules.Size; column++)
            {
                Vector2Int coord = new Vector2Int(column, row);
                Vector3 localPosition = new Vector3(
                    (column - 1) * cellSpacing,
                    0f,
                    (row - 1) * cellSpacing);

                GameObject cellObject = new GameObject("Cell " + (char)('A' + column) + (row + 1));
                cellObject.transform.SetParent(transform, false);
                cellObject.transform.localPosition = localPosition;

                BoxCollider collider = cellObject.AddComponent<BoxCollider>();
                collider.center = new Vector3(0f, 0.18f, 0f);
                collider.size = new Vector3(cellSize + 0.16f, 0.44f, cellSize + 0.16f);

                Material tileMaterial = new Material(slotMaterial);
                tileMaterial.SetColor("_BaseColor", slotColor);
                tileMaterial.EnableKeyword("_EMISSION");
                tileMaterial.SetColor("_EmissionColor", Color.black);
                tileMaterial.name = "Cell " + column + row;

                CreateCube(
                    "Tile",
                    cellObject.transform,
                    new Vector3(0f, -0.04f, 0f),
                    new Vector3(cellSize, 0.08f, cellSize),
                    tileMaterial);

                BoardCell cell = cellObject.AddComponent<BoardCell>();
                cell.Init(coord, tileMaterial, slotColor, slotHoverColor);
                cells[column, row] = cell;
            }
        }
    }

    private GameObject CreateCube(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;

        Collider collider = cube.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            Destroy(collider);
        }

        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = localScale;
        cube.transform.localRotation = Quaternion.identity;

        if (material != null)
            cube.GetComponent<Renderer>().sharedMaterial = material;

        return cube;
    }

    public void ShowMark(Vector2Int coord, int player)
    {
        BoardCell cell = cells[coord.x, coord.y];
        GameObject mark = player == 1 ? BuildX(cell.transform) : BuildO(cell.transform);

        cell.Mark = mark;
        marks.Add(mark);
        StartCoroutine(PopIn(mark));
    }

    /// <summary>X is two crossed, extruded bars.</summary>
    private GameObject BuildX(Transform parent)
    {
        GameObject mark = new GameObject("X");
        mark.transform.SetParent(parent, false);
        mark.transform.localPosition = new Vector3(0f, MarkHeight, 0f);

        CreateCube("X Bar 1", mark.transform, Vector3.zero,
            new Vector3(1.05f, 0.2f, 0.22f), playerXMaterial).transform.localRotation =
            Quaternion.Euler(0f, 45f, 0f);

        CreateCube("X Bar 2", mark.transform, Vector3.zero,
            new Vector3(1.05f, 0.2f, 0.22f), playerXMaterial).transform.localRotation =
            Quaternion.Euler(0f, -45f, 0f);

        return mark;
    }

    /// <summary>O is a ring built from small blocks arranged in a circle.</summary>
    private GameObject BuildO(Transform parent)
    {
        GameObject mark = new GameObject("O");
        mark.transform.SetParent(parent, false);
        mark.transform.localPosition = new Vector3(0f, MarkHeight, 0f);

        const int segments = 16;
        const float radius = 0.38f;
        const float segmentLength = 0.22f;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            GameObject segment = CreateCube("O Segment", mark.transform, position,
                new Vector3(segmentLength, 0.2f, 0.15f), playerOMaterial);

            // Rotate each block so its long side follows the circle.
            segment.transform.localRotation = Quaternion.Euler(0f, 90f - angle * Mathf.Rad2Deg, 0f);
        }

        return mark;
    }

    private IEnumerator PopIn(GameObject mark)
    {
        float duration = 0.16f;
        float time = 0f;

        while (time < duration)
        {
            if (mark == null)
                yield break;

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / duration);
            float scale = Mathf.Sin(progress * Mathf.PI * 0.5f);
            mark.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        if (mark != null)
            mark.transform.localScale = Vector3.one;
    }

    public void HighlightWin(Vector2Int[] line)
    {
        if (line == null || line.Length < 3 || winLineMaterial == null)
            return;

        Vector3 start = CellCentre(line[0]);
        Vector3 end = CellCentre(line[2]);
        Vector3 direction = end - start;

        GameObject lineObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lineObject.name = "Winning Line";

        Collider collider = lineObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            Destroy(collider);
        }

        lineObject.transform.SetParent(transform, false);
        lineObject.transform.position = (start + end) * 0.5f;
        lineObject.transform.up = direction.normalized;
        lineObject.transform.localScale = new Vector3(LineThickness, direction.magnitude * 0.5f, LineThickness);

        winLineMaterial.SetColor("_BaseColor", winColor);
        winLineMaterial.SetColor("_EmissionColor", winColor * 2f);
        lineObject.GetComponent<Renderer>().sharedMaterial = winLineMaterial;

        winLine = lineObject;
        StartCoroutine(PulseWin());
    }

    private IEnumerator PulseWin()
    {
        while (winLine != null)
        {
            float pulse = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
            winLineMaterial.SetColor("_EmissionColor", winColor * Mathf.Lerp(1.5f, 3.5f, pulse));

            Vector3 scale = winLine.transform.localScale;
            scale.x = Mathf.Lerp(LineThickness, LineThickness * 1.8f, pulse);
            scale.z = scale.x;
            winLine.transform.localScale = scale;

            yield return null;
        }
    }

    private Vector3 CellCentre(Vector2Int coord)
    {
        return cells[coord.x, coord.y].transform.position + Vector3.up * 0.42f;
    }

    public void ClearBoard()
    {
        StopAllCoroutines();

        foreach (GameObject mark in marks)
        {
            if (mark != null)
                Destroy(mark);
        }
        marks.Clear();

        if (winLine != null)
        {
            Destroy(winLine);
            winLine = null;
        }

        foreach (BoardCell cell in cells)
            cell.ResetVisual();
    }
}
