using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AbilitySOList : ScriptableObject
{
    [System.Serializable]
    public class AbilitySOEntry
    {
        public Faction faction;
        public AbilitySO ability;
    }

    public AbilitySOEntry[] abilities;
}
