using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : MonoBehaviour
{
    public GameObject towerPrefab;
    
    private static Camera _camera;
    private static GameObject _towerPreview;
    private static TowerType _towerType;
    public GameObject connectionPrefab;
    
    public static List<Tower> towers = new();
    public int towerCost = 100;
    
    enum TowerType
    {
        Basic,
        GlassCannon,
        Shield,
        Sniper,
        BShield
    }

    public void Start()
    {
        _camera = Camera.main;
        _towerPreview = Instantiate(towerPrefab, Vector3.zero, Quaternion.identity, transform);
        
        towerPrefab.GetComponent<Tower>().SetDetails(100, 12, 1.2f, 0.3f);
        _towerType = TowerType.Basic;
        towerCost = 100;
        _towerPreview.transform.GetChild(1).gameObject.SetActive(true);
        _towerPreview.transform.GetChild(1).localScale = new Vector3(towerPrefab.GetComponent<Tower>().Range * 2 / towerPrefab.transform.localScale.x, towerPrefab.GetComponent<Tower>().Range * 2 / towerPrefab.transform.localScale.x, 0);
        
        LevelState.StateChanged.AddListener(OnStateChanged);
    }

    private void OnStateChanged()
    {
        if (LevelState.State == LevelState.GameState.Placing) _towerPreview.SetActive(true);
        else _towerPreview.SetActive(false);
    }
    
    private void OtherUpdate()
    {
        if (LevelState.State != LevelState.GameState.Placing) return;
        
        // Update the range preview
        _towerPreview.transform.GetChild(1).gameObject.SetActive(true);
        _towerPreview.transform.GetChild(1).localScale = new Vector3(towerPrefab.GetComponent<Tower>().Range * 2 / towerPrefab.transform.localScale.x, towerPrefab.GetComponent<Tower>().Range * 2 / towerPrefab.transform.localScale.x, 0);
        
        // Set the preview to the mouse position
        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        _towerPreview.transform.position = ClosestTilePosition(mousePosition);
        
        // If clicked, place the tower and set the state to running.
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            PlaceTower(ClosestTilePosition(mousePosition));
            LevelState.SetState(LevelState.GameState.Running);
            _towerPreview.SetActive(false);
        }
    }

    
    private void UpdateState()
    {
        // Update the range preview
        LevelState.SetState(LevelState.GameState.Placing);
        _towerPreview.transform.GetChild(1).gameObject.SetActive(true);
        _towerPreview.transform.GetChild(1).localScale = new Vector3(towerPrefab.GetComponent<Tower>().Range * 2 / towerPrefab.transform.localScale.x, towerPrefab.GetComponent<Tower>().Range * 2 / towerPrefab.transform.localScale.x, 0);
    }
    
    public void BasicTower()
    {
        towerPrefab.GetComponent<Tower>().SetDetails(100, 12, 1.2f, 0.3f);
        _towerType = TowerType.Basic;
        towerCost = 100;
        UpdateState();
    }

    public void GlassCannon()
    {
        towerPrefab.GetComponent<Tower>().SetDetails(60, 20, 1.8f,0.3f);
        _towerType = TowerType.GlassCannon;
        towerCost = 200;
        UpdateState();
    }

    public void Shield()
    {
        towerPrefab.GetComponent<Tower>().SetDetails(500, 12, 0.8f,1f);
        _towerType = TowerType.Shield;
        towerCost = 85;
        UpdateState();
    }
    
    public void Sniper()
    {
        towerPrefab.GetComponent<Tower>().SetDetails(100, 60, 2.5f, 3f);
        _towerType = TowerType.Sniper;
        towerCost = 300;
        UpdateState();
    }
    
    public void BShield()
    {
        towerPrefab.GetComponent<Tower>().SetDetails(1000, 8, 0.8f,1f);
        _towerType = TowerType.BShield;
        towerCost = 400;
        UpdateState();
    }



    public void ShowPreview()
    {
        // Set the preview to the mouse position
        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        _towerPreview.transform.position = ClosestTilePosition(mousePosition);
        
        // If clicked, place the tower and set the state to running.
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            PlaceTower(ClosestTilePosition(mousePosition));
            LevelState.SetState(LevelState.GameState.Running);
            _towerPreview.SetActive(false);
        }
    }

    private Vector3 ClosestTilePosition(Vector3 position)
    {
        GameObject closest = null;
        float closestDistance = float.MaxValue;
        // Find the closest tile to the position.
        foreach (GameObject tile in FieldGenerator.Field)
        {
            // return if not tagged as a tile
            if (!tile.CompareTag("Field")) continue;
            
            float distance = Vector2.Distance(position, tile.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = tile;
            }
        }
        return closest!.transform.position;
    }
    
    private Vector2Int ClosestTileIndex(Vector3 position)
    {
        Vector2Int index = Vector2Int.zero;
        float closestDistance = float.MaxValue;
        // Find the closest tile to the position.
        foreach (GameObject tile in FieldGenerator.Field)
        {
            // return if not tagged as a tile
            if (!tile.CompareTag("Field")) continue;
            
            float distance = Vector2.Distance(position, tile.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                index = FieldGenerator.GetTilePosition(tile);
            }
        }
        return index;
    }

    // Checks if the tower is next to another tower
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    private bool IsConnectedToTower(Vector2Int tile, Vector2Int tile2)
    {
        // Check if tile is next to tile2
        if (tile.x == tile2.x && tile.y == tile2.y + 1) return true;
        if (tile.x == tile2.x && tile.y == tile2.y - 1) return true;
        if (tile.x == tile2.x + 1 && tile.y == tile2.y) return true;
        if (tile.x == tile2.x - 1 && tile.y == tile2.y) return true;

        return false;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void PlaceTower(Vector3 mousePosition)
    {
        if (Economy.Money < towerCost) return;
        
        Economy.Money -= towerCost;
        
        // Create a new tower and add it to the list.
        GameObject tower = Instantiate(towerPrefab, mousePosition, Quaternion.identity);
        Tower towerScript = tower.GetComponent<Tower>();
        towerScript.SetDetails(towerPrefab.GetComponent<Tower>().GetDetails());
        towers.Add(towerScript);
        
        // Set the tower coordinates
        Vector2Int tileIndex = ClosestTileIndex(mousePosition);
        towerScript.coordinates = tileIndex;
        
        // Set all neightbour towers of the tower, and vice versa
        SetNeightbours(towerScript);
        
        // Set the range gameobject child to false
        tower.transform.GetChild(1).gameObject.SetActive(false);
    }

    private void SetNeightbours(Tower towerScript)
    {
        foreach (Tower neighbour in towers)
        {
            if (!IsConnectedToTower(towerScript.coordinates, neighbour.coordinates)) continue;
            
            // Place a connection in the middle of the two towers, and stretch and rotate it to touch both towers
            Vector3 direction = towerScript.transform.position - neighbour.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Vector3 scale = new Vector3(direction.magnitude, 0.1f, 1);
            Vector3 position = (towerScript.transform.position + neighbour.transform.position) / 2;
            position.z = 0;
            GameObject connection = Instantiate(connectionPrefab, position, Quaternion.Euler(0, 0, angle), transform);
            connection.transform.localScale = scale;
            towerScript.connections.Add(connection.GetComponent<SpriteRenderer>());
            neighbour.connections.Add(connection.GetComponent<SpriteRenderer>());
            
            UpdateNeighbours(towerScript, neighbour);
        }
    }

    private static void UpdateNeighbours(Tower tower1, Tower tower2)
    {
        List<Tower> AllNeighbours = new List<Tower> { tower1, tower2 };
        List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        AllNeighbours.AddRange(tower1.neighbours);
        AllNeighbours.AddRange(tower2.neighbours);
        AllNeighbours = AllNeighbours.Distinct().ToList();
        
        float healthPercentageSum = 0;
        foreach (Tower neighbour in AllNeighbours)
        {
            neighbour.neighbours = AllNeighbours;
            neighbour.damageModifier = 1 + (AllNeighbours.Count - 1) * 0.1f;
            neighbour.transform.localScale = _towerPreview.transform.localScale * Mathf.Pow(neighbour.damageModifier, 0.4f);
            healthPercentageSum += neighbour.Health / neighbour.MaxHealth;
        }
        healthPercentageSum /= AllNeighbours.Count;
        
        foreach (Tower neighbour in AllNeighbours)
        {
            neighbour.Health = healthPercentageSum * neighbour.MaxHealth;
        }

        List<SpriteRenderer> AllConnections = AllNeighbours.SelectMany(neighbour => neighbour.connections).Distinct().ToList();

        foreach (Tower neighbour in AllNeighbours)
        {
            neighbour.connections = AllConnections;
        }
    }

    public void UpdateTowers()
    {
        foreach (Tower tower in towers)
        {
            tower.UpdateTower();
        }
    }

    public static void DestoryTower(Tower tower)
    {
        // Remove the tower from the list
        towers.Remove(tower);
        
        // Destroy all neighbours
        while (tower.neighbours.Count > 0){
            Tower neighbour = tower.neighbours[0];
            towers.Remove(neighbour);
            tower.neighbours.Remove(neighbour);
            Destroy(neighbour.gameObject);
        }
        
        // Destroy all connections
        while (tower.connections.Count > 0){
            SpriteRenderer connection = tower.connections[0];
            tower.connections.Remove(connection);
            Destroy(connection.gameObject);
        }
        
        // Destroy the tower
        Destroy(tower.gameObject);
    }
}