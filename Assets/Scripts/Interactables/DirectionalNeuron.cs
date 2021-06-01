using UnityEngine;

[RequireComponent(typeof(FieldOfView3D))]
public class DirectionalNeuron : Neuron
{
    FieldOfView3D fieldOfView3D;

    RotateObject rotateObject;
    MoveObject moveObject;

    protected override void Start()
    {
        base.Start();

        //get reference to field of view
        fieldOfView3D = GetComponent<FieldOfView3D>();
    }

    void OnEnable()
    {
        //if there is a rotate object, add event
        rotateObject = GetComponent<RotateObject>();
        if(rotateObject)
        {
            rotateObject.onEndRotation += OnEndAnimation;
        }

        //if there is a move object, add event
        moveObject = GetComponent<MoveObject>();
        if(moveObject)
        {
            moveObject.onEndMovement += OnEndAnimation;
        }
    }

    void OnDisable()
    {
        //if there is a rotate object, remove event
        if (rotateObject)
        {
            rotateObject.onEndRotation -= OnEndAnimation;
        }

        //if there is a move object, remove event
        if (moveObject)
        {
            moveObject.onEndMovement -= OnEndAnimation;
        }
    }

    protected override bool CanAttach(Interactable interactable)
    {
        //check if interactable is inside our area of vision
        foreach(Transform target in fieldOfView3D.VisibleTargets)
        {
            //foreach target (collider transform) check if parent (where there is script Interactable) is the same of interactable parameter
            Interactable i = target.GetComponentInParent<Interactable>();
            if(i != null && i == interactable)
            {
                //if inside our area of vision, check if can attach
                return base.CanAttach(interactable);
            }
        }

        return false;
    }

    void OnEndAnimation()
    {
        //when rotate object or move object stops, if this neuron is attached to something
        if(attachedTo)
        {
            //check if attachTo is still in our area of vision - then return
            foreach (Transform target in fieldOfView3D.VisibleTargets)
            {
                Interactable i = target.GetComponentInParent<Interactable>();
                if(i != null && i == attachedTo)
                {
                    return;
                }
            }

            //else rewind
            RewindRope();
        }
    }
}
