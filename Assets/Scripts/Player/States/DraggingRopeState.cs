using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096;

[System.Serializable]
public class DraggingRopeState : NormalState
{
    [Header("Joint")]
    [SerializeField] float spring = 200;
    [SerializeField] float damper = 200;
    [SerializeField] float massScale = 100;

    [Header("Rope")]
    [SerializeField] LayerMask ropeLayer = default;
    [SerializeField] float distanceBetweenPoints = 0.5f;

    [Header("Change Hand")]
    [SerializeField] KeyCode changeHand = KeyCode.Space;
    [SerializeField] Transform rightHand = default;
    [SerializeField] Transform leftHand = default;

    SpringJoint joint;
    Vector3 handPosition;
    bool usingRightHand = true;

    int anglePositive;
    [ReadOnly] public List<Vector3> ropePositions = new List<Vector3>();
    Vector3 lastRope => ropePositions[ropePositions.Count - 1];
    Vector3 penultimaRope => ropePositions[ropePositions.Count - 2];

    public override void Enter()
    {
        base.Enter();

        //start with position at connected point
        if (ropePositions.Count <= 0)
            ropePositions.Add(player.connectedPoint.RopeStartPoint.position);

        //create spring joint from connected point        
        RecreateSpringJoint();
    }

    public override void Update()
    {
        base.Update();

        //change hand
        if (Input.GetKeyDown(changeHand))
        {
            ChangeHand();
        }

        //show line renderer
        UpdateRope();
    }

    public override void Exit()
    {
        base.Exit();

        //clear vars
        ropePositions.Clear();
        anglePositive = -1;
    }

    #region private API

    void RecreateSpringJoint()
    {
        //destroy previous joint
        if (joint != null)
            Object.Destroy(joint);

        //add joint
        joint = stateMachine.gameObject.AddComponent<SpringJoint>();

        //set joint connected anchor
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = lastRope;

        //calculate length
        float length = GameManager.instance.levelManager.RopeLength;
        for(int i = 0; i < ropePositions.Count -1; i++)
        {
            length -= Vector3.Distance(ropePositions[i], ropePositions[i + 1]);
        }

        //set joint distances
        joint.maxDistance = length;
        joint.minDistance = 0;

        //set joint spring
        joint.spring = spring;
        joint.damper = damper;
        joint.massScale = massScale;
    }

    void UpdateRope()
    {
        //if player is connected to something
        if (player.connectedPoint != null)
        {
            //get hand position and rpevious previous hand position
            handPosition = usingRightHand ? rightHand.position : leftHand.position;
            RemovePreviousHandPosition();

            //detect rope collision enter
            DetectRopeCollisionEnter();

            //detect rope collision exit
            if (ropePositions.Count > 1)
            {
                //if no angle, check if angle is positive or negative
                if (anglePositive < 0)
                    UpdateAngle();

                DetectRopeCollisionExit();
            }

            //add hand position for lineRenderer
            ropePositions.Add(handPosition);

            //update rope position
            player.connectedPoint.UpdateRope(ropePositions);
        }
    }

    protected override void Interact()
    {
        Interactable interactable = FindInteractable();

        //if attach rope
        if (interactable && player.connectedPoint.AttachRope(interactable))
        {
            //remove player connected point and joint
            player.connectedPoint = null;
            Object.Destroy(joint);

            //back to normal state
            player.SetState(player.normalState);
        }
    }

    protected override void RewindRope()
    {
        //do nothing
    }

    protected override void DetachRope()
    {
        //hide rope and remove every collider
        player.connectedPoint.UpdateRope(new List<Vector3>());
        player.connectedPoint.DestroyAllColliders();

        //remove player connected point and joint
        player.connectedPoint = null;
        Object.Destroy(joint);

        //back to normal state
        player.SetState(player.normalState);        
    }

    void ChangeHand()
    {
        //change hand
        usingRightHand = !usingRightHand;

        //change player hand
        player.onChangeHand(usingRightHand ? rightHand : leftHand);

        //recreate joint
        RecreateSpringJoint();
    }

    #endregion

    #region rope

    void RemovePreviousHandPosition()
    {
        //remove previous hand position
        if (ropePositions.Count > 1)
        {
            ropePositions.Remove(lastRope);
        }
    }

    void DetectRopeCollisionEnter()
    {
        //if hit something
        RaycastHit hit;
        if (Physics.Linecast(handPosition, lastRope, out hit, ropeLayer)
            //&& Vector3.Distance(hit.point, handPosition) < Vector3.Distance(lastRope, handPosition)     //check is near then last point
            && Vector3.Distance(hit.point, lastRope) > distanceBetweenPoints)                           //check distance to not hit always same point
        {
            //be sure didn't hit connected point
            Interactable interactable = hit.transform.GetComponentInParent<Interactable>();
            if (interactable && interactable == player.connectedPoint)
                return;

            //add point and recreate joint, and add collider
            ropePositions.Add(hit.point);
            RecreateSpringJoint();

            //reset angle
            anglePositive = -1;
        }
    }

    void DetectRopeCollisionExit()
    {
        RaycastHit newHit;
        if (Physics.Linecast(handPosition, penultimaRope, out newHit, ropeLayer) == false
            || Vector3.Distance(newHit.point, penultimaRope) < distanceBetweenPoints)   //check if hit near to point, in this case calculate as same point
        {
            //greater than 180 if now is positive and before was negative or viceversa
            if (IsAngleChanged())
            {
                //remove last point and recreate joint, and remove collider
                ropePositions.Remove(lastRope);
                RecreateSpringJoint();

                //reset angle
                anglePositive = -1;
            }
        }
    }

    void UpdateAngle()
    {
        //calculate if angle is positive or negative
        Vector3 directionFromHand = lastRope - handPosition;
        Vector3 directionNext = lastRope - penultimaRope;
        int angle = Mathf.RoundToInt(Vector3.SignedAngle(directionFromHand, directionNext, Vector3.up));

        if (angle > 0 && angle < 180)
            anglePositive = 1;
        else if (angle < 0 && angle > -180)
            anglePositive = 0;
    }

    bool IsAngleChanged()
    {
        //calculate angle
        Vector3 directionFromHand = lastRope - handPosition;
        Vector3 directionNext = lastRope - penultimaRope;
        float angle = Vector3.SignedAngle(directionFromHand, directionNext, Vector3.up);

        //if greater than 180 (now is positive and before was negative or viceversa)
        if (angle > 0 && anglePositive == 0 || angle < 0 && anglePositive == 1)
            return true;

        return false;
    }

    #endregion
}
