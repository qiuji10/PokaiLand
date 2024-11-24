using UnityEngine;
using UnityEngine.SceneManagement;

namespace PokaiLand.Events.Internal
{
    public struct OnSceneStartLoaded
    {
        public LoadSceneMode Mode;
        public string SceneName;
        public AsyncOperation AsyncOperation;

        public OnSceneStartLoaded(LoadSceneMode mode, string sceneName, AsyncOperation asyncOperation)
        {
            this.Mode = mode;
            this.SceneName = sceneName;
            this.AsyncOperation = asyncOperation;
        }
    }

    public struct OnSceneEndLoad
    {
    }
}