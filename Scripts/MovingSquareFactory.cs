using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovingSquareFactory : MonoBehaviour
{
    public GameObject square;
    public Transform squareHolder;
    public TextMeshProUGUI tmpLevel;
    public Slider timerSlider;
    private GameField gameField;
    private MovableSquare holdedSquare;
    private float timer; //Timer to next Square to be created.
    private int amountOfSquaresMade = 0;
    //Game settings.
    private const float timerMax = 3f;
    private int level = 1; //Rises everytime counter goes faster. Just for the player to see.
    private float timerIncreaseModifier = 1;
    private float amountOfTimerIncreaseEachLevel = 0.3f;
    private const int amountToIncreaseLevel = 5;

    private void Awake()
    {
        timerSlider.maxValue = timerMax;
        timerSlider.minValue = 0;
    }

    private void Start()
    {
        gameField = GameController.Instance.gameField;
    }

    private void OnEnable()
    {
        //Basically, reset everything.
        amountOfSquaresMade = 0;
        GenerateNewSquare();
        level = 1;
        timerIncreaseModifier = 1f;
        tmpLevel.text = "Level: " + level;
    }

    public void GenerateNewSquare()
    {
        holdedSquare = Instantiate(square, new Vector3(transform.position.x, transform.position.y), Quaternion.identity, squareHolder).GetComponent<MovableSquare>();
        timer = timerMax;
        amountOfSquaresMade++;
        if (amountOfSquaresMade % amountToIncreaseLevel == 0) IncreaseLevel();
    }

    private void Update()
    {
        if (!GameController.Instance.Gamestate.Equals(GameState.GAME)) return;
        if (gameField.GetIsGameFieldFull())
        {
            GameController.Instance.StartGameEndCountDown();
            return;
        }
        if (holdedSquare.HasBeenUsed) GenerateNewSquare();
        else SendSquareToGrid();
        timerSlider.value = timer;
    }

    private void SendSquareToGrid()
    {
        //Stop timer during explosion chain.
        if (GameController.Instance.ExplosionChain < 1) timer -= Time.deltaTime * timerIncreaseModifier;
        //When timer over, send held square to grid.
        if (timer < 0)
        {
            holdedSquare.ReturnToGrid(transform.position, false);
            timer = timerMax;
        }
    }

    private void IncreaseLevel()
    {
        level++;
        tmpLevel.text = "Level: " + level;
        timerIncreaseModifier += amountOfTimerIncreaseEachLevel;
    }
}
