using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections;
using Photon.Pun;
using System;

namespace Anthro
{
    public class TTML
    {
        public string Text { get; set; }
        public float Start { get; set; }
        public float End { get; set; }
    }

    /// <summary>
    /// Video Class. Has reference to video clip and TextAsset holding subtitle data. 
    /// When video is selected subtitle doc is parsed and loaded into memory. 
    /// While video is playing GetSubtitle is called to update subtitles.
    /// </summary>
    [System.Serializable]
    public class Video
    {
        public int Index;
        public VideoClip Clip;
        public TextAsset Subtitles;

        List<TTML> ttmlElements;

        public void LoadSubtitles()
        {
            if (!Subtitles)
            {
                return;
            }

            XDocument xdoc = XDocument.Parse(Subtitles.text);
            var ns = (from x in xdoc.Root.DescendantsAndSelf()
                      select x.Name.Namespace).First();

            ttmlElements =
            (
            from item in xdoc.Descendants(ns + "body").Descendants(ns + "div").Descendants(ns + "p")
            select new TTML
            {
                Text = ReadInnerXML(item),
                Start = XmlConvert.ToSingle(Regex.Replace(item.Attribute("begin").Value, @"\t|\n|\r|s|m|h", "")),
                End = XmlConvert.ToSingle(Regex.Replace(item.Attribute("end").Value, @"\t|\n|\r|s|m|h", "")),
            }).ToList<TTML>();
        }

        public string GetSubtitle(float time)
        {
            if (ttmlElements.Count < 1)
            {
                return string.Empty;
            }

            int minNum = 0; int maxNum = ttmlElements.Count - 1;
            while (minNum <= maxNum)
            {
                int mid = (minNum + maxNum) / 2;
                if (time >= ttmlElements[mid].Start && time <= ttmlElements[mid].End)
                {
                    return ttmlElements[mid].Text;
                }
                else if (time < ttmlElements[mid].Start)
                {
                    maxNum = mid - 1;
                }
                else
                {
                    minNum = mid + 1;
                }
            }

            return string.Empty;
        }

        private static string ReadInnerXML(XElement parent)
        {
            string temp = "%br%;";
            parent = XElement.Parse(Regex.Replace(parent.ToString(), @"<\/? ?br ?\/?>", temp));
            var reader = parent.CreateReader();
            reader.MoveToContent();

            var innerText = reader.ReadInnerXml();
            innerText = Regex.Replace(innerText, @"\t|\n|\r", "");
            return innerText.Replace(temp, "\n").Trim();
        }
    }

    /// <summary>
    /// Video Manager. Handles loading and playing videos, as well as user controls. 
    /// Sends network messages about state of video.
    /// On scene start displays all videos in scrollable list on host machine.
    /// Videos are clicked to be played.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public List<Video> videos = new List<Video>();
        public Transform videoContainer;
        public GameObject videoControlsPanel;

        [HideInInspector]
        public VideoPlayer v_player;
        [HideInInspector]
        public bool videoStarted;
        [HideInInspector]
        public bool isSeeking;

        private bool seekDone;
        private bool loadDone;

        private bool videoIsPlaying;
        private bool videoIsPaused;
        private int videoIndex;
        private double videoStartTime;

        string currentSubtitle;
        Material panoramicVideo;
        RenderTexture rt;
        Video currentVideo;

        #region Unity Methods

