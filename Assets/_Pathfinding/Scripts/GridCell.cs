using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    [Header("Cell Properties")]
    public int row;
    public int col;
    public bool isObstacle;
    
    [Header("Visual Settings")]
    public Color walkableColor = Color.white;
    public Color obstacleColor = Color.black;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    public Color pathColor = Color.yellow;
    
    private Image cellImage;
    private Button cellButton;
    
    public enum CellType
    {
        Walkable,
        Obstacle,
        Start,
        End,
        Path
    }
    
    private CellType currentType = CellType.Walkable;
    
    void Awake()
    {
        cellImage = GetComponent<Image>();
        if (cellImage == null)
        {
            cellImage = gameObject.AddComponent<Image>();
        }
        
        cellButton = GetComponent<Button>();
        if (cellButton == null)
        {
            cellButton = gameObject.AddComponent<Button>();
        }
        
        cellButton.onClick.AddListener(OnCellClicked);
    }
    
    public void Initialize(int cellRow, int cellCol, bool obstacle)
    {
        row = cellRow;
        col = cellCol;
        isObstacle = obstacle;
        
        SetCellType(obstacle ? CellType.Obstacle : CellType.Walkable);
        UpdateVisual();
    }
    
    public void SetCellType(CellType type)
    {
        currentType = type;
        
        switch (type)
        {
            case CellType.Walkable:
                isObstacle = false;
                break;
            case CellType.Obstacle:
                isObstacle = true;
                break;
            case CellType.Start:
            case CellType.End:
            case CellType.Path:
                isObstacle = false;
                break;
        }
        
        UpdateVisual();
    }
    
    void UpdateVisual()
    {
        if (cellImage == null) return;
        
        switch (currentType)
        {
            case CellType.Walkable:
                cellImage.color = walkableColor;
                break;
            case CellType.Obstacle:
                cellImage.color = obstacleColor;
                break;
            case CellType.Start:
                cellImage.color = startColor;
                break;
            case CellType.End:
                cellImage.color = endColor;
                break;
            case CellType.Path:
                cellImage.color = pathColor;
                break;
        }
    }
    
    void OnCellClicked()
    {
        // if (currentType == CellType.Walkable)
        // {
        //     SetCellType(CellType.Obstacle);
        // }
        // else if (currentType == CellType.Obstacle)
        // {
        //     SetCellType(CellType.Walkable);
        // }
        //
        // Debug.Log($"Cell clicked: ({row}, {col}) - Type: {currentType}");
    }
    
    public CellType GetCellType()
    {
        return currentType;
    }
    
    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(col, row);
    }
    
    public void HighlightCell(Color highlightColor, float duration = 1f)
    {
        StartCoroutine(HighlightCoroutine(highlightColor, duration));
    }
    
    private System.Collections.IEnumerator HighlightCoroutine(Color highlightColor, float duration)
    {
        Color originalColor = cellImage.color;
        cellImage.color = highlightColor;
        
        yield return new WaitForSeconds(duration);
        
        UpdateVisual();
    }
}