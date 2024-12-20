using System.Collections.Generic;
using NavMeshMaps;
using NavMeshMaps.Extensions;
using UnityEngine;

namespace NavMeshMapsSample
{
    public class DistanceMapController : MonoBehaviour
    {
        [SerializeField]
        private NavMeshTriangleMap _map = null;

        private DistanceMap _distanceMap;
        public DistanceMap DistanceMap => _distanceMap;

        private List<NavMeshTriangleMap.Polygon> _polygonBuffer = new List<NavMeshTriangleMap.Polygon>();

        private void Start()
        {
            _distanceMap = new DistanceMap(_map.GetPolygonCount());
        }

        private void Update()
        {
            _polygonBuffer.Clear();
            _map.GetPolygons(_polygonBuffer);
            var index = NavMeshTriUtility.GetClosestPolygon(transform.position, _polygonBuffer);
            _distanceMap.Update(_map, index);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_map == null || _distanceMap == null)
            {
                return;
            }

            var count = _map.GetPolygonCount();
            for (int i = 0; i < count; i++)
            {
                var polygon = _map.GetPolygon(i);
                var distance = _distanceMap.GetDistance(i);
                var guistyle = new GUIStyle
                {
                    fontSize = 10,
                    normal = { textColor = Color.blue },
                    alignment = TextAnchor.LowerCenter,
                };
                UnityEditor.Handles.Label(polygon.center, distance.ToString("F1"), guistyle);
            }
        }
#endif // UNITY_EDITOR
    }
}
