using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CursorLogic : MonoBehaviour
{
    [SerializeField] public TMP_Text underCursor;
    [SerializeField] private float mouseX, mouseY;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Image img;
    [SerializeField] private GameObject unitCount;
    [SerializeField] private GameObject abilityArea;
    // Start is called before the first frame update
    void Start()
    {
        foreach (AbilityButtonLogic abilityButton in FindObjectsOfType<AbilityButtonLogic>())
        {
            abilityButton.IsSelectedOnValueChanged += AbilityButton_IsSelectedOnValueChanged;
        }
        gameObject.SetActive(false);
    }

    private void AbilityButton_IsSelectedOnValueChanged(object sender, System.EventArgs e)
    {
        AbilityButtonLogic abilityButton = (AbilityButtonLogic)sender;
        if (abilityButton.IsSelected)
        {
            gameObject.SetActive(true);
            abilityArea.SetActive(true);
            mouseX = 0f;
            mouseY = 0f;
            abilityArea.transform.localScale = new Vector3(3 * abilityButton.ability.abilityArea, 3 * abilityButton.ability.abilityArea, 1);
        }
        else
        {
            mouseX = 1.5f;
            mouseY = -1.5f;
            abilityArea.SetActive(false);
            gameObject.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 cursorPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(cursorPos.x + mouseX, cursorPos.y + mouseY);
    }

    public Image GetFillImage()
    {
        return img;
    }
    public void ShowCursor()
    {
        gameObject.SetActive(true);
        unitCount.SetActive(true);
        Vector2 cursorPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(cursorPos.x + mouseX, cursorPos.y + mouseY);
        Debug.Log(gameObject.activeSelf);
    }

    public void HideCursor()
    {
        gameObject.SetActive(false);
        unitCount.SetActive(false);
        Debug.Log(gameObject.activeSelf);
    }

}
