namespace BrainPuzzle
{
    using UnityEngine;
    public enum TileType
    {
        ActivatorDoor,
        WallActivatorDoor,
        ActivatorNeuron,
        DirectionalNeuron,
        Door,
        Generator,
        MoveableWall,
        Neuron,
        SwitchNeuron,
        NotTransitable,
        Transitable
    }

    [AddComponentMenu("BrainPuzzle/TileGrid/Tile Color Map")]
    [SelectionBase]
    public class TileColorMap : TileBase
    {
        [Header("Color Map")]
        public Color tileColor = Color.white;
        public TileType Type;
    }
}