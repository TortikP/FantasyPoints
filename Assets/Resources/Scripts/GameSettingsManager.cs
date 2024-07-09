using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class GameSettingsManager : MonoBehaviour
{
    public TMP_Dropdown dropdownFactions;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnFactionChanged(int factionNumber)
    {
        print(factionNumber);
        print(GameMultiplayerManager.Instance);
        switch (factionNumber)
        {
            case 0:
                GameMultiplayerManager.Instance.playerFaction = Faction.elementalist;
                break;
            case 1:
                GameMultiplayerManager.Instance.playerFaction = Faction.druid;
                break;
            case 2:
                GameMultiplayerManager.Instance.playerFaction = Faction.chaos;
                break;
            default:
                GameMultiplayerManager.Instance.playerFaction = Faction.elementalist;
                break;
        }
        print(GameMultiplayerManager.Instance.playerFaction);
    }

}
