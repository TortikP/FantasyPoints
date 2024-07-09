using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public enum Scene
    {
        StartUp,
        Menu,
        GameScene
    }

    private static Scene targetScene;
    
    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
}
