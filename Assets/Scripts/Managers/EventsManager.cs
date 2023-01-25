using UnityEngine;

namespace Anthro
{
    /// <summary>
    /// EventsManager. Singleton to broadcast events to any objects listening.
    /// </summary>
    public class EventsManager
    {
        private static EventsManager instance;
        public static EventsManager Instance
        {
            get { return instance ?? (instance = new EventsManager()); }
        }

        public delegate void VideoTime(string s);
        public VideoTime OnVideoTime;

        public void BroadcastVideoTime(string s)
        {
            OnVideoTime?.Invoke(s);
        }

        public delegate void Subtitle_Update(string s);
        public Subtitle_Update OnSubtitle_Update;

        public void BroadcastSubtitle_Update(string s)
        {
            OnSubtitle_Update?.Invoke(s);
        }
    }
}