        private void Awake()
        {
            v_player = GetComponent<VideoPlayer>();
            AudioSource videoAudio = gameObject.AddComponent<AudioSource>();
            videoAudio.spatialBlend = 0f;
            v_player.audioOutputMode = VideoAudioOutputMode.AudioSource;
            v_player.SetTargetAudioSource(0, videoAudio);
            panoramicVideo = Resources.Load("Materials/PanoramicVideo") as Material;

            videoControlsPanel.SetActive(false);

            if (!GameManager.IsVRPlayer)
			{
                // Set as master client, to make things easier around syncing
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                // Takeover ownership of this object, as no one alse should have it
                photonView.RequestOwnership();

                // Load video buttons and display
                GameObject button_Video = Resources.Load("Prefabs/Button (Video)") as GameObject;
                for (int i = 0; i < videos.Count; i++)
                {
                    videos[i].Index = i;
                    GameObject obj = Instantiate(button_Video);
                    obj.transform.parent = videoContainer;
                    obj.transform.localScale = Vector3.one;
                    obj.GetComponent<VideoButton>().SetData(this, videos[i], i);
                }
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            Debug.Log($"On App Focus change:{focus}");

            if (GameManager.IsVRPlayer)
			{
                if (!focus)
                {
                    v_player.Pause();
                }
                else
                {
                    if (videoIsPlaying)
                    {
                        SeekToCurrentVideoTime(false);
                    }
                }
            }   
            else
			{
                if (!PhotonNetwork.IsMasterClient)
				{
                    // Set as master client, to make things easier around syncing
                    PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                    // Takeover ownership of this object, as no one alse should have it
                    photonView.RequestOwnership();
                }
            }
            
        }

        public override void OnEnable()
        {
            v_player.prepareCompleted += VideoPreparedComplete;
            v_player.seekCompleted += VideoSeekCompleted;

            base.OnEnable();
        }

        public override void OnDisable()
        {
            v_player.prepareCompleted -= VideoPreparedComplete;
            v_player.seekCompleted -= VideoSeekCompleted;

            base.OnDisable();
        }

        private void Update()
        {
            string videoTime = videoIsPlaying ? v_player.time.ToString("0.00") : "n/a";
            EventsManager.Instance.BroadcastVideoTime(videoTime);

            if (v_player.isPlaying)
            {
                string s = currentVideo.GetSubtitle((float)v_player.time);
                if (s != currentSubtitle)
                {
                    currentSubtitle = s;
                    EventsManager.Instance.BroadcastSubtitle_Update(currentSubtitle);
                }
            }
        }

        #endregion

        #region Event Callbacks
 
        void VideoPreparedComplete(VideoPlayer source)
        {
            loadDone = true;
            StartCoroutine(WaitToShowControls());
        }

        void VideoSeekCompleted(VideoPlayer source)
        {
            seekDone = true;
        }

        #endregion

        #region Button Events

        public void ButtonPressed_Video(int index)
        {
            photonView.RPC(nameof(PlayVideo), RpcTarget.All, index);
        }

        public void ButtonPressed_Play()
        {
            if (v_player.isPaused)
            {
                v_player.Play();
                videoStartTime = PhotonNetwork.Time - v_player.time;
            }
        }

        public void ButtonPressed_Pause()
        {
            if (v_player.isPlaying)
            {
                v_player.Pause();
            }
        }

        public void ButtonPressed_Stop()
        {
            photonView.RPC(nameof(StopVideo), RpcTarget.All);
        }

        #endregion

        #region Public Methods

        public void VideoSeekStart()
        {
            if (v_player.clip)
            {
                isSeeking = true;
                if (v_player.isPlaying)
                {
                    v_player.Pause();
                }
            }
        }

        public void VideoSeek(float time)
        {
            if (v_player.clip && isSeeking)
            {
                v_player.time = time * v_player.clip.length;
            }
        }

        public void VideoSeekEnd(float newTime)
        {
            if (v_player.clip)
            {
                newTime *= (float)v_player.clip.length;
                isSeeking = false;
                v_player.time = newTime;
                videoStartTime = PhotonNetwork.Time - newTime;
            }
        }

        #endregion

        #region Private Methods

        private void LoadVideo()
		{
            loadDone = false;
            // Load video and subtitle data
            currentVideo = videos[videoIndex];
            currentVideo.LoadSubtitles();

            // Prepare render texture and video player
            rt?.Release();
            rt = new RenderTexture((int)currentVideo.Clip.width, (int)currentVideo.Clip.height, 0);
            panoramicVideo.SetTexture("_MainTex", rt);
            v_player.targetTexture = rt;
            v_player.clip = currentVideo.Clip;
            v_player.Prepare();
        }

        private void ClearVideo()
        {
            StopAllCoroutines();
            ClearSubtitles();
            currentVideo = null;
            RenderTexture rtTemp = RenderTexture.active;
            Graphics.SetRenderTarget(rt);
            GL.Clear(false, true, Color.black);
            Graphics.SetRenderTarget(rtTemp);
        }

        private void ClearSubtitles()
		{
            EventsManager.Instance.BroadcastSubtitle_Update(String.Empty);
        }

        private void SeekTo(double t)
        {
            var seekTime = Math.Min(t, currentVideo.Clip.length);
            seekDone = false;
            v_player.time = seekTime;
        }

        private void SeekToCurrentVideoTime(bool loadVideo)
        {
            double offsetTime = (PhotonNetwork.Time - videoStartTime);
            StartCoroutine(PlayVideoFromTimeCoroutine(loadVideo, offsetTime));
        }

        #region PUN2 RPC Calls

        [PunRPC]
        private void PlayVideo(int index)
        {
            if (photonView.IsMine)
			{
                videoIndex = index;
                LoadVideo();
                v_player.Pause();
                videoIsPlaying = true;
            }
        }

        [PunRPC]
        private void StopVideo()
        {
            videoIsPlaying = false;
            v_player.Stop();
            ClearVideo();

            if (!GameManager.IsVRPlayer)
			{
                videoControlsPanel.SetActive(false);
            }
        }

		#endregion

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(videoIsPlaying);
                stream.SendNext(v_player.isPaused);
                stream.SendNext(videoStartTime);
                stream.SendNext(videoIndex);
            }
            else if (stream.IsReading)
            {
                bool newVideoIsPlaying = (bool)stream.ReceiveNext();
                bool newVideoIsPaused = (bool)stream.ReceiveNext();
                videoStartTime = (double)stream.ReceiveNext();
                int newIndex = (int)stream.ReceiveNext();

                videoIsPaused = newVideoIsPaused;

                if (newIndex != videoIndex)
                {
                    videoIndex = newIndex;
                    ClearSubtitles();

                    videoIsPlaying = newVideoIsPlaying;

                    SeekToCurrentVideoTime(true);
                }
                else if (newVideoIsPaused != v_player.isPaused)
                {
                    if (videoIsPaused)
                    {
                        v_player.Pause();
                    }
                    else
                    {
                        SeekToCurrentVideoTime(false);
                    }
                }
            }
        }

        #endregion

		#region Coroutines
		IEnumerator WaitToShowControls()
        {
            yield return new WaitForSeconds(1f);

            videoControlsPanel.SetActive(true);
        }

        private IEnumerator PlayVideoFromTimeCoroutine(bool loadVideo, double t)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (loadVideo)
			{
                LoadVideo();
                yield return new WaitUntil(() => loadDone);
            }

            if (Mathf.Abs(((float)(v_player.time - t + stopwatch.Elapsed.TotalSeconds))) > 1)
            {
                SeekTo(t + stopwatch.Elapsed.TotalSeconds);

                yield return new WaitUntil(() => seekDone);
            }

            if (!videoIsPaused)
			{
                v_player.Play();
            }
        }

		#endregion
    }
}
