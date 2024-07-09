using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;

public class GameMultiplayerLobby : NetworkBehaviour
{
    public static GameMultiplayerLobby Instance { get; private set; }

    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    private const string HOST_IP = "HostIP";
    private const string HOST_PORT = "HostPort";

    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyPullTimer;
    private StringTable stringTable;
    public LocalizedStringTable localizedStringTable;
    public NetworkManagerUI networkManagerUI;


    public string lobbyCode;

    private LobbyEventCallbacks callbacks;
    private ILobbyEvents lobbyEvents;


    // Start is called before the first frame update
    public void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();

        callbacks = new LobbyEventCallbacks();
        callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
        callbacks.LobbyChanged += Callbacks_LobbyChanged;
    }

    private async void Callbacks_LobbyChanged(ILobbyChanges obj)
    {

        obj.ApplyToLobby(joinedLobby);

        print(joinedLobby.Players.Count);
        print(joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value);

        if (AuthenticationService.Instance.PlayerId == joinedLobby.HostId && joinedLobby.Players.Count >= joinedLobby.MaxPlayers && joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value == "0")
        {

            print("Allocating Relay");

            Allocation allocation = await AllocateServerRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                IsPrivate = true,
                Data = new Dictionary<string, DataObject> {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
#if DEDICATED_SERVER

            GameMultiplayerManager.Instance.StartServer();

#endif

#if NO_DEDICATED_SERVER

            GameMultiplayerManager.Instance.StartHost();

            LockGame();
#endif
            return;
        }

        HandleRelayConnection();
    }

    public void OnGameSceneLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
        if(sceneName == Loader.Scene.GameScene.ToString())
        {
            NetworkManagerUI.STATE_KEY = "State.spawning_level";
            networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
        }
    }

    void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {

        print(state + ", lobby is private: " + joinedLobby.IsPrivate);

#if DEDICATED_SERVER
        return;
#endif

        switch (state)
        {
            case LobbyEventConnectionState.Subscribing:

                if(joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    NetworkManagerUI.STATE_KEY = "Lobby.creating";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                }
                else if(joinedLobby.IsPrivate)
                {
                    NetworkManagerUI.STATE_KEY = "Lobby.joining";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                }
                break;
            case LobbyEventConnectionState.Subscribed:
                if (joinedLobby.IsPrivate && joinedLobby.AvailableSlots > 0 && joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    NetworkManagerUI.STATE_KEY = "State.waiting";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                }
                else if (joinedLobby.IsPrivate)
                {
                    NetworkManagerUI.STATE_KEY = "State.connecting";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                }
                break;

        }
    }
    [ClientRpc]
    public void SetLoadStateClientRpc()
    {
        if (networkManagerUI != null)
        {
            NetworkManagerUI.STATE_KEY = "State.loading_level";
            networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
        }
    } 

    void Update()
    {
        HandleHeartbeat();
    }

    private void HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }    
    }

    private async void HandleRelayConnection()
    {
        if (joinedLobby != null) {

            if (joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value != "0")
            {
                if (!IsLobbyHost())
                {
                    print(joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value);

                    LockGame();

                    string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

                    JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

                    GameMultiplayerManager.Instance.StartClient();

                    NetworkManager.Singleton.SceneManager.OnLoad += OnGameSceneLoad;

                    LeaveLobby();
                }
            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

#if DEDICATED_SERVER
        print("Dedicated_server lobby");
#endif

        }

        stringTable = localizedStringTable.GetTable();
        print(stringTable);

        LocalizationSettings.Instance.OnSelectedLocaleChanged += Instance_OnSelectedLocaleChanged;

        Loader.Load(Loader.Scene.Menu);
    }

    private void Instance_OnSelectedLocaleChanged(Locale obj)
    {
        stringTable = localizedStringTable.GetTable();
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

            lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callbacks);

        } catch (LobbyServiceException ex) {

            switch (ex.Reason)
            {
                case LobbyExceptionReason.NoOpenLobbies:
                    print(ex.Reason);
                    NetworkManagerUI.STATE_KEY = "State.no_lobby";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                    break;
                case LobbyExceptionReason.InvalidJoinCode:
                    print(ex.Reason);
                    NetworkManagerUI.STATE_KEY = "State.no_lobby";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                    break;
                default:
                    print(ex.Reason);
                    NetworkManagerUI.STATE_KEY = "State.failed";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                    break;
            }
        }
    }

    public async void CreateLobby(string lobbyName)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameMultiplayerManager.MAX_PLAYER_AMOUNT, new CreateLobbyOptions {
                IsPrivate = false
            });

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions { 
                Data = new Dictionary<string, DataObject> { 
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) } }       
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            GameMultiplayerManager.Instance.StartHost();

            NetworkManager.Singleton.SceneManager.OnLoad += OnGameSceneLoad;
        }
        catch (LobbyServiceException ex) {
            print(ex);
        }
    }

    public async void CreatePrivateLobby(string lobbyName)
    {
        try
        {

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameMultiplayerManager.MAX_PLAYER_AMOUNT, new CreateLobbyOptions
            {
                IsPrivate = true,
                Data = new Dictionary<string, DataObject> {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            });

            lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callbacks);

            lobbyCode = joinedLobby.LobbyCode;

            networkManagerUI.lobbyCode.text = lobbyCode;

            if (joinedLobby.HostId == AuthenticationService.Instance.PlayerId && joinedLobby.IsPrivate)
            {
                networkManagerUI.codeText.SetActive(true);
            }

        }
        catch (LobbyServiceException ex)
        {
            print(ex);
        }
    }

    public async void CreateServerLobby(string lobbyName)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameMultiplayerManager.MAX_PLAYER_AMOUNT + 1, new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject> {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            });

            lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            print(ex);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                SampleResults = false,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                        )
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created),
                }
                
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);

            print(queryResponse.Results.Count);

            foreach(Lobby lobby in queryResponse.Results)
            {
                print(lobby.Name + " " + lobby.Id);
            }

            if (queryResponse.Results.Count > 0)
            {
                joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);

                lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callbacks);

            }
            else
            {
                NetworkManagerUI.STATE_KEY = "State.no_server";
                networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
            }
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.NoOpenLobbies: 
                    print(ex.Reason);
                    NetworkManagerUI.STATE_KEY = "State.no_server";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                    break;
                default: 
                    print(ex.Reason);
                    NetworkManagerUI.STATE_KEY = "State.failed";
                    networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
                    break;
            }

        }
    }
    public async void DeleteLobby()
    {
        if(joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                lobbyEvents = null;

                joinedLobby = null;

                print("Lobby deleted");
            }
            catch (LobbyServiceException ex)
            {
                print(ex);
            }
        }
    }

    public async void CloseLobby()
    {
        if(joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    IsPrivate = true
                });
            }
            catch (LobbyServiceException ex)
            {
                print(ex);
            }
        }
    }

    public async void OpenLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    IsPrivate = false
                });
            }
            catch (LobbyServiceException ex)
            {
                print(ex);
            }
        }
    }
    public async void LeaveLobby()
    {
        if(joinedLobby != null)
        {
            try
            {
                if (!IsLobbyHost())
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                    lobbyEvents = null;

                    joinedLobby = null;
                }
                else
                {
                    await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                    lobbyEvents = null;

                    joinedLobby = null;
                }
            }
            catch (LobbyServiceException ex)
            {
                print(ex);
            }
        }
    }

    public void LockGame()
    {
        NetworkManagerUI.STATE_KEY = "State.connecting";
        networkManagerUI.stopSearch.interactable = false;
        networkManagerUI.connectionStateText.text = stringTable[NetworkManagerUI.STATE_KEY].GetLocalizedString();
    }

    public void OnApplicationQuit()
    {
        GameMultiplayerManager.Instance.Disconnect();
        LeaveLobby();
    }

    public async void KickPlayer(string lobbyPlayerId)
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, lobbyPlayerId);
            }

            catch (LobbyServiceException ex)
            {
                print(ex);
            }
        }
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameMultiplayerManager.MAX_PLAYER_AMOUNT - 1);

            return allocation;
        }
        catch (RelayServiceException ex)
        {
            print(ex);

            return default;
        }
    }

    private async Task<Allocation> AllocateServerRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameMultiplayerManager.MAX_PLAYER_AMOUNT);

            return allocation;
        }
        catch (RelayServiceException ex)
        {
            print(ex);

            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return relayJoinCode;
        }
        catch (RelayServiceException ex)
        {
            print(ex);

            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            return joinAllocation;
        }
        catch (RelayServiceException ex)
        {
            print(ex);

            return default;
        }
    }
}
