using UnityEngine;
using redd096;

[RequireComponent(typeof(Player))]
public class PlayerFeedback : MonoBehaviour
{
    #region variables in inspector

    [Header("On Change Hand")]
    [SerializeField] ParticleSystem[] particlesOnChangeHand = default;
    [SerializeField] AudioStruct[] soundOnChangeHand = default;

    #endregion

    Player player;

    private void OnEnable()
    {
        player = GetComponent<Player>();

        //add events
        if(player)
        {
            player.onChangeHand += OnChangeHand;
        }
    }

    private void OnDisable()
    {
        //remove events
        if(player)
        {
            player.onChangeHand -= OnChangeHand;
        }
    }

    #region events

    void OnChangeHand(Transform hand)
    {
        //instantiate particles and sounds
        ParticlesManager.instance.Play(particlesOnChangeHand, hand.position, hand.rotation);
        SoundManager.instance.Play(soundOnChangeHand, hand.position);
    }

    #endregion
}
