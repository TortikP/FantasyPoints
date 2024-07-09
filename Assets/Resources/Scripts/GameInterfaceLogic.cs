using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public class GameInterfaceLogic : MonoBehaviour
{
    public const float ABILITY_WAIT_TIME = 5.0f;

    public LocalizedStringTable localizedStringTable;
    public ResourceBar resources;
    public GameObject PauseWindow;
    public GameObject LoseWindow;
    public GameObject WinWindow;
    public GameObject GameMenu;
    public GameObject cursor;
    public GameObject firstSkill;
    public GameObject secondSkill;
    public GameObject actionText;
    public ResourceBar unitsDrawn;
    public GameObject hostDisconnect;
    public GameObject loadingScreen;

    private float currentWaitTime;
    private StringTable stringTable;
    private void Start()
    {
        stringTable = localizedStringTable.GetTable();
        firstSkill.GetComponent<AbilityButtonLogic>().OnAbilityClick += Ability_OnAbilityClick;
        secondSkill.GetComponent<AbilityButtonLogic>().OnAbilityClick += Ability_OnAbilityClick;
    }

    private void Ability_OnAbilityClick(object sender, bool isEnoughResouces)
    {
        AbilityButtonLogic abilityButton = (AbilityButtonLogic)sender;
        if (isEnoughResouces)
        {
            if (!abilityButton.IsSelected)
            {
                abilityButton.IsSelected = true;
                StartCoroutine(ShowActionText(abilityButton.ability.abilityPrompt.GetLocalizedString()));
            }
            else
            {
                abilityButton.IsSelected = false;
            }
        }
        else
        {
            StartCoroutine(ShowActionText(stringTable["Game.no_mana"].GetLocalizedString()));
        }

        print(abilityButton.ability.abilityPrompt);
    }
    // Update is called once per frame
    void Update()
    {
        if(currentWaitTime > 0)
        {
            currentWaitTime -= Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space) && !GameMenu.activeSelf && !GameManager.Instance.gameOver)
        {
            OpenPauseWindow();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !PauseWindow.activeSelf && !GameManager.Instance.gameOver)
        {
            OpenGameMenu();
        }
        if (GameManager.Instance.myPlayerObject != null)
        {
            resources.current = GameManager.Instance.myPlayerObject.resourceCount;
        }
    }

    public void TogglePause()
    {
        if (GameMultiplayerManager.Instance.isSingleplayer)
        {
            if (!GameManager.Instance.pause)
            {
                Debug.Log("Game Paused");
                Time.timeScale = 0f;
                GameManager.Instance.pause = true;
            }
            else
            {
                Time.timeScale = 1f;
                GameManager.Instance.pause = false;
                Debug.Log("Game Unpaused");
            }
        }

    }
    public void OpenPauseWindow()
    {
        TogglePause();
        if (PauseWindow.activeSelf)
        {
            PauseWindow.SetActive(false);
        }
        else
        {
            PauseWindow.SetActive(true);
        }
    }
    public void OpenGameMenu()
    {
        TogglePause();
        if (GameMenu.activeSelf)
            GameMenu.SetActive(false);
        else
            GameMenu.SetActive(true);
    }
    public void ReturnToMainMenu()
    {
        TogglePause();
        loadingScreen.SetActive(true);
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.Menu);
    }

    public void RestartLevel()
    {
        TogglePause();
        loadingScreen.SetActive(true);
        Loader.LoadNetwork(Loader.Scene.GameScene);
    }

    public void EnemyDisconnect()
    {
        StartCoroutine(ShowActionText(stringTable["Game.enemy_disconnected"].GetLocalizedString()));
    }

    public IEnumerator ShowActionText(string abilityActionText)
    {
        actionText.GetComponentInChildren<TMP_Text>().text = abilityActionText;
        actionText.SetActive(true);
        currentWaitTime = ABILITY_WAIT_TIME;
        if(GameManager.Instance.myPlayerObject.currentAbility == null)
        {
            yield return new WaitUntil(() => currentWaitTime <= 0);
        }
        else
        {
            yield return new WaitUntil(() => GameManager.Instance.myPlayerObject.currentAbility == null);
        }
        actionText.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DefineAbilities(List<AbilitySO> abilities)
    {
        firstSkill.GetComponent<AbilityButtonLogic>().SetAbility(abilities[0]);
        secondSkill.GetComponent<AbilityButtonLogic>().SetAbility(abilities[1]);
    }

}
