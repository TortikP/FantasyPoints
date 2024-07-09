using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapLayerSorter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TilemapRenderer>().sortingOrder = Mathf.RoundToInt(-transform.position.y);
    }

}
