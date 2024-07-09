using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHit : MonoBehaviour
{

    float timeToLive = 3f;

    // Update is called once per frame
    void Update()
    {
        if(timeToLive <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color * new Color(1f, 1f, 1f, timeToLive/3f*255);
            timeToLive -= Time.deltaTime;
        }
    }
}
