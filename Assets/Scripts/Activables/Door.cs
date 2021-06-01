using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Activable
{
    bool calledFromConnectedDoor;
    bool forceDoor;

    public bool isOpen { get; private set; } = false;
    public List<Door> connectedDoors { get; private set; } = new List<Door>();

    protected override void Active()
    {
        //open door
        OpenCloseDoor(true);

        //only if not called from connected door and not forced (the player opened this door)
        if (calledFromConnectedDoor == false && forceDoor == false)
        {
            RoomParent.OnOpenDoor(this);
        }

        //if called from connected door
        if(calledFromConnectedDoor)
        {
            RoomParent.OnOpenFromConnectedDoor(this);
        }
    }

    protected override void Deactive()
    {
        //close door
        OpenCloseDoor(false);

        //only if not called from connected door and not forced (the player closed this door)
        if (calledFromConnectedDoor == false && forceDoor == false)
        {
            RoomParent.OnCloseDoor(this);
        }

        //if called from connected door
        if(calledFromConnectedDoor)
        {
            RoomParent.OnCloseFromConnectedDoor(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        //if hit player
        Player player = other.GetComponentInParent<Player>();
        if (player)
        {
            //check if enter in room, then call EnterRoom and deactivate previous rooms
            if (CheckTargetIsEnteringRoom(player.transform))
            {
                RoomParent.OnEnterRoom(this);
            }
            //if exit from room, be sure connected rooms are active (if player come back to previous room, without connect door, it must to reactivate)
            else
            {
                RoomParent.OnExitRoom(this);
            }
        }
    }

    #region private API

    void OpenCloseDoor(bool open)
    {
        //set is open or closed
        isOpen = open;

        foreach (Renderer rend in ObjectToControl.GetComponentsInChildren<Renderer>())
        {
            //disable/enable renderer
            rend.enabled = !open;
        }
        foreach (Collider col in ObjectToControl.GetComponentsInChildren<Collider>())
        {
            //set collider trigger/NOTTrigger
            col.isTrigger = open;
        }
    }

    bool CheckTargetIsEnteringRoom(Transform target)
    {
        //get door distance and target distance from room center
        float doorDistanceFromRoom = Vector3.Distance(transform.position, RoomParent.transform.position);
        float targetDistanceFromRoom = Vector3.Distance(target.position, RoomParent.transform.position);

        //if target is near than door, then is entering
        return targetDistanceFromRoom < doorDistanceFromRoom;
    }

    #endregion

    #region public API

    /// <summary>
    /// Add to list connected doors
    /// </summary>
    /// <param name="doors">list connected doors to add</param>
    public void AddConnectedDoors(List<Door> doors)
    {
        foreach (Door door in doors)
        {
            //be sure is not this door and is not already in the list
            if (door != this && connectedDoors.Contains(door) == false)
            {
                connectedDoors.Add(door);
            }
        }
    }

    /// <summary>
    /// Call connected door (OnOpenFromConnectedDoor or OnCloseFromConnectedDoor)
    /// </summary>
    public void CallConnectedDoors(bool open)
    {
        //foreach connected door
        foreach (Door door in connectedDoors)
        {
            if (door != null)
            {
                //set we are calling it
                door.calledFromConnectedDoor = true;

                //active or deactive
                if (open)
                    door.Active();
                else
                    door.Deactive();

                //stop calling
                door.calledFromConnectedDoor = false;
            }
        }
    }

    /// <summary>
    /// Force open/close door (so without add to list openedDoor, without add to enter door, without active/deactive rooms)
    /// </summary>
    public void ForceOpenCloseDoor(bool open)
    {
        forceDoor = true;

        //force to open or close door
        if (open)
            Active();
        else
            Deactive();

        forceDoor = false;
    }

    #endregion
}
