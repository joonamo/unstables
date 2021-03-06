using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    nameEntry,
    menu,
    game,
    death,
    deathMenu
}

public enum GamePhase
{
    tutorial = 0,
    oneHorse = 1,
    twoHorse = 2,
    flyingHorse = 3
}

public class GameManager : MonoBehaviour
{
    public static GameManager GetGameManager() {
        return GameObject.FindObjectOfType<GameManager>();
    }

    public TMPro.TextMeshPro scoreText;
    public TMPro.TextMeshPro highScoreText;
    public GameObject endSplash;
    public GameObject startSplash;
    public GameObject logo;
    public GameObject scoreLogo;
    public GameObject tutorial;
    public GameObject tooltip;
    public GameObject staminaBar;
    public SpriteRenderer staminaRender;
    public Color maxStaminaColor;
    public Color minStaminaColor;
    int score = 0;
    public Canvas nameEntryCanvas;
    public TMPro.TMP_InputField nameField;
    public ScoreService scoreService;

    public GameObject WhatIsHuman;
    public List<GameObject> WhatAreHorses;
    public GameObject WhatIsCollectible;

    public float collectibleInterval = 10.0f;
    float nextCollectibleDelay;

    public Human human;
    public List<Horse> horses = new List<Horse>();

    public GameState state = GameState.menu;
    public GamePhase phase = GamePhase.tutorial;
    float timeOfDeath = 0.0f;

    public List<Vector2> minBounds;
    public List<Vector2> maxBounds;
    public List<Vector2> minBoundsHorse;
    public List<Vector2> maxBoundsHorse;
    public bool debugBounds = false;
    public bool debugBoundsHorse = false;
    
    public List<int> scoreLimits;

    public AudioSource audioOut;
    public AudioSource musicOut;
    public AudioClip collectSound;
    public AudioClip winSound;

    void JustDestroy<T>() where T : MonoBehaviour {
        foreach (var Target in GameObject.FindObjectsOfType<T>()) {
            GameObject.Destroy(Target.gameObject);
        }
    }

    void ResetGame() {
        score = 0;
        state = GameState.menu;
        phase = GamePhase.tutorial;
        nextCollectibleDelay = 0.0f;

        highScoreText.enabled = false;
        RenderScore();

        Time.timeScale = 0.0f;

        JustDestroy<Human>();
        JustDestroy<Horse>();
        JustDestroy<Collectible>();

        horses.Clear();

        var humanGo = Instantiate(WhatIsHuman, new Vector3(0, 0, -1), Quaternion.identity);
        humanGo.TryGetComponent<Human>(out human);

        SpawnHorse(new Vector3(0, 0, 1));

        var collectible = SpawnCollectible();
        collectible.expireTime = float.PositiveInfinity;

        endSplash.SetActive(false);
        startSplash.SetActive(false);
        logo.SetActive(false);
        scoreLogo.SetActive(true);
        tooltip.SetActive(true);
        tutorial.SetActive(true);
        staminaBar.SetActive(true);
        staminaBar.transform.localScale = Vector3.one;
        staminaRender.color = maxStaminaColor;
    }

    Horse SpawnHorse (Vector3 where) {
        var horseGo = Instantiate(WhatAreHorses[horses.Count], where, Quaternion.identity);
        Horse horse;
        horseGo.TryGetComponent<Horse>(out horse);
        horses.Add(horse);

        horse.maxBounds = maxBoundsHorse[horses.Count - 1];
        horse.minBounds = minBoundsHorse[horses.Count - 1];

        human.UpdateHorseParts(horses);
        return horse;
    }

    void StartGame() {
        state = GameState.game;

        musicOut.volume = 0.0f;
        musicOut.Play();

        tutorial.SetActive(false);
    }

    void RenderScore() {
        scoreText.text = "" + score;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Application.targetFrameRate = 60;
        nameField.ActivateInputField();
        nameField.onSubmit.AddListener(ReportName);
        Time.timeScale = 0.0f;
        scoreText.text = "";
        highScoreText.text = "High Scores\nLoading...";
        // highScoreText.enabled = false;

        tooltip.SetActive(false);
        tutorial.SetActive(false);
        staminaBar.SetActive(false);
    }

    public void ReportDeath() {
        if (state == GameState.game) {
            timeOfDeath = Time.unscaledTime;
            state = GameState.death;
            if (score > 0) {
                scoreService.ReportScore(score);
            } else {
                scoreService.RefreshScores();
            }

            tooltip.SetActive(false);
            staminaBar.SetActive(false);
            audioOut.clip = winSound;
            audioOut.Play();
        }
    }

    public void ShowDeathMenu() {
        highScoreText.enabled = true;
        state = GameState.deathMenu;

        JustDestroy<Human>();
        JustDestroy<Horse>();
        JustDestroy<Collectible>();

        horses.Clear();
        musicOut.Stop();

        endSplash.SetActive(true);
    }

