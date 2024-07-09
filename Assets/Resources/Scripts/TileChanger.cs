using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileChanger : MonoBehaviour
{
    public Sprite[] tiles;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = tiles[(int) GetComponentInParent<BaseLogic>().baseFaction.Value + 1];
        GetComponentInParent<BaseLogic>().baseFaction.OnValueChanged += BaseFaction_OnValueChanged;
    }

    private void BaseFaction_OnValueChanged(Faction previousValue, Faction currentValue)
    {
        GetComponent<SpriteRenderer>().sprite = tiles[(int) currentValue + 1];
    }

    /*public void SetGround(Faction previousFaction,Faction currentFaction)
    {
        if (currentFaction >= 0)
        {
            Tilemap tilemap = GetComponent<Tilemap>();
            tilemap.SwapTile(tile[(int)previousFaction + 1], tile[(int)currentFaction + 1]);
        }

    }*/


}
