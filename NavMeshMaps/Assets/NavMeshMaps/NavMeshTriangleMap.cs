using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshMaps
{
    public class NavMeshTriangleMap : MonoBehaviour
    {
        public class Polygon
        {
            public Vector3[] vertices;
            public Vector3 center;
            public Connection[] connections;
        }

        public class Connection
        {
            public int from;
            public int to;
            public float length;
        }

        public int AreaMask { get; set; }

        private List<Polygon> _polygons = new List<Polygon>();
        private List<Connection> _connections = new List<Connection>();

        private void Awake()
        {
            AreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
        }

        public void Build()
        {
            _polygons.Clear();
            _connections.Clear();

            // Calculate polygons
            var tris = NavMesh.CalculateTriangulation();
            var indicesCount = tris.indices.Length;
            for (int i = 0; i <= indicesCount - 3; i += 3)
            {
                var polygon = new Polygon();
                polygon.vertices = new Vector3[3] { tris.vertices[tris.indices[i]], tris.vertices[tris.indices[i + 1]], tris.vertices[tris.indices[i + 2]] };
                var v0 = tris.vertices[tris.indices[i]];
                var v1 = tris.vertices[tris.indices[i + 1]];
                var v2 = tris.vertices[tris.indices[i + 2]];
                polygon.center = (v0 + v1 + v2) / 3;
                _polygons.Add(polygon);
            }

            // Calculate connections
            var raycastHeight = 0.5f;
            var raycastLayerMask = 1 << LayerMask.NameToLayer("Default");
            var polygonCount = _polygons.Count;
            for (int i = 0; i < polygonCount; i++)
            {
                var currentPoly = _polygons[i];
                var currentPolyHead = currentPoly.center + Vector3.up * raycastHeight;
                for (int j = i + 1; j < polygonCount; j++)
                {
                    var targetPoly = _polygons[j];
                    if (GetSameCount(currentPoly.vertices, targetPoly.vertices) == 0)
                    {
                        continue;
                    }

                    var targetPolyHead = targetPoly.center + Vector3.up * raycastHeight;
                    if (!NavMesh.Raycast(currentPoly.center, targetPoly.center, out _, AreaMask)
                        && !Physics.Raycast(currentPolyHead, (targetPolyHead - currentPolyHead).normalized, (targetPolyHead - currentPolyHead).magnitude, raycastLayerMask))
                    {
                        var length = Vector3.Distance(currentPoly.center, targetPoly.center);
                        _connections.Add(new Connection
                        {
                            from = i,
                            to = j,
                            length = length,
                        });
                        _connections.Add(new Connection
                        {
                            from = j,
                            to = i,
                            length = length,
                        });
                    }
                }
            }

            var connectionBuffer = new List<Connection>();
            for (int i = 0; i < polygonCount; i++)
            {
                connectionBuffer.Clear();
                foreach (var connection in _connections)
                {
                    if (connection.from == i)
                    {
                        connectionBuffer.Add(connection);
                    }
                }
                _polygons[i].connections = connectionBuffer.ToArray();
            }

            Debug.Log($"Polygons: {_polygons.Count}, Connections: {_connections.Count}");
        }

        private int GetSameCount(Vector3[] verts1, Vector3[] verts2)
        {
            var sameVertCount = 0;
            for (int i = 0; i < verts1.Length; i++)
            {
                for (int j = 0; j < verts2.Length; j++)
                {
                    if (verts1[i] == verts2[j])
                    {
                        sameVertCount++;
                    }
                }
            }
            return sameVertCount;
        }

        public int GetPolygonCount()
        {
            return _polygons.Count;
        }

        public Polygon GetPolygon(int index)
        {
            return _polygons[index];
        }

        public void GetPolygons(List<Polygon> resultPolygons)
        {
            resultPolygons.AddRange(_polygons);
        }

        public void GetConnections(List<Connection> resultConnections)
        {
            resultConnections.AddRange(_connections);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            foreach (var polygon in _polygons)
            {
                Gizmos.DrawSphere(polygon.center, 0.1f);
            }
            foreach (var connection in _connections)
            {
                Gizmos.DrawLine(_polygons[connection.from].center, _polygons[connection.to].center);
            }
        }
    }

    public static class NavMeshTriUtility
    {
        public static int GetClosestPolygon(Vector3 position, IReadOnlyList<NavMeshTriangleMap.Polygon> polygons)
        {
            var minDist = float.MaxValue;
            var minIndex = -1;
            for (var i = 0; i < polygons.Count; i++)
            {
                var dist = Vector3.SqrMagnitude(position - polygons[i].center);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIndex = i;
                }
            }
            return minIndex;
        }
    }
}
