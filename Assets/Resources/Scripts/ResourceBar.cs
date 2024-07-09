using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode()]
public class ResourceBar : MonoBehaviour
{
    public int maximum;
    public int current;
    public Image filledImage;
    public TMP_Text fillText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetCurrentFill();
    }

    void SetCurrentFill()
    {
        if(current < 0)
        {
            current = 0;
        }
        else if (current > maximum)
        {
            current = maximum;
        }
        float fillAmount = (float) current / (float) maximum;
        string barCount = current.ToString() + "/" + maximum.ToString();
        fillText.text = barCount;
        filledImage.fillAmount = fillAmount;
    }
}
