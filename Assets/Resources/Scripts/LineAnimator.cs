using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAnimator : MonoBehaviour
{
    Material lineMaterial;
    float textureOffset = 0;

    void Awake()
    {
        lineMaterial = GetComponent<LineRenderer>().materials[0];
    }
    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            textureOffset -= Time.deltaTime;
            lineMaterial.mainTextureOffset = new Vector2(textureOffset, 0.0f);
            if(textureOffset < -1.0f)
            {
                textureOffset = 0.0f;
            }
        }
    }

    void OnEnable()
    {
        lineMaterial.mainTextureOffset = new Vector2(0.0f, 0.0f);
        print(lineMaterial.mainTexture);
        print(lineMaterial);
    }

    void OnDisable()
    {
        textureOffset = 0.0f;
    }
}
