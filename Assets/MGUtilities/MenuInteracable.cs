using UnityEngine;
namespace MGUtilities {
    public class MenuInteracable : MonoBehaviour
    {
        [Tooltip("Slide: Slides the given transform from the slide start position to the slide end position in effect time seconds" +
        "\nScale: Scales the given transform from the scale start position to the scale end position in effect time seconds" +
        "\nFade: Fades the given CavasGroup from 1 to 0 in effect time seconds")]
        public InteractionType m_InteractionType;
        public Vector2 m_slideStart, m_slideEnd;
        public Vector3 m_scaleStart, m_scaleEnd;
        public Vector3[] m_popScalePath;
        public float m_effectTime;
        public Transform m_transform;
        public CanvasGroup m_fadeCanvasGroup;

        public Coroutine m_Coroutine;
    }
}