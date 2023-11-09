using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        Field,
        Path,
        Start,
        End
    }
    
    public Sprite fieldSprite;
    public Sprite pathSprite;
    public Sprite startSprite;
    public Sprite endSprite;
    
    public TileType tileType;
    
    public void SetTileType(TileType type)
    {
        tileType = type;
        switch (type)
        {
            case TileType.Field:
                GetComponent<SpriteRenderer>().sprite = fieldSprite;
                gameObject.tag = "Field";
                break;
            case TileType.Path:
                GetComponent<SpriteRenderer>().sprite = pathSprite;
                gameObject.tag = "Path";
                break;
            case TileType.Start:
                GetComponent<SpriteRenderer>().sprite = startSprite;
                gameObject.tag = "Start";
                break;
            case TileType.End:
                GetComponent<SpriteRenderer>().sprite = endSprite;
                gameObject.tag = "End";
                break;
        }
    }
    
    
}
