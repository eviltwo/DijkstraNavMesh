using NavMeshMaps;
using NavMeshMaps.Extensions;
using UnityEngine;

namespace NavMeshMapsSample
{
    public class CostMapContainer : MonoBehaviour
    {
        [SerializeField]
        private DijkstraGraphContainer _graphContainer = null;

        [SerializeField]
        private int _topLayerIteration = 50;

        [SerializeField]
        private int _subLayerIteration = 10;

        private DijkstraCostGraph _costMap;
        public DijkstraCostGraph CostMap => _costMap;

        private void Start()
        {
            _costMap = new DijkstraCostGraph(_graphContainer.Graph);
        }

        private void Update()
        {
            _costMap.TopLayerIteration = _topLayerIteration;
            _costMap.SubLayerIteration = _subLayerIteration;
            var closestIndex = _costMap.GetClosestNodeIndex(transform.position);
            if (closestIndex >= 0)
            {
                _costMap.Update(closestIndex);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_costMap != null)
            {
                var count = _costMap.NodeCount;
                for (int i = 0; i < count; i++)
                {
                    var position = _costMap.GetPosition(i);
                    var distance = _costMap.GetCost(i);
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
                    var position = _costMap.GetPosition(i);
                    Gizmos.DrawCube(position, cubeSize);
                }
            }
        }
#endif // UNITY_EDITOR
    }
}
