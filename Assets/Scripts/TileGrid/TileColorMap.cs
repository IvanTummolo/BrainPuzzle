namespace BrainPuzzle
{
    using UnityEngine;

    [AddComponentMenu("BrainPuzzle/TileGrid/Tile Color Map")]
    [SelectionBase]
    public class TileColorMap : TileBase
    {
        [Header("Color Map")]
        public Color tileColor = Color.white;
    }
}