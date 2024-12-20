using System.Collections.Generic;
using NavMeshMaps.UnityExtensions;
using UnityEngine;

namespace NavMeshMaps.Extensions
{
    public class DistanceMap
    {
        private const int ItrLimit = 100000;
        private readonly Layer _topLayer;
        private readonly Layer[] _subLayers;
        private int _editSubLayerIndex = 0;
        private int _stableSubLayerIndex = 1;
        private bool _subLayerInitialized;

        public int SubLayerIteration = 10;

        public DistanceMap(int nodeCount)
        {
            _topLayer = new Layer(nodeCount, 10f);
            _subLayers = new Layer[2];
            for (int i = 0; i < _subLayers.Length; i++)
            {
                _subLayers[i] = new Layer(nodeCount, 999f);
            }
        }

        public void Update(NavMeshTriangleMap map, int startIndex)
        {
            if (!_subLayerInitialized)
            {
                var editLayer = _subLayers[_editSubLayerIndex];
                editLayer.Setup(map, startIndex);
                var stableLayer = _subLayers[_stableSubLayerIndex];
                stableLayer.Setup(map, startIndex);
                var itr = 0;
                while (!stableLayer.IsFinished() && itr < ItrLimit)
                {
                    itr++;
                    stableLayer.ForwardStep();
                }
                _subLayerInitialized = true;
            }

            // Top layer
            {
                _topLayer.Setup(map, startIndex);
                var itr = 0;
                while (!_topLayer.IsFinished() && itr < ItrLimit)
                {
                    itr++;
                    _topLayer.ForwardStep();
                }
            }

            // Sub layers
            {
                // Forward step
                var editLayer = _subLayers[_editSubLayerIndex];
                var itr = 0;
                while (!editLayer.IsFinished() && itr < SubLayerIteration)
                {
                    itr++;
                    editLayer.ForwardStep();
                }
                // Swap layers
                if (editLayer.IsFinished())
                {
                    var oldEditLayerIndex = _editSubLayerIndex;
                    _editSubLayerIndex = _stableSubLayerIndex;
                    _stableSubLayerIndex = oldEditLayerIndex;
                    editLayer = _subLayers[_editSubLayerIndex];
                    editLayer.Setup(map, startIndex);
                }
            }
        }

        public float GetDistance(int index)
        {
            var topLayerNode = _topLayer.GetNode(index);
            if (topLayerNode.isFixed)
            {
                return topLayerNode.distance;
            }
            var editLayerNode = _subLayers[_editSubLayerIndex].GetNode(index);
            if (editLayerNode.isFixed)
            {
                return editLayerNode.distance;
            }
            var stableLayerNode = _subLayers[_stableSubLayerIndex].GetNode(index);
            return stableLayerNode.distance;
        }

        private class Layer
        {
            private readonly Node[] _nodes;
            private readonly float _maxCalcDistance;
            public Layer(int nodeCount, float maxCalcDistance)
            {
                _maxCalcDistance = maxCalcDistance;
                _nodes = new Node[nodeCount];
                for (int i = 0; i < nodeCount; i++)
                {
                    _nodes[i] = new Node();
                }
                ResetNodes();
            }

            private void ResetNodes()
            {
                for (int i = 0; i < _nodes.Length; i++)
                {
                    _nodes[i].isFixed = false;
                    _nodes[i].distance = float.PositiveInfinity;
                    _nodes[i].previousNode = -1;
                }
            }

            private readonly List<NavMeshTriangleMap.Polygon> _polygons = new List<NavMeshTriangleMap.Polygon>();
            private readonly List<int> _checkNodes = new List<int>();
            public void Setup(NavMeshTriangleMap map, int startIndex)
            {
                _polygons.Clear();
                map.GetPolygons(_polygons);
                if (_polygons.Count != _nodes.Length)
                {
                    Debug.LogError($"Map size mismatch: {_polygons.Count} != {_nodes.Length}");
                    return;
                }
                ResetNodes();
                _checkNodes.Clear();

                // Init start node
                {
                    var node = _nodes[startIndex];
                    node.isFixed = true;
                    node.distance = 0;
                    node.previousNode = -1;
                    _checkNodes.Add(startIndex);
                }
            }

            public void ForwardStep()
            {
                using (new ProfilerSampleScope("DistanceMap.Layer.ForwardStep"))
                {
                    var index = _checkNodes[0];
                    var node = _nodes[index];
                    _checkNodes.RemoveAt(0);
                    node.isFixed = true;

                    if (node.distance > _maxCalcDistance)
                    {
                        return;
                    }

                    var poly = _polygons[index];
                    for (int i = 0; i < poly.connections.Length; i++)
                    {
                        var connection = poly.connections[i];
                        var nextNode = _nodes[connection.to];
                        if (nextNode.isFixed)
                        {
                            continue;
                        }
                        var newDistance = node.distance + connection.length;
                        if (newDistance < nextNode.distance)
                        {
                            nextNode.distance = newDistance;
                            nextNode.previousNode = index;
                            _checkNodes.Remove(connection.to);
                            InsertNodeSortedByDistance(_checkNodes, connection.to, _nodes);
                        }
                    }
                }
            }

            public bool IsFinished()
            {
                return _checkNodes.Count == 0;
            }

            private static void InsertNodeSortedByDistance(List<int> ids, int id, IReadOnlyList<Node> nodes)
            {
                using (new ProfilerSampleScope("DistanceMap.Layer.InsertNodeSortedByDistance"))
                {
                    var distance = nodes[id].distance;
                    for (int i = 0; i < ids.Count; i++)
                    {
                        if (nodes[ids[i]].distance > distance)
                        {
                            ids.Insert(i, id);
                            return;
                        }
                    }
                    ids.Add(id);
                }
            }

            public Node GetNode(int index)
            {
                return _nodes[index];
            }
        }

        private class Node
        {
            public bool isFixed;
            public float distance;
            public int previousNode;
        }
    }
}
