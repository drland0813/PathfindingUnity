using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PathfindingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Pathfinding pathfinding;
    
    [Header("UI Controls")]
    [SerializeField] private Button findPathButton;
    [SerializeField] private Button clearPathButton;
    [SerializeField] private Button regenerateGridButton;
    [SerializeField] private Toggle allowDiagonalToggle;
    [SerializeField] private Toggle showSearchProcessToggle;
    [SerializeField] private Slider visualizationSpeedSlider;
    
    [SerializeField] private TMP_InputField xValue;
    [SerializeField] private TMP_InputField yValue;

    
    
    [Header("Statistics")]
    [SerializeField] private TextMeshProUGUI pathLengthText;
    [SerializeField] private TextMeshProUGUI searchTimeText;
    [SerializeField] private TextMeshProUGUI nodesExploredText;
    
    private int lastPathLength = 0;
    
    void Start()
    {
        InitializeComponents();
        SetupUI();
    }
    
    void InitializeComponents()
    {
        if (pathfinding == null)
            pathfinding = FindObjectOfType<Pathfinding>();
            
        if (gridManager == null)
            Debug.LogError("GridManager not found!");
            
        if (pathfinding == null)
            Debug.LogError("Pathfinding not found!");
    }
    
    void SetupUI()
    {
        if (findPathButton != null)
            findPathButton.onClick.AddListener(FindPath);
            
        if (clearPathButton != null)
            clearPathButton.onClick.AddListener(ClearPath);
            
        if (regenerateGridButton != null)
            regenerateGridButton.onClick.AddListener(RegenerateGrid);
        
        if (allowDiagonalToggle != null)
        {
            allowDiagonalToggle.onValueChanged.AddListener(OnDiagonalToggleChanged);
            allowDiagonalToggle.isOn = false; 
        }
        
        if (showSearchProcessToggle != null)
        {
            showSearchProcessToggle.onValueChanged.AddListener(OnShowSearchProcessToggleChanged);
            showSearchProcessToggle.isOn = true; 
        }
        
        if (visualizationSpeedSlider != null)
        {
            visualizationSpeedSlider.onValueChanged.AddListener(OnVisualizationSpeedChanged);
            visualizationSpeedSlider.value = 0.1f;
            visualizationSpeedSlider.minValue = 0.01f;
            visualizationSpeedSlider.maxValue = 1f;
        }
    }
    
    public void FindPath()
    {
        if (gridManager == null || pathfinding == null)
        {
            Debug.LogError("GridManager or Pathfinding is null!");
            return;
        }
        
        Vector2Int start = gridManager.GetStartPosition();
        Vector2Int end = gridManager.GetEndPosition();
        
        Debug.Log($"Finding path from {start} to {end}");
        
        pathfinding.ClearPath();
        pathfinding.FindPath();
        UpdateStatistics();
    }
    
    public void FindPathWithVisualization()
    {
        if (pathfinding != null)
        {
            pathfinding.FindPath();
        }
    }
    
    public void ClearPath()
    {
        pathfinding.ClearPath();
        
        lastPathLength = 0;
        UpdateStatistics();
    }
    
    public void RegenerateGrid()
    {
        var x = xValue.text != "" ? int.Parse(xValue.text) : 10;
        var y = yValue.text != "" ? int.Parse(yValue.text) : 10;
        pathfinding.RegenerateGrid(x, y);
        ClearPath();
    }
    
    private void OnDiagonalToggleChanged(bool value)
    {
        if (pathfinding != null)
        {
            pathfinding.SetAllowDiagonal(value);
        }
    }
    
    private void OnShowSearchProcessToggleChanged(bool value)
    {
        if (pathfinding != null)
        {
            pathfinding.SetShowSearchProcess(value);
        }
    }
    
    private void OnVisualizationSpeedChanged(float value)
    {
        if (pathfinding != null)
        {
            pathfinding.SetVisualizationDelay(value);
        }
    }
    
    private void ToggleDiagonal()
    {
        if (allowDiagonalToggle != null)
        {
            allowDiagonalToggle.isOn = !allowDiagonalToggle.isOn;
        }
        else if (pathfinding != null)
        {
            pathfinding.SetAllowDiagonal(!pathfinding.AllowDiagonal);
        }
    }
    
    private void UpdateStatistics()
    {
        if (pathLengthText != null)
        {
            pathLengthText.text = lastPathLength > 0 ? $"Path Length: {lastPathLength}" : "Path Length: No Path";
        }

        nodesExploredText.text = pathfinding.ResultText;
    }
    
    // Method to find path between custom points
    public void FindCustomPath(Vector2Int start, Vector2Int end)
    {
        if (pathfinding == null) return;
        
        List<Vector2Int> path = pathfinding.FindPathBetween(start, end);
        
        if (path != null && path.Count > 0)
        {
            // gridManager.ShowPath(path);
            Debug.Log($"Custom path found! From {start} to {end}, Length: {path.Count}");
        }
        else
        {
            Debug.Log($"No path found from {start} to {end}");
        }
    }
}