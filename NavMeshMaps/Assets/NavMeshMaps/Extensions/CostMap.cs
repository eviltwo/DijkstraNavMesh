using System.Collections.Generic;
using System.Linq;
using NavMeshMaps.UnityExtensions;
using UnityEngine;

namespace NavMeshMaps.Extensions
{
    public class CostMap
    {
        private readonly CostLayer _topLayer;
        private readonly CostLayer[] _subLayers;
        private int _editSubLayerIndex = 0;
        private int _stableSubLayerIndex = 1;
        private bool _subLayerInitialized;

        private readonly Graph _graph;

        public int NodeCount => _graph.Nodes.Count;

        public int TopLayerIteration { get; set; } = 50;

        public int SubLayerIteration { get; set; } = 10;

        public CostMap(NavMeshTriangleMap navMeshTriangleMap)
        {
            _graph = new Graph(navMeshTriangleMap);
            _topLayer = new CostLayer(_graph);
            _subLayers = new CostLayer[2];
            for (int i = 0; i < _subLayers.Length; i++)
            {
                _subLayers[i] = new CostLayer(_graph);
            }
        }

        public void Update(int startNodeIndex)
        {
            if (!_subLayerInitialized)
            {
                var editLayer = _subLayers[_editSubLayerIndex];
                editLayer.Setup(startNodeIndex);
                var stableLayer = _subLayers[_stableSubLayerIndex];
                stableLayer.Setup(startNodeIndex);
                _subLayerInitialized = true;
            }

            // Top layer
            {
                _topLayer.Setup(startNodeIndex);
                for (int i = 0; i < TopLayerIteration; i++)
                {
                    _topLayer.ForwardStep();
                    if (_topLayer.IsFinished())
                    {
                        break;
                    }
                }
            }

            // Sub layers
            {
                // Forward step
                var editLayer = _subLayers[_editSubLayerIndex];
                for (int i = 0; i < SubLayerIteration; i++)
                {
                    editLayer.ForwardStep();
                    if (editLayer.IsFinished())
                    {
                        break;
                    }
                }
                // Swap layers
                if (editLayer.IsFinished())
                {
                    var oldEditLayerIndex = _editSubLayerIndex;
                    _editSubLayerIndex = _stableSubLayerIndex;
                    _stableSubLayerIndex = oldEditLayerIndex;
                    editLayer = _subLayers[_editSubLayerIndex];
                    editLayer.Setup(startNodeIndex);
                }
            }
        }

