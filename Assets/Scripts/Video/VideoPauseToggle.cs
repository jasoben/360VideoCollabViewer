using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Anthro
{
    /// <summary>
    /// VideoPauseToggle. Toggles pause and play button visibility.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoPauseToggle : MonoBehaviour
    {
        public Button PlayButton;
        public Button PauseButton;

        VideoPlayer v_player;

        #region Unity Methods

        private void Awake()
        {
            v_player = GetComponent<VideoPlayer>();
            ToggleButtons(true);
        }

        private void Update()
        {
            ToggleButtons(v_player.isPlaying);
        }

        void ToggleButtons(bool showPause)
        {
            PlayButton.gameObject.SetActive(!showPause);
            PauseButton.gameObject.SetActive(showPause);
        }

        #endregion
    }
}