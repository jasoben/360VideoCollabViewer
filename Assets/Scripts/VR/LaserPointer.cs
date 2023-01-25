using Photon.Pun;
using UnityEngine;

namespace Anthro
{ 
    /// <summary>
    /// LaserPointer. Clients in VR press touchpad to turn on laser pointer. 
    /// At end of laser pointer is target, which is broadcast to all other clients.
    /// Clients cycle four laser pointer colors: cyan, green, magenta, yellow
    /// </summary>
    public class LaserPointer : MonoBehaviourPunCallbacks
    {
        public static Color[] PointerColors = { Color.cyan, Color.green, Color.magenta, Color.yellow };

        public GameObject CastTarget;
        public float CastLength;
        public Transform R_Controller;
        public Transform L_Controller;

        LineRenderer l;
        Transform currentTransform;

        #region Untiy Methods

        private void Awake()
        {
            l = gameObject.AddComponent<LineRenderer>();
            l.material = Resources.Load("Materials/LaserPointer") as Material;
            l.startWidth = l.endWidth = .01f;
            l.positionCount = 2;
            l.enabled = false;

            l.startColor = l.endColor = PointerColors[photonView.ControllerActorNr % PointerColors.Length];

            CastTarget.GetComponent<Renderer>().material = l.material;
            CastTarget.GetComponent<Renderer>().material.SetColor("_Color", l.startColor);
        }

        private void FixedUpdate()
        { 
            if (OVRInput.Get(OVRInput.Button.PrimaryTouchpad))
            {
                l.enabled = true;
                Cast();      
            }
            else
            {
                l.enabled = false;
            }
        }

        #endregion

        #region Private Methods

        void Cast()
        {
            OVRInput.Controller activeController = OVRInput.GetActiveController();
            currentTransform = OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote) ? R_Controller : L_Controller;
            l.SetPosition(0, currentTransform.position);
            l.SetPosition(1, currentTransform.position + currentTransform.forward * CastLength);
            DrawTarget(currentTransform.position + currentTransform.forward * CastLength);
        }

        void DrawTarget(Vector3 pos)
        {
            CastTarget.transform.position = pos;
        }

        #endregion
    }
}