        public int GetClosestNodeIndex(Vector3 position)
        {
            var minDistance = float.MaxValue;
            var minIndex = -1;
            for (int i = 0; i < _graph.Nodes.Count; i++)
            {
                var distance = Vector3.SqrMagnitude(_graph.Nodes[i].position - position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        public Vector3 GetPosition(int index)
        {
            return _graph.Nodes[index].position;
        }

        public float GetCost(int index)
        {
            var topLayerNode = _topLayer.GetNode(index);
            if (topLayerNode.isFixed)
            {
                return topLayerNode.cost;
            }
            var editLayerNode = _subLayers[_editSubLayerIndex].GetNode(index);
            if (editLayerNode.isFixed)
            {
                return editLayerNode.cost;
            }
            var stableLayerNode = _subLayers[_stableSubLayerIndex].GetNode(index);
            return stableLayerNode.cost;
        }

        public void GetConnectedNodes(int node, List<int> results)
        {
            results.Clear();
            results.AddRange(_graph.Nodes[node].connections.Select(v => v.toNode));
        }

        public void DrawGizmos()
        {
            if (_graph != null)
            {
                Gizmos.color = Color.blue;
                foreach (var node in _graph.Nodes)
                {
                    Gizmos.DrawSphere(node.position, 0.05f);
                    foreach (var connection in node.connections)
                    {
                        var fromNode = _graph.Nodes[connection.fromNode];
                        var toNode = _graph.Nodes[connection.toNode];
                        Gizmos.DrawLine(fromNode.position, toNode.position);
                    }
                }
            }
        }

        private class Graph
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

            public Graph(NavMeshTriangleMap map)
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                // Create nodes
                var vertToNode = new int[map.Vertices.Count];
                for (int i = 0; i < map.Vertices.Count; i++)
                {
                    var node = new Node
                    {
                        position = map.Vertices[i],
                    };
                    _nodes.Add(node);
                    vertToNode[i] = _nodes.Count - 1;
                }

                var triToNode = new int[map.Triangles.Count];
                for (int i = 0; i < map.Triangles.Count; i++)
                {
                    var tri = map.Triangles[i];
                    var node = new Node
                    {
                        position = tri.center,
                    };
                    _nodes.Add(node);
                    triToNode[i] = _nodes.Count - 1;
                }

                // Create connections
                for (int i = 0; i < map.Triangles.Count; i++)
                {
                    var tri = map.Triangles[i];
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

                for (int i = 0; i < map.TriangleConnections.Count; i++)
                {
                    var connection = map.TriangleConnections[i];
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

        private class CostLayer
        {
            public class CostNode
            {
                public bool isFixed;
                public float cost;
                public int previousNode;
            }

            private readonly Graph _graph;
            private readonly CostNode[] _costNodes;
            public CostLayer(Graph graph)
            {
                _graph = graph;
                _costNodes = new CostNode[_graph.Nodes.Count];
                for (int i = 0; i < _costNodes.Length; i++)
                {
                    _costNodes[i] = new CostNode();
                }
                ResetNodes();
            }

            private void ResetNodes()
            {
                for (int i = 0; i < _costNodes.Length; i++)
                {
                    _costNodes[i].isFixed = false;
                    _costNodes[i].cost = float.PositiveInfinity;
                    _costNodes[i].previousNode = -1;
                }
            }

            private readonly List<int> _checkCostNodes = new List<int>();
            public void Setup(int startNodeIndex)
            {
                ResetNodes();
                _checkCostNodes.Clear();

                // Init start node
                {
                    var costNode = _costNodes[startNodeIndex];
                    costNode.isFixed = true;
                    costNode.cost = 0;
                    costNode.previousNode = -1;
                    _checkCostNodes.Add(startNodeIndex);
                }
            }

            public void ForwardStep()
            {
                using (new ProfilerSampleScope("DistanceMap.Layer.ForwardStep"))
                {
                    var index = _checkCostNodes[0];
                    var costNode = _costNodes[index];
                    _checkCostNodes.RemoveAt(0);
                    costNode.isFixed = true;

                    var graphNode = _graph.Nodes[index];
                    for (int i = 0; i < graphNode.connections.Count; i++)
                    {
                        var connection = graphNode.connections[i];
                        var nextNode = _costNodes[connection.toNode];
                        if (nextNode.isFixed)
                        {
                            continue;
                        }
                        var newCost = costNode.cost + connection.length;
                        if (newCost < nextNode.cost)
                        {
                            nextNode.cost = newCost;
                            nextNode.previousNode = index;
                            _checkCostNodes.Remove(connection.toNode);
                            InsertNodeSortedByDistance(_checkCostNodes, connection.toNode, _costNodes);
                        }
                    }
                }
            }

            public bool IsFinished()
            {
                return _checkCostNodes.Count == 0;
            }

            private static void InsertNodeSortedByDistance(List<int> ids, int id, IReadOnlyList<CostNode> nodes)
            {
                using (new ProfilerSampleScope("DistanceMap.Layer.InsertNodeSortedByDistance"))
                {
                    var distance = nodes[id].cost;
                    for (int i = 0; i < ids.Count; i++)
                    {
                        if (nodes[ids[i]].cost > distance)
                        {
                            ids.Insert(i, id);
                            return;
                        }
                    }
                    ids.Add(id);
                }
            }

            public CostNode GetNode(int index)
            {
                return _costNodes[index];
            }
        }
    }
}
