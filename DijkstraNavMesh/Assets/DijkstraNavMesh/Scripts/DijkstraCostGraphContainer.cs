using UnityEngine;

namespace DijkstraNavMesh
{
    /// <summary>
    /// This is a class that calculates the cost of each node using Dijkstra's algorithm.  
    /// Attach this component to each object that will serve as a goal for Dijkstra's algorithm.
    /// </summary>
    public class DijkstraCostGraphContainer : MonoBehaviour
    {
        [SerializeField]
        public DijkstraGraphContainer GraphContainer = null;

        [SerializeField]
        public int TopLayerIteration = 50;

        [SerializeField]
        public int SubLayerIteration = 10;

        private bool _initialized;

        private DijkstraCostGraph _costGraph;
        public DijkstraCostGraph CostGraph => _costGraph;

        private void Start()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (GraphContainer != null)
            {
                _initialized = true;
                _costGraph = new DijkstraCostGraph(GraphContainer.Graph);
            }
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            _costGraph.TopLayerIteration = TopLayerIteration;
            _costGraph.SubLayerIteration = SubLayerIteration;
            var closestIndex = _costGraph.GetClosestNodeIndex(transform.position);
            if (closestIndex >= 0)
            {
                _costGraph.Update(closestIndex);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_costGraph != null)
            {
                var count = _costGraph.NodeCount;
                for (int i = 0; i < count; i++)
                {
                    var position = _costGraph.GetPosition(i);
                    var distance = _costGraph.GetCost(i);
                    var guistyle = new GUIStyle
                    {
                        fontSize = 10,
                        normal = { textColor = Color.blue },
                        alignment = TextAnchor.LowerCenter,
                    };
                    UnityEditor.Handles.Label(position, distance.ToString("F1"), guistyle);
                }

                Gizmos.color = Color.blue;
                var cubeSize = Vector3.one * 0.02f;
                for (int i = 0; i < count; i++)
                {
                    var position = _costGraph.GetPosition(i);
                    Gizmos.DrawCube(position, cubeSize);
                }
            }
        }
#endif // UNITY_EDITOR
    }
}
