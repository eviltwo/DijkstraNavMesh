using System.Collections.Generic;
using DijkstraNavMesh;
using UnityEngine;
using UnityEngine.AI;

namespace DijkstraNavMeshSample
{
    public class Chaser : MonoBehaviour
    {
        [SerializeField]
        public DijkstraCostGraphContainer CostGraphContainer = null;

        [SerializeField]
        public float Speed = 1.0f;

        [SerializeField]
        public int WaypointCount = 2;

        [SerializeField]
        public bool AllowSkipWaypoint = true;

        private List<int> _waypoints = new List<int>();

        private void Update()
        {
            if (CostGraphContainer == null)
            {
                return;
            }

            // Set first waypoint.
            if (_waypoints.Count == 0)
            {
                var index = CostGraphContainer.CostGraph.GetClosestNodeIndex(transform.position);
                _waypoints.Add(index);
            }

            // Add waypoints.
            var requiredWaypointCount = Mathf.Max(WaypointCount - _waypoints.Count, 0);
            for (int i = 0; i < requiredWaypointCount; i++)
            {
                var lastIndex = _waypoints[_waypoints.Count - 1];
                var connections = CostGraphContainer.CostGraph.GetConnections(lastIndex);
                var minScore = Vector3.Distance(transform.position, CostGraphContainer.CostGraph.GetPosition(lastIndex)) + CostGraphContainer.CostGraph.GetCost(lastIndex);
                var minIndex = lastIndex;
                foreach (var connection in connections)
                {
                    var score = connection.length + CostGraphContainer.CostGraph.GetCost(connection.toNode);
                    if (score < minScore)
                    {
                        minScore = score;
                        minIndex = connection.toNode;
                    }
                }
                if (minIndex == lastIndex)
                {
                    break;
                }
                _waypoints.Add(minIndex);
            }

            // Move to destination.
            if (_waypoints.Count > 0)
            {
                var destinationIndex = _waypoints[0];
                var destination = CostGraphContainer.CostGraph.GetPosition(destinationIndex);
                transform.position = Vector3.MoveTowards(transform.position, destination, Speed * Time.deltaTime);
                if (Vector3.SqrMagnitude(transform.position - destination) < 0.1f)
                {
                    _waypoints.RemoveAt(0);
                }
            }

            // Skip waypoint if possible.
            if (AllowSkipWaypoint && _waypoints.Count > 1)
            {
                var nextIndex = _waypoints[1];
                var nextPosition = CostGraphContainer.CostGraph.GetPosition(nextIndex);
                var layerMask = 1 << NavMesh.GetAreaFromName("Walkable");
                if (!NavMesh.Raycast(transform.position, nextPosition, out _, layerMask))
                {
                    _waypoints.RemoveAt(0);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (CostGraphContainer == null || _waypoints.Count == 0)
            {
                return;
            }
            Gizmos.color = Color.red;
            var firstPosition = CostGraphContainer.CostGraph.GetPosition(_waypoints[0]);
            Gizmos.DrawLine(transform.position, firstPosition);
            for (int i = 0; i < _waypoints.Count; i++)
            {
                var position = CostGraphContainer.CostGraph.GetPosition(_waypoints[i]);
                Gizmos.DrawSphere(position, 0.1f);
                if (i > 0)
                {
                    var previousPosition = CostGraphContainer.CostGraph.GetPosition(_waypoints[i - 1]);
                    Gizmos.DrawLine(previousPosition, position);
                }
            }
        }
    }
}
