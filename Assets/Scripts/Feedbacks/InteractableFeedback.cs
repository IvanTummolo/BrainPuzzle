using UnityEngine;
using redd096;

[RequireComponent(typeof(Interactable))]
public class InteractableFeedback : MonoBehaviour
{
    #region variables in inspector

    [Header("On Pick Rope")]
    [SerializeField] ParticleSystem[] particlesOnPick = default;
    [SerializeField] AudioStruct[] soundOnPick = default;

    [Header("On Attach")]
    [SerializeField] ParticleSystem[] particlesOnAttach = default;
    [SerializeField] AudioStruct[] soundOnAttach = default;

    [Header("On Detach")]
    [SerializeField] ParticleSystem[] particlesOnDetach = default;
    [SerializeField] AudioStruct[] soundOnDetach = default;

    [Header("On Rewind")]
    [SerializeField] ParticleSystem[] particlesOnRewind = default;
    [SerializeField] AudioStruct[] soundOnRewind = default;

    #endregion

    Interactable interactable;

    void OnEnable()
    {
        interactable = GetComponent<Interactable>();

        //add events
        if(interactable)
        {
            interactable.onPickRope += OnPickRope;
            interactable.onAttach += OnAttach;
            interactable.onDetach += OnDetach;
            interactable.onRewind += OnRewind;
        }
    }

    void OnDisable()
    {
        //remove events
        if (interactable)
        {
            interactable.onPickRope -= OnPickRope;
            interactable.onAttach -= OnAttach;
            interactable.onDetach -= OnDetach;
            interactable.onRewind -= OnRewind;
        }
    }

    #region events

    void OnPickRope(Vector3 position, Quaternion rotation)
    {
        //instantiate particles and sounds
        ParticlesManager.instance.Play(particlesOnPick, position, rotation);
        SoundManager.instance.Play(soundOnPick, position);
    }

    void OnAttach(Vector3 position, Quaternion rotation)
    {
        //instantiate particles and sounds
        ParticlesManager.instance.Play(particlesOnAttach, position, rotation);
        SoundManager.instance.Play(soundOnAttach, position);
    }

    void OnDetach(Vector3 position, Quaternion rotation)
    {
        //instantiate particles and sounds
        ParticlesManager.instance.Play(particlesOnDetach, position, rotation);
        SoundManager.instance.Play(soundOnDetach, position);
    }

    void OnRewind(Vector3 position, Quaternion rotation)
    {
        //instantiate particles and sounds
        ParticlesManager.instance.Play(particlesOnRewind, position, rotation);
        SoundManager.instance.Play(soundOnRewind, position);
    }

    #endregion
}
