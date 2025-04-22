using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _GameAssets._Scripts
{
    [RequireComponent(typeof(AStar))]
    public class MazeScript : MonoBehaviour
    {
        #region Properties
        //Properties
        private bool CanPathFinding => _isMazeGenerated && _startNode && _targetNode;

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
        
        [SerializeField]
        private InputField _sizeXInputField;
        [SerializeField]
        private InputField _sizeYInputField;

        [Space(3)]
        [SerializeField]
        private Vector2Int _size;

        // Private
        public  Node[,]          _nodes;
        private Node             _startNode;
        private Node             _targetNode;
        private bool             _isMazeGenerated;
        private List<GameObject> _nodePool;
        #endregion


        #region Unity Func

        private void Awake()
        {
            _nodePool             = new List<GameObject>();
            _sizeXInputField.text = _size.x.ToString();
            _sizeYInputField.text = _size.y.ToString();
        }

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

        private void Start()
        {
            CreateMap();
        }

        #endregion

        
        #region Public Func

        [ContextMenu("CreateMap")]
        public async void CreateMap()
        {
            if(string.IsNullOrEmpty(_sizeXInputField.text) || string.IsNullOrEmpty(_sizeYInputField.text))
                return;
            _size.x = int.Parse(_sizeXInputField.text);
            _size.y = int.Parse(_sizeYInputField.text);
            if (_size.x < 5 || _size.y < 5)
            {
                Debug.LogError("Kích thước tối thiểu 1 cạnh là 5");
                return;
            }
            if (_size.x % 2 == 0)
            {
                _size.x++;
            }
            if (_size.y % 2 == 0)
            {
                _size.y++;
            }
            _isMazeGenerated = false;
            EventManager.resetState?.Invoke();
            //
            while (_girdContent.childCount > 0)
            {
                var node = _girdContent.GetChild(0);
                node.gameObject.SetActive(false);
                _nodePool.Add(node.gameObject);
                node.SetParent(null);
            }
            await Task.Yield();
            //
            CreateGrid(_size);
            //
            GenerateContentMap();
        }

        public void PathFinding()
        {
            if (!CanPathFinding)
            {
                Debug.LogError("NPC hoặc điểm đích vẫn chưa được lựa chọn");

                return;
            }

            _aStar.PathFinding(_startNode, _targetNode, _nodes, _size);
        }

        #endregion

        #region Event Func

        private void OnNodeSelected(Node node)
        {
            if (CanPathFinding)
            {
                _startNode  = null;
                _targetNode = null;
                EventManager.resetState?.Invoke();
            }

            if (!_startNode)
            {
                _startNode = node;
                _startNode.IsNPC();
            }
            else
            {
                _targetNode = node;
                _targetNode.IsTarget();
            }
        }

        #endregion

        #region Local Func

        private void CreateGrid(Vector2Int sizeXY)
        {
            var rect     = _girdContent.rect;
            var sizeCell = Mathf.Min(rect.width / sizeXY.x, rect.height / sizeXY.y);
            var spacingX = (rect.width - (sizeCell * sizeXY.x)) / 2f;
            _gridLayoutGroup.padding.left  = (int)spacingX;
            _gridLayoutGroup.padding.right = (int)spacingX;
            _gridLayoutGroup.cellSize      = new Vector2(sizeCell, sizeCell);
            _nodes                         = new Node[sizeXY.x, sizeXY.y];
            for (var i = 0; i < sizeXY.y; i++)
            {
                for (var j = 0; j < sizeXY.x; j++)
                {
                    Node node;

                    if (_nodePool.Count > 0)
                    {
                        var index = _nodePool.Count - 1;
                        node = _nodePool[index].GetComponent<Node>();
                        node.gameObject.SetActive(true);
                        node.transform.SetParent(_girdContent);
                        _nodePool.RemoveAt(index);
                    }
                    else
                    {
                        node = Instantiate(_nodePrefab, _girdContent).GetComponent<Node>();
                    }
                    
                    node.Init(new Vector2Int(j, i), true);
                    _nodes[j, i] = node;
                }
            }
        }
        
        private void GenerateContentMap()
        {
            var currentPosHalf = new Vector2Int(0, 0);
            var walkedCount    = 1;
            var neighbourCount = 0;
            var neighbours     = new Node[3];
            var totalWalkCont  = Mathf.CeilToInt(_size.x / 2f) * Mathf.CeilToInt(_size.y / 2f);
            //
            var nodeSet        = _nodes[0, 0];
            nodeSet.IsWalked = true;
            nodeSet.SetWall(false);
            while (walkedCount < totalWalkCont)
            {
                LoadNeighbours_L(currentPosHalf);
                if (neighbourCount == 0)
                {
                    nodeSet.IsBlocked = true;
                    currentPosHalf    = nodeSet.PassPosHalf;
                    nodeSet           = GetNodeWithHalfPos(currentPosHalf);
                    continue;
                }
                var randomIndex = Random.Range(0, neighbourCount);
                var nodeToWalk  = neighbours[randomIndex];
                nodeToWalk.PassPosHalf = currentPosHalf;
                nodeToWalk.IsWalked    = true;
                nodeToWalk.SetWall(false);
                
                Vector2Int sign;
                if (nodeToWalk.Position.x != nodeSet.Position.x)
                {
                    sign = (nodeSet.Position.x > nodeToWalk.Position.x ? -1 : 1) * Vector2Int.right;
                }
                else
                {
                    sign = (nodeSet.Position.y > nodeToWalk.Position.y ? -1 : 1) * Vector2Int.up;
                }
                //
                var nodeMid        =  GetNode(nodeSet.Position + sign);
                nodeMid.SetWall(false);
                //
                nodeSet        =  nodeToWalk;
                currentPosHalf += sign;
                //
                walkedCount++;
            }

            //
            _isMazeGenerated = true;

            // Local Func
            void LoadNeighbours_L(Vector2Int posHalf)
            {
                neighbourCount = 0;
                var node = GetNodeWithHalfPos(posHalf + Vector2Int.left);

                if (ValidateNeighbour(node))
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }

                node = GetNodeWithHalfPos(posHalf + Vector2Int.right);

                if (ValidateNeighbour(node))
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }

                node = GetNodeWithHalfPos(posHalf + Vector2Int.up);

                if (ValidateNeighbour(node))
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }

                node = GetNodeWithHalfPos(posHalf + Vector2Int.down);

                if (ValidateNeighbour(node))
                {
                    neighbours[neighbourCount] = node;
                    neighbourCount++;
                }
            }
        }
        
        private bool ValidateNeighbour(Node node)
        {
            if (!node || node.IsWalked || node.IsBlocked)
                return false;

            return true;
        }
        
        private Node GetNodeWithHalfPos(Vector2Int pos)
        {
            return GetNode(pos * 2);
        }
        
        private Node GetNode(Vector2Int pos)
        {
            if (pos.x < 0 || pos.x >= _size.x || pos.y < 0 || pos.y >= _size.y)
                return null;
            return _nodes[pos.x, pos.y];
        }

        #endregion
    }
}