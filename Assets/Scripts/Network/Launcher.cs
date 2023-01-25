using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.Android;

public class Launcher : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private TextMeshProUGUI statusLabel;

	private string gameVersion = "1";
	private bool isConnecting;
	private List<string> statusMessages = new List<string>();

	private string roomToLoad = "Anthro Room 1";
	private string sceneToLoad = "Video_Scene";

	static public string launcherSceneName { get; private set; }

	[SerializeField]
	private GameObject localPlayerVR;

	[SerializeField]
	private GameObject localPlayerAdmin;

	private void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
		launcherSceneName = SceneManagerHelper.ActiveSceneName;
		AddStatusMessage("Launcher loaded.");

		Instantiate(GameManager.IsVRPlayer ? localPlayerVR: localPlayerAdmin);

		Connect();
	}

	private void HandleGroupFailed(string errorMessage)
	{
		AddStatusMessage(errorMessage);
		StopAllCoroutines();
	}

	public void Connect()
	{
		string _msg = "Connecting to network";

		Debug.Log(_msg);

		AddStatusMessage(_msg);

		if (PhotonNetwork.IsConnected)
		{
			JoinPhotonRoom(roomToLoad);
		}
		else
		{
			isConnecting = PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.GameVersion = gameVersion;
		}
	}

	void AddStatusMessage(string msg)
	{
		statusMessages.Insert(0, msg);

		int _msgCount = Mathf.Min(statusMessages.Count, 5) - 1;
		string _statusLog = string.Empty;

		for (int i = _msgCount; i >= 0; i--)
		{
			_statusLog += statusMessages[i] + "\n";
		}

		statusLabel.text = _statusLog;
	}

	void JoinPhotonRoom(string roomName)
	{		
		RoomOptions options = new RoomOptions
		{
			PlayerTtl = 10000,
		};

		PhotonNetwork.JoinOrCreateRoom(roomToLoad, options, TypedLobby.Default);
	}

	#region MonobehaviourPunCallbacks

	public override void OnConnectedToMaster()
	{
		string _msg = "OnConnectToMaster() called. IsConnecting = " + isConnecting;

		Debug.Log(_msg);

		AddStatusMessage(_msg);

		if (isConnecting)
		{
			JoinPhotonRoom(roomToLoad);
			isConnecting = false;
		}
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		isConnecting = false;

		string _msg = string.Format("On Disconnect() called with reason {0}. Will attempt to reconnect.", cause);

		Debug.LogWarningFormat(_msg);

		AddStatusMessage(_msg);

		Connect();
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		string _msg = "Failed to join room, retrying in 5s";

		Debug.Log(_msg);

		Debug.Log($"OnJoinRoomFailed returnCode:{returnCode} message:{message}");
		AddStatusMessage(_msg);

		Connect();
	}

	public override void OnJoinedRoom()
	{
		// This only happens for the first player in the room, others load the current network scene
		// and so this doesn't fire

		string _msg = string.Format("You have joined the room {0}.", roomToLoad);

		Debug.Log(_msg);

		AddStatusMessage(_msg);

		// If we are the only player, load into experience scene.
		if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
		{
			_msg = "Loading room " + sceneToLoad;

			Debug.Log(_msg);

			AddStatusMessage(_msg);

			string applicationInfo = string.Format(
			"{0} v: {1}\nunity version: {2}",
			Application.productName,
			Application.version,
			Application.unityVersion);

			PhotonNetwork.LoadLevel(sceneToLoad);
		}
	}

	#endregion
}
