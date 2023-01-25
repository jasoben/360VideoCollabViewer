using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR;

public class GameManager : MonoBehaviourPunCallbacks
{
	public static GameManager Instance;
	
	public GameObject videoControls;

	[Header("Networked Player Prefabs")]
	public GameObject vrPlayerPrefab;
	public GameObject adminPlayerPrefab;

	public static bool IsVRPlayer => XRSettings.isDeviceActive;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}

		Instance = this;
	}

	private void Start()
	{
		Initialise();
	}

	private void Initialise()
	{
		if (PlayerManager.LocalPlayerInstance == null)
		{
			if (IsVRPlayer)
			{
				Player localPlayer = PhotonNetwork.LocalPlayer;

				PhotonNetwork.Instantiate(vrPlayerPrefab.name, Vector3.zero, Quaternion.identity);

				videoControls.SetActive(false);
			}
			else
			{
				PhotonNetwork.Instantiate(adminPlayerPrefab.name, Vector3.zero, Quaternion.identity);
			}
		}
	}

	public void _LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
	}

	#region MonobehaviourPunCallbacks

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		string _msg = string.Format("OnPlayerEnteredRoom() {0}", newPlayer.NickName);
		Debug.LogFormat(_msg);

		if (PhotonNetwork.IsMasterClient)
		{
			Debug.LogFormat(_msg);
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (otherPlayer.IsInactive)
		{
			return;
		}

		string _msg = $"OnPlayerLeftRoom() {otherPlayer.NickName}";
		Debug.LogFormat(_msg);

		if (PhotonNetwork.IsMasterClient)
		{
			_msg = string.Format("OnPlayerLeftRoom IsMasterClient");
			Debug.LogFormat(_msg);
		}
	}

	public override void OnLeftRoom()
	{
		SceneManager.LoadScene(Launcher.launcherSceneName);
	}

	#endregion
}
