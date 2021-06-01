using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapCamera : MonoBehaviour
{
    [Header("Layer")]
    [SerializeField] string minimapLayer = "Minimap";

    [Header("Movement")]
    [SerializeField] float durationMovement = 1;
    [SerializeField] bool followRotation = true;

    [Header("Interact")]
    [SerializeField] bool toggleable = true;
    [SerializeField] bool canInteract = true;
    [SerializeField] bool forceToClickOnMinimap = true;

    Camera cam;
    Coroutine movementCoroutine;
    RoomGame previousRoom;
    Vector3 startPosition;

    //player moves on minimap
    EventSystem eventSystem;
    GraphicRaycaster graphicRaycaster;
    bool playerMovesOnMinimap;
    List<RaycastResult> playerHits = new List<RaycastResult>();
    Vector2 previousPosition;

    void Awake()
    {
        //get references
        cam = Camera.main;
        eventSystem = FindObjectOfType<EventSystem>();
        graphicRaycaster = GetComponentInChildren<GraphicRaycaster>();

        //set layers for cameras
        SetLayers();

        //if toggleable, by default hide minimap
        if(toggleable)
            gameObject.SetActive(false);

        //if can interact, save start position
        if (canInteract)
            startPosition = transform.position;
    }

    void Update()
    {
        //if player can interact and pressing mouse
        if(canInteract && Input.GetKey(KeyCode.Mouse0))
        {
            playerHits = RaycastHits();

            //if hit minimap
            if(playerHits.Count > 0)
            {
                //player start to move
                if (playerMovesOnMinimap == false)
                {
                    //be sure to start with a key down. Else player can press input out of minimap and move anyway
                    if(Input.GetKeyDown(KeyCode.Mouse0) || forceToClickOnMinimap == false)
                        playerMovesOnMinimap = true;
                }
                //or player continue to move
                else
                {
                    MoveOnMinimap();
                }

                //and save last position
                previousPosition = playerHits[0].screenPosition;
                return;
            }            
        }

        //if was moving and stop click or stop hit minimap, stop movement
        if (playerMovesOnMinimap)
        {
            playerMovesOnMinimap = false;
        }
    }

    void LateUpdate()
    {
        //if current room != previous room, move to new position
        if (GameManager.instance.levelManager.currentRoom && GameManager.instance.levelManager.currentRoom != previousRoom)
        {
            Movement();
            previousRoom = GameManager.instance.levelManager.currentRoom;   //set new room
        }

        //if must follow rotation, rotate with main camera
        if (followRotation)
        {
            Rotation();
        }
    }

    #region private API

    void SetLayers()
    {
        //remove minimap layer from main camera
        cam.cullingMask = LayerUtility.RemoveLayer(cam.cullingMask, LayerUtility.NameToLayer(minimapLayer));

        //set layer to this camera
        GetComponent<Camera>().cullingMask = CreateLayer.LayerOnly(minimapLayer);
    }

    void Movement()
    {
        //move to new position
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(MovementCoroutine());
    }

    IEnumerator MovementCoroutine()
    {
        //set vars
        Vector3 startPosition = transform.position;
        Vector3 endPosition = GameManager.instance.levelManager.currentRoom.transform.position + Vector3.up * 20;   //current room position + up 

        //movement animation
        float delta = 0;
        while(delta < 1)
        {
            delta += Time.deltaTime / durationMovement;

            transform.position = Vector3.Lerp(startPosition, endPosition, delta);

            yield return null;
        }
    }

    void Rotation()
    {
        //follow main camera rotation on Y axis
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, cam.transform.eulerAngles.y, transform.eulerAngles.z);
    }

    #endregion

    #region player moves on minimap

    List<RaycastResult> RaycastHits()
    {
        //set pointer event on input position
        PointerEventData pointerEvent = new PointerEventData(eventSystem);
        pointerEvent.position = Player.InputPosition();

        //then raycast and get hits
        List<RaycastResult> hits = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEvent, hits);

        return hits;
    }

    void MoveOnMinimap()
    {
        //calculate movement input and get camera movement from it
        Vector2 movementInput = previousPosition - playerHits[0].screenPosition;
        Vector3 cameraMovement = new Vector3(movementInput.x, 0, movementInput.y);

        //rotate on Y axis to get local movement
        cameraMovement = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * cameraMovement;

        //move camera
        transform.position += cameraMovement;
    }

    #endregion

    #region public API

    public void ToggleMinimap()
    {
        //do only if toggleable
        if (toggleable == false)
            return;

        //stop camera movement and show minimap - restart camera movement and hide minimap
        GameManager.instance.cameraMovement.enabled = gameObject.activeInHierarchy;
        gameObject.SetActive(!gameObject.activeInHierarchy);

        //be sure to be at start position (no moved by player when can interact)
        if(gameObject.activeInHierarchy)
            transform.position = startPosition;
    }

    #endregion
}
