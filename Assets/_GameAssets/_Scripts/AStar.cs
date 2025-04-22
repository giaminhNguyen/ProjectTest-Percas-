using System.Collections.Generic;
using UnityEngine;

namespace _GameAssets._Scripts
{
    public class AStar : MonoBehaviour
    {
        #region Properties
        private Node[,] _nodes;
        private List<Node> _openNodes;
        private Vector2Int _size;

        #endregion

        #region Unity Func

        private void Awake()
        {
            _openNodes = new List<Node>();
        }

        #endregion
        
        public List<Vector2Int> PathFinding(Node start,Node target,Node[,] nodes,Vector2Int size)
        {
            var path = new List<Vector2Int>();
            _openNodes.Clear();
            _openNodes.Add(start);
            _nodes = nodes;
            _size  = size;
            
            while (_openNodes.Count > 0)
            {
                var currentNode  = _openNodes[0];
                var currentIndex = 0;
                for (var i = 1; i < _openNodes.Count; i++)
                {
                    var node = _openNodes[i];
                    if (node.IsWall || node.IsVisited) continue;
                    node.CalculateCost(start,target);
                    if(currentNode.FCost < node.FCost) continue;
                    if(currentNode.FCost == node.FCost && currentNode.HCost <= node.HCost) continue;
                    currentNode  = node;
                    currentIndex = i;
                }
                _openNodes.RemoveAt(currentIndex);
                currentNode.IsVisited = true;
                if (currentNode == target)
                {
                    while (currentNode != start)
                    {
                        path.Add(currentNode.Position);
                        currentNode.IsPath();
                        currentNode = currentNode.PreviousNode;
                    }
                    path.Reverse();
                    break;
                }

                foreach (var node in GetNeighbours(currentNode))
                {
                    if(!node || node.IsWall || node.IsVisited) continue;
                    node.SetPreviousNode(currentNode);
                    _openNodes.Add(node);
                }
            }
            
            return path;
        }
        
        
        private Node[] GetNeighbours(Node node)
        {
            var neighbours = new Node[4];
            neighbours[0] = GetNode(node.Position + Vector2Int.left);
            neighbours[1] = GetNode(node.Position + Vector2Int.right);
            neighbours[2] = GetNode(node.Position + Vector2Int.up);
            neighbours[3] = GetNode(node.Position + Vector2Int.down);
            return neighbours;
        }
        
        private Node GetNode(Vector2Int pos)
        {
            if (pos.x < 0 || pos.x >= _size.x || pos.y < 0 || pos.y >= _size.y) return null;
            return _nodes[pos.x, pos.y];
        }
        
    }
}