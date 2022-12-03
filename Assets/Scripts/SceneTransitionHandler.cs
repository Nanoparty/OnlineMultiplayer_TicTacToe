using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler : NetworkBehaviour
{
    static public SceneTransitionHandler sceneTransitionHandler { get; private set; }

    [SerializeField]
    public string DefaultMainMenu = "Menu";

    [HideInInspector]
    public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);
    [HideInInspector]
    public event ClientLoadedSceneDelegateHandler OnClientLoadedScene;

    [HideInInspector]
    public delegate void SceneStateChangedDelegateHandler(SceneStates newState);
    [HideInInspector]
    public event SceneStateChangedDelegateHandler OnSceneStateChanged;

    private int numberOfLoadedClients;
    private SceneStates sceneState;

    public enum SceneStates
    {
        Menu,
        Lobby,
        Ingame
    }

    public void SetSceneState(SceneStates newScene)
    {
        sceneState = newScene;
        if (OnSceneStateChanged != null)
        {
            OnSceneStateChanged.Invoke(sceneState);
        }
    }

    public SceneStates GetCurrentSceneState()
    {
        return sceneState;
    }

    private void Awake()
    {
        if (sceneTransitionHandler != this && sceneTransitionHandler != null)
        {
            GameObject.Destroy(sceneTransitionHandler.gameObject);
        }
        sceneTransitionHandler = this;
        //SetSceneState(SceneStates.Init);
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        
    }

    public void RegisterCallbacks()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public void SwitchScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            numberOfLoadedClients = 0;
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        numberOfLoadedClients += 1;
        OnClientLoadedScene?.Invoke(clientId);
    }

    public bool AllClientsAreLoaded()
    {
        return numberOfLoadedClients == NetworkManager.Singleton.ConnectedClients.Count;
    }

    public void ExitAndLoadStartMenu()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        OnClientLoadedScene = null;
        SetSceneState(SceneStates.Menu);
        SceneManager.LoadScene("Menu");
    }
}
