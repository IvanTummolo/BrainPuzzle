using UnityEngine;

public class RopeColliderInteraction : MonoBehaviour
{
    public Interactable attachedFrom;

    /// <summary>
    /// Set attached from
    /// </summary>
    public void Init(Interactable attachedFrom)
    {
        this.attachedFrom = attachedFrom;
    }

    public void DestroyRope()
    {
        //rewind to our attached from
        if (attachedFrom)
        {
            attachedFrom.RewindRope();
        }
    }
}
