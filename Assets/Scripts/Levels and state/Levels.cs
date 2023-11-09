public static class Levels
{
    // Array of levels coded with numbers.

    private const Tile.TileType S = Tile.TileType.Start;
    private const Tile.TileType E = Tile.TileType.End;
    private const Tile.TileType P = Tile.TileType.Path;
    private const Tile.TileType _ = Tile.TileType.Field;

    // Every level has a path from a start position to and end position. This results in the path the enemies will take in this tower defense.

    public static Tile.TileType[,] GetLevel(int level)
    {
        // Check which level is requested.
        switch (level)
        {
            case 1:
                return _1;
            default:
                return null;
        }
    }

    // Level 1
    private static readonly Tile.TileType[,] _1 =
    {
        {_, _, _, _, _, _, _, _, _, _},
        {S, P, P, _, P, P, P, P, P, _},
        {_, _, P, _, P, _, _, _, P, _},
        {_, _, P, P, P, _, _, _, P, _},
        {_, _, _, _, _, _, _, _, P, _},
        {_, _, _, P, P, P, _, _, P, _},
        {_, _, _, P, _, P, P, P, P, _},
        {_, _, _, P, _, _, _, _, _, _},
        {_, _, _, P, P, P, P, P, P, E},
        {_, _, _, _, _, _, _, _, _, _},
    };
}