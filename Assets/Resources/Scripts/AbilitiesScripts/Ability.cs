using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



public class Ability : NetworkBehaviour
{
    public int abilityDuration;
    public int abilityPower;
    public NetworkVariable<PlayerNumber> playerOwner;

}
