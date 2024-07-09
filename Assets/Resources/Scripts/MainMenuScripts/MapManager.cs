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
        //���������� ������ �� ������ � ����������� ����
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
        //���� �������� ������ ������ ������
        for (int i = 0;i < mapSOList.mapSO.Length; i++)
        {
            //�������� ������ ������ ��� �������� ������ ���������� ��� �������
            GameObject map = Instantiate(mapTemplate, mapListLayout);
            //���������� ������ �� MapSO � ��������� ������ ������
            map.GetComponent<MapButtonManager>().mapSO = mapSOList.mapSO[i]; 
            //������� ������� ������ ��� ���������� ������
            if(i == 0)
            {
                map.GetComponentInChildren<Button>().GetComponent<Image>().color = Color.grey;
                selectedButton = map.GetComponentInChildren<Button>().GetComponent<Image>();
            }
            //��������� ������ ������ �� ������
            map.GetComponentInChildren<TMP_Text>().text = (i + 1).ToString();
            //��������� ����������� ��� ������� ��� ������� �� ������ 
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
        //����� ������ ���� ���������� ������ �� �����
        previousImage.color = Color.white;
        //����� ������ ���� ��������� ������ �� �����
        currentImage.color = Color.gray;
        //���������� ������ �� ��������� ������
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
