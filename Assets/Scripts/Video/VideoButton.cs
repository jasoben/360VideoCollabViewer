using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Anthro
{
    /// <summary>
    /// VideoButton. UI button displayed on host machine. Click to play video.
    /// </summary>
    public class VideoButton : MonoBehaviour
    {
        #region Public Methods

        public void SetData(VideoManager v_m, Video v, int i)
        {
            GetComponent<Button>().onClick.AddListener(() => v_m.ButtonPressed_Video(i));
            transform.GetComponentInChildren<TMP_Text>().text = v.Clip.name;
        }

        #endregion
    }
}
