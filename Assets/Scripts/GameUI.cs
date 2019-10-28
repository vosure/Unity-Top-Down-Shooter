using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour
{

    public Image fade;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;

    Spawner spawner;

    void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "Wave " + numbers[waveNumber - 1];
        string enemyCountString = (spawner.waves[waveNumber - 1].infinite) ? "Infinite" : spawner.waves[waveNumber - 1].enemiesCount + "";
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;

        StartCoroutine(AnimateNewWaveBanner());
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float delayTime = 1.0f;
        float speed = 2.5f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime)
                    dir = -1;
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-170, 90, animatePercent);
            yield return null;
        }
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fade.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

}
