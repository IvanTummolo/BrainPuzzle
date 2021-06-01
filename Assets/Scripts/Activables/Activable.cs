using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(Activable), true)]
[CanEditMultipleObjects]
public class ActivableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Fill Objects for Activate"))
        {
            //get every interactable
            Interactable[] interactables = FindObjectsOfType<Interactable>();

            foreach (Object o in targets)
            {
                //only if cast on activable
                Activable activable = o as Activable;
                if (activable == null)
                    continue;

                foreach (Interactable interactable in interactables)
                {
                    //if contains this activable
                    if (ContainsActivable(interactable, activable))
                    {
                        //add this interactable to this activable
                        if (activable.ObjectsForActivate.Contains(interactable) == false)
                            activable.ObjectsForActivate.Add(interactable);
                    }
                }
            }

            //repaint scene
            SceneView.RepaintAll();
        }
    }

    bool ContainsActivable(Interactable interactable, Activable activable)
    {
        //cast interactable to activator neuron or similar, and check if activable is inside its list
        if(interactable is ActivatorNeuron)
        {
            return ((ActivatorNeuron)interactable).ObjectsToActivate.Contains(activable);
        }
        else if (interactable is SwitchNeuron)
        {
            return ((SwitchNeuron)interactable).ObjectsToActivate.Contains(activable);
        }

        return false;
    }
}

#endif

public abstract class Activable : MonoBehaviour
{
    [Header("Important")]
    [Tooltip("The object to activate or deactivate")] [SerializeField] GameObject objectToControl = default;

    [Header("Activable")]
    [Tooltip("How many objects of the list to activate? (-1 means all the list)")] [SerializeField] int howManyObjectsToActivate = -1;
    [Tooltip("List of objects necessary to activate this activable")] public List<Interactable> ObjectsForActivate = new List<Interactable>();

    public GameObject ObjectToControl => objectToControl != null ? objectToControl : gameObject;

    public RoomGame RoomParent { get; protected set; }

    protected bool isActive;
    List<Interactable> alreadyActiveObjects = new List<Interactable>();
    int necessaryToActivate => howManyObjectsToActivate < 0 ? ObjectsForActivate.Count : howManyObjectsToActivate;  //necessary number or all the list

    #region events

    public System.Action onActive;
    public System.Action onDeactive;

    #endregion

    protected virtual void Start()
    {
        //save room parent
        RoomParent = GetComponentInParent<RoomGame>();
    }

    protected abstract void Active();

    protected abstract void Deactive();

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        //draw a line to every object necessary to activate this
        foreach (Interactable interactable in ObjectsForActivate)
            if (interactable)
                Gizmos.DrawLine(interactable.ObjectToControl.transform.position + Vector3.right * 0.05f, ObjectToControl.transform.position + Vector3.right * 0.05f);   //a bit moved, to not override Interactable gizmos
    }

    #region private API

    void AddElementInTheList(Interactable interactable)
    {
        //add if not already inside the list
        if (alreadyActiveObjects.Contains(interactable) == false)
        {
            alreadyActiveObjects.Add(interactable);
        }
    }

    void RemoveElementFromTheList(Interactable interactable)
    {
        //remove interactable if inside the list of already active
        if (alreadyActiveObjects.Contains(interactable))
        {
            alreadyActiveObjects.Remove(interactable);
        }
    }

    void TryActivate()
    {
        //do only if not already active
        if (isActive)
            return;

        //if reach necessary
        if(alreadyActiveObjects.Count >= necessaryToActivate)
        {
            isActive = true;
            Active();

            //call event
            onActive?.Invoke();
        }
    }

    void TryDeactivate()
    {
        //do only if not already deactive
        if (isActive == false)
            return;

        //if not reach necessary
        if(alreadyActiveObjects.Count < necessaryToActivate)
        {
            isActive = false;
            Deactive();

            //call event
            onDeactive?.Invoke();
        }
    }

    #endregion

    #region public API

    /// <summary>
    /// Function to activate or deactivate object
    /// </summary>
    /// <param name="interactable">interactable used to call this function</param>
    /// <param name="active">try activate object when true, or deactivate when false</param>
    public virtual void ToggleObject(Interactable interactable, bool active)
    {
        //if interactable is inside the list
        if (ObjectsForActivate.Contains(interactable))
        {
            if (active)
            {
                //add to the list of already active and try activate
                AddElementInTheList(interactable);
                TryActivate();
            }
            else
            {
                //remove from the list of already active and try deactivate
                RemoveElementFromTheList(interactable);
                TryDeactivate();
            }
        }
    }

    #endregion
}
