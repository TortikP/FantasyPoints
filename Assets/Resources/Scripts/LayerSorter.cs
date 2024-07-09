using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSorter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach(SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
