namespace BrainPuzzle
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("BrainPuzzle/MonoBehaviours/UI Manager")]
    public class UIManager : MonoBehaviour
    {
        [Header("Menu")]
        [SerializeField] GameObject pauseMenu = default;

        void Start()
        {
            //by default, deactive pause menu
            PauseMenu(false);
        }

        public void PauseMenu(bool active)
        {
            if (pauseMenu == null)
            {
                Debug.LogWarning("There is no pause menu");
                return;
            }

            //active or deactive pause menu
            pauseMenu.SetActive(active);
        }
    }
}