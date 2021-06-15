namespace BrainPuzzle
{
    using UnityEngine;

#if UNITY_EDITOR

    using UnityEditor;
    using System.IO;
    using System.Reflection;

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
                EditorGUI.HelpBox(new Rect(rect.x + 100, rect.y + 25, 250, 250), "Level Texture is missing", MessageType.Error);

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


            if (GUILayout.Button("Generate ColorMap"))
            {

                int MaxX = 0;
                int MaxY = 0;

                foreach (TileColorMap current_tile in obj.GetComponentsInChildren<TileColorMap>())
                {
                    if (current_tile.PositionInGrid.x > MaxX)
                        MaxX = current_tile.PositionInGrid.x;

                    if (current_tile.PositionInGrid.y > MaxY)
                        MaxY = current_tile.PositionInGrid.y;

                }

                MaxX++;
                MaxY++;

                Texture2D texture = new Texture2D(MaxX, MaxY, TextureFormat.RGB24, false, true);
                texture.filterMode = FilterMode.Point;

                texture.wrapMode = TextureWrapMode.Clamp;
                texture.Apply();

                for (int x = 0; x < MaxX; x++)
                {
                    for (int y = 0; y < MaxY; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        TileColorMap tile = obj.GetTile(pos);
                        Color color = tile.tileColor;
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();

                byte[] bytes;
                bytes = texture.EncodeToPNG();

                System.IO.FileStream fileSave;
                fileSave = new FileStream(Application.dataPath + "/Common/ColorMaps/Colormap.png", FileMode.Create);
                System.IO.BinaryWriter binary;
                binary = new BinaryWriter(fileSave);
                binary.Write(bytes);
                fileSave.Close();
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

                        Vector2Int left = new Vector2Int(x, y - 1);
                        Vector2Int right = new Vector2Int(x, y + 1);
                        Vector2Int top = new Vector2Int(x - 1, y);
                        Vector2Int down = new Vector2Int(x + 1, y);


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
                            tile.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
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

            if (GUILayout.Button("Update Prefabs"))
            {
                for (int x = 0; x < obj.SizeX; x++)
                {
                    for (int y = 0; y < obj.SizeY; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        TileColorMap tile = obj.GetTile(pos);

                        //prelevare il prefab corrispondente a questo ColorMap
                        TileBase new_tile_prefab = obj.GetTilePrefab(tile.tileColor);

                        //istanziare un nuovo gameobject di quel ColorMap
                        GameObject new_Tile = PrefabUtility.InstantiatePrefab(new_tile_prefab.gameObject, obj.transform) as GameObject;

                        new_Tile.transform.position = tile.transform.position;
                        new_Tile.transform.rotation = tile.transform.rotation;

                        //in base al tipo di prefab, copiare i valori del componente specifico


                        bool need_component_copy = false;

                        Component old_component = null;
                        Component new_component = null;

                        switch (tile.Type)
                        {
                            case TileType.ActivatorDoor:
                                {
                                    Debug.Log("");
                                    need_component_copy = true;
                                    old_component = tile.GetComponentInChildren<ActivatorNeuron>();
                                    new_component = new_Tile.GetComponentInChildren<ActivatorNeuron>();

                                    //TODO Copy feedback component if needed and other components

                                    break;
                                }
                            case TileType.ActivatorNeuron:
                                {
                                    Debug.Log("");
                                    need_component_copy = true;

                                    old_component = tile.GetComponentInChildren<ActivatorNeuron>();
                                    new_component = new_Tile.GetComponentInChildren<ActivatorNeuron>();

                                    //TODO Copy feedback component if needed and other components

                                    break;
                                }

                            case TileType.DirectionalNeuron:
                                {
                                    Debug.Log("DirectionalNeuron");
                                    need_component_copy = true;
                                    old_component = tile.GetComponentInChildren<DirectionalNeuron>();
                                    new_component = new_Tile.GetComponentInChildren<DirectionalNeuron>();


                                    FieldOfView3D old_component_3d = tile.GetComponentInChildren<FieldOfView3D>();
                                    FieldOfView3D new_component_3d = new_Tile.GetComponentInChildren<FieldOfView3D>();

                                    if (old_component_3d == null )
                                    {
                                        Debug.LogError("old_component_3d doesn't have ActivatorNeuron abort.");
                                        break;
                                    }

                                    if (new_component_3d == null )
                                    {
                                        Debug.LogError("new_component_3d doesn't have ActivatorNeuron abort.");
                                        break;
                                    }

                                    foreach (FieldInfo f in old_component_3d.GetType().GetFields())
                                    {
                                        f.SetValue(new_component_3d, f.GetValue(old_component_3d));
                                    }

                                    //TODO Copy feedback component if needed and other components

                                    break;
                                }
                            case TileType.Door:
                                {
                                    Debug.Log("Door");
                                    need_component_copy = true;
                                    old_component = tile.GetComponentInChildren<Door>();
                                    new_component = new_Tile.GetComponentInChildren<Door>();

                                    //TODO Copy feedback component if needed and other components


                                    break;
                                }
                            case TileType.Generator:
                                {
                                    Debug.Log("Generator");
                                    need_component_copy = true;

                                    old_component = tile.GetComponentInChildren<Generator>();
                                    new_component = new_Tile.GetComponentInChildren<Generator>();
                                    //TODO Copy feedback component if needed and other components


                                    break;
                                }
                            case TileType.MoveableWall:
                                {
                                    Debug.Log("MoveableWall");
                                    need_component_copy = true;

                                    old_component = tile.GetComponentInChildren<MoveObject>();
                                    new_component = new_Tile.GetComponentInChildren<MoveObject>();

                                    //TODO Copy feedback component if needed and other components

                                    break;
                                }
                            case TileType.Neuron:
                                {
                                    Debug.Log("Neuron");
                                    need_component_copy = true;

                                    old_component = tile.GetComponentInChildren<Neuron>();
                                    new_component = new_Tile.GetComponentInChildren<Neuron>();

                                    //TODO Copy feedback component if needed and other components

                                    break;
                                }
                            case TileType.SwitchNeuron:
                                {
                                    Debug.Log("SwitchNeuron");
                                    need_component_copy = true;
                                    old_component = tile.GetComponentInChildren<SwitchNeuron>();
                                    new_component = new_Tile.GetComponentInChildren<SwitchNeuron>();

                                    //TODO Copy feedback component if needed and other components


                                    break;
                                }
                            case TileType.WallActivatorDoor:
                                {
                                    Debug.Log("WallActivatorDoor");
                                    need_component_copy = true;

                                    old_component = tile.GetComponentInChildren<ActivatorNeuron>();
                                    new_component = new_Tile.GetComponentInChildren<ActivatorNeuron>();

                                    //TODO Copy feedback component if needed and other components

                                    break;
                                }
                        }

                        if (old_component == null && need_component_copy)
                        {
                            Debug.LogError("Old component doesn't have ActivatorNeuron abort.");
                            break;
                        }

                        if (new_component == null && need_component_copy)
                        {
                            Debug.LogError("New component doesn't have ActivatorNeuron abort.");
                            break;
                        }

                        if (need_component_copy)
                        {
                            foreach (FieldInfo f in old_component.GetType().GetFields())
                            {
                                f.SetValue(new_component, f.GetValue(old_component));
                            }
                        }

                        //rimuovo oggetti vecchi
                        GameObject.DestroyImmediate(tile.gameObject);
                    }
                }
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

         public override TileBase GetTilePrefab(Color color)
         {
            //get color in texture2D
            //foreach tile in list, find tile with this color
            foreach (TileColorMap tile in tiles)
            {
                if (tile.tileColor == color)
                    return tile;
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