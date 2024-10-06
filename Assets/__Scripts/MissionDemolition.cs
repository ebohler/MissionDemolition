using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameMode {
    idle,
    playing,
    levelEnd
}

public class MissionDemolition : MonoBehaviour
{
    static public MissionDemolition S;

    [Header("Inscribed")]
    public Text uitLevel;
    public Text uitShots;
    public Text uitTimer;  // Text object to display the timer
    public Vector3 castlePos;
    public GameObject[] castles;

    [Header("Dynamic")]
    public int level;
    public int levelMax;
    public int shotsLeft;
    public GameObject castle;
    public GameMode mode = GameMode.idle;
    public string showing = "Show Slingshot";

    private float lastShotFiredTime = -1f;  // To track time of last shot
    private float gameOverDelay = 15f;  // 15 second delay for game over

    void Start() {
        S = this;

        level = 0;
        levelMax = castles.Length;

        StartLevel();

        // Hide the timer text initially
        uitTimer.text = "";
    }

    void StartLevel() {
        if (castle != null) {
            Destroy(castle);
        }

        Projectile.DESTROY_PROJECTILES();

        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        Goal.goalMet = false;

        // Reset shots to 10 for each level
        shotsLeft = 10;

        UpdateGUI();

        mode = GameMode.playing;
        
        uitTimer.text = "";

        // Zoom out to show both
        FollowCam.SWITCH_VIEW(FollowCam.eView.both);
    }

    void UpdateGUI() {
        uitLevel.text = "Level: " + (level+1) + " of " + levelMax;
        uitShots.text = "Shots Left: " + shotsLeft;
    }

    void Update() {
        UpdateGUI();

        // Check if goal is met and handle level end
        if ((mode == GameMode.playing) && Goal.goalMet) {
            mode = GameMode.levelEnd;

            FollowCam.SWITCH_VIEW(FollowCam.eView.both);

            Invoke("NextLevel", 2f);
        }

        // Handle timer display and countdown after last shot
        if (shotsLeft <= 0 && lastShotFiredTime > 0) {
            float timeRemaining = gameOverDelay - (Time.time - lastShotFiredTime);

            // Update the timer on the screen
            if (timeRemaining > 0) {
                uitTimer.text = "Game Over in: " + Mathf.Ceil(timeRemaining).ToString() + "s";
            } else {
                uitTimer.text = "";
                GameOver();  // When the timer hits 0, game over
            }
        }
    }

    void NextLevel() {
        level++;
        if (level == levelMax) {
            SceneManager.LoadScene("_Scene_Victory");
        }
        StartLevel();
    }

    static public void SHOT_FIRED() {
        S.shotsLeft--;

        if (S.shotsLeft == 0) {
            // Start the 15-second timer after the last shot is fired
            S.lastShotFiredTime = Time.time;
        }
    }

    void GameOver() {
        SceneManager.LoadScene("_Scene_GameOver");  // Load Game Over Scene
    }

    static public GameObject GET_CASTLE() {
        return S.castle;
    }
}
