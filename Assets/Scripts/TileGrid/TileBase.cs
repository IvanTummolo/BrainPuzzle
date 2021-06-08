using UnityEngine;

namespace BrainPuzzle
{
    public abstract class TileBase: MonoBehaviour // ScriptableObject
    {
        [Header("Debug")]
        [SerializeField] protected Vector2Int positionInGrid;

        public Vector2Int PositionInGrid => positionInGrid;

        public void Init(Vector2Int positionInGrid)
        {
            this.positionInGrid = positionInGrid;
        }
    }
}