using UnityEngine;

namespace DijkstraNavMesh
{
    public class DijkstraGraphContainer : MonoBehaviour
    {
        [SerializeField]
        private bool _buildOnAwake = true;

        private NavMeshTriangleData _triangleData;

        private DijkstraGraph _graph;
        public DijkstraGraph Graph => _graph;

        private void Awake()
        {
            if (_buildOnAwake)
            {
                Build();
            }
        }

        public void Build()
        {
            _triangleData = new NavMeshTriangleData();
            _triangleData.Build();
            _graph = new DijkstraGraph(_triangleData);
        }

        private void OnDrawGizmosSelected()
        {
            if (_graph == null)
            {
                return;
            }
            Gizmos.color = Color.blue;
            foreach (var node in _graph.Nodes)
            {
                Gizmos.DrawSphere(node.position, 0.05f);
                foreach (var connection in node.connections)
                {
                    var fromNode = _graph.Nodes[connection.fromNode];
                    var toNode = _graph.Nodes[connection.toNode];
                    Gizmos.DrawLine(fromNode.position, toNode.position);
                }
            }
        }
    }
}
