using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Interactable
{
    protected override void Start()
    {
        base.Start();

        //generator is active by default
        ActiveInteractable(true, null);
    }

    protected override bool CanBeAttach(Interactable interactable)
    {
        //can't attach to generator
        return false;
    }

    public override bool CreateRope()
    {
        bool pickRopeFromGenerator = base.CreateRope();

        //when pick rope, force closing enter door
        if(pickRopeFromGenerator)
        {
            RoomParent.ForceOpenCloseEnterDoor(false);
        }

        return pickRopeFromGenerator;
    }
}
