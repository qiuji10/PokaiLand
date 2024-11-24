using PokaiLand.Events.Internal;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PokaiLand.Scene
{
    using PokaiLand.Events;
    using Scene = UnityEngine.SceneManagement.Scene;
    
    public sealed class SceneLoaderWrapper : NetworkBehaviour
    {
        private bool IsNetworkSceneManagementEnabled => NetworkManager != null && NetworkManager.SceneManager != null && NetworkManager.NetworkConfig.EnableSceneManagement;
        private bool _mIsInitialized;

        public static SceneLoaderWrapper Instance { get; private set; }

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.OnServerStarted += OnNetworkingSessionStarted;
            NetworkManager.OnClientStarted += OnNetworkingSessionStarted;
            NetworkManager.OnServerStopped += OnNetworkingSessionEnded;
            NetworkManager.OnClientStopped += OnNetworkingSessionEnded;
        }

        private void OnNetworkingSessionStarted()
        {
            if (_mIsInitialized) return;
            
            if (IsNetworkSceneManagementEnabled)
            {
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            }

            _mIsInitialized = true;
        }

        private void OnNetworkingSessionEnded(bool unused)
        {
            if (!_mIsInitialized) return;
            
            if (IsNetworkSceneManagementEnabled)
            {
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            }

            _mIsInitialized = false;
        }

        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (NetworkManager != null)
            {
                NetworkManager.OnServerStarted -= OnNetworkingSessionStarted;
                NetworkManager.OnClientStarted -= OnNetworkingSessionStarted;
                NetworkManager.OnServerStopped -= OnNetworkingSessionEnded;
                NetworkManager.OnClientStopped -= OnNetworkingSessionEnded;
            }
            base.OnDestroy();
        }

        public void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (useNetworkSceneManager)
            {
                if (IsSpawned && IsNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress)
                {
                    if (NetworkManager.IsServer)
                    {
                        NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                    }
                }
            }
            else
            {
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                if (loadSceneMode == LoadSceneMode.Single)
                {
                    EventBus.Execute(new OnSceneStartLoaded(loadSceneMode, sceneName, loadOperation));
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!IsSpawned || NetworkManager.ShutdownInProgress)
            {
                EventBus.Execute(new OnSceneEndLoad());
            }
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load:
                    if (NetworkManager.IsClient)
                    {
                        var sceneName = sceneEvent.SceneName;
                        var loadOperation = sceneEvent.AsyncOperation;
                        
                        EventBus.Execute(new OnSceneStartLoaded(sceneEvent.LoadSceneMode, sceneName, loadOperation));
                    }
                    break;
                case SceneEventType.LoadEventCompleted: // Server told client that all clients finished loading a scene
                    if (NetworkManager.IsClient) // Only executes on client or host
                    {
                        EventBus.Execute(new OnSceneEndLoad());
                    }
                    break;
                case SceneEventType.Synchronize: // Server told client to start synchronizing scenes
                    // Only executes on client that is not the host
                    if (NetworkManager.IsClient && !NetworkManager.IsHost)
                    {
                        if (NetworkManager.SceneManager.ClientSynchronizationMode == LoadSceneMode.Single)
                        {
                            // If using the Single ClientSynchronizationMode, unload all currently loaded additive
                            // scenes. In this case, we want the client to only keep the same scenes loaded as the
                            // server. Netcode For GameObjects will automatically handle loading all the scenes that the
                            // server has loaded to the client during the synchronization process. If the server's main
                            // scene is different to the client's, it will start by loading that scene in single mode,
                            // unloading every additively loaded scene in the process. However, if the server's main
                            // scene is the same as the client's, it will not automatically unload additive scenes, so
                            // we do it manually here.

                            UnloadAdditiveScenes();
                        }
                    }
                    break;
                case SceneEventType.SynchronizeComplete:
                    if (NetworkManager.IsServer)
                    {
                        // Send client RPC to make sure the client stops the loading screen after the server handles what it needs to after the client finished synchronizing,
                        // for example character spawning done server side should still be hidden by loading screen.
                        ClientStopLoadingScreenRpc(RpcTarget.Group(new[] { sceneEvent.ClientId }, RpcTargetUse.Temp));
                    }
                    break;
            }
        }

        private void UnloadAdditiveScenes()
        {
            var activeScene = SceneManager.GetActiveScene();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene != activeScene)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ClientStopLoadingScreenRpc(RpcParams clientRpcParams = default)
        {
            EventBus.Execute(new OnSceneEndLoad());
        }
    }
}