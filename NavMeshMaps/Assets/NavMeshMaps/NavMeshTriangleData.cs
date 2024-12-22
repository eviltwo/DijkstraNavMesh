using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshMaps
{
    public class NavMeshTriangleData
    {
        private List<Vector3> _vertices = new List<Vector3>();
        public IReadOnlyList<Vector3> Vertices => _vertices;

        public class Triangle
        {
            public int[] indices;
            public int[] triangleConnections;
            public Vector3 center;
        }

        private List<Triangle> _triangles = new List<Triangle>();
        public IReadOnlyList<Triangle> Triangles => _triangles;

        public class TriangleConnection
        {
            public int triangleA;
            public int triangleB;
            public int overlapedVertexCount;
        }

        private List<TriangleConnection> _triangleConnections = new List<TriangleConnection>();
        public IReadOnlyList<TriangleConnection> TriangleConnections => _triangleConnections;

        private List<int> _verticesTranslationBuffer = new List<int>();
        private List<int> _triangleConnectionBuffer = new List<int>();

        public void Build()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var sourceTris = NavMesh.CalculateTriangulation();

            // Collect and merge vertices
            _vertices.Clear();
            _verticesTranslationBuffer.Clear();
            var sourceVerticesCount = sourceTris.vertices.Length;
            for (int i = 0; i < sourceVerticesCount; i++)
            {
                var vertex = sourceTris.vertices[i];
                var merged = false;
                for (int j = 0; j < _vertices.Count; j++)
                {
                    if (Vector3.SqrMagnitude(vertex - _vertices[j]) < 0.001f)
                    {
                        _verticesTranslationBuffer.Add(j);
                        merged = true;
                        break;
                    }
                }
                if (!merged)
                {
                    _vertices.Add(vertex);
                    _verticesTranslationBuffer.Add(_vertices.Count - 1);
                }
            }

            // Collect triangles
            _triangles.Clear();
            var sourceIndicesCount = sourceTris.indices.Length;
            for (int i = 0; i <= sourceIndicesCount - 3; i += 3)
            {
                var idx0 = sourceTris.indices[i];
                var idx1 = sourceTris.indices[i + 1];
                var idx2 = sourceTris.indices[i + 2];
                var triangle = new Triangle();
                triangle.indices = new int[]
                {
                    _verticesTranslationBuffer[idx0],
                    _verticesTranslationBuffer[idx1],
                    _verticesTranslationBuffer[idx2],
                };
                triangle.center = (_vertices[triangle.indices[0]] + _vertices[triangle.indices[1]] + _vertices[triangle.indices[2]]) / 3;
                _triangles.Add(triangle);
            }

            // Calculate triangle connections
            _triangleConnections.Clear();
            var polygonCount = _triangles.Count;
            for (int i = 0; i < polygonCount; i++)
            {
                var currentPoly = _triangles[i];
                for (int j = i + 1; j < polygonCount; j++)
                {
                    var targetPoly = _triangles[j];
                    var overlapCount = CountSameValues(currentPoly.indices, targetPoly.indices);
                    if (overlapCount < 1)
                    {
                        continue;
                    }
                    _triangleConnections.Add(new TriangleConnection
                    {
                        triangleA = i,
                        triangleB = j,
                        overlapedVertexCount = overlapCount,
                    });
                }
            }
            for (int i = 0; i < _triangles.Count; i++)
            {
                var triangle = _triangles[i];
                _triangleConnectionBuffer.Clear();
                for (int j = 0; j < _triangleConnections.Count; j++)
                {
                    var triangleConnection = _triangleConnections[j];
                    if (triangleConnection.triangleA == i || triangleConnection.triangleB == i)
                    {
                        _triangleConnectionBuffer.Add(j);
                    }
                }
                triangle.triangleConnections = _triangleConnectionBuffer.ToArray();
            }

            Debug.Log($"Polygons: {_triangles.Count}, Connections: {_triangleConnections.Count}, Time: {stopwatch.ElapsedMilliseconds}ms");
        }

        private int CountSameValues(int[] values0, int[] values1)
        {
            var count = 0;
            for (int i = 0; i < values0.Length; i++)
            {
                for (int j = 0; j < values1.Length; j++)
                {
                    if (values0[i] == values1[j])
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
