using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using Unity.Services.Authentication;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class MapManager : MonoBehaviour
{

    public static MapManager Instance { get; private set; }

    [SerializeField] private GameObject mapTemplate;
    [SerializeField] public Transform mapListLayout;
    [SerializeField] public MapsSOList mapSOList;
    public Image selectedButton;
    public int selectedMapIndex;
    public GameObject loadingScreen;

    void Awake()
    {
        //Сохранение ссылки на объект в статические поле
        Instance = this;

#if DEDICATED_SERVER
         Camera.main.enabled = false;

         GameMultiplayerManager.Instance.Disconnect();

         GameMultiplayerLobby.Instance.DeleteLobby();

         GameMultiplayerLobby.Instance.CreateServerLobby("lobby " + AuthenticationService.Instance.PlayerId);
#endif
    }
    // Start is called before the first frame update
    void Start()
    {
        float layoutHeight = mapListLayout.GetComponent<GridLayoutGroup>().cellSize.y + mapListLayout.GetComponent<GridLayoutGroup>().spacing.y;
        int currentColumnCount = 0;
        //Цикл создания кнопок выбора уровня
        for (int i = 0;i < mapSOList.mapSO.Length; i++)
        {
            //Создание кнопки уровня как дочерний объект контейнера для уровней
            GameObject map = Instantiate(mapTemplate, mapListLayout);
            //Сохранение ссылки на MapSO в созданный объект кнопки
            map.GetComponent<MapButtonManager>().mapSO = mapSOList.mapSO[i]; 
            //Отметка первого уровня как выбранного уровня
            if(i == 0)
            {
                map.GetComponentInChildren<Button>().GetComponent<Image>().color = Color.grey;
                selectedButton = map.GetComponentInChildren<Button>().GetComponent<Image>();
            }
            //Установка номера уровня на кнопке
            map.GetComponentInChildren<TMP_Text>().text = (i + 1).ToString();
            //Установка обработчика для события при нажатии на кнопку 
            map.GetComponent<MapButtonManager>().OnMapSelection += Map_OnMapSelection;
            currentColumnCount++;
            if(currentColumnCount >= Mathf.RoundToInt(mapListLayout.GetComponent<RectTransform>().sizeDelta.x /
            (mapListLayout.GetComponent<GridLayoutGroup>().cellSize.x + mapListLayout.GetComponent<GridLayoutGroup>().spacing.x)))
            {
                layoutHeight += mapListLayout.GetComponent<GridLayoutGroup>().cellSize.y + mapListLayout.GetComponent<GridLayoutGroup>().spacing.y;
                currentColumnCount = 0;
            }
            
        }
        mapListLayout.GetComponent<RectTransform>().sizeDelta = new Vector2(mapListLayout.GetComponent<RectTransform>().sizeDelta.x, 
            layoutHeight);
        GameMultiplayerManager.Instance.isSingleplayer = false;
    }

    public void Map_OnMapSelection(Image previousImage, Image currentImage)
    {
        //Смена общего тона предыдущей кнопки на белый
        previousImage.color = Color.white;
        //Смена общего тона выбранной кнопки на серый
        currentImage.color = Color.gray;
        //Сохранение ссылки на выбранную кнопку
        selectedButton = currentImage;
    }

    public void Begin()
    {
        GameMultiplayerManager.Instance.isSingleplayer = true;
        GameMultiplayerManager.Instance.StartSingleplayer();
    }

    public void Quit()
    {
        Application.Quit();
    }
    public void ChooseFaction(int factionID)
    {
        GameMultiplayerManager.Instance.playerFaction = (Faction) factionID;
    }

    public void SetLanguage(string language)
    {
        switch(language)
        {
            case "ru":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                break;
            case "en":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
                break;
        }
        print(language);
    }

}
