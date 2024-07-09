using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Action(ulong id);
    public void Action(ulong id, ulong objectId);
    public void Action();
}
