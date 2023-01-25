using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Anthro
{
    /// <summary>
    /// VideoSlider. Simple scrubber control for video. Drag to notfity VideoManager to change video time.
    /// </summary>
    public class VideoSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        VideoManager v_manager;
        
        Slider slider_VideoTime;
        bool pointerDown;
        bool updateSlider = true;
        
        #region Unity Methods

        void Awake()
        {
            slider_VideoTime = GetComponent<Slider>();
            slider_VideoTime.onValueChanged.AddListener((float val) => ScrollbarCallback(val));
        }

        void OnEnable()
        {
            v_manager.v_player.frameReady += VideoFrameReady;
        }

        void OnDisable()
        {
            v_manager.v_player.frameReady -= VideoFrameReady;
        }

        void LateUpdate()
        {
            if (updateSlider)
            {
                slider_VideoTime.value = (float)(v_manager.v_player.time / v_manager.v_player.clip.length);
            }
        }

        #endregion

        #region Event Callbacks

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!pointerDown)
            {
                v_manager.VideoSeekStart();
                updateSlider = false;
                v_manager.v_player.sendFrameReadyEvents = true;
                pointerDown = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerDown)
            {
                v_manager.VideoSeekEnd(slider_VideoTime.value);
                pointerDown = false;
            }
        }

        public void ScrollbarCallback(float time)
        {
            v_manager.VideoSeek(time);
        }

        private void VideoFrameReady(VideoPlayer source, long frameIdx)
        {
            updateSlider = true;
            v_manager.v_player.sendFrameReadyEvents = false;
        }

        #endregion
    }
}
