using System.Collections;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameField gameField;
    //Game Windows
    public GameObject mainMenuGO;
    public GameObject gameBoardGO;
    public GameObject gameEndGO;
    public GameObject tapToReturnTextGO;
    //Text
    public TextMeshProUGUI gameEndText;
    public TextMeshProUGUI scoreCounter;
    private const string scoreCounterText = "Score: \n";
    public TextMeshProUGUI bestScore;
    //Explosion chain
    public TextMeshProUGUI explosionChainTMP;
    private Coroutine explosionChainCoroutine;
    private int explosionChain = 0;
    private float explosionChainTime = 2f; //Amount of break time after explosion.
    //Game end countdown
    public TextMeshProUGUI countDownTMP;
    private Coroutine gameEndCountdownCoroutine;
    private float gameEndCountDown = 3f;
    private float gameEndCountDownStart = 3f;

    private GameState gamestate;
    private const string highscoreStr = "BestScore";
    private int score = 0;

    public static GameController Instance { get; private set; }
    public GameState Gamestate { get => gamestate; }
    public int ExplosionChain { get => explosionChain; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ReturnToMainMenu();
    }

    internal void ReturnToMainMenu()
    {
        gamestate = GameState.MAIN_MENU;
        bestScore.text = "Best Score:\n" + PlayerPrefs.GetInt(highscoreStr);
        gameBoardGO.SetActive(false);
        gameEndGO.SetActive(false);
        mainMenuGO.SetActive(true);
        gameField.ResetGameField();
    }

    public void AddToCurrentScore(int i)
    {
        score += i * (explosionChain + 1);
        scoreCounter.text = scoreCounterText + score;
    }

    //Called by "Start game" Button.
    public void StartGame()
    {
        gamestate = GameState.GAME;
        mainMenuGO.SetActive(false);
        gameBoardGO.SetActive(true);
        ResetGame();
    }

    private void ResetGame()
    {
        explosionChain = 0;
        score = 0;
        scoreCounter.text = scoreCounterText + 0;
        gameEndCountDown = gameEndCountDownStart;
        //UI elements
        explosionChainTMP.gameObject.SetActive(false);
        countDownTMP.gameObject.SetActive(false);
    }

    public void EndGame()
    {
        if (gamestate.Equals(GameState.GAME_END) | gamestate.Equals(GameState.GAME_END_READY_TO_RETURN)) return;
        gamestate = GameState.GAME_END;
        gameEndGO.SetActive(true);
        tapToReturnTextGO.SetActive(false);
        if (score > PlayerPrefs.GetInt(highscoreStr))
        {
            PlayerPrefs.SetInt(highscoreStr, score);
            gameEndText.text = "Congratulations! New highscore!";
            gameEndText.color = Color.yellow;
        }
        else
        {
            gameEndText.text = "Better luck next time!";
            gameEndText.color = Color.white;
        }
        if (explosionChainCoroutine != null) StopCoroutine(explosionChainCoroutine);
        StartCoroutine(ReadyToReturnToMainMenuCoroutine(0.8f));
    }

    internal void StartGameEndCountDown()
    {
        if (gameEndCountdownCoroutine != null) return;
        gameEndCountdownCoroutine = StartCoroutine(GameEndCountdownCoroutine());
    }

    public void AddExplosionChain()
    {
        explosionChain += 1;
        if (explosionChainCoroutine != null) return;
        explosionChainCoroutine = StartCoroutine(ExplosionChainTextFadeoutCoroutine());
    }

    //When grid is full, a countdown appears. Game ends only when this countdown is 0.
    private IEnumerator GameEndCountdownCoroutine()
    {
        countDownTMP.gameObject.SetActive(true);
        AudioController.Instance.PlayGameEndWarningSound();
        float soundTimer = 1f;
        while (gameEndCountDown > 0 & gameField.GetIsGameFieldFull())
        {
            countDownTMP.text = "" + (int)(gameEndCountDown + 0.5f);
            gameEndCountDown -= Time.deltaTime;
            //Sounds
            soundTimer -= Time.deltaTime;
            if (soundTimer <= 0)
            {
                AudioController.Instance.PlayGameEndWarningSound();
                soundTimer = 1f;
            }
            yield return new WaitForEndOfFrame();
        }
        if (gameEndCountDown <= 0) EndGame();
        gameEndCountDown += 0.2f;
        gameEndCountdownCoroutine = null;
        countDownTMP.gameObject.SetActive(false);
    }

    private IEnumerator ReadyToReturnToMainMenuCoroutine(float f)
    {
        yield return new WaitForSeconds(f);
        tapToReturnTextGO.SetActive(true);
        gamestate = GameState.GAME_END_READY_TO_RETURN;
    }

    private IEnumerator ExplosionChainTextFadeoutCoroutine()
    {
        int chain = explosionChain;
        float timer = explosionChainTime;
        explosionChainTMP.text = "CHAIN x" + chain;
        explosionChainTMP.gameObject.SetActive(true);
        while (timer > 0)
        {
            if (timer < explosionChainTime / 2) explosionChainTMP.alpha = timer / (explosionChainTime / 2);
            if (explosionChain > chain)
            {
                chain = explosionChain;
                explosionChainTMP.text = "CHAIN x" + chain;
                timer = explosionChainTime;
            }
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        explosionChainTMP.gameObject.SetActive(false);
        explosionChainCoroutine = null;
        explosionChain = 0;
        explosionChainTMP.alpha = 1f;
    }
}

public enum GameState
{
    MAIN_MENU, GAME, GAME_END, GAME_END_READY_TO_RETURN
}