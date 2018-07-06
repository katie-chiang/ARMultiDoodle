using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;

public class ExampleMessage : MessageBase
{
	public byte [] byteBuffer;
}

public class ExamplePlayerScript : CaptainsMessPlayer
{
	public Image image;
	public Text nameField;
	public Text readyField;
	public Text rollResultField;
	public Text totalPointsField;

	[SyncVar]
	public Color myColour;

	// Simple game states for a dice-rolling game

	[SyncVar]
	public int rollResult;

	[SyncVar]
	public int totalPoints;

	[SyncVar]
	public bool locationSynced;

	public GameObject spherePrefab;

	private byte[] savedBytes;

	private bool locationSent;

	private ARLocationSync _arLocationSync;
	private ExampleARSessionManager _exampleARSessionManager;

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		_arLocationSync = GetComponent<ARLocationSync> ();
		_exampleARSessionManager = FindObjectOfType<ExampleARSessionManager>();


		// Send custom player info
		// This is an example of sending additional information to the server that might be needed in the lobby (eg. colour, player image, personal settings, etc.)

		myColour = UnityEngine.Random.ColorHSV(0,1,1,1,1,1);
		CmdSetCustomPlayerInfo(myColour);
		locationSent = false;
	}



	public IEnumerator RelocateDevice(byte[] receivedBytes)
	{
		yield return null;
		//actually sync up using arrelocator
		yield return _arLocationSync.Relocate(receivedBytes);
		CmdSetLocationSynced();
		yield return null;
	}

	[Command]
	public void CmdSetLocationSynced()
	{
		locationSynced = true;
	}


	[Command]
	public void CmdSetCustomPlayerInfo(Color aColour)
	{
		myColour = aColour;
	}

	[Command]
	public void CmdRollDie()
	{
		rollResult = UnityEngine.Random.Range(1, 7);
	}


	[Command]
	public void CmdPlayAgain()
	{
		ExampleGameSession.instance.PlayAgain();
	}

	public override void OnClientEnterLobby()
	{
		base.OnClientEnterLobby();

		// Brief delay to let SyncVars propagate
		Invoke("ShowPlayer", 0.5f);
	}

	public override void OnClientReady(bool readyState)
	{
		if (readyState)
		{
			readyField.text = "READY!";
			readyField.color = Color.green;
		}
		else
		{
			readyField.text = "not ready";
			readyField.color = Color.red;
		}
	}

	void ShowPlayer()
	{
		transform.SetParent(GameObject.Find("Canvas/PlayerContainer").transform, false);

		image.color = myColour;	
		nameField.text = deviceName;
		readyField.gameObject.SetActive(true);

		rollResultField.gameObject.SetActive(false);
		totalPointsField.gameObject.SetActive(false);

		OnClientReady(IsReady());
	}


	bool canStartDraw = true;
	GameObject currentBezier = null;

	public void Update()
	{
		string synced = locationSynced ? "SYNC" : "NO";
		totalPointsField.text = "Points: " + totalPoints.ToString() + synced;
		if (rollResult > 0) {
			rollResultField.text = "Roll: " + rollResult.ToString();
		} else {
			rollResultField.text = "";
		}

		if(Input.touchCount < 1){
			return;
		}

		//the instantiation current curve logic
        ExampleGameSession gameSession = ExampleGameSession.instance;
        if(isLocalPlayer && gameSession && gameSession.gameState == GameState.WaitingForRolls){
            
			Touch touch = Input.GetTouch(0);

            if(canStartDraw && touch.phase == TouchPhase.Began){

                Debug.Log("ENTERED instantiation");                            
                Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 point = raycast.GetPoint(2);
                canStartDraw = false;
                //instantiate bezier across all clients
                CmdMakeSphere(point);
                
            }else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){
                //if the user right click, then finished this bezier
                Debug.Log("stop draw");
                currentBezier.GetComponent<BezierMaster.BezierMaster>().stop = true;
                currentBezier = null;
                canStartDraw = true;
            }
        }
	}

	[Command]
    public void CmdMakeSphere(Vector3 position)
    {    
        GameObject obj = (GameObject)Instantiate(spherePrefab, position, Quaternion.identity);
        NetworkServer.SpawnWithClientAuthority(obj, connectionToClient);
        NetworkInstanceId id = obj.GetComponent<NetworkIdentity>().netId;
        Debug.Log("network id: " + id.ToString());
        Debug.Log("INSTANTIATED!!!!!!!!!!!!");
        //dont set colour for now
        //RpcSetSphereColor (sphere, myColour.r, myColour.g, myColour.b);
        RpcgetReferenceToObject(id);
    }

    [ClientRpc]
    void RpcgetReferenceToObject(NetworkInstanceId id){
        //only update currentBezier if it's local player
        if(isLocalPlayer){
            currentBezier = ClientScene.FindLocalObject(id);
            Debug.Log("assigned");
        }
        //if not, then don't do anything to currentBezier
    }

	[ClientRpc]
	public void RpcSetSphereColor(GameObject sphere, float r, float g, float b)
	{
		sphere.GetComponent<Renderer> ().material.color = new Color (r, g, b);
	}


	[ClientRpc]
	public void RpcOnStartedGame()
	{
		readyField.gameObject.SetActive(false);

		rollResultField.gameObject.SetActive(true);
		totalPointsField.gameObject.SetActive(true);
	}

	//this local bool is for drawing one bezier curve for now
	
	//the gui that is displayed on the user's screen

	void OnGUI()
	{
		if (isLocalPlayer)
		{
			GUILayout.BeginArea(new Rect(0, Screen.height * 0.8f, Screen.width, 100));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			ExampleGameSession gameSession = ExampleGameSession.instance;
			if (gameSession)
			{
				if (gameSession.gameState == GameState.Lobby ||
					gameSession.gameState == GameState.Countdown)
				{
					if (GUILayout.Button(IsReady() ? "Not ready" : "Ready", GUILayout.Width(Screen.width * 0.3f), GUILayout.Height(100)))
					{
						if (IsReady()) {
							SendNotReadyToBeginMessage();
						} else {
							SendReadyToBeginMessage();
						}
					}
				}
				else if (gameSession.gameState == GameState.WaitForLocationSync)
				{	
					//world map is sent during location sync
					//cmd is client to server
					if (isServer && !locationSent)
					{
						gameSession.CmdSendWorldMap();
						locationSent = true;
					}
					
				}
				else if (gameSession.gameState == GameState.WaitingForRolls)
				{
						//enter if statement if touched the button
						// if (GUILayout.Button ("Make Sphere", GUILayout.Width (Screen.width * 0.6f), GUILayout.Height (100))) {
						// 	Transform camTransform = _exampleARSessionManager.CameraTransform ();
						// 	Vector3 spherePosition = camTransform.position + (camTransform.forward.normalized * 0.02f); //place sphere 2cm in front of device
						// 	CmdMakeSphere (spherePosition,camTransform.rotation);
						// }

				}
				else if (gameSession.gameState == GameState.GameOver)
				{
					if (isServer)
					{
						if (GUILayout.Button("Play Again", GUILayout.Width(Screen.width * 0.3f), GUILayout.Height(100)))
						{
							CmdPlayAgain();
						}
					}
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
    	}
	}
}
