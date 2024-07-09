using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject playerPref;
    public bool pause;
    public bool gameOver;
    public bool showTutorial;
    public TutorialWindowLogic tutorialWindow;
    public PlayerController myPlayerObject;
    public PlayerController enemyObject;
    public AbilitySOList abilitiesSOList;
    public AudioClip[] factionAudio;
    public Material[] materials;
    private List<BaseLogic> baseList = new List<BaseLogic>();
    private GameInterfaceLogic gameInterfaceLogic;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;

#if DEDICATED_SERVER
         Camera.main.enabled = false;         
#endif

        gameInterfaceLogic = FindObjectOfType<GameInterfaceLogic>();
        Instantiate(MapManager.Instance.mapSOList.mapSO.ElementAt(GameMultiplayerManager.Instance.mapIndex.Value).mapPrefab);
        baseList = FindObjectsOfType<BaseLogic>().ToList();
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
        if (GameMultiplayerManager.Instance.isSingleplayer)
        {
            GameObject enemyAI = Instantiate(playerPref);
            enemyAI.GetComponent<PlayerController>().playerFaction.Value = GetRandomFaction();
            enemyAI.GetComponent<PlayerController>().playerId.Value = 1;
            enemyAI.GetComponent<PlayerController>().SetAI(true);
            enemyAI.GetComponent<NetworkObject>().Spawn(true);
            enemyObject = enemyAI.GetComponent<PlayerController>();
            if (GameMultiplayerManager.Instance.mapIndex.Value == 0)
            {
                tutorialWindow.OpenTutorial();
            }
        }
    }
 

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPref);
            player.GetComponent<PlayerController>().playerFaction.Value = GameMultiplayerManager.Instance.GetPlayerDataFromClientId(clientId).playerFaction;
            player.GetComponent<PlayerController>().playerId.Value = GameMultiplayerManager.Instance.GetPlayerDataFromClientId(clientId).playerId;
            int abilityCount = 0;
            for(int i = 0; i < abilitiesSOList.abilities.Count(); i++)
            {
                if (abilitiesSOList.abilities[i].faction == player.GetComponent<PlayerController>().playerFaction.Value)
                {
                    if(abilityCount == 0)
                    {
                        player.GetComponent<PlayerController>().firstAbilityId.Value = abilitiesSOList.abilities[i].ability.abilityId;
                        abilityCount++;
                    }
                    else if (abilityCount == 1)
                    {
                        player.GetComponent<PlayerController>().secondAbilityId.Value = abilitiesSOList.abilities[i].ability.abilityId;
                    }
                }
            }
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            player.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
            print(GameMultiplayerManager.Instance.GetPlayerDataFromClientId(clientId).playerFaction);
        }
        print(clientsCompleted.Count);
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayersReadyServerRpc()
    {
        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            if (!player.isReady.Value)
            {
                return;
            }
        }
        GameLoadedClientRpc();
    }

    [ClientRpc]
    public void GameLoadedClientRpc()
    {
        gameInterfaceLogic.loadingScreen.SetActive(false);
    }

    public IEnumerator EndGame()
    {
        yield return null;
        print(myPlayerObject.mainBases.Value);
        if (myPlayerObject.mainBases.Value <= 0)
        {
            gameInterfaceLogic.TogglePause();
            gameInterfaceLogic.LoseWindow.SetActive(true);
        }
        else
        {
            gameInterfaceLogic.TogglePause();
            gameInterfaceLogic.WinWindow.SetActive(true);
        }
    }

    public List<BaseLogic> BaseList()
    {
        return baseList;
    }

    public Faction GetRandomFaction()
    {
        Faction[] A = (Faction[]) Enum.GetValues(typeof(Faction));
        System.Random r = new System.Random();
        var faction = A.Where(p => p != Faction.neutral).OrderBy(p => r.Next()).FirstOrDefault();
        
        return faction;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        }
    }
}
