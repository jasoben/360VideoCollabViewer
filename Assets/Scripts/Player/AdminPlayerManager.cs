using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class AdminPlayerManager : PlayerManager, IPunInstantiateMagicCallback
{
	[SerializeField]
	private Camera adminCamera;

	[SerializeField]
	private GameObject ui;

	private void Awake()
	{
		if (photonView.IsMine)
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
		LocalPlayerInstance = this;
	}

	private void SetupNetworkedPlayer()
	{
		adminCamera.gameObject.SetActive(false);
	}

	#region Interface Callbacks

	void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
	{
		// e.g. store this gameobject as this player's charater in Player.TagObject
		info.Sender.TagObject = this;
	}

	#endregion
}