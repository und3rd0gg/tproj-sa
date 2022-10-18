using System;
using System.Collections;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void Load(string name, string sceneToUnload = null, Action onLoaded = null)
        {
            if (sceneToUnload != null)
            {
                _coroutineRunner.StartCoroutine(UnloadScene(SceneManager.GetSceneByName(sceneToUnload),
                    () => _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded))));
                return;
            }

            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));
        }

        private IEnumerator LoadScene(string name, Action onLoaded = null)
        {
            if (SceneManager.GetSceneByName(name) == SceneManager.GetActiveScene())
            {
                Debug.Log("Loading of an open scene attempt");
                yield break;
            }

            var waitNextScene = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

            while (!waitNextScene.isDone) yield return null;

            onLoaded?.Invoke();
        }

        private IEnumerator UnloadScene(Scene scene, Action onUnloaded = null)
        {
            Debug.Log($"{scene.name} trying to unload..");
            var waitNextScene = SceneManager.UnloadSceneAsync(scene);

            while (!waitNextScene.isDone) yield return null;

            onUnloaded?.Invoke();
        }
    }
}