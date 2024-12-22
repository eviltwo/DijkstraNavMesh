using UnityEngine;

namespace DijkstraNavMesh
{
    public class DijkstraCostGraphContainer : MonoBehaviour
    {
        [SerializeField]
        private DijkstraGraphContainer _graphContainer = null;

        [SerializeField]
        private int _topLayerIteration = 50;

        [SerializeField]
        private int _subLayerIteration = 10;

        private DijkstraCostGraph _costGraph;
        public DijkstraCostGraph CostGraph => _costGraph;

        private void Start()
        {
            _costGraph = new DijkstraCostGraph(_graphContainer.Graph);
        }

        private void Update()
        {
            _costGraph.TopLayerIteration = _topLayerIteration;
            _costGraph.SubLayerIteration = _subLayerIteration;
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
