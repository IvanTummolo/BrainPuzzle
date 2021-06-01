using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096;

[AddComponentMenu("Project Brain/Procedural Map/Map Manager")]
public class ProceduralMapManagerGame : ProceduralMapManager
{
    [Header("Player")]
    [SerializeField] Player playerPrefab = default;

    public override IEnumerator EndGeneration()
    {
        yield return base.EndGeneration();

        //instantiate player
        Instantiate(playerPrefab, Vector3.up, Quaternion.identity);

        //wait
        yield return new WaitForFixedUpdate();

        //get rooms in scene (cause we destroy and replace rooms, so rooms in ProceduralMapManager list are null now)
        RoomGame[] roomsInScene = GetComponentsInChildren<RoomGame>();

        //foreach room, connect doors
        foreach (RoomGame room in roomsInScene)
        {
            ConnectDoors(room);
        }

        //now deactive every room, apart the first
        foreach(RoomGame room in roomsInScene)
        {
            if (room.ID != 0)
                room.gameObject.SetActive(false);
            //set enter in first room (to move camera)
            else
                room.OnEnterRoom();
        }
    }

    void ConnectDoors(RoomGame room)
    {
        //foreach door struct do overlap and get activable doors
        foreach (DoorStruct door in room.Doors)
        {
            Collider[] colliders = Physics.OverlapSphere(door.doorTransform.position, 2);
            List<Door> activableDoors = new List<Door>();

            foreach (Collider col in colliders)
            {
                Door activableDoor = col.GetComponentInParent<Door>();
                if (activableDoor && activableDoors.Contains(activableDoor) == false)       //be sure is not already in the list
                {
                    activableDoors.Add(activableDoor);
                }
            }

            //save connections in every activable door
            foreach (Door activableDoor in activableDoors)
            {
                activableDoor.AddConnectedDoors(activableDoors);
            }
        }
    }
}
