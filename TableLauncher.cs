using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class TableLauncher : MonoBehaviourPunCallbacks
{
    #region Private fields
    string gameVersion = "v1.0.0";  // Current version of the game
    #endregion
    public bool isConnecting = false;
    public bool joinExisting = false;

    [SerializeField]
    private Text roomNameInput, numOfGlinds, raise, bootValue, buyInAmt, numofPlayers, variation, tableCodeInput, userNameInput, enterValidCode; // These fields are taking value from the user

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // To sync the same scene on all the available users
    }

    #region MonobehaviourPunCallbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("On connected master was called by PUN");
        if (isConnecting)
        {
            RoomOptions roomOps = new RoomOptions();
            roomOps.IsVisible = true;
            roomOps.IsOpen = true;
            roomOps.MaxPlayers = 10;

            //Generating a 8-digit alphanumeric code
            System.Random ran = new System.Random();
            String b = "abcdefghijklmnopqrstuvwxyz0123456789";
            int length = 8;

            String random = "";
            for (int i = 0; i < length; i++)
            {
                int a = ran.Next(b.Length); 
                random = random + b.ElementAt(a);
            }

            roomOps.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            roomOps.CustomRoomProperties.Add("numOfGlinds", numOfGlinds.text);
            roomOps.CustomRoomProperties.Add("raise", raise.text);
            roomOps.CustomRoomProperties.Add("bootValue", bootValue.text);
            roomOps.CustomRoomProperties.Add("buyInAmt", buyInAmt.text);
            roomOps.CustomRoomProperties.Add("numofPlayers", numofPlayers.text);
            roomOps.CustomRoomProperties.Add("variation", variation.text);
            roomOps.CustomRoomProperties.Add("tableCode", random);
            roomOps.CustomRoomPropertiesForLobby = CreateRoomPropertiesForLobby();
            PhotonNetwork.JoinOrCreateRoom(roomNameInput.text, roomOps, TypedLobby.Default);
            isConnecting = false;

        }

        else if(joinExisting)
        {
            ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            expectedCustomRoomProperties.Add("tableCode", tableCodeInput.text);
            PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 10);
        }
    }

    public override void OnCreatedRoom()
    {
        foreach (DictionaryEntry entry in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            Debug.Log(entry.Key + " : " + entry.Value);
        }
        SceneManager.LoadScene(1);
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("OnDisconnected() was called");    
        isConnecting = false;
        joinExisting = false;
    }
    #endregion

    public void OnCreateButtonClick()
    {
        isConnecting = PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }

    public void OnJoinClick()
    {
        joinExisting = PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        enterValidCode.gameObject.SetActive(true);
        Debug.Log("Enter a valid code");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Table joined");
    }

    string[] CreateRoomPropertiesForLobby()
    {
        return new string[]
        {
            "tableCode"
        };
    }
}
