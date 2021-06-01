using redd096;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Rope")]
    public LineRenderer RopePrefab = default;
    public RopeColliderInteraction ColliderPrefab = default;
    public float RopeLength = 12;

    [Header("Minimap")]
    [SerializeField] SpriteRenderer playerIconPrefab = default;
    [SerializeField] SpriteRenderer minimapIconPrefab = default;
    [SerializeField] Sprite spriteRoom = default;
    [SerializeField] Sprite spriteCurrentRoom = default;

    [Header("Debug")]
    [ReadOnly] public RoomGame currentRoom;

    Transform minimapIconsParent;
    Transform playerIcon;

    void Awake()
    {
        //create parent icons
        minimapIconsParent = new GameObject("Minimap Icons").transform;

        //instantiate player icon
        playerIcon = Instantiate(playerIconPrefab, minimapIconsParent).transform;
    }

    public SpriteRenderer CreateIcon(Vector3 position, float tileSize, int width, int height)
    {
        //instantiate minimap icon
        SpriteRenderer minimapIcon = Instantiate(minimapIconPrefab, minimapIconsParent);
        minimapIcon.transform.position = position;
        minimapIcon.transform.localScale = new Vector3(width * tileSize, height * tileSize, 1);

        //deactive by default
        minimapIcon.gameObject.SetActive(false);

        return minimapIcon;
    }

    public void ChangeRoom(RoomGame newRoom)
    {
        //active minimap icon when enter for the first time in the room
        if (newRoom.minimapIcon.gameObject.activeInHierarchy == false)
            newRoom.minimapIcon.gameObject.SetActive(true);

        //set current room and previous room icons
        newRoom.minimapIcon.sprite = spriteCurrentRoom;
        if (currentRoom)
            currentRoom.minimapIcon.sprite = spriteRoom;

        //set new current room and move player icon
        currentRoom = newRoom;
        playerIcon.position = currentRoom.minimapIcon.transform.position + Vector3.up;
    }
}
