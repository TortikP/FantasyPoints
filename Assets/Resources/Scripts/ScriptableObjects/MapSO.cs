using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MapSO : ScriptableObject
{
    public int mapId;
    public string mapName;
    public GameObject mapPrefab;
}
