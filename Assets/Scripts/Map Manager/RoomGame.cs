using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096;

[AddComponentMenu("Project Brain/Procedural Map/Room")]
public class RoomGame : Room
{
    [Header("Camera Position")]
    public Transform cameraPosition = default;
    public float timeToMoveCamera = 1;

    [Header("This room alternatives")]
    [SerializeField] float precisionPosition = 0.1f;
    [SerializeField] List<RoomGame> roomAlternatives = new List<RoomGame>();

    public int ID => id;
    public List<DoorStruct> Doors => doors;

    Transform cam;
    Coroutine moveCameraCoroutine;

    public Door enterDoor { get; set; }                                 //opened from previous room, which give electricity to this room
    public List<Door> openedDoors { get; set; } = new List<Door>();     //doors in this room connected from player (so enter door is not in this list, apart if player connect that door to come back)

    public SpriteRenderer minimapIcon { get; set; }

    public override IEnumerator EndRoom()
    {
        RoomGame currentRoom = this;

        //foreach alternative
        foreach (RoomGame alternative in roomAlternatives)
        {
            //find one with same doors
            if (SameDoors(alternative.doors))
            {
                currentRoom = RegenRoom(alternative);
                break;
            }
        }

        //wait next frame (so room is already instatiated)
        yield return null;

        //create minimap icon
        currentRoom.minimapIcon = GameManager.instance.levelManager.CreateIcon(currentRoom.transform.position, currentRoom.tileSize, currentRoom.width, currentRoom.height);
    }

    #region select alternative

    bool SameDoors(List<DoorStruct> alternativeDoors)
    {
        //do only if same number of doors
        if (alternativeDoors.Count != usedDoors.Count)
            return false;

        //copy used doors
        List<DoorStruct> doorsToCheck = new List<DoorStruct>(usedDoors);

        //foreach alternative door, check if there is the same door in doorsToCheck
        foreach(DoorStruct alternativeDoor in alternativeDoors)
        {
            foreach(DoorStruct door in doorsToCheck)
            {
                if(Vector3.Distance(alternativeDoor.doorTransform.localPosition, door.doorTransform.localPosition) < precisionPosition &&       //check door transform has same local position
                    alternativeDoor.direction == door.direction &&                                                                              //check same direction
                    alternativeDoor.typeOfDoor == door.typeOfDoor)                                                                              //check same type
                {
                    //remove from doorsToCheck and go to next alternativeDoor
                    doorsToCheck.Remove(door);
                    break;
                }
            }
        }

        //if no doors to check, all the doors are the same
        return doorsToCheck.Count <= 0;
    }

    RoomGame RegenRoom(RoomGame roomPrefab)
    {
        //instantiate new room
        RoomGame room = Instantiate(roomPrefab, transform.parent);
        room.transform.position = transform.position;
        room.transform.rotation = transform.rotation;

        //register room (no set adjacent room and so on, cause also other rooms will be destroyed)
        room.Register(id, teleported);

        //and destroy this room
        Destroy(gameObject);

        return room;
    }

    #endregion

    #region private API

    IEnumerator MoveCameraCoroutine()
    {
        //be sure there is cameraPosition
        if (cameraPosition == null)
        {
            Debug.LogWarning($"Manca la camera position nella camera {gameObject.name}");
            yield break;
        }

        //get cam if null
        if (cam == null)
            cam = Camera.main.transform;

        //set vars
        Vector3 startPosition = cam.position;
        Quaternion startRotation = cam.rotation;

        //remove pivot camera
        GameManager.instance.cameraMovement.RemovePivot();

        //move cam smooth to position and rotation
        float delta = 0;
        while (delta < 1)
        {
            delta += Time.deltaTime / timeToMoveCamera;
            cam.transform.position = Vector3.Lerp(startPosition, cameraPosition.position, delta);
            cam.transform.rotation = Quaternion.Lerp(startRotation, cameraPosition.rotation, delta);

            yield return null;
        }

        //set new pivot
        GameManager.instance.cameraMovement.SetPivot(transform);
    }

