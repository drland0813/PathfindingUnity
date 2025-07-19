using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 10;
    [SerializeField] private int cols = 10;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float spacing = 2f;
    
    [Header("Components")]
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rectTransform;
    private Image backgroundImage;
    
    [Header("Grid Data")]
    private GameObject[,] gridCells;
    private bool[,] gridData; // true = obstacle, false = walkable
    
    [Header("Start/End Points")]
    private Vector2Int startPosition;
    private Vector2Int endPosition;
    
    void Start()
    {
        InitializeComponents();
        GenerateRandomGrid();
    }
    
    void InitializeComponents()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        backgroundImage = GetComponent<Image>();
        
        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = gameObject.AddComponent<GridLayoutGroup>();
        }
        
        if (backgroundImage == null)
        {
            backgroundImage = gameObject.AddComponent<Image>();
        }
    }
    
    void GenerateRandomGrid()
    {
        ClearGrid();
        
        gridData = new bool[rows, cols];
        gridCells = new GameObject[rows, cols];
        
        CalculateGridLayout();
        GenerateRandomObstacles();
        CreateVisualGrid();
        SelectRandomStartAndEnd();
    }
    
    void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
    
    void CalculateGridLayout()
    {
        Vector2 containerSize = rectTransform.sizeDelta;
        
        float availableWidth = containerSize.x - (spacing * (cols - 1));
        float availableHeight = containerSize.y - (spacing * (rows - 1));
        
        float cellWidth = availableWidth / cols;
        float cellHeight = availableHeight / rows;
        
        float cellSize = Mathf.Min(cellWidth, cellHeight);
        cellSize = Mathf.Round(cellSize);
        
        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        gridLayoutGroup.spacing = new Vector2(spacing, spacing);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = cols;
        gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
    }
    
    void GenerateRandomObstacles()
    {
        float obstacleChance = 0.3f;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                gridData[row, col] = Random.Range(0f, 1f) < obstacleChance;
            }
        }
    }
    
    void CreateVisualGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                GameObject cell = CreateCell(row, col);
                gridCells[row, col] = cell;
            }
        }
    }
    
    GameObject CreateCell(int row, int col)
    {
        GameObject cell;
        
        if (cellPrefab != null)
        {
            cell = Instantiate(cellPrefab, transform);
        }
        else
        {
            cell = new GameObject($"Cell_{row}_{col}");
            cell.transform.SetParent(transform);
            
            Image cellImage = cell.AddComponent<Image>();
            cellImage.color = gridData[row, col] ? Color.black : Color.white;
        }
        
        cell.name = $"Cell_{row}_{col}";
        
        GridCell cellScript = cell.GetComponent<GridCell>();
        if (cellScript == null)
        {
            cellScript = cell.AddComponent<GridCell>();
        }
        
        cellScript.Initialize(row, col, gridData[row, col]);
        
        return cell;
    }
    
    void SelectRandomStartAndEnd()
    {
        List<Vector2Int> walkablePositions = GetAllWalkablePositions();
        
        if (walkablePositions.Count < 2)
        {
            Debug.LogWarning("Not enough walkable positions to place start and end points!");
            return;
        }
        
        int startIndex = Random.Range(0, walkablePositions.Count);
        startPosition = walkablePositions[startIndex];
        
        walkablePositions.RemoveAt(startIndex);
        
        int endIndex = Random.Range(0, walkablePositions.Count);
        endPosition = walkablePositions[endIndex];
        
        UpdateStartEndVisuals();
        
        Debug.Log($"Start Position: ({startPosition.x}, {startPosition.y})");
        Debug.Log($"End Position: ({endPosition.x}, {endPosition.y})");
    }
    
    List<Vector2Int> GetAllWalkablePositions()
    {
        List<Vector2Int> walkablePositions = new List<Vector2Int>();
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (!gridData[row, col]) // If not an obstacle
                {
                    walkablePositions.Add(new Vector2Int(row, col));
                }
            }
        }
        
        return walkablePositions;
    }
    
    void UpdateStartEndVisuals()
    {
        if (gridCells[startPosition.x, startPosition.y] != null)
        {
            GridCell startCell = gridCells[startPosition.x, startPosition.y].GetComponent<GridCell>();
            if (startCell != null)
            {
                startCell.SetCellType(GridCell.CellType.Start);
            }
        }
        
        if (gridCells[endPosition.x, endPosition.y] != null)
        {
            GridCell endCell = gridCells[endPosition.x, endPosition.y].GetComponent<GridCell>();
            if (endCell != null)
            {
                endCell.SetCellType(GridCell.CellType.End);
            }
        }
    }
    
    public bool IsWalkable(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return false;
        
        return !gridData[row, col];
    }
    
    public Vector2Int GetGridSize()
    {
        return new Vector2Int(cols, rows);
    }
    
    public GameObject GetCell(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return null;
        
        return gridCells[row, col];
    }
    
    public Vector2Int GetStartPosition()
    {
        return startPosition;
    }
    
    public Vector2Int GetEndPosition()
    {
        return endPosition;
    }
    
    public void SetStartPosition(int row, int col)
    {
        if (IsWalkable(row, col))
        {
            if (gridCells[startPosition.x, startPosition.y] != null)
            {
                GridCell oldStartCell = gridCells[startPosition.x, startPosition.y].GetComponent<GridCell>();
                if (oldStartCell != null)
                {
                    oldStartCell.SetCellType(GridCell.CellType.Walkable);
                }
            }
            
            startPosition = new Vector2Int(row, col);
            if (gridCells[row, col] != null)
            {
                GridCell newStartCell = gridCells[row, col].GetComponent<GridCell>();
                if (newStartCell != null)
                {
                    newStartCell.SetCellType(GridCell.CellType.Start);
                }
            }
        }
    }
    
    public void SetEndPosition(int row, int col)
    {
        if (IsWalkable(row, col))
        {
            if (gridCells[endPosition.x, endPosition.y] != null)
            {
                GridCell oldEndCell = gridCells[endPosition.x, endPosition.y].GetComponent<GridCell>();
                if (oldEndCell != null)
                {
                    oldEndCell.SetCellType(GridCell.CellType.Walkable);
                }
            }
            
            endPosition = new Vector2Int(row, col);
            if (gridCells[row, col] != null)
            {
                GridCell newEndCell = gridCells[row, col].GetComponent<GridCell>();
                if (newEndCell != null)
                {
                    newEndCell.SetCellType(GridCell.CellType.End);
                }
            }
        }
    }
    
    [ContextMenu("Regenerate Grid")]
    public void RegenerateGrid()
    {
        GenerateRandomGrid();
    }
    
    public void UpdateGridSize(int newRows, int newCols)
    {
        rows = newRows;
        cols = newCols;
        GenerateRandomGrid();
    }
    
    public void ShowPath(List<Vector2Int> path)
    {
        ClearPath();
        
        foreach (Vector2Int position in path)
        {
            if (position != startPosition && position != endPosition)
            {
                if (gridCells[position.x, position.y] != null)
                {
                    GridCell pathCell = gridCells[position.x, position.y].GetComponent<GridCell>();
                    if (pathCell != null)
                    {
                        pathCell.SetCellType(GridCell.CellType.Path);
                    }
                }
            }
        }
    }
    
    public void ClearPath()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (gridCells[row, col] != null)
                {
                    GridCell cell = gridCells[row, col].GetComponent<GridCell>();
                    if (cell != null && cell.GetCellType() == GridCell.CellType.Path)
                    {
                        cell.SetCellType(GridCell.CellType.Walkable);
                    }
                }
            }
        }
    }
}