    public void ReportScore(int amount) {
        score += amount;
        RenderScore();
        human.RefreshStamina();
        
        if (phase < GamePhase.flyingHorse && score >= scoreLimits[(int) phase]) {
            phase++;
            if (phase == GamePhase.twoHorse) {
                SpawnHorse(new Vector3(Info.screenWidth + 3.0f, 0.0f, 2.0f));
            } else if (phase == GamePhase.flyingHorse) {
                SpawnHorse(new Vector3(Info.screenWidth + 3.0f, Info.screenHeight, 3.0f));
            }
        }
        nextCollectibleDelay = 0.0f;

        if (!audioOut.isPlaying) {
            audioOut.clip = collectSound;
            audioOut.Play();
        }
    }

    public void ReportHighScoresAvailable() {
        highScoreText.text = "";
        var maxScore = 0;
        if (scoreService.scores.scores.Length > 0) {
            maxScore = scoreService.scores.scores[0].score;
        }
        int i = 1;
        foreach (var score in scoreService.scores.scores) {
            var showScore = score.score.ToString("D4");
            var showI = i == 10 ? "10" : "  " + i; 
            highScoreText.text += $"{showI}: {showScore} {score.player}\n";
            i++;
            if (i > 10) {
                break;
            }
        }
    }

    Collectible SpawnCollectible() {
        var currentMinBounds = minBounds[(int) phase];
        var currentMaxBounds = maxBounds[(int) phase];
        var side = Random.Range(0.0f, 1.0f) > 0.5f ? 1.0f : -1.0f;
        var spawnPoint = new Vector3(
            side * (Info.screenWidth + 1),
            Random.Range(-Info.screenHeight * 0.5f, 0.0f),
            -2.0f);
        var targetPoint = new Vector3(
            Random.Range(currentMinBounds.x, currentMaxBounds.x),
            Random.Range(currentMinBounds.y, currentMaxBounds.y),
            -2.0f
        );

        var go = Instantiate(WhatIsCollectible, spawnPoint, Quaternion.identity);
        var collectible = go.GetComponent<Collectible>();
        collectible.targetPosition = targetPoint;

        nextCollectibleDelay = collectibleInterval;

        return collectible;
    }

    void ReportName(string name) {
        if (state == GameState.nameEntry && name.Length > 0) {
            scoreService.ReportName(name);
            nameEntryCanvas.enabled = false;
            ResetGame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (state) {
            case (GameState.nameEntry): {
                break;
            }
            case (GameState.menu): {
                if (Input.GetButtonDown("hand")) {
                    StartGame();
                }
                break;
            }
            case (GameState.game): {
                if (Time.timeScale < 1.0f) {
                    var phase = Mathf.Clamp01(Time.timeScale + Time.unscaledDeltaTime);
                    Time.timeScale = phase;
                    musicOut.volume = phase;
                }
                if (phase > GamePhase.tutorial) {
                    nextCollectibleDelay -= Time.deltaTime;
                    if (nextCollectibleDelay <= 0.0f) {
                        SpawnCollectible();
                    }
                }

                var staminaPhase = human.stamina / 100.0f;
                staminaBar.transform.localScale = new Vector3(1.0f, staminaPhase, 1.0f);
                staminaRender.color = Vector4.Lerp(minStaminaColor, maxStaminaColor, staminaPhase);

                break;
            }
            case (GameState.death): {
                if (Time.timeScale > 0.00f) {
                    var phase = Mathf.Clamp01(Time.timeScale - Time.unscaledDeltaTime);
                    musicOut.volume = phase;
                    Time.timeScale = phase;
                }

                if (Input.GetButtonDown("ok") || Input.GetButtonDown("Submit") || Time.unscaledTime - timeOfDeath > 3.0) {
                    ShowDeathMenu();
                }
                break;
            }
            case (GameState.deathMenu): {
                if (Input.GetButtonDown("ok") || Input.GetButtonDown("Submit")) {
                    ResetGame();
                }
                break;
            }
        }
    }

    void OnDrawGizmos() {
        if (debugBounds) {
            for (int i = 0; i <= 3; ++i) {
                var mi = minBounds[i];
                var ma = maxBounds[i];
                Gizmos.DrawLine(new Vector3(mi.x, mi.y, 0.0f), new Vector3(mi.x, ma.y, 0.0f));
                Gizmos.DrawLine(new Vector3(mi.x, mi.y, 0.0f), new Vector3(ma.x, mi.y, 0.0f));
                Gizmos.DrawLine(new Vector3(ma.x, mi.y, 0.0f), new Vector3(ma.x, ma.y, 0.0f));
                Gizmos.DrawLine(new Vector3(mi.x, ma.y, 0.0f), new Vector3(ma.x, ma.y, 0.0f));
            }
        }
        if (debugBoundsHorse)??{
            for (int i = 0; i <= 2; ++i) {
                var mi = minBoundsHorse[i];
                var ma = maxBoundsHorse[i];
                Gizmos.DrawLine(new Vector3(mi.x, mi.y, 0.0f), new Vector3(mi.x, ma.y, 0.0f));
                Gizmos.DrawLine(new Vector3(mi.x, mi.y, 0.0f), new Vector3(ma.x, mi.y, 0.0f));
                Gizmos.DrawLine(new Vector3(ma.x, mi.y, 0.0f), new Vector3(ma.x, ma.y, 0.0f));
                Gizmos.DrawLine(new Vector3(mi.x, ma.y, 0.0f), new Vector3(ma.x, ma.y, 0.0f));
            }
        }
    }

}
