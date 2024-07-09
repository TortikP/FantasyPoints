using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public enum AbilityHits
{
    OneHit,
    TwoHits
}

[CreateAssetMenu]
public class AbilitySO : ScriptableObject
{
    public int abilityId;
    public string abilityName;
    public int abilityCost;
    public LocalizedString abilityDescription;
    public LocalizedString abilityPrompt;
    public float abilityArea;
    public AbilityHits hits;
    public Sprite abilityImage;
    public GameObject abilityPref;

}
