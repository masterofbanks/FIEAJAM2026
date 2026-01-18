using System.Runtime.CompilerServices;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    [Header("UI Componenets")]
    [SerializeField] private GameObject _homeScreen;
    [SerializeField] private GameObject _navigationAppUI;
    [SerializeField] private GameObject[] _minigameUIComponents;

    [Header("Minigame Timings")]
    [SerializeField] private int _minTimeBetweenGames = 2;
    [SerializeField] private int _maxTimeBetweenGames = 8;
    private int _timeBetweenGames;
    private float t;

    private System.Random rnd;
    bool inMinigame = false;

    private void Awake()
    {
        rnd = new System.Random();
    }
    private void Start()
    {
        t = 0;
        PickNewTimeBetweenGame();
    }

    private void FixedUpdate()
    {
        if (!inMinigame)
        {
            t += Time.fixedDeltaTime;
            if (t > _timeBetweenGames)
            {
                StartNewMinigame();
            }

        }
        
    }

    public void ReturnToHomeScreen(GameObject minigame)
    {
        _homeScreen.SetActive(true);
        _navigationAppUI.SetActive(false);
        inMinigame = false;
        minigame.SetActive(false);
    }

    private void StartNewMinigame()
    {
        inMinigame = true;
        int randIndex = rnd.Next(0, _minigameUIComponents.Length);
        _minigameUIComponents[randIndex].SetActive(true);
        t = 0;
        PickNewTimeBetweenGame();
    }

    private void PickNewTimeBetweenGame()
    {
        _timeBetweenGames = rnd.Next(_minTimeBetweenGames, _maxTimeBetweenGames);
    }
}
