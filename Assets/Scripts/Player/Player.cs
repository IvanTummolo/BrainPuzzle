using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096;

public class Player : StateMachine
{
    [Header("Minimap")]
    [SerializeField] KeyCode minimapInput = KeyCode.Tab;

    [Header("States")]
    public NormalState normalState;
    public DraggingRopeState draggingRopeState;

    [HideInInspector] public Interactable connectedPoint;

    public System.Action<Transform> onChangeHand { get; set; }

    private void Start()
    {
        //set default state to normalState
        SetState(normalState);
    }

    protected override void Update()
    {
        base.Update();

        //when press input, toggle minimap
        ToggleMinimap(Input.GetKeyDown(minimapInput));
    }

    private void OnDrawGizmosSelected()
    {
        //draw radius pick of normalState
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, normalState.radiusInteract);
    }

    #region private API

    void ToggleMinimap(bool input)
    {
        //when press input, toggle minimap
        if (input)
        {
            GameManager.instance.minimapCamera.ToggleMinimap();
        }
    }

    #endregion

    #region public static API

    /// <summary>
    /// Get touch or mouse position
    /// </summary>
    public static Vector3 InputPosition()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return Input.GetTouch(0).position;
#else
        return Input.mousePosition;
#endif
    }

    #endregion
}
