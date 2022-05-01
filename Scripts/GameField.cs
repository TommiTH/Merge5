using System;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    //Important: Grid and array y is reversed so we must give grid negative y values.
    //Don't directly use it, use GetCellCenterWorld(Vector2Int gridPos)
    private Grid unityGrid;
    //Game field is x: 0 to 3 and y: 0 to -5
    private int maxWidth = 4;
    private int maxHeight = 6;
    private GridSquare[,] gridArray;
    private int fieldSquareCount = 0;
    public Transform squareHolder;
    private TouchController playerController;

    void Awake()
    {
        InitializeGrid();
        unityGrid = GetComponentInChildren<Grid>();
    }

    private void Start()
    {
        playerController = GameController.Instance.transform.GetComponent<TouchController>();
    }

    private void InitializeGrid()
    {
        gridArray = new GridSquare[maxWidth, maxHeight];
        for (int y = 0; y < maxHeight; y++)
        {
            for (int x = 0; x < maxWidth; x++)
            {
                gridArray[x, y] = new GridSquare();
            }
        }
    }

    public void ResetGameField()
    {
        for (int i = 0; i < squareHolder.childCount; i++)
        {
            Destroy(squareHolder.GetChild(i).gameObject);
        }
        InitializeGrid();
        fieldSquareCount = 0;
    }

    public bool GetIsGameFieldFull()
    {
        if (playerController.IsPlayerHoldingASquare) return fieldSquareCount + 1 > 23f;
        return fieldSquareCount > 23f;
    }

    public Vector3 GetCellCenterWorld(Vector2Int gridPos)
    {
        //gridPos.y * -1 because unityGrid y values are reverse.
        return unityGrid.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y * -1, 0));
    }

    //If we let go of a square outside of the playarea, this gives us the closest gridArray position.
    public Vector2Int GetClosestCell(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;
        int x = Mathf.FloorToInt(localPosition.x);
        x = SetXToBounds(x);
        //* -1 so we get gridArray coordinates.
        int y = Mathf.FloorToInt(1 + localPosition.y * -1);
        y = SetYToBounds(y);
        return new Vector2Int(x, y);
    }

    public Vector2Int GetClosestEmptyCell(Vector3 worldPosition)
    {
        Vector2Int startCell = GetClosestCell(worldPosition);
        if (!IsGridSpaceOccupied(startCell)) return startCell;
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> checkedList = new List<Vector2Int>();
        queue.Enqueue(startCell);
        return FindClosestEmptyFromGrid(queue, checkedList);
    }

    private Vector2Int FindClosestEmptyFromGrid(Queue<Vector2Int> queue, List<Vector2Int> checkedList)
    {
        if (queue.Count < 1) return Vector2Int.zero;
        Vector2Int gridMiddle = queue.Dequeue();
        checkedList.Add(gridMiddle);
        //Add new positions
        Vector2Int nextGridPos1 = gridMiddle + new Vector2Int(1, 0);
        if (CheckCellIfEmptyOrAddToQueueAndList(nextGridPos1, queue, checkedList)) return nextGridPos1;
        Vector2Int nextGridPos2 = gridMiddle + new Vector2Int(-1, 0);
        if (CheckCellIfEmptyOrAddToQueueAndList(nextGridPos2, queue, checkedList)) return nextGridPos2;
        Vector2Int nextGridPos3 = gridMiddle + new Vector2Int(0, 1);
        if (CheckCellIfEmptyOrAddToQueueAndList(nextGridPos3, queue, checkedList)) return nextGridPos3;
        Vector2Int nextGridPos4 = gridMiddle + new Vector2Int(0, -1);
        if (CheckCellIfEmptyOrAddToQueueAndList(nextGridPos4, queue, checkedList)) return nextGridPos4;
        return FindClosestEmptyFromGrid(queue, checkedList);
    }

    private bool CheckCellIfEmptyOrAddToQueueAndList(Vector2Int pos, Queue<Vector2Int> queue, List<Vector2Int> checkedList)
    {
        if (!checkedList.Contains(pos) && IsPositionInsideGrid(pos))
            if (IsGridSpaceOccupied(pos)) queue.Enqueue(pos);
            else return true;
        return false;
    }

    private bool IsPositionInsideGrid(Vector2Int gridPos)
    {
        if (gridPos.x > maxWidth - 1 || gridPos.x < 0 || gridPos.y > maxHeight - 1 || gridPos.y < 0)
        {
            return false;
        }
        return true;
    }

    private int SetXToBounds(int x)
    {
        if (x > maxWidth - 1) x = maxWidth - 1;
        if (x < 0) x = 0;
        return x;
    }

    private int SetYToBounds(int y)
    {
        if (y > maxHeight - 1) y = maxHeight - 1;
        if (y < 0) y = 0;
        return y;
    }

    public void PlaceToGrid(Vector2Int gridPos, MovableSquare movableSquare)
    {
        gridArray[gridPos.x, gridPos.y].InsertMovableSquare(movableSquare);
        fieldSquareCount++;
    }

    public void RemoveFromGrid(Vector2Int gridPos)
    {
        gridArray[gridPos.x, gridPos.y].RemoveMovableSquare();
        fieldSquareCount--;
    }

    public bool IsGridSpaceOccupied(Vector2Int gridPos)
    {
        return gridArray[gridPos.x, gridPos.y].IsOccupied;
    }

    public int GetMovableSquareCurrentValueFromGrid(Vector2Int gridPos)
    {
        return gridArray[gridPos.x, gridPos.y].HoldedSquare.CurrentValue;
    }

    internal bool TryMerging(MovableSquare movableSquare, Vector2Int gridPos)
    {
        if (gridArray[gridPos.x, gridPos.y].CurrentlyHeldValue.Equals(movableSquare.CurrentValue))
        {
            gridArray[gridPos.x, gridPos.y].Merge();
            if (movableSquare.CurrentValue >= 5)
            {
                bool[,] explosionLocations = new bool[maxWidth, maxHeight];
                explosionLocations = Explosion(gridPos, explosionLocations);
                PlayExplosionSFX(explosionLocations);
            }
            else AudioController.Instance.PlayMergeSound(movableSquare.CurrentValue + 1);
            return true;
        }
        return false;
    }

    private bool[,] Explosion(Vector2Int gridPos, bool[,] explosionLocations)
    {
        GameController.Instance.AddExplosionChain();
        int startX = gridPos.x - 1;
        int startY = gridPos.y - 1;
        for (int y = startY; y < startY + 3; y++)
        {
            for (int x = startX; x < startX + 3; x++)
            {
                Vector2Int p = new Vector2Int(x, y);
                if (!IsPositionInsideGrid(p)) continue;
                if (gridArray[x, y].IsOccupied)
                {
                    if (p != gridPos && gridArray[x, y].HoldedSquare.CurrentValue >= 5) explosionLocations = Explosion(p, explosionLocations);
                    else gridArray[x, y].HoldedSquare.Explode();
                }
                explosionLocations[p.x, p.y] = true;
            }
        }
        return explosionLocations;
    }

    private void PlayExplosionSFX(bool[,] explosionLocations)
    {
        for (int y = 0; y < maxHeight; y++)
        {
            for (int x = 0; x < maxWidth; x++)
            {
                if (explosionLocations[x, y])
                {
                    SFXController.Instance.PlayExplosionSFX(GetCellCenterWorld(new Vector2Int(x, y)));
                }
            }
        }
        AudioController.Instance.PlayExplosionSound();
    }
}
