namespace BrainPuzzle
{
    using UnityEngine;

#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(GridColorMap))]
    public class GridBaseEditor : Editor
    {
        public void OnSceneGUI()
        {  //for every pixel in image
            GridColorMap t = target as GridColorMap;

            TileColorMap[] gos = t.GetComponentsInChildren<TileColorMap>();

            if (t.ShowGrid)
            foreach (TileColorMap current_tile in gos) 
                {

               Handles.color = current_tile.tileColor;

                if (Handles.Button(current_tile.transform.position, Quaternion.identity, t.TileSize.x, t.TileSize.y, Handles.CubeHandleCap))
                {
                    TileBase prefab = t.GetTilePrefab(current_tile.PositionInGrid.x, current_tile.PositionInGrid.y);
                    this.Repaint();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GridColorMap obj = target as GridColorMap;
            GUILayout.Space(300);

            Rect rect = GUILayoutUtility.GetLastRect();

            GUI.Label(new Rect(rect.x + 10, rect.y + 25, 250, 25), "Size X " + obj.SizeX);

            rect = GUILayoutUtility.GetLastRect();

            GUI.Label(new Rect(rect.x + 10, rect.y + 50, 250, 25), "Size Y " + obj.SizeY);

            rect = GUILayoutUtility.GetLastRect();

            if (obj.gridImage != null)
                GUI.DrawTexture(new Rect(rect.x + 100, rect.y + 25, 250, 250), obj.gridImage, ScaleMode.ScaleAndCrop, true, 0);
            else
                EditorGUI.HelpBox(new Rect(rect.x + 100, rect.y + 25, 250, 250),"Level Texture is missing",MessageType.Error);
          
            if (GUILayout.Button("Regen Grid"))
            {
                ((GridColorMap)target).RegenGrid();

                //set undo
                Undo.RegisterFullObjectHierarchyUndo(target, "Regen Grid");
            }

            if (GUILayout.Button("Destroy Grid"))
            {
                ((GridColorMap)target).DestroyGrid();

                //set undo
                Undo.RegisterFullObjectHierarchyUndo(target, "Destroy Grid");
            }


            if (GUILayout.Button("Update Grid Rotation"))
            {

                //start from 0,0
                //get neightbour
                for (int x = 0; x < obj.SizeX; x++)
                {
                    for (int y = 0; y < obj.SizeY; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);

                        //      top
                        //      x-1
                        //      x,y
                        // y-1 left 0,0 -> right y++
                        //      | 
                        //      v
                        //     x++ down

                        Vector2Int left = new Vector2Int(x , y-1);
                        Vector2Int right = new Vector2Int(x, y+1);
                        Vector2Int top = new Vector2Int(x-1, y );
                        Vector2Int down = new Vector2Int(x+1, y );


                        TileColorMap tile = obj.GetTile(pos);
                        TileColorMap left_tile = obj.GetTile(left);
                        TileColorMap right_tile = obj.GetTile(right);
                        TileColorMap top_tile = obj.GetTile(top);
                        TileColorMap down_tile = obj.GetTile(down);

                        //      X 
                        //      |
                        // X-- 0,0 --    0 
                        //      |
                        //
                        if (left_tile == null && top_tile == null && down_tile != null && right_tile != null) 
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
                        }

                        //      X 
                        //      |
                        //  -- 0,0 --    0 
                        //      |
                        //
                        if (left_tile != null && top_tile == null && down_tile != null && right_tile != null)
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        }

                        //      X 
                        //      |
                        //  -- 0,0 --X    0 
                        //      |
                        //
                        if (left_tile != null && top_tile == null && down_tile != null && right_tile != null)
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        }


                        //       
                        //      |
                        // X-- 0,0 --    -90
                        //      |
                        //
                        if (left_tile == null && top_tile != null && down_tile != null && right_tile != null)
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                        }

                        //       
                        //      |
                        // X-- 0,0 --  -180 
                        //      |
                        //      X
                        if (left_tile == null && top_tile != null && down_tile == null && right_tile != null)
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
                        }
                        //       
                        //      |
                        //  -- 0,0 --  -180 
                        //      |
                        //      X
                        if (left_tile != null && top_tile != null && down_tile == null && right_tile != null)
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
                        }
                        //       
                        //      |
                        //  -- 0,0 --  -180 
                        //      |
                        //      X
                        if (left_tile != null && top_tile != null && down_tile == null && right_tile == null)
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
                        }

                        //       
                        //      |
                        //  -- 0,0 --X   -270
                        //      |
                        //      
                        if (left_tile != null && top_tile != null && down_tile != null && right_tile == null)
                        {
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -270, 0));
                        }
                    }
                }

                //set undo
                Undo.RegisterFullObjectHierarchyUndo(target, "Update Grid Rotations");
            }
        }
    }

#endif

    [AddComponentMenu("BrainPuzzle/TileGrid/Grid Color Map")]
    public class GridColorMap : GridBase
    {
        [Tooltip("When import texture, set Non-Power of 2 to None, and enable Read/Write")]
        [Header("Color Map")]
        [SerializeField]
        public Texture2D gridImage ;

        [SerializeField] 
        TileColorMap[] tiles ;

        public bool ShowGrid;
        public int SizeX => gridImage!=null? gridImage.width : -1;
        public int SizeY => gridImage != null ? gridImage.height : -1;

        public TileColorMap GetTile(Vector2Int pos)
        {
            return GetTile(pos.x, pos.y);
        }
        public TileColorMap GetTile(int x, int y)
        {
            Vector2Int positionInGrid = new Vector2Int(x, y);

            foreach (TileColorMap current_tile in gameObject.GetComponentsInChildren<TileColorMap>()){
                if (positionInGrid == current_tile.PositionInGrid)
                    return current_tile;
            }
           
            return null;
        }

     
        public override TileBase GetTilePrefab(int x, int y)
        {
            //get color in texture2D
            Color color = gridImage.GetPixel(x, y);

            //foreach tile in list, find tile with this color
            foreach (TileColorMap tile in tiles)
            {
                if (tile.tileColor == color)
                    return tile;
            }

            return null;
        }

        public void RegenGrid()
        {
            //remove old grid and generate new one
            RemoveOldGrid();
            GenerateGrid(gridImage.width, gridImage.height);
        }

        public void DestroyGrid()
        {
            //remove old grid
            RemoveOldGrid();
        }

       
    }
}