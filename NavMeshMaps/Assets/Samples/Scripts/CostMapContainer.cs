using NavMeshMaps;
using NavMeshMaps.Extensions;
using UnityEngine;

namespace NavMeshMapsSample
{
    public class CostMapContainer : MonoBehaviour
    {
        [SerializeField]
        private NavMeshTriangleMap _map = null;

        private CostMap _costMap;
        public CostMap CostMap => _costMap;

        private void Start()
        {
            _costMap = new CostMap(_map);
        }

        private void Update()
        {
            var closestIndex = _costMap.GetClosestNodeIndex(transform.position);
            if (closestIndex >= 0)
            {
                _costMap.Update(closestIndex);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
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
