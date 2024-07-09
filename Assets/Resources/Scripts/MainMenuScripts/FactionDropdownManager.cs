using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactionDropdownManager : MonoBehaviour
{

    private void OnEnable()
    {
        GetComponent<TMP_Dropdown>().value = (int)GameMultiplayerManager.Instance.playerFaction;
    }

}
