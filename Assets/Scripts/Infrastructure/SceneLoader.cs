using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void Load(string name, Action onLoaded = null)
        {
            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));
        }

        public IEnumerator LoadScene(string name, Action onLoaded = null)
        {
            if (SceneManager.GetSceneByName(name) == SceneManager.GetActiveScene())
            {
                Debug.Log("Loading of an open scene attempt");
                yield break;
            }

            var waitNextScene = SceneManager.LoadSceneAsync(name);

            while (!waitNextScene.isDone) yield return null;

            onLoaded?.Invoke();
        }
    }
}