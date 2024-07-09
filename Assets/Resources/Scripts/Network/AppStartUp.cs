using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class AppStartUp : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabsList _networkPrefabsList;

    private void Start()
    {
        RegisterNetworkPrefabs();
    }

    private void RegisterNetworkPrefabs()
    {
        var prefabs = _networkPrefabsList.PrefabList.Select(x => x.Prefab);
        foreach (var prefab in prefabs)
        {
            NetworkManager.Singleton.AddNetworkPrefab(prefab);
            print("Added");
        }
    }
}
