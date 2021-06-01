using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NormalState : State
{
    [Header("Movement")]
    [SerializeField] float speed = 5;

    [Header("Interact")]
    public float radiusInteract = 1.5f;
    [SerializeField] KeyCode interactInput = KeyCode.E;
    [SerializeField] KeyCode detachRopeInput = KeyCode.Q;
    [SerializeField] float timeToKeepPressedToRewind = 0.5f;
    [SerializeField] bool takeRopeWhenRewind = false;

    protected Player player;
    Rigidbody rb;
    Transform cam;

    float timerRewind;

    public override void Enter()
    {
        base.Enter();

        player = stateMachine as Player;

        rb = stateMachine.GetComponent<Rigidbody>();
        cam = Camera.main.transform;

        //lock rigidbody rotation
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public override void Update()
    {
        base.Update();

        Movement();

        //if timer is finished, rewind and don't check other inputs
        if (timerRewind > 0 && Time.time > timerRewind)
        {
            timerRewind = 0;
            RewindRope();
            return;
        }

        //interact
        if (Input.GetKeyDown(interactInput))
        {
            Interact();
        }
        //when press detach input
        else if(Input.GetKeyDown(detachRopeInput))
        {
            //set timer
            timerRewind = Time.time + timeToKeepPressedToRewind;
        }
        //when release detach input (only if timer is running)
        else if(timerRewind > 0 && Input.GetKeyUp(detachRopeInput))
        {
            timerRewind = 0;
            DetachRope();
        }
    }

    #region private API

    void Movement()
    {
        //get direction by input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical);

        //direction based on camera - ignore y and normalize
        direction = cam.TransformDirection(direction);
        direction.y = 0;
        direction = direction.normalized;

        //move player on X and Z
        rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, direction.z * speed);
    }

    protected Interactable FindInteractable()
    {
        List<Interactable> listInteractables = new List<Interactable>();

        //check every collider in area
        Collider[] colliders = Physics.OverlapSphere(stateMachine.transform.position, radiusInteract);
        foreach (Collider col in colliders)
        {
            //if found interactable
            Interactable interactable = col.GetComponentInParent<Interactable>();
            if (interactable)
            {
                //add to list
                listInteractables.Add(interactable);
            }
        }

        //only if found something
        if (listInteractables.Count > 0)
        {
            //find nearest
            return FindNearest(listInteractables, stateMachine.transform.position);
        }

        return null;
    }

    /// <summary>
    /// Find nearest to position
    /// </summary>
    Interactable FindNearest(List<Interactable> list, Vector3 position)
    {
        Interactable nearest = null;
        float distance = Mathf.Infinity;

        //foreach element in the list
        foreach (Interactable element in list)
        {
            //check distance to find nearest
            float newDistance = Vector3.Distance(element.transform.position, position);
            if (newDistance < distance)
            {
                distance = newDistance;
                nearest = element;
            }
        }

        return nearest;
    }

    protected virtual void Interact()
    {
        Interactable interactable = FindInteractable();

        //if create rope
        if (interactable && interactable.CreateRope())
        {
            ConnectToInteractable(interactable);
        }
    }

    protected virtual void RewindRope()
    {
        Interactable interactable = FindInteractable();

        //if rewind rope
        if (interactable && interactable.RewindRope())
        {
            //if bool is true, take rope (else rewindRope already rewinded rope to this interactable)
            if (takeRopeWhenRewind)
            {
                ConnectToInteractable(interactable);
            }
        }
    }

    protected virtual void DetachRope()
    {
        Interactable interactable = FindInteractable();

        //if detach rope
        if (interactable && interactable.DetachRope(out interactable, out player.draggingRopeState.ropePositions))
        {
            ConnectToInteractable(interactable);
        }
    }

    void ConnectToInteractable(Interactable interactable)
    {
        //connect to interactable
        player.connectedPoint = interactable;

        //change state to dragging rope
        player.SetState(player.draggingRopeState);
    }

    #endregion
}
