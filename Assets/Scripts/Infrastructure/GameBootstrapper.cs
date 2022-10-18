using UnityEngine;

namespace Infrastructure
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        private Game _game;

        private void Awake()
        {
            PreventDuplication();
            _game = new Game(this);
            _game.StateMachine.Enter<BootstrapState>();
            DontDestroyOnLoad(this);
        }

        private void PreventDuplication()
        {
            var bootstraps = FindObjectsOfType<GameBootstrapper>();

            if (bootstraps == null) return;

            foreach (var bootstrapper in bootstraps)
                if (bootstrapper != this)
                    Destroy(bootstrapper.gameObject);
        }
    }
}