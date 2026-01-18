using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float _timeInLevel = 180;
    [SerializeField] private GameObject _portal;
    [SerializeField] private string NextSceneName;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private GameObject _blackOutEffect;
    [SerializeField] private AnimationClip _blackOutEffectClip;
    [SerializeField] private float _timeBeforeBlackOut = 3f;

    private float t;
    private void Start()
    {
        t = _timeInLevel;
    }

    private void Update()
    {
        if(t > 0)
        {
            t -= Time.deltaTime;
            TimeSpan tSpan = TimeSpan.FromSeconds(t);
            _timerText.text = tSpan.ToString(@"mm\:ss");
            if (t <= 0)
            {
                t = 0;
                _portal.SetActive(false);
                _timerText.text = "You ran out of time!";
                StartCoroutine(LoseGame());
            }
        }
        
    }

    IEnumerator LoseGame()
    {
        _blackOutEffect.SetActive(true);
        float offsetFromBlackOut = 0.5f;
        yield return new WaitForSeconds(_timeBeforeBlackOut + _blackOutEffectClip.length + offsetFromBlackOut);
        SceneManager.LoadScene("LoseScreen");
    }

    IEnumerator WinGame()
    {
        _blackOutEffect.SetActive(true);
        float offsetFromBlackOut = 0.5f;
        yield return new WaitForSeconds(_timeBeforeBlackOut + _blackOutEffectClip.length + offsetFromBlackOut);
        SceneManager.LoadScene("WinScreen");
    }

    public void ProceedToNextLevel()
    {
        SceneManager.LoadScene(NextSceneName);
    }

    public void ProceedToWin()
    {
        StartCoroutine(WinGame());
    }
}
