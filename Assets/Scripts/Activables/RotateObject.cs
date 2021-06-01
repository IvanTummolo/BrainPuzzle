using System.Collections;
using UnityEngine;

public class RotateObject : Activable
{
    [Header("Rotation")]
    [SerializeField] float speedRotation = 1;
    [SerializeField] Vector3 localRotationOnActive = Vector3.zero;

    Quaternion localRotationOnDeactive;
    Coroutine movementCoroutine;

    public System.Action onEndRotation { get; set; }

    protected override void Start()
    {
        base.Start();

        //save rotations when active or deactive
        localRotationOnDeactive = ObjectToControl.transform.localRotation;
    }

    void OnDisable()
    {
        //if deactivate while coroutine is running, set last rotation
        if (movementCoroutine != null)
        {
            Quaternion endRotation = isActive ? Quaternion.Euler(localRotationOnActive) : localRotationOnDeactive;
            ObjectToControl.transform.localRotation = endRotation;

            //call event
            onEndRotation?.Invoke();
            movementCoroutine = null;
        }
    }

    protected override void Active()
    {
        //be sure there is no a coroutine running
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        //start coroutine with active true
        if(gameObject.activeInHierarchy)
            movementCoroutine = StartCoroutine(MovementCoroutine());
    }

    protected override void Deactive()
    {
        //be sure there is no a coroutine running
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        //start coroutine with active false
        if (gameObject.activeInHierarchy)
            movementCoroutine = StartCoroutine(MovementCoroutine());
    }

    IEnumerator MovementCoroutine()
    {
        //set vars
        Quaternion endRotation = isActive ? Quaternion.Euler(localRotationOnActive) : localRotationOnDeactive;

        while (true)
        {
            //rotate
            ObjectToControl.transform.localRotation = Quaternion.RotateTowards(ObjectToControl.transform.localRotation, endRotation, speedRotation * Time.deltaTime);

            //if angle is 0 (reached rotation), stop
            if (Quaternion.Angle(ObjectToControl.transform.localRotation, endRotation) <= 0)
            {
                ObjectToControl.transform.localRotation = endRotation;
                break;
            }

            yield return null;
        }

        //call event
        onEndRotation?.Invoke();
        movementCoroutine = null;
    }

    void OnCollisionEnter(Collision collision)
    {
        //if hit rope collider
        RopeColliderInteraction ropeColliderInteraction = collision.gameObject.GetComponentInParent<RopeColliderInteraction>();
        if (ropeColliderInteraction)
        {
            //destroy rope
            ropeColliderInteraction.DestroyRope();
        }
    }
}
