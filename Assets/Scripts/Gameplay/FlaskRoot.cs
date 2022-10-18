using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class FlaskRoot : MonoBehaviour, IDeactivatable
    {
        [SerializeField] private PipeController _pipeController;
        [SerializeField] private SuctionAxisController _suctionAxisController;
        [SerializeField] private float _pipeReturnTime;

        private List<IDeactivatable> _deactivatableDependencies;

        private void Awake()
        {
            Initialize();
        }

        public void Activate()
        {
            foreach (var deactivatable in _deactivatableDependencies) deactivatable.Activate();
        }

        public void Deactivate()
        {
            foreach (var deactivatable in _deactivatableDependencies) deactivatable.Deactivate();
        }

        private void Initialize()
        {
            _deactivatableDependencies = GetComponentsInChildren<IDeactivatable>().ToList();
            _deactivatableDependencies.Remove(this);
            Deactivate();
        }

        public IEnumerator ReturnPipe()
        {
            _pipeController.MoveToStart(_pipeReturnTime);
            _suctionAxisController.MoveToStart(_pipeReturnTime);
            yield return new WaitForSeconds(_pipeReturnTime);
        }

        public IEnumerator MovePipeToLastPosition()
        {
            _suctionAxisController.MoveToLastPosition(_pipeReturnTime);
            yield return new WaitForSeconds(_pipeReturnTime);
        }
    }
}