using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets._Scripts
{
    [RequireComponent(typeof(AStar))]
    public class MazeScript : MonoBehaviour
    {
        #region Properties
        //Properties
        private bool    CanPathFinding => _isMazeGenerated && _startNode && _targetNode;
        //Serialized Filed
        [Header("References")]
        [SerializeField]
        private AStar _aStar;

        [SerializeField]
        private RectTransform _girdContent;
        [SerializeField]
        private GridLayoutGroup _gridLayoutGroup;
        [SerializeField]
        private GameObject _nodePrefab;
        [Space(3)]
        [SerializeField]
        private Vector2Int _size;
        // Private
        public Node[,] _nodes;
        private Node    _startNode;
        private Node    _targetNode;
        private bool _isMazeGenerated;
        
        #endregion


        #region Unity Func
        private void OnValidate()
        {
            if (!_aStar)
            {
                _aStar = GetComponent<AStar>();
            }
        }

        private void OnEnable()
        {
            EventManager.onNodeSelected += OnNodeSelected;
        }

        private void OnDisable()
        {
            EventManager.onNodeSelected -= OnNodeSelected;
        }

        private async void Start()
        {
            await Task.Yield();
            GenerateMap();
        }

        #endregion

        #region Public Func
        [ContextMenu("Generate Map")]
        public void GenerateMap()
        {
            foreach (Transform node in _girdContent)
            {
                Destroy(node.gameObject);
            }
            _isMazeGenerated = false;
            if(_size.x < 5 || _size.y < 5) return;
            var rect  = _girdContent.rect;
            var size  = Mathf.Min(rect.width/_size.x,rect.height/_size.y);
            var spacingX = (rect.width - (size * _size.x)) / 2f;
            _gridLayoutGroup.padding.left = (int)spacingX;
            _gridLayoutGroup.padding.right = (int)spacingX;
            _gridLayoutGroup.cellSize     = new Vector2(size,size);
            _nodes                        = new Node[_size.x, _size.y];
            //
            var isRowWall = false;
            for(var i = 0; i < _size.x; i++)
            {
                var isWall = false;
                for (var j = 0; j < _size.y; j++)
                {
                    var node = Instantiate(_nodePrefab, _girdContent).GetComponent<Node>();
                    node.Init(new Vector2Int(i,j),isRowWall || isWall);
                    _nodes[i,j] = node;
                    isWall      = !isWall;
                }
                isRowWall = !isRowWall;
            }
            //
            var currentPosHalf = new Vector2Int(0, 0);
            var walkedCount      = 0;
            var neighbourCount = 0;
            var neighbours      = new Node[4];
            _isMazeGenerated = true;
            
            while (walkedCount < _nodes.Length)
            {
                var node = GetNode_L(currentPosHalf * 2);
                node.isWalked = true;
                LoadNeighbours_L(currentPosHalf);
                if (neighbourCount == 0)
                {
                    node.isBlocked   = true;
                    currentPosHalf = node.passPosHalf;

                    if (currentPosHalf == Vector2Int.zero)
                    {
                        break;
                    }
                    continue;
                }
                var  randomIndex = UnityEngine.Random.Range(0, neighbourCount);
                var  nodeToWalk  = neighbours[randomIndex];
                nodeToWalk.passPosHalf = currentPosHalf;
                Node nodeMid;
                if (nodeToWalk.Position.x != node.Position.x)
                {
                    var sign = (node.Position.x > nodeToWalk.Position.x ? -1 : 1) * Vector2Int.right;
                    var pos  = node.Position +  sign;
                    currentPosHalf +=  sign;
                    nodeMid        =  GetNode_L(pos);
                    nodeMid.SetWall(false);
                }
                else
                {
                    var sign = (node.Position.y > nodeToWalk.Position.y ? -1 : 1) * Vector2Int.up;
                    var pos  = node.Position + sign;
                    currentPosHalf += sign;
                    nodeMid        =  GetNode_L(pos);
                    nodeMid.SetWall(false);
                }
                nodeMid.SetWall(false);
                walkedCount++;
            }
            
            // Local Func
            void LoadNeighbours_L(Vector2Int posHalf)
            {
                neighbourCount = 0;
                var node = GetNodeNeighbour_L(posHalf + Vector2Int.left);
                if (node)
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }
                node = GetNodeNeighbour_L(posHalf + Vector2Int.right);
                if (node)
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }
                node = GetNodeNeighbour_L(posHalf + Vector2Int.up);
                if (node)
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }
                node = GetNodeNeighbour_L(posHalf + Vector2Int.down);
                if (node)
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }
            }
            
            Node GetNodeNeighbour_L(Vector2Int pos)
            {
                var node = GetNode_L(pos * 2);
                return !node || node.isWalked || node.isBlocked ? null : node;
            }
            
            Node GetNode_L(Vector2Int pos)
            {
                if (pos.x < 0 || pos.x >= _size.x || pos.y < 0 || pos.y >= _size.y) return null;
                return _nodes[pos.x, pos.y];
            }
        }

        public void PathFinding()
        {
            if (!CanPathFinding)
            {
                Debug.LogError("NPC hoặc điểm đích vẫn chưa được lựa chọn");
                return;
            }
            _aStar.PathFinding(_startNode,_targetNode,_nodes,_size);
        }

        #endregion

        #region Event Func

        private void OnNodeSelected(Node node)
        {
            if (CanPathFinding)
            {
                _startNode?.ResetColor();
                _targetNode?.ResetColor();
                _startNode = null;
                _targetNode = null;
            }

            if (!_startNode)
            {
                _startNode = node;
            }
            else
            {
                _targetNode = node;
            }
        }

        #endregion

    }
}