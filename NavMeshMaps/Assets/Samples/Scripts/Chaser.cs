using System.Collections.Generic;
using UnityEngine;

namespace NavMeshMapsSample
{
    public class Chaser : MonoBehaviour
    {
        [SerializeField]
        private CostMapContainer _distanceMapController = null;

        [SerializeField]
        private float _speed = 1.0f;

        private bool _hasDestination;
        private Vector3 _destination;
        private List<int> _connectedNodeBuffer = new List<int>();

        private void Update()
        {
            if (!_hasDestination)
            {
                var currentIndex = _distanceMapController.CostMap.GetClosestNodeIndex(transform.position);
                _distanceMapController.CostMap.GetConnectedNodes(currentIndex, _connectedNodeBuffer);
                var minScore = float.MaxValue;
                var minIndex = currentIndex;
                foreach (var connectedNode in _connectedNodeBuffer)
                {
                    var score = _distanceMapController.CostMap.GetCost(connectedNode);
                    if (score < minScore)
                    {
                        minScore = score;
                        minIndex = connectedNode;
                    }
                }
                _destination = _distanceMapController.CostMap.GetPosition(minIndex);
                _hasDestination = true;
            }

            if (_hasDestination)
            {
                transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.deltaTime);
                if (Vector3.SqrMagnitude(transform.position - _destination) < 0.1f)
                {
                    _hasDestination = false;
                }
            }
        }
    }
}
