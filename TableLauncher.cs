using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

public class TableLauncher : MonoBehaviourPunCallbacks
{
    #region Private fields
    string gameVersion = "v1.0.0";  // Current version of the game  
    string tableCode = "";

    bool isConnecting = false;
    bool joinExisting = false;
    bool loadExisting = false;
    string basefileName = "/tableData"; // This will be used to store files and with file number appended at the end
    table tableTobeRestored;
    static int numOfExistingtables;
    public List<string> tableName_list;
    #endregion

    #region Public fields
    public Dropdown listOfTablesDD;
    #endregion

    #region Fields taken from inspector as input
    [SerializeField]
    private Text roomNameInput, numOfGlinds, raise, bootValue, buyInAmt, numofPlayers, variation, tableCodeInput, userNameInput, enterValidCode; // These fields are taking value from the user
    #endregion

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // To sync the same scene on all the available users
    }

    private void Start()
    {
       // PlayerPrefs.SetInt("numoftable", 0);
        numOfExistingtables = PlayerPrefs.GetInt("numoftable");
        for (int i = 0; i < numOfExistingtables; i++)
        {
            LoadTable(i + 1);
            tableName_list.Add(tableTobeRestored.TableName);            
        }
        listOfTablesDD.AddOptions(tableName_list);
    }

    private void CreatingPreviouslySavedTable()
    {
        PhotonNetwork.ConnectUsingSettings();
        loadExisting = true;
    }

    #region MonobehaviourPunCallbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("On connected master was called by PUN");
        if (isConnecting)                     // This will get executed if room is getting created for the first time
        {
            RoomOptions roomOps = new RoomOptions();
            roomOps.IsVisible = true;
            roomOps.IsOpen = true;
            roomOps.MaxPlayers = 10;

            // Generating a 8-digit random alphanumeric code
            System.Random ran = new System.Random();
            string b = "abcdefghijklmnopqrstuvwxyz0123456789";
            int length = 8;
            
            for (int i = 0; i < length; i++)
            {
                int a = ran.Next(b.Length);
                tableCode = tableCode + b.ElementAt(a);
            }
            ////////////////////////////////////////////////

            roomOps.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            roomOps.CustomRoomProperties.Add("numOfGlinds", numOfGlinds.text);
            roomOps.CustomRoomProperties.Add("raise", raise.text);
            roomOps.CustomRoomProperties.Add("bootValue", bootValue.text);
            roomOps.CustomRoomProperties.Add("buyInAmt", buyInAmt.text);
            roomOps.CustomRoomProperties.Add("numofPlayers", numofPlayers.text);
            roomOps.CustomRoomProperties.Add("variation", variation.text);
            roomOps.CustomRoomProperties.Add("tableCode", tableCode);
            roomOps.CustomRoomProperties.Add("roomName", roomNameInput.text);
            roomOps.CustomRoomPropertiesForLobby = CreateRoomPropertiesForLobby();
            PhotonNetwork.JoinOrCreateRoom(roomNameInput.text, roomOps, TypedLobby.Default);
            isConnecting = false;
        }

        else if(joinExisting)               // This will get executed if there is a table join request
        {
            ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            expectedCustomRoomProperties.Add("tableCode", tableCodeInput.text);
            PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 10);  // To join a table with specific code JoinRandomRoom method has to be used and be careful that the roomm that is supposed to be joined isVisible is set to true
        }

        else if(loadExisting)
        {
            RoomOptions roomOps = new RoomOptions();
            roomOps.IsVisible = true;
            roomOps.IsOpen = true;
            roomOps.MaxPlayers = 10;

            roomOps.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            roomOps.CustomRoomProperties.Add("numOfGlinds", tableTobeRestored.NumberOfGlinds);
            roomOps.CustomRoomProperties.Add("raise", tableTobeRestored.Raise);
            roomOps.CustomRoomProperties.Add("bootValue", tableTobeRestored.BootValue);
            roomOps.CustomRoomProperties.Add("buyInAmt", tableTobeRestored.BuyInAmt);
            roomOps.CustomRoomProperties.Add("numofPlayers", tableTobeRestored.NumberOfPlayers);
            roomOps.CustomRoomProperties.Add("variation", tableTobeRestored.Variation);
            roomOps.CustomRoomProperties.Add("tableCode", tableTobeRestored.TableCode);
            roomOps.CustomRoomProperties.Add("roomName", tableTobeRestored.TableName);
            roomOps.CustomRoomPropertiesForLobby = CreateRoomPropertiesForLobby();
            PhotonNetwork.JoinOrCreateRoom(tableTobeRestored.TableName, roomOps, TypedLobby.Default);
        }
    }

    public override void OnCreatedRoom()
    {
        /**foreach (DictionaryEntry entry in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            Debug.Log(entry.Key + " : " + entry.Value);
        }**/
        SceneManager.LoadScene(1);
        if (!loadExisting)
        {
            tableName_list.Add(roomNameInput.text);
            numOfExistingtables = PlayerPrefs.GetInt("numoftable");
            numOfExistingtables++;
            PlayerPrefs.SetInt("numoftable", numOfExistingtables);
            SaveTable();     // Once a new room is created save this table configuration for later use
        }
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("OnDisconnected() was called");    
        isConnecting = false;
        joinExisting = false;
        loadExisting = false;
    }
    #endregion

    #region Public Functions that are trigerred on user's input
    public void OnCreateButtonClick()
    {
        isConnecting = PhotonNetwork.ConnectUsingSettings();  // Connect to Photon server
        PhotonNetwork.GameVersion = gameVersion;
    }

    public void OnJoinButtonClick()
    {
        joinExisting = PhotonNetwork.ConnectUsingSettings();  // Connect to Photon server
        PhotonNetwork.GameVersion = gameVersion;
    }
    #endregion

    string[] CreateRoomPropertiesForLobby()  // Found this on Unity forum - CustomRoomPropertiesForLobby has to be defined in order for JoinRandomRoom function to work
    {
        return new string[]
        {
            "tableCode"
        };
    }

    #region Saving and Loading tables function
    void SaveTable()
    {
        FileStream file = null;

        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            file = File.Create(Application.persistentDataPath + basefileName + numOfExistingtables.ToString());

            table t = new table(roomNameInput.text, tableCode, numOfGlinds.text, bootValue.text, raise.text, buyInAmt.text, numofPlayers.text, variation.text);
            bf.Serialize(file, t);
        }
        catch(Exception e)
        {
            if (e != null) ;
                // Handle Exception
        }
        finally
        {
            if (file != null)
                file.Close();
        }
    }

    void LoadTable(int a)
    {
        FileStream file = null;
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            file = File.Open(Application.persistentDataPath +  basefileName + a.ToString(), FileMode.Open);
            tableTobeRestored = bf.Deserialize(file) as table;
        }
        catch(Exception e)
        {
            if (e != null) ;
            // Handle Exception
        }
        finally
        {
            if (file != null)
                file.Close();
        }
    }
    #endregion

    public void ListOfTablesDropdown_ValueChanged(int option_selected)
    {
        LoadTable(option_selected + 1);
        CreatingPreviouslySavedTable();
    }
}
