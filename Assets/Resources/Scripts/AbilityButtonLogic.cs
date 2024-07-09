using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

public class AbilityButtonLogic : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event EventHandler<bool> OnAbilityClick;
    public event EventHandler IsSelectedOnValueChanged;

    public LocalizedStringTable localizedStringTable;
    public Image abilityImg;
    public Image cancelImg;
    public GameObject abilityTip;
    public TMP_Text abilityTipText;
    public TMP_Text abilityCostText;
    public AbilitySO ability;

    private StringTable stringTable;
    private bool isSelected;
    public bool IsSelected {
        set {
            isSelected = value;
            IsSelectedOnValueChanged?.Invoke(this, EventArgs.Empty);
        }
        get { return isSelected; }
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach(AbilityButtonLogic abilityButton in FindObjectsOfType<AbilityButtonLogic>())
        {
            abilityButton.IsSelectedOnValueChanged += IsSelected_OnValueChanged;
        }
    }

    public void UseAbility()
    {
        if (ability.abilityCost <= GameManager.Instance.myPlayerObject.resourceCount)
        {
            //������� ��������
            OnAbilityClick?.Invoke(this, true);
        }
        else
        {
            //�� ������� ��������
            OnAbilityClick?.Invoke(this, false);
        }
        /*if (GameManager.Instance.myPlayerObject.currentAbility == null)
        {
            if (ability.abilityCost <= GameManager.Instance.myPlayerObject.resourceCount)
            {
                GameManager.Instance.myPlayerObject.currentAbility = ability;
                GameManager.Instance.myPlayerObject.abilityButton = this;
                abilityActionText = ability.abilityPrompt;
                abilityImg.gameObject.SetActive(false);
                cancelImg.gameObject.SetActive(true);
            }
            else
            {
                abilityActionText = "Not enough resources";
            }
            GetComponentInParent<GameInterfaceLogic>().StartCoroutine(GetComponentInParent<GameInterfaceLogic>().ShowActionText(abilityActionText));
        }
        else
        {
            GameManager.Instance.myPlayerObject.currentAbility = null;
            abilityImg.gameObject.SetActive(true);
            cancelImg.gameObject.SetActive(false);
        }*/
    }

    private void IsSelected_OnValueChanged(object sender, EventArgs args)
    {
        if (this == (AbilityButtonLogic) sender)
        {
            print("IsSelected: " + isSelected);
            if (!isSelected)
            {
                abilityImg.gameObject.SetActive(true);
                cancelImg.gameObject.SetActive(false);
            }
            else
            {
                abilityImg.gameObject.SetActive(false);
                cancelImg.gameObject.SetActive(true);
            }
        }
        else
        {
            isSelected = false;
            abilityImg.gameObject.SetActive(true);
            cancelImg.gameObject.SetActive(false);
        }
    }

    public void SetAbility(AbilitySO ability)
    {
        this.ability = ability;
        stringTable = localizedStringTable.GetTable();
        abilityImg.sprite = ability.abilityImage;
        abilityTipText.text = ability.abilityDescription.GetLocalizedString();
        abilityCostText.text = stringTable["ManaCost"].GetLocalizedString() + ": " + ability.abilityCost;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Tip shown");
        abilityTip.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Tip hidden");
        abilityTip.SetActive(false);
    }

}
