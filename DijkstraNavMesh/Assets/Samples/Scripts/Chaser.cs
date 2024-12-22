using System.Collections.Generic;
using DijkstraNavMesh;
using UnityEngine;

namespace DijkstraNavMeshSample
{
    public class Chaser : MonoBehaviour
    {
        [SerializeField]
        private DijkstraCostGraphContainer _costGraphContainer = null;

        [SerializeField]
        private float _speed = 1.0f;

        private bool _hasDestination;
        private Vector3 _destination;
        private List<int> _connectedNodeBuffer = new List<int>();

        private void Update()
        {
            if (!_hasDestination)
            {
                var currentIndex = _costGraphContainer.CostGraph.GetClosestNodeIndex(transform.position);
                _costGraphContainer.CostGraph.GetConnectedNodes(currentIndex, _connectedNodeBuffer);
                var minScore = float.MaxValue;
                var minIndex = currentIndex;
                foreach (var connectedNode in _connectedNodeBuffer)
                {
                    var score = _costGraphContainer.CostGraph.GetCost(connectedNode);
                    if (score < minScore)
                    {
                        minScore = score;
                        minIndex = connectedNode;
                    }
                }
                _destination = _costGraphContainer.CostGraph.GetPosition(minIndex);
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
