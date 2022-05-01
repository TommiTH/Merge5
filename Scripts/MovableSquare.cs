using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovableSquare : MonoBehaviour
{
    private int currentValue = 0;
    private Coroutine moveSquareCoroutine;
    private float moveSpeed = 20f;
    private GameField gameField;
    private Vector2Int currentGridPosition;
    private bool hasBeenUsed = false;
    //Sprites
    private SpriteRenderer spriteRenderer;
    public Sprite square1;
    public Sprite square2;
    public Sprite square3;
    public Sprite square4;
    public Sprite square5;


    public int CurrentValue { get => currentValue; }
    public bool HasBeenUsed { get => hasBeenUsed; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        float f = UnityEngine.Random.Range(0, 32);
        int value = 1;
        if (f >= 5) value = 2;
        if (f >= 15) value = 3;
        if (f >= 25) value = 4;
        if (f >= 31) value = 5;
        SetSquareValue(value);
    }

    private void Start()
    {
        gameField = GameController.Instance.gameField;
    }

    public void SetSquareValue(int value)
    {
        currentValue = value;
        switch (value)
        {
            case int n when (n <= 1):
                spriteRenderer.sprite = square1;
                break;
            case int n when (n == 2):
                spriteRenderer.sprite = square2;
                break;
            case int n when (n == 3):
                spriteRenderer.sprite = square3;
                break;
            case int n when (n == 4):
                spriteRenderer.sprite = square4;
                break;
            case int n when (n >= 5):
                spriteRenderer.sprite = square5;
                break;
        }
    }

    public void ReturnToGrid(Vector3 worldPosition, bool isAbleToMerge)
    {
        hasBeenUsed = true;
        if (moveSquareCoroutine != null) return;
        Vector2Int cell = gameField.GetClosestCell(worldPosition);
        if (gameField.IsGridSpaceOccupied(cell))
        {
            if (isAbleToMerge && gameField.TryMerging(this, cell))
            {
                moveSquareCoroutine = StartCoroutine(MoveSquareToPositionAndDestroy(gameField.GetCellCenterWorld(cell)));
                return;
            }
            cell = gameField.GetClosestEmptyCell(worldPosition);
        }
        AudioController.Instance.PlaySquareLetGoSound();
        gameField.PlaceToGrid(cell, this);
        currentGridPosition = cell;
        moveSquareCoroutine = StartCoroutine(MoveSquareToPosition(gameField.GetCellCenterWorld(cell)));
    }

    public void RemoveFromGrid()
    {
        if (!hasBeenUsed)
        {
            hasBeenUsed = true;
            return;
        }
        gameField.RemoveFromGrid(currentGridPosition);
        StopAllCoroutines();
        moveSquareCoroutine = null;
    }

    public void Explode()
    {
        GameController.Instance.AddToCurrentScore(currentValue * 2);
        gameField.RemoveFromGrid(currentGridPosition);
        Destroy(gameObject);
    }

    private IEnumerator MoveSquareToPosition(Vector3 position)
    {
        while (Vector3.Distance(position, transform.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }
        transform.position = position;
        moveSquareCoroutine = null;
    }

    private IEnumerator MoveSquareToPositionAndDestroy(Vector3 position)
    {
        while (Vector3.Distance(position, transform.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * moveSpeed * 2);
            yield return new WaitForEndOfFrame();
        }
        transform.position = position;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
