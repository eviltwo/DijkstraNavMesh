using System.Collections.Generic;
using DijkstraNavMesh;
using UnityEngine;

namespace DijkstraNavMeshSample
{
    public class Chaser : MonoBehaviour
    {
        [SerializeField]
        public DijkstraCostGraphContainer CostGraphContainer = null;

        [SerializeField]
        public float Speed = 1.0f;

        private bool _hasDestination;
        private Vector3 _destination;
        private List<int> _connectedNodeBuffer = new List<int>();

        private void Update()
        {
            if (CostGraphContainer == null)
            {
                return;
            }

            if (!_hasDestination)
            {
                var currentIndex = CostGraphContainer.CostGraph.GetClosestNodeIndex(transform.position);
                CostGraphContainer.CostGraph.GetConnectedNodes(currentIndex, _connectedNodeBuffer);
                var minScore = float.MaxValue;
                var minIndex = currentIndex;
                foreach (var connectedNode in _connectedNodeBuffer)
                {
                    var score = CostGraphContainer.CostGraph.GetCost(connectedNode);
                    if (score < minScore)
                    {
                        minScore = score;
                        minIndex = connectedNode;
                    }
                }
                _destination = CostGraphContainer.CostGraph.GetPosition(minIndex);
                _hasDestination = true;
            }

            if (_hasDestination)
            {
                transform.position = Vector3.MoveTowards(transform.position, _destination, Speed * Time.deltaTime);
                if (Vector3.SqrMagnitude(transform.position - _destination) < 0.1f)
                {
                    _hasDestination = false;
                }
            }
        }
    }
}
