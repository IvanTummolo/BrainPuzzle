using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(Neuron), true)]
[CanEditMultipleObjects]
public class NeuronEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        //show button only on necessary neurons
        if (IsNecessaryNeuron(target) == false)
            return;

        if (GUILayout.Button("Fill Objects to Activate"))
        {
            //get every activable
            Activable[] activables = FindObjectsOfType<Activable>();

            foreach (Object o in targets)
            {
                //only if cast on neuron
                Neuron neuron = o as Neuron;
                if (neuron == null)
                    continue;

                foreach (Activable activable in activables)
                {
                    //if contains this neuron
                    if (activable.ObjectsForActivate.Contains(neuron))
                    {
                        //add this activable to this neuron
                        AddToNeuron(neuron, activable);
                    }
                }
            }

            //repaint scene
            SceneView.RepaintAll();
        }
    }

    bool IsNecessaryNeuron(Object o)
    {
        if (o is ActivatorNeuron)
            return true;
        else if (o is SwitchNeuron)
            return true;

        return false;
    }

    void AddToNeuron(Interactable interactable, Activable activable)
    {
        //cast interactable to activator neuron or similar, and try add activable to the list
        if (interactable is ActivatorNeuron)
        {
            ActivatorNeuron neuron = interactable as ActivatorNeuron;

            if (neuron.ObjectsToActivate.Contains(activable) == false)
                neuron.ObjectsToActivate.Add(activable);
        }
        else if (interactable is SwitchNeuron)
        {
            SwitchNeuron neuron = interactable as SwitchNeuron;

            if (neuron.ObjectsToActivate.Contains(activable) == false)
                neuron.ObjectsToActivate.Add(activable);
        }
    }
}

#endif

public class Neuron : Interactable
{
}