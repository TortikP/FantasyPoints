using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMultiplayerManager : NetworkBehaviour
{
    public static GameMultiplayerManager Instance { get; private set; }

    public const int MAX_PLAYER_AMOUNT = 2;

    public NetworkList<PlayerData> playerDataNetworkList;

    public Faction playerFaction = Faction.elementalist;
    public List<Faction> playerFactions;
    public bool isSingleplayer = false;
    public NetworkVariable<int> mapIndex;

    private void Awake()
    {
        Instance = this;
        playerFactions = new List<Faction>();
        playerDataNetworkList = new NetworkList<PlayerData>();
        mapIndex = new NetworkVariable<int>(0);
        if (FindObjectsOfType<GameMultiplayerManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        NetworkManager.Singleton.OnConnectionEvent += Singleton_OnConnectionEvent;

    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback -= NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnLoad += Scene_OnLoad;
        mapIndex.Value = MapManager.Instance.selectedMapIndex;

    }

    public void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback -= NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartServer();
        mapIndex.Value = MapManager.Instance.selectedMapIndex;
    }

    public void Scene_OnLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
        MapManager.Instance.loadingScreen.SetActive(true);
    }

    public void StartSingleplayer()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnLoad += Scene_OnLoad;
        mapIndex.Value = MapManager.Instance.selectedMapIndex;
        Loader.LoadNetwork(Loader.Scene.GameScene);
    }

    private void Singleton_OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEvent)
    {
        if(connectionEvent.EventType == ConnectionEvent.ClientDisconnected)
        {
            if (IsServer)
            {
                for (int i = 0; i < playerDataNetworkList.Count; i++)
                {
                    PlayerData playerData = playerDataNetworkList[i];
                    
                    if (playerData.clientId == connectionEvent.ClientId)
                    {
                        playerDataNetworkList.RemoveAt(i);
                    }
                }
                
                if (networkManager.ConnectedClientsList.Count <= 1 && !IsHost)
                {
                    if(SceneManager.GetActiveScene().name == Loader.Scene.GameScene.ToString())
                    {
                        print("Session finished");
                        Loader.Load(Loader.Scene.Menu);
                    }
                }

                FindObjectOfType<GameInterfaceLogic>().EnemyDisconnect();
            }
            if (IsClient && SceneManager.GetActiveScene().name == Loader.Scene.GameScene.ToString())
            {
                if (connectionEvent.ClientId == NetworkManager.ServerClientId)
                {
                    FindObjectOfType<GameInterfaceLogic>().hostDisconnect.SetActive(true);
                }
                else
                {
                    FindObjectOfType<GameInterfaceLogic>().EnemyDisconnect();
                }
            }
        }
        else if (connectionEvent.EventType == ConnectionEvent.ClientConnected)
        {
            if (connectionEvent.ClientId == NetworkManager.Singleton.LocalClientId && IsHost)
            {
                playerDataNetworkList.Clear();
                playerDataNetworkList.Add(new PlayerData
                {
                    clientId = connectionEvent.ClientId,
                    playerId = playerDataNetworkList.Count,
                    playerFaction = playerFaction
                });
            }
            else if (!IsServer)
            {
                SetPlayerDataServerRpc(new PlayerData
                {
                    clientId = connectionEvent.ClientId,
                    lobbyPlayerId = AuthenticationService.Instance.PlayerId,
                    playerFaction = playerFaction
                });

            }
        }
    }

    public void StartLevel()
    {
        GameMultiplayerLobby.Instance.CloseLobby();
        MapManager.Instance.loadingScreen.SetActive(true);
        Loader.LoadNetwork(Loader.Scene.GameScene);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.OnLoad += Scene_OnLoad;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerDataServerRpc(PlayerData playerData)
    {
        playerData.playerId = playerDataNetworkList.Count;
        playerDataNetworkList.Add(playerData);
        foreach (PlayerData player in playerDataNetworkList)
        {
            print(player.playerId + " " + player.playerFaction + " " + player.lobbyPlayerId);
        }
        print("Client connected");
        if (playerDataNetworkList.Count >= MAX_PLAYER_AMOUNT)
        {
            int mapIndex = UnityEngine.Random.Range(0, MapManager.Instance.mapSOList.mapSO.Length);
            this.mapIndex.Value = mapIndex;
            GameMultiplayerLobby.Instance.SetLoadStateClientRpc();
            GameMultiplayerLobby.Instance.DeleteLobby();
            StartLevel();
        }
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }
        connectionApprovalResponse.Approved = true;
    }


    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }
}
