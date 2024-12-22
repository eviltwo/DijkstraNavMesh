using System.Collections.Generic;
using UnityEngine;

namespace DijkstraNavMesh
{
    public class DijkstraGraph
    {
        public class Node
        {
            public Vector3 position;
            public List<Connection> connections = new List<Connection>();
        }

        public class Connection
        {
            public int fromNode;
            public int toNode;
            public float length;
        }

        private readonly List<Node> _nodes = new List<Node>();
        public IReadOnlyList<Node> Nodes => _nodes;

        public DijkstraGraph(NavMeshTriangleData data)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Create nodes
            var vertToNode = new int[data.Vertices.Count];
            for (int i = 0; i < data.Vertices.Count; i++)
            {
                var node = new Node
                {
                    position = data.Vertices[i],
                };
                _nodes.Add(node);
                vertToNode[i] = _nodes.Count - 1;
            }

            var triToNode = new int[data.Triangles.Count];
            for (int i = 0; i < data.Triangles.Count; i++)
            {
                var tri = data.Triangles[i];
                var node = new Node
                {
                    position = tri.center,
                };
                _nodes.Add(node);
                triToNode[i] = _nodes.Count - 1;
            }

            // Create connections
            for (int i = 0; i < data.Triangles.Count; i++)
            {
                var tri = data.Triangles[i];
                var centerNodeIdx = triToNode[i];
                var centerNode = _nodes[centerNodeIdx];
                for (int j = 0; j < tri.indices.Length; j++)
                {
                    // Edge
                    var fromNodeIdx = vertToNode[tri.indices[j]];
                    var fromNode = _nodes[fromNodeIdx];
                    var toNodeIdx = vertToNode[tri.indices[(j + 1) % tri.indices.Length]];
                    var toNode = _nodes[toNodeIdx];
                    var edgeLength = Vector3.Distance(fromNode.position, toNode.position);
                    fromNode.connections.Add(new Connection
                    {
                        fromNode = fromNodeIdx,
                        toNode = toNodeIdx,
                        length = edgeLength,
                    });
                    toNode.connections.Add(new Connection
                    {
                        fromNode = toNodeIdx,
                        toNode = fromNodeIdx,
                        length = edgeLength,
                    });
                    // Center to vertex
                    var vertLength = Vector3.Distance(centerNode.position, fromNode.position);
                    centerNode.connections.Add(new Connection
                    {
                        fromNode = centerNodeIdx,
                        toNode = fromNodeIdx,
                        length = vertLength,
                    });
                    fromNode.connections.Add(new Connection
                    {
                        fromNode = fromNodeIdx,
                        toNode = centerNodeIdx,
                        length = vertLength,
                    });
                }
            }

            for (int i = 0; i < data.TriangleConnections.Count; i++)
            {
                var connection = data.TriangleConnections[i];
                if (connection.overlapedVertexCount < 2)
                {
                    continue;
                }
                var fromNodeIdx = triToNode[connection.triangleA];
                var fromNode = _nodes[fromNodeIdx];
                var toNodeIdx = triToNode[connection.triangleB];
                var toNode = _nodes[toNodeIdx];
                var length = Vector3.Distance(fromNode.position, toNode.position);
                fromNode.connections.Add(new Connection
                {
                    fromNode = fromNodeIdx,
                    toNode = toNodeIdx,
                    length = length,
                });
                toNode.connections.Add(new Connection
                {
                    fromNode = toNodeIdx,
                    toNode = fromNodeIdx,
                    length = length,
                });
            }

            Debug.Log($"Nodes: {_nodes.Count}, Time: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
