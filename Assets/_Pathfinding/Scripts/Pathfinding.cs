using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pathfinding : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    [SerializeField] private bool allowDiagonal = false;
    public bool AllowDiagonal => allowDiagonal;
    [SerializeField] private float visualizationDelay = 0.1f;
    [SerializeField] private bool showSearchProcess = true;
    
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    
    [Header("Debug Colors")]
    [SerializeField] private Color openSetColor = Color.cyan;
    [SerializeField] private Color closedSetColor = Color.magenta;
    
    private List<Node> openSet;
    private List<Node> closedSet;
    private bool isSearching = false;

    public string ResultText;
    public class Node
    {
        public Vector2Int position;
        public Node parent;
        public float gCost; 
        public float hCost; 
        public float fCost => gCost + hCost; 
        
        public Node(Vector2Int pos)
        {
            position = pos;
            parent = null;
            gCost = 0;
            hCost = 0;
        }
        
        public Node(Vector2Int pos, Node parentNode, float g, float h)
        {
            position = pos;
            parent = parentNode;
            gCost = g;
            hCost = h;
        }
    }
    
    void Start()
    {
        // Get GridManager if not assigned
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found! Please assign it in the inspector.");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSearching)
        {
            FindPath();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearPath();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            gridManager.RegenerateGrid();
        }
    }

    public void RegenerateGrid()
    {
        gridManager.RegenerateGrid();
    }
    
    public void RegenerateGrid(int x, int y)
    {
        gridManager.UpdateGridSize(x, y);
    }
    
    public void FindPath()
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager is null!");
            return;
        }
        
        Vector2Int start = gridManager.GetStartPosition();
        Vector2Int end = gridManager.GetEndPosition();
        
        Debug.Log($"Finding path from {start} to {end}");
        
        if (showSearchProcess)
        {
            StartCoroutine(FindPathCoroutine(start, end));
        }
        else
        {
            List<Vector2Int> path = AStar(start, end);
            if (path != null && path.Count > 0)
            {
                gridManager.ShowPath(path);
                ResultText = $"Path found! Length: {path.Count}";
            }
            else
            {
                ResultText = "No path found!";
            }
        }
    }
    
    private System.Collections.IEnumerator FindPathCoroutine(Vector2Int start, Vector2Int end)
    {
        isSearching = true;
        gridManager.ClearPath();
        
        openSet = new List<Node>();
        closedSet = new List<Node>();
        
        Node startNode = new Node(start);
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            if (showSearchProcess && visualizationDelay > 0)
            {
                VisualizeSearchProcess();
                yield return new WaitForSeconds(visualizationDelay);
            }
            
            if (currentNode.position == end)
            {
                List<Vector2Int> path = ReconstructPath(currentNode);
                gridManager.ShowPath(path);
                ResultText = $"Path found! Length: {path.Count}";
                isSearching = false;
                yield break;
            }
            
            List<Vector2Int> neighbors = GetNeighbors(currentNode.position);
            
            foreach (Vector2Int neighborPos in neighbors)
            {
                if (closedSet.Any(n => n.position == neighborPos))
                    continue;
                
                float tentativeGCost = currentNode.gCost + GetDistance(currentNode.position, neighborPos);
                
                Node neighborNode = openSet.FirstOrDefault(n => n.position == neighborPos);
                
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos, currentNode, tentativeGCost, GetHeuristic(neighborPos, end));
                    openSet.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.parent = currentNode;
                    neighborNode.gCost = tentativeGCost;
                }
            }
        }
        
        ResultText = "No path found!";
        isSearching = false;
    }
    
    public List<Vector2Int> AStar(Vector2Int start, Vector2Int end)
    {
        openSet = new List<Node>();
        closedSet = new List<Node>();
        
        Node startNode = new Node(start);
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            if (currentNode.position == end)
            {
                return ReconstructPath(currentNode);
            }
            
            List<Vector2Int> neighbors = GetNeighbors(currentNode.position);
            
            foreach (Vector2Int neighborPos in neighbors)
            {
                if (closedSet.Any(n => n.position == neighborPos))
                    continue;
                
                float tentativeGCost = currentNode.gCost + GetDistance(currentNode.position, neighborPos);
                
                Node neighborNode = openSet.FirstOrDefault(n => n.position == neighborPos);
                
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos, currentNode, tentativeGCost, GetHeuristic(neighborPos, end));
                    openSet.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.parent = currentNode;
                    neighborNode.gCost = tentativeGCost;
                }
            }
        }
        
        return null;
    }
    
    private List<Vector2Int> ReconstructPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;
        
        while (currentNode != null)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        return path;
    }
    
    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        
        // 4-directional movement (up, down, left, right)
        Vector2Int[] directions = {
            new Vector2Int(-1, 0), // Up
            new Vector2Int(1, 0),  // Down
            new Vector2Int(0, -1), // Left
            new Vector2Int(0, 1)   // Right
        };
        
        // Add diagonal movement if allowed
        if (allowDiagonal)
        {
            Vector2Int[] diagonalDirections = {
                new Vector2Int(-1, -1), // Up-Left
                new Vector2Int(-1, 1),  // Up-Right
                new Vector2Int(1, -1),  // Down-Left
                new Vector2Int(1, 1)    // Down-Right
            };
            
            List<Vector2Int> allDirections = new List<Vector2Int>(directions);
            allDirections.AddRange(diagonalDirections);
            directions = allDirections.ToArray();
        }
        
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = position + direction;
            
            if (gridManager.IsWalkable(neighbor.x, neighbor.y))
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    private float GetDistance(Vector2Int posA, Vector2Int posB)
    {
        if (allowDiagonal)
        {
            return Vector2Int.Distance(posA, posB);
        }
        else
        {
            return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y);
        }
    }
    
    private float GetHeuristic(Vector2Int from, Vector2Int to)
    {
        if (allowDiagonal)
        {
            return Vector2Int.Distance(from, to);
        }
        else
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }
    }
    
    private void VisualizeSearchProcess()
    {
        foreach (Node node in openSet)
        {
            GameObject cell = gridManager.GetCell(node.position.x, node.position.y);
            if (cell != null)
            {
                GridCell gridCell = cell.GetComponent<GridCell>();
                if (gridCell != null && gridCell.GetCellType() == GridCell.CellType.Walkable)
                {
                    gridCell.HighlightCell(openSetColor, visualizationDelay);
                }
            }
        }
        
        foreach (Node node in closedSet)
        {
            GameObject cell = gridManager.GetCell(node.position.x, node.position.y);
            if (cell != null)
            {
                GridCell gridCell = cell.GetComponent<GridCell>();
                if (gridCell != null && gridCell.GetCellType() == GridCell.CellType.Walkable)
                {
                    gridCell.HighlightCell(closedSetColor, visualizationDelay);
                }
            }
        }
    }
    
    public void ClearPath()
    {
        if (gridManager != null)
        {
            gridManager.ClearPath();
        }
    }
    
    public void SetVisualizationDelay(float delay)
    {
        visualizationDelay = delay;
    }
    
    public void SetShowSearchProcess(bool show)
    {
        showSearchProcess = show;
    }
    
    public void SetAllowDiagonal(bool allow)
    {
        allowDiagonal = allow;
    }
    
    public List<Vector2Int> FindPathImmediate(Vector2Int start, Vector2Int end)
    {
        return AStar(start, end);
    }
    
    public List<Vector2Int> FindPathBetween(Vector2Int start, Vector2Int end)
    {
        if (!gridManager.IsWalkable(start.x, start.y) || !gridManager.IsWalkable(end.x, end.y))
        {
            Debug.LogWarning("Start or end position is not walkable!");
            return null;
        }
        
        return AStar(start, end);
    }
}