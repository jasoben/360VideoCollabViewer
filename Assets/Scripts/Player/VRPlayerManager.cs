using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class VRPlayerManager : PlayerManager, IPunInstantiateMagicCallback
{
	[Header("Player References")]
	[SerializeField]
	private GameObject prefabRoot;

	[SerializeField]
	private GameObject vrCamera;

	[SerializeField]
	private GameObject ovrManager;

	private bool isLocal;

	private void Awake()
	{
		isLocal = photonView.IsMine;

		if (isLocal)
		{
			SetupLocalPlayer();
		}
		else
		{
			SetupNetworkedPlayer();
		}

		DontDestroyOnLoad(gameObject);
	}

	private void SetupLocalPlayer()
	{
		gameObject.name = "Local Player";

		LocalPlayerInstance = this;
		vrCamera.GetComponent<Camera>().enabled = true;
		vrCamera.GetComponent<AudioListener>().enabled = true;

		ovrManager.SetActive(true);

		Player localPlayer = PhotonNetwork.LocalPlayer;
	}

	private void SetupNetworkedPlayer()
	{
		gameObject.name = "Network Player";

		vrCamera.SetActive(false);
	}

	#region Interface Callbacks

	void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
	{
		// e.g. store this gameobject as this player's charater in Player.TagObject
		info.Sender.TagObject = this;
	}
	#endregion
}

