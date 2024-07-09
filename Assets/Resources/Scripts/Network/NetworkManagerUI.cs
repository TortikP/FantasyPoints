using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    public static string STATE_KEY = "State.searching";

    public GameObject lobbyScreen;
    public GameObject codeText;
    public TMP_Text connectionStateText;
    public TMP_Text lobbyCode;
    public TMP_InputField lobbyCodeField;
    public Button stopSearch;
    public LocalizedStringTable localizedStringTable;
    private StringTable stringTable;
    public void Start()
    {
        GameMultiplayerLobby.Instance.networkManagerUI = this;
        stringTable = localizedStringTable.GetTable();
        LocalizationSettings.Instance.OnSelectedLocaleChanged += Instance_OnSelectedLocaleChanged;
    }

    public void StopGame()
    {
        GameMultiplayerManager.Instance.Disconnect();
    }

    public void FindGame()
    {
        STATE_KEY = "State.searching";
        connectionStateText.text = stringTable[STATE_KEY].GetLocalizedString();
        GameMultiplayerLobby.Instance.QuickJoin();
        lobbyScreen.SetActive(true);
    }

    public void StartPrivateLobby()
    {
        STATE_KEY = "Lobby.creating";
        connectionStateText.text = stringTable[STATE_KEY].GetLocalizedString();
        GameMultiplayerLobby.Instance.CreatePrivateLobby("Name");
        lobbyCode.text = GameMultiplayerLobby.Instance.lobbyCode;
        lobbyScreen.SetActive(true);
    }

    public void JoinPrivateLobby()
    {
        if (lobbyCodeField.text == "" || lobbyCodeField.text.Length < 6)
        {
            lobbyCodeField.GetComponent<Animator>().SetTrigger("codeError");
            return;
        }
        STATE_KEY = "Lobby.joining";
        connectionStateText.text = stringTable[STATE_KEY].GetLocalizedString();
        GameMultiplayerLobby.Instance.JoinLobbyByCode(lobbyCodeField.text);
        print(lobbyCodeField.text);
        lobbyScreen.SetActive(true);
    }

    public void StopLobby()
    {
        GameMultiplayerLobby.Instance.LeaveLobby();
        codeText.SetActive(false);
        lobbyScreen.SetActive(false);
    }

    private void Instance_OnSelectedLocaleChanged(Locale obj)
    {
        stringTable = localizedStringTable.GetTable();
        print(stringTable[STATE_KEY].GetLocalizedString());
        connectionStateText.text = stringTable[STATE_KEY].GetLocalizedString();
    }

    private void OnDestroy()
    {
        LocalizationSettings.Instance.OnSelectedLocaleChanged -= Instance_OnSelectedLocaleChanged;
    }
}
