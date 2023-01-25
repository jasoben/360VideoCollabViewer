using Anthro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoTime : MonoBehaviour
{
    Text videoTimeLabel;

    #region Unity Methods

    private void Awake()
    {
        videoTimeLabel = GetComponent<Text>();
    }

    private void OnEnable()
    {
        EventsManager.Instance.OnVideoTime += OnVideoTime;
    }

    private void OnDisable()
    {
        EventsManager.Instance.OnVideoTime -= OnVideoTime;
    }

    #endregion

    #region Event Callbacks

    void OnVideoTime(string s)
    {
        videoTimeLabel.text = s;
    }

    #endregion
}

