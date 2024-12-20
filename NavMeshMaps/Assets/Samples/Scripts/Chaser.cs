using System.Collections.Generic;
using NavMeshMaps;
using UnityEngine;

namespace NavMeshMapsSample
{
    public class Chaser : MonoBehaviour
    {
        [SerializeField]
        private NavMeshTriangleMap _map = null;

        [SerializeField]
        private DistanceMapController _distanceMapController = null;

        [SerializeField]
        private float _speed = 1.0f;

        private bool _hasDestination;
        private Vector3 _destination;
        private List<NavMeshTriangleMap.Polygon> _polygonBuffer = new List<NavMeshTriangleMap.Polygon>();

        private void Update()
        {
            if (!_hasDestination)
            {
                _polygonBuffer.Clear();
                _map.GetPolygons(_polygonBuffer);
                var currentIndex = NavMeshTriUtility.GetClosestPolygon(transform.position, _polygonBuffer);
                var polygon = _polygonBuffer[currentIndex];
                var minScore = float.MaxValue;
                var minIndex = currentIndex;
                foreach (var connection in polygon.connections)
                {
                    var score = _distanceMapController.DistanceMap.GetDistance(connection.to);
                    if (score < minScore)
                    {
                        minScore = score;
                        minIndex = connection.to;
                    }
                }
                _destination = _map.GetPolygon(minIndex).center;
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
