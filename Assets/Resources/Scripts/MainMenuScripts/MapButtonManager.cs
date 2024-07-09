using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapButtonManager : MonoBehaviour
{
    public delegate void MapSelectionHandler(Image previousImage, Image currentImage);

    public event MapSelectionHandler OnMapSelection;

    public MapSO mapSO;

    public void MapSelection()
    {
        MapManager.Instance.selectedMapIndex = mapSO.mapId;
        OnMapSelection?.Invoke(MapManager.Instance.selectedButton, GetComponentInChildren<Image>());
    }


}
