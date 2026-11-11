using UnityEngine;

/// <summary>
/// One clickable square of the 3 x 3 board. It remembers its grid coordinate
/// and handles the hover colour of its tile.
/// </summary>
public class BoardCell : MonoBehaviour
{
    public Vector2Int Coord { get; private set; }
    public GameObject Mark { get; set; }

    private Material tileMaterial;
    private Color normalColor;
    private Color highlightColor;

    public void Init(Vector2Int coord, Material material, Color normal, Color highlight)
    {
        Coord = coord;
        tileMaterial = material;
        normalColor = normal;
        highlightColor = highlight;
        SetHover(false);
    }

    public void SetHover(bool hovered)
    {
        if (tileMaterial == null)
            return;

        if (hovered)
        {
            tileMaterial.SetColor("_BaseColor", highlightColor);
            tileMaterial.SetColor("_EmissionColor", highlightColor * 0.35f);
        }
        else
        {
            tileMaterial.SetColor("_BaseColor", normalColor);
            tileMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    public void ResetVisual()
    {
        Mark = null;
        SetHover(false);
    }
}
