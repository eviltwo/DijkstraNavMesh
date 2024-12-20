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
    }
}
