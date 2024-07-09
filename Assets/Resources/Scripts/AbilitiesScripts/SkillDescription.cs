using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SkillDescription : MonoBehaviour
{
    public TMP_Text description;

    public void SetDescription(string newDescription)
    {
        description.text = newDescription;
    }

}
