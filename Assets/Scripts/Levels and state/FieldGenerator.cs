using System;
using UnityEngine;
using UnityEngine.Events;

public class FieldGenerator : MonoBehaviour
{
    // The field will be a 2D array of GameObjects. The GameObjects will be the tiles.
    public static GameObject[,] Field;
    public int fieldWidth;
    public int fieldHeight;
    public GameObject tilePrefab;
    public static UnityEvent OnLevelGenerated = new();
    public float scalingPercentage = 0.8f;
    private float _gridScale;
    

    public void Start()
    {
        GenerateLevel(1);
    }

    public void GenerateLevel(int level)
    {
        // Destroy all children
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Set the scale of the tiles
        SetTileScales();
        
        // Get the level from the Levels class.
        Tile.TileType[,] levelArray = Levels.GetLevel(level);

        // Create a new field with the same size as the levelArray.
        Field = new GameObject[levelArray.GetLength(0), levelArray.GetLength(1)];

        PlaceTiles(levelArray);
        
        // Invoke the OnLevelGenerated event.
        OnLevelGenerated.Invoke();
    }

    public void SetTileScales()
    {
        // Get the lowest of the camera orthographic size height or width
        Camera mainCamera = Camera.main ? Camera.main : throw new ArgumentNullException("Camera.main was null");
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = mainCamera.aspect * halfHeight;
        
        float minScreenDistance = 2 * Mathf.Min(halfHeight, halfWidth);
        _gridScale = scalingPercentage * minScreenDistance;
    }
    
    private void PlaceTiles(Tile.TileType[,] tiles)
    {
        // Loop through the levelArray.
        fieldWidth = tiles.GetLength(1);
        fieldHeight = tiles.GetLength(0);
        for (int y = 0; y < fieldHeight; y++)
        {
            for (int x = 0; x < fieldWidth; x++)
            {
                // Get the tile type from the levelArray.
                Tile.TileType tileType = tiles[y, x];

                // Instantiate the node at the correct position
                Vector3 pos = new Vector3(
                    (x - fieldWidth / 2.0f + 0.5f) * _gridScale / fieldHeight, 
                    (-y + fieldHeight / 2.0f - 0.5f) * _gridScale / fieldHeight, 0);
                
                // print(pos);
                
                // Instantiate the tile prefab
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                
                // Scale the tile
                tile.transform.localScale = new Vector3(_gridScale / fieldHeight, _gridScale / fieldHeight, 1);
                
                tile.GetComponent<Tile>().SetTileType(tileType);
                
                Field[y, x] = tile;
            }
        }
    }

    public static Vector2Int GetTilePosition(GameObject startTile)
    {
        // Loop through the field to find the start tile
        for (int y = 0; y < Field.GetLength(0); y++)
        {
            for (int x = 0; x < Field.GetLength(1); x++)
            {
                if (Field[y, x] == startTile)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.zero;
    }
}