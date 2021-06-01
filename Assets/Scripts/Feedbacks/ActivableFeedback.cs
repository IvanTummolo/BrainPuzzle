using UnityEngine;
using redd096;

[RequireComponent(typeof(Activable))]
public class ActivableFeedback : MonoBehaviour
{
    #region variables in inspector

    [Header("On Active")]
    [SerializeField] ParticleSystem[] particlesOnActive = default;
    [SerializeField] AudioStruct[] soundOnActive = default;

    [Header("On Deactive")]
    [SerializeField] ParticleSystem[] particlesOnDeactive = default;
    [SerializeField] AudioStruct[] soundOnDeactive = default;

    #endregion

    Activable activable;

    private void OnEnable()
    {
        activable = GetComponent<Activable>();

        //add events
        if (activable)
        {
            activable.onActive += OnActive;
            activable.onDeactive += OnDeactive;
        }
    }

    private void OnDisable()
    {
        //remove events
        if (activable)
        {
            activable.onActive -= OnActive;
            activable.onDeactive -= OnDeactive;
        }
    }

    #region events

    void OnActive()
    {
        //instantiate particles and sounds
        ParticlesManager.instance.Play(particlesOnActive, activable.ObjectToControl.transform.position, activable.ObjectToControl.transform.rotation);
        SoundManager.instance.Play(soundOnActive, activable.ObjectToControl.transform.position);
    }

    void OnDeactive()
    {
        //instantiate particles and sounds
        ParticlesManager.instance.Play(particlesOnDeactive, activable.ObjectToControl.transform.position, activable.ObjectToControl.transform.rotation);
        SoundManager.instance.Play(soundOnDeactive, activable.ObjectToControl.transform.position);
    }

    #endregion
}
