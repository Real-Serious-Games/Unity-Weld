using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace UnityWeld
{
    /// <summary>
    /// Initialize all bindings when scene loaded
    /// </summary>
    public static class WeldContext
    {
        /// <summary>
        /// Automatically called before scene load
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void WeldContextStartup()
        {
            SceneManager.sceneLoaded += SceneManager_SceneLoaded;
        }

        static void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var parentT in scene.GetRootGameObjects())
            {
                //Call Init() on all bindings
                foreach (var binding in parentT.GetComponentsInChildren<AbstractMemberBinding>(true)) //including inactive GameObjects
                {
                    binding.Init();
                }
            }
        }
    }
}
