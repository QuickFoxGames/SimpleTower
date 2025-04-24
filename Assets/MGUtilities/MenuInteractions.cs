using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace MGUtilities
{
    /// <summary>
    /// Slide: Slides the given transform from the slide start position to the slide end position in effect time seconds
    /// Scale: Scales the given transform from the scale start position to the scale end position in effect time seconds
    /// Fade: Fades the given CavasGroup from 1 to 0 in effect time seconds
    /// </summary>
    public enum InteractionType
    {
        Slide,
        Scale,
        Pop,
        Fade
    }
    public class MenuInteractions : MonoBehaviour
    {
        [SerializeField] private KeyCode m_pauseInteractButton;
        [SerializeField] private GameObject m_pauseMenu;
        private void Update()
        {
            if (Input.GetKeyDown(m_pauseInteractButton)) TogglePauseGame(m_pauseMenu);
        }
        public void QuitGame()
        {
            Application.Quit();
        }
        public void LoadScene(int sceneId)
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(sceneId);
        }
        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(sceneName);
        }
        public void TogglePauseGame(GameObject pauseMenu)
        {
            if (Time.timeScale == 0.0f) Time.timeScale = 1.0f;
            else Time.timeScale = 0.0f;
            ToggleElement(pauseMenu);
        }
        public void ToggleElement(GameObject obj)
        {
            obj.SetActive(!obj.activeInHierarchy);
        }
        public void ActivateElement(GameObject obj)
        {
            obj.SetActive(true);
        }
        public void DeactivateElement(GameObject obj)
        {
            obj.SetActive(false);
        }
        public void DisableUIInteraction(Selectable selectable)
        {
            selectable.interactable = false;
        }
        public void EnableUIInteraction(Selectable selectable)
        {
            selectable.interactable = true;
        }
        public void ActivateElementInteraction(MenuInteracable interacable)
        {
            switch (interacable.m_InteractionType)
            {
                case InteractionType.Slide: 
                    Slide(interacable);
                    break;
                case InteractionType.Scale:
                    Scale(interacable);
                    break;
                case InteractionType.Pop:
                    Pop(interacable);
                    break;
                case InteractionType.Fade:
                    Fade(interacable);
                    break;
            }
        }
        private void Slide(MenuInteracable interacable)
        {
            interacable.m_Coroutine = 
                StartCoroutine(Coroutines.LerpVector3OverTime(interacable.m_slideStart, interacable.m_slideEnd, interacable.m_effectTime, value => interacable.m_transform.position = value));
        }
        private void Scale(MenuInteracable interacable)
        {
            interacable.m_Coroutine =
                StartCoroutine(Coroutines.LerpVector3OverTime(interacable.m_scaleStart, interacable.m_scaleEnd, interacable.m_effectTime, value => interacable.m_transform.localScale = value));
        }
        private void Pop(MenuInteracable interacable)
        {
            interacable.m_Coroutine =
                StartCoroutine(Coroutines.LerpVector3OverPath(interacable.m_popScalePath, interacable.m_effectTime, false, value => interacable.m_transform.localScale = value));
        }
        private void Fade(MenuInteracable interacable)
        {
            interacable.m_Coroutine =
                StartCoroutine(Coroutines.LerpFloatOverTime(1f, 0f, interacable.m_effectTime, value => interacable.m_fadeCanvasGroup.alpha = value));
        }
    }
}