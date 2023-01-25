using UnityEngine;
using UnityEngine.UI;
using Anthro;

public class VRSubtitleUI : MonoBehaviour
{
    Canvas subtitleCanvas;
    Text subtitleLabel;

    #region Unity Methods

    private void Awake()
    {
        subtitleCanvas = transform.GetComponentInParent<Canvas>();
        subtitleLabel = GetComponent<Text>();
    }

    private void OnEnable()
    {
        EventsManager.Instance.OnSubtitle_Update += OnSubtitleUpdate;
    }

    private void OnDisable()
    {
        EventsManager.Instance.OnSubtitle_Update -= OnSubtitleUpdate;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Back))
        {
            subtitleCanvas.enabled = !subtitleCanvas.enabled;
        }
    }

    #endregion

    #region Event Callbacks

    void OnSubtitleUpdate(string s)
    {
        subtitleLabel.text = s;
    }

    #endregion
}
