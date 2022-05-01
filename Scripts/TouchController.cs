using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public float speed = 10.0f;
    public GameField gameField;
    private MovableSquare currentlyHeldSquare;
    private GameController gameController;
    private Vector3 lastTouchPosition;

    public bool IsPlayerHoldingASquare { get => currentlyHeldSquare != null; }

    private void Awake()
    {
        gameController = GetComponent<GameController>();
    }

    void Update()
    {
        //HandleMouseInput();
        switch (gameController.Gamestate)
        {
            case GameState.GAME:
                HandleGameInput();
                break;
            case GameState.GAME_END_READY_TO_RETURN:
                if (PlayerTapped()) gameController.ReturnToMainMenu();
                break;
            default:
                break;
        }
    }

    private bool PlayerTapped()
    {
        return Input.touchCount > 0 | Input.GetMouseButton(0);
    }

    private void HandleGameInput()
    {
        if (Input.touchCount > 0)
        {
            TouchTheGamefield(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButton(0))
        {
            TouchTheGamefield(Input.mousePosition);
        }
        else
        {
            HandleNoInput();
        }
    }

    private void TouchTheGamefield(Vector3 position)
    {
        //Touching
        lastTouchPosition = Camera.main.ScreenToWorldPoint(position);
        lastTouchPosition.z = 0f;
        if (currentlyHeldSquare == null) CheckForMovableSquare(lastTouchPosition);
        else currentlyHeldSquare.transform.position = lastTouchPosition;
    }

    private void HandleNoInput()
    {
        if (currentlyHeldSquare != null)
        {
            currentlyHeldSquare.ReturnToGrid(lastTouchPosition, true);
            currentlyHeldSquare = null;
        }
    }

    private void CheckForMovableSquare(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        {
            if (hit.collider != null && hit.transform.tag.Equals("Movable") && hit.transform.TryGetComponent(out MovableSquare ms))
            {
                currentlyHeldSquare = ms;
                ms.RemoveFromGrid();
            }
        }
    }
}