    void ActiveDeactiveConnectedRooms(bool active, Door door)
    {
        if (door == null)
            return;

        //foreach connected door, activate/deactive room
        foreach (Door connectedDoor in door.connectedDoors)
        {
            if (connectedDoor != null)
            {
                connectedDoor.RoomParent.gameObject.SetActive(active);
            }
        }
    }

    #endregion

    #region public API

    /// <summary>
    /// When player enter in this room
    /// </summary>
    /// <param name="door">entered from this door</param>
    public void OnEnterRoom(Door door = null)
    {       
        GameManager.instance.levelManager.ChangeRoom(this);

        //start coroutine (move camera)
        moveCameraCoroutine = StartCoroutine(MoveCameraCoroutine());

        //deactive connected room
        ActiveDeactiveConnectedRooms(false, door);
    }

    /// <summary>
    /// When player exit from this room
    /// </summary>
    /// <param name="door">exit from this door</param>
    public void OnExitRoom(Door door)
    {
        //stop coroutine (movement camera)
        if (moveCameraCoroutine != null)
            StopCoroutine(moveCameraCoroutine);

        //active connected room
        ActiveDeactiveConnectedRooms(true, door);
    }

    /// <summary>
    /// When player open a door in this room
    /// </summary>
    public void OnOpenDoor(Door door)
    {
        //active next room
        ActiveDeactiveConnectedRooms(true, door);

        //active connected door (door in next room)
        door.CallConnectedDoors(true);

        //when open a door, force open also room's enter door
        ForceOpenCloseEnterDoor(true);

        //add this door to opened doors of this room (already open, so not necessary to force open)
        openedDoors.Add(door);
    }

    /// <summary>
    /// When player close a door in this room
    /// </summary>
    public void OnCloseDoor(Door door)
    {
        //deactive other room
        ActiveDeactiveConnectedRooms(false, door);

        //deactive connected door (door in next room)
        door.CallConnectedDoors(false);

        //remove this door from opened doors of this room (already closed, so not necessary to force close)
        openedDoors.Remove(door);

        //force closing enter door of this room (like when pick rope from generator, because player is resolving puzzle again)
        ForceOpenCloseEnterDoor(false);
    }

    /// <summary>
    /// When player open a door in another room, that it's connected to a door in this room
    /// </summary>
    /// <param name="door">this room's door</param>
    public void OnOpenFromConnectedDoor(Door door)
    {
        //if no enter door, now this one is the enter door
        if (enterDoor == null)
        {
            enterDoor = door;
        }

        //if this one is the enter door
        if (enterDoor == door)
        {
            //open every previously opened door in this room (but without activate rooms, add to lists or other, so we can use Force open)
            //and every opened door will do the same with them connected doors
            foreach (Door openedDoor in openedDoors)
            {
                //be sure to do only if the door is not open (to stop infinite loop)
                if(openedDoor.isOpen == false)
                { 
                    openedDoor.ForceOpenCloseDoor(true);
                    openedDoor.CallConnectedDoors(true);
                }
            }
        }
    }

    /// <summary>
    /// When player close a door in another room, that it's connected to a door in this room
    /// </summary>
    /// <param name="open">this room's door</param>
    public void OnCloseFromConnectedDoor(Door door)
    {
        //if is the enter door
        if (enterDoor == door)
        {
            //close every previously opened door in this room (but without deactivate rooms, remove from lists or other, so we can use Force close)
            //and every opened door, will do the same with them connected doors
            foreach (Door openedDoor in openedDoors)
            {
                //be sure to do only if the door is open (to stop infinite loop)
                if (openedDoor.isOpen)
                {
                    openedDoor.ForceOpenCloseDoor(false);
                    openedDoor.CallConnectedDoors(false);
                }
            }

            //this one is no more the enter door
            enterDoor = null;
        }
    }

    /// <summary>
    /// Force to open or close a door (so the door will open/close, but not its connected doors, and there are no other checks)
    /// </summary>
    public void ForceOpenCloseEnterDoor(bool open)
    {
        if (enterDoor)
        {
            //force opening/closing enter door
            enterDoor.ForceOpenCloseDoor(open);

            //force opening/closing also other opened doors in the room
            foreach (Door door in openedDoors)
                door.ForceOpenCloseDoor(open);
        }
    }

    #endregion
}
