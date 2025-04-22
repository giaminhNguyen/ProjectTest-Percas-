using _GameAssets._Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    #region Properties

    public bool isBlocked;
    public bool isWalked;
    public Vector2Int passPosHalf;
    //Properties
    public int FCost => GCost + HCost;
    public int GCost { get; private set; }
    public int HCost { get; private set; }
    public bool IsWall { get; private set; }
    public bool IsVisited { get;  set; }
    public Vector2Int Position => _position;
    public Node PreviousNode => _previousNode;
    public bool IsCalculated { get; private set; }
    //Serialized Field
    [SerializeField]
    private Image _imageNode;

    [SerializeField]
    private Color _wallColor;
    
    [SerializeField]
    private Color _defaultColor;
    
    [SerializeField]
    private Color _selectedColor;
    
    [SerializeField]
    private Color _pathColor;
    
    
    //private
    private Node _previousNode;
    private Vector2Int _position;
    
    #endregion

    #region Public Func

    public void Init(Vector2Int pos,bool isWall)
    {
        IsVisited        = false;
        IsCalculated     = false;
        _position        = pos;
        SetWall(isWall);
    }
    
    public void CalculateCost(Node startNode,Node targetNode)
    {
        if(IsWall || !IsCalculated) return;
        IsCalculated = true;
        GCost        = Mathf.Abs(startNode.Position.x - _position.x) + Mathf.Abs(startNode.Position.y - _position.y);
        HCost        = Mathf.Abs(targetNode.Position.x - _position.x) + Mathf.Abs(targetNode.Position.y - _position.y);
    }

    public void SetWall(bool isWall)
    {
        IsWall           = isWall;
        _imageNode.color = IsWall ? _wallColor : _defaultColor;
    }

    public void SetPreviousNode(Node previousNode)
    {
        if(IsWall) return;
        _previousNode = previousNode;
    }
    
    public void OnSelected()
    {
        if(IsWall) return;
        EventManager.onNodeSelected?.Invoke(this);
        _imageNode.color = _selectedColor;
    }

    public void ResetColor()
    {
        _imageNode.color = IsWall ? _wallColor : _defaultColor;
    }

    public void IsPath()
    {
        if(IsWall) return;
        _imageNode.color = _pathColor;
    }

    #endregion

}
