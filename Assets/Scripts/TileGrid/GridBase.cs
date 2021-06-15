namespace BrainPuzzle
{
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class GridBase : MonoBehaviour
    {
        [Header("Use Z instead of Y")]
        [SerializeField] bool useZ = true;

        [Header("Grid Base")]
        [SerializeField] 
        protected Vector3 startPosition = Vector3.zero;
        [SerializeField] 
        public Vector3 TileSize = Vector3.one;

        public Dictionary<Vector2Int, TileBase> Grid = new Dictionary<Vector2Int, TileBase>();
   

        protected virtual void Awake()
        {
            //update in editor doesn't save dictionary, so we need to regenerate it
            GenerateReferences();
        }

        #region regen grid

        protected void RemoveOldGrid()
        {
            //remove every child
            foreach (Transform child in transform)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }

            //then clear dictionary
            Grid.Clear();
        }

        protected virtual void GenerateGrid(int gridSizeX, int gridsizeY)
        {

            //for every pixel in image
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridsizeY; y++)
                {
                    //instantiate tile
#if UNITY_EDITOR
                    TileBase tile = UnityEditor.PrefabUtility.InstantiatePrefab(GetTilePrefab(x, y), transform) as TileBase;
#else
                    TileBase tile = Instantiate(GetTilePrefab(x, y), transform);
#endif
                    if (tile == null)
                    {
                        Debug.LogError("Base Tile is null, TileBase component is missing or color is not defined for TileBase MonoBehaviour?");
                        continue;
                    }

                    tile.transform.position = startPosition +                   //from start position
                        (useZ ?
                        new Vector3(x * TileSize.x, 0, y * TileSize.z) :        //if use Z, move on X and Z
                        new Vector3(x * TileSize.x, y * TileSize.y, 0));        //if use Y, move on X and Y
                    tile.transform.rotation = Quaternion.identity;  //set rotation

                    //init tile and add to dictionary
                    Vector2Int positionInGrid = new Vector2Int(x, y);
                    tile.Init(positionInGrid);
                    Grid.Add(positionInGrid, tile);
                }
            }
        }

        #endregion

        #region awake

        void GenerateReferences()
        {
            //create dictionary
            Grid.Clear();
            foreach (TileBase tile in FindObjectsOfType<TileBase>())
            {
                //if not already inside grid, add it
                if (Grid.ContainsKey(tile.PositionInGrid) == false)
                    Grid.Add(tile.PositionInGrid, tile);
            }
        }

        public TileBase GetTilePrefab(Vector2Int pos)
        {
            return GetTilePrefab(pos.x, pos.y);
        }

        #endregion

        /// <summary>
        /// Get Tile Prefab
        /// </summary>
        public abstract TileBase GetTilePrefab(int x, int y);

        public abstract TileBase GetTilePrefab(Color color);
    }
}