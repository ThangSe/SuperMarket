using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text _gameEndText, _gameEndScoreText, _textTimer;
    [SerializeField] private GameObject _backgroundPanel, _homePage, _tutorialPage, _basePage, _gamePage, _gameEndPage;

    [SerializeField] private GameObject _popupGO;
    [SerializeField] private Transform[] _recipeTransform;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioGamePlay, _audioPoint, _audioWrong, _audioGameEnd;

    private bool _firstTime = true;
    private int _index;
    private void Start()
    {
        MainManager.Instance.OnStateChanged += MainManager_OnStateChanged;
        MainManager.Instance.OnRecipeChanged += MainManager_OnRecipeChanged;
        NewItem.OnWrongItem += NewItem_OnWrongItem;
    }

    private void NewItem_OnWrongItem(object sender, EventArgs e)
    {
        _audioWrong.Play();
    }

    private void Update()
    {
        if(MainManager.Instance.IsGamePlayingState())
        {
            float timer = MainManager.Instance.GetPlayingTimer();
            if (Mathf.RoundToInt(timer) < Mathf.CeilToInt(timer))
            {
                SetTimerText(timer);
            }
        }
    }

    private void SetTimerText(float timer)
    {
        if (timer >= 60f)
        {
            int minute = Mathf.RoundToInt(timer / 60);
            int second = Mathf.RoundToInt(timer - 60 * minute);
            if (second < 0)
            {
                minute--;
                second += 60;
            }
            if (second < 10)
            {
                _textTimer.text = minute + ":0" + second;
            }
            else
            {
                _textTimer.text = minute + ":" + second;
            }
        }
        if (timer < 60f && timer > 0f)
        {
            if (timer < 10)
            {
                _textTimer.text = "0:0" + Mathf.RoundToInt(timer);
            }
            else
            {
                _textTimer.text = "0:" + Mathf.RoundToInt(timer);
            }

        }
        if (timer <= 0f)
        {
            _textTimer.text = "0:00";
        }
    }
    private void MainManager_OnRecipeChanged(object sender, EventArgs e)
    {
        if (_firstTime)
        {
            for (int i = 0; i < MainManager.Instance.GetStuffList().Count; i++)
            {
                int stuffIndex = Int32.Parse(new String(MainManager.Instance.GetStuffList()[i].StuffTag.Where(Char.IsDigit).ToArray()));
                _recipeTransform[i].GetChild(0).GetComponent<Image>().sprite = MainManager.Instance.GetImageStuff(stuffIndex).sprite;
                _recipeTransform[i].GetChild(0).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
                    _recipeTransform[i].GetChild(0).GetComponent<Image>().sprite.bounds.size.y * 100 / _recipeTransform[i].GetChild(0).GetComponent<Image>().sprite.bounds.size.y);
                _recipeTransform[i].GetChild(0).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                    _recipeTransform[i].GetChild(0).GetComponent<Image>().sprite.bounds.size.x * 100 / _recipeTransform[i].GetChild(0).GetComponent<Image>().sprite.bounds.size.y);
                _recipeTransform[i].GetChild(1).GetComponent<Text>().text = MainManager.Instance.GetStuffList()[i].Amount.ToString();
                _recipeTransform[i].GetChild(2).gameObject.SetActive(false);
            }
            _firstTime = false;
        } else
        {
            for (int i = _index; i < MainManager.Instance.GetStuffList().Count; i++)
            {
                _recipeTransform[i].GetChild(1).GetComponent<Text>().text = MainManager.Instance.GetStuffList()[i].Amount.ToString();
                if (MainManager.Instance.GetStuffList()[i].Amount == 0)
                {
                    _recipeTransform[i].GetChild(2).gameObject.SetActive(true);
                    _audioPoint.Play();
                    _index++;
                }
            }
        }    
    }

    private void MainManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (MainManager.Instance.IsHomeState())
        {

            _homePage.SetActive(true);
            _gameEndPage.SetActive(false);
            _audioGameEnd.volume = 0f;
        }
        if (MainManager.Instance.IsWaitingToStartState())
        {
            _audioGameEnd.volume = 0f;
            _tutorialPage.SetActive(true);
            _homePage.SetActive(false);
            _gameEndPage.SetActive(false);
        }
        if (MainManager.Instance.IsCountdownToStartState())
        {
            _audioGamePlay.time = 0f;
            if (!_audioGamePlay.isPlaying) _audioGamePlay.Play();
            _audioGamePlay.volume = 1f;
            SetTimerText(MainManager.Instance.GetPlayingTimer());
            _index = 0;
            _backgroundPanel.SetActive(false);
            _tutorialPage.SetActive(false);
            _gamePage.SetActive(true);
        }
        if (MainManager.Instance.IsGamePlayingState())
        {

        }
        if (MainManager.Instance.IsGameEndState())
        {
            _audioGamePlay.volume = 0f;
            _audioGameEnd.time = 0f;
            if (!_audioGameEnd.isPlaying) _audioGameEnd.Play();
            _audioGameEnd.volume = 1f;
            _firstTime = true;
            _gamePage.SetActive(false);
            _backgroundPanel.SetActive(true);
            _gameEndPage.SetActive(true);
        }
    }
}
