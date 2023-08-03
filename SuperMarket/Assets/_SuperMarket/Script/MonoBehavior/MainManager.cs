using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnRecipeChanged;

    [SerializeField] private SOGameSettings _gameSettingRefsSO;
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _cameraFollow;
    [SerializeField] private Camera _mainCamera;


    [Header("Button")]
    [SerializeField] private Button _tutorialButton, _playButton, _replayButton;

    private Vector3 _distanceFromMouseDrag;
    private Vector3 _mouseWorldPosition;
    private Vector3 _originCamera;
    private bool _drag;
    private bool _isPickUp;
    private bool _isComplete;
    [SerializeField] private GameObject _itemGO, _payGO;
    [SerializeField] private Transform _cartTransform;
    [SerializeField] private UIManager _uiManager;

    public struct StuffInfo
    {
        public string StuffTag;
        public int Amount;
        public bool IsFull;
    }

    public List<StuffInfo> RecipeStuff;
    private enum State
    {
        Home,
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameEnd,

    }

    private State _state;
    [SerializeField] private Image[] _stuffsImg;
    List<IObject> _objectList;
    List<IAction> _actionList;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        _gameSettingRefsSO.TurnPlayLeft.Value = _gameSettingRefsSO.TurnPlayMax.Value;
        _state = State.WaitingToStart;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Start()
    {
        _objectList = new List<IObject>();
        _actionList = new List<IAction>();
        RecipeStuff = new List<StuffInfo>();
        SetUpDefault();
        /*_tutorialButton.onClick.AddListener(() =>
        {
            if (_gameSettingRefsSO.TurnPlayLeft.Value > 0)
            {
                _state = State.WaitingToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        });*/
        _replayButton.onClick.AddListener(() =>
        {
            SetUpDefault();
            if (_gameSettingRefsSO.TurnPlayLeft.Value > 0)
            {
                _state = State.WaitingToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _state = State.Home;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        });
        _playButton.onClick.AddListener(() =>
        {
            _state = State.CountdownToStart;
            OnRecipeChanged?.Invoke(this, EventArgs.Empty);
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        NewItem.OnCorrectItem += NewItem_OnCorrectItem;
    }

    private void NewItem_OnCorrectItem(object sender, NewItem.OnCorrectItemEventArgs e)
    {
        for(int i = 0; i < RecipeStuff.Count; i++)
        {
            if (RecipeStuff[i].StuffTag == e.stuffTag)
            {
                StuffInfo sf = RecipeStuff[i];
                sf.Amount --;
                if (sf.Amount == 0) sf.IsFull = true;
                RecipeStuff[i] = sf;
                CheckComplete();
                OnRecipeChanged?.Invoke(this, EventArgs.Empty);
                break;
            }
        }
    }

    private void SetUpDefault()
    {
        _gameSettingRefsSO.CountdownToStartTime.Value = _gameSettingRefsSO.CountdownToStartTimeMax.Value;
        _gameSettingRefsSO.TimePlayingLeft.Value = _gameSettingRefsSO.TimePlayingMax.Value;
        Camera.main.transform.position = _gameSettingRefsSO.StartingPosition;
        _isComplete = false;
        RandomRecipe();
        for(int i = 0; i < _objectList.Count; i++)
        {
            _objectList[i].ActivateState = false;
        }
    }

    private void RandomRecipe()
    {
        RecipeStuff.Clear();
        for(int i = 0; i < 4; i++)
        {
            StuffInfo newStuff = new StuffInfo();
            int randomChild = UnityEngine.Random.Range(0, GameObject.Find("Store_" + i).transform.childCount);
            newStuff.StuffTag = GameObject.Find("Store_" + i).transform.GetChild(randomChild).tag;
            newStuff.Amount = UnityEngine.Random.Range(1, 11);
            newStuff.IsFull = false;
            RecipeStuff.Add(newStuff);
        }
    }
    private void Update()
    {
        switch (_state)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                _gameSettingRefsSO.CountdownToStartTime.Value -= Time.deltaTime;
                if (_gameSettingRefsSO.CountdownToStartTime.Value < 0f)
                {
                    _state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                _gameSettingRefsSO.TimePlayingLeft.Value -= Time.deltaTime;
                if(_gameSettingRefsSO.TimePlayingLeft.Value < 0f)
                {
                    _state = State.GameEnd;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                PickUpStuff();
                PayStuff();
                UpdateAction();
                break;
            case State.GameEnd:
                break;
        }
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            _distanceFromMouseDrag = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            if (_drag == false)
            {
                _drag = true;
                _originCamera = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else
        {
            _drag = false;
        }

        if(_drag)
        {
            Vector3 moveHorizontal = _originCamera - _distanceFromMouseDrag;
            moveHorizontal.y = 0;
            if(moveHorizontal.x < 0) moveHorizontal.x = 0;
            if(moveHorizontal.x > 60) moveHorizontal.x = 60;
            Camera.main.transform.position = moveHorizontal;
        }
    }

    public Image GetImageStuff(int index)
    {
        return _stuffsImg[index];
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseClikPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseClikPos.z = 0;
        return mouseClikPos;
    }

    private void PickUpStuff()
    {
        if (Input.GetMouseButtonDown(0) && !_drag)
        {
            _mouseWorldPosition = GetMouseWorldPosition();
            if(!_isPickUp) CurrentPlayerPosition();
            else _isPickUp = false;
        }
    }

    private void CurrentPlayerPosition()
    {
        bool check = false;
        if(_mouseWorldPosition.x  <= 10.5)
        {
            foreach(Transform stuffStore in GameObject.Find("Store_0").transform)
            {
                check = FindStuff(stuffStore);
                if(check) break;
            }
            if (check) return;
        } 
        if(_mouseWorldPosition.x > 5.5 &&
           _mouseWorldPosition.x < 25)
        {
            foreach (Transform stuffStore in GameObject.Find("Store_1").transform)
            {
                check = FindStuff(stuffStore);
                if (check) break;
            }
            if (check) return;
        }
        if (_mouseWorldPosition.x > 20 &&
           _mouseWorldPosition.x < 40)
        {
            foreach (Transform stuffStore in GameObject.Find("Store_2").transform)
            {
                check = FindStuff(stuffStore);
                if (check) break;
            }
            if (check) return;
        }
        if (_mouseWorldPosition.x > 36 &&
           _mouseWorldPosition.x < 54)
        {
            foreach (Transform stuffStore in GameObject.Find("Store_3").transform)
            {
                check = FindStuff(stuffStore);
                if (check) break;
            }
            if (check) return;
        }
    }

    public bool FindStuff(Transform stuffStore)
    {
        if (stuffStore.GetComponent<SpriteRenderer>() == null) return false;
        if (!stuffStore.tag.Contains("Stuff")) return false;
        if (stuffStore.position.x + stuffStore.GetComponent<SpriteRenderer>().bounds.size.x / 2 > _mouseWorldPosition.x &&
            stuffStore.position.x - stuffStore.GetComponent<SpriteRenderer>().bounds.size.x / 2 < _mouseWorldPosition.x &&
            stuffStore.position.y + stuffStore.GetComponent<SpriteRenderer>().bounds.size.y / 2 > _mouseWorldPosition.y &&
            stuffStore.position.y - stuffStore.GetComponent<SpriteRenderer>().bounds.size.y / 2 < _mouseWorldPosition.y)
        {
            int stuffIndex = Int32.Parse(new String(stuffStore.tag.Where(Char.IsDigit).ToArray()));
            AddItem(_mouseWorldPosition, stuffIndex);
            _isPickUp = true;
            return true;
        } else
        {
            return false;
        }
    }

    public (Transform cartTransform, float widthCartMin, float widthCartMax, float heightCartMin, float heightCartMax) GetSizeCart()
    {
        return (
            _cartTransform,
            _cartTransform.position.x - _cartTransform.GetComponent<SpriteRenderer>().bounds.size.x / 2,
            _cartTransform.position.x + _cartTransform.GetComponent<SpriteRenderer>().bounds.size.x / 2,
            _cartTransform.position.y - _cartTransform.GetComponent<SpriteRenderer>().bounds.size.y / 2,
            _cartTransform.position.y + _cartTransform.GetComponent<SpriteRenderer>().bounds.size.y / 2);
    }

    private void PayStuff()
    {
        if (!_isComplete) return;
        if (_mouseWorldPosition.x > 58 && 
            _payGO.transform.position.x + _payGO.GetComponent<SpriteRenderer>().bounds.size.x / 2 > _mouseWorldPosition.x &&
            _payGO.transform.position.x - _payGO.GetComponent<SpriteRenderer>().bounds.size.x / 2 < _mouseWorldPosition.x &&
            _payGO.transform.position.y + _payGO.GetComponent<SpriteRenderer>().bounds.size.y / 2 > _mouseWorldPosition.y &&
            _payGO.transform.position.y - _payGO.GetComponent<SpriteRenderer>().bounds.size.y / 2 < _mouseWorldPosition.y &&
            Input.GetMouseButtonDown(0) &&
            !_drag)
        {
            _state = State.GameEnd;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private void CheckComplete()
    {
        _isComplete = true;
        for(int i = 0; i < RecipeStuff.Count; i++)
        {
            if (RecipeStuff[i].Amount != 0)
            {
                _isComplete = false;
                break;
            }
        }
    }

    public float GetPlayingTimer()
    {
        return _gameSettingRefsSO.TimePlayingLeft.Value;
    }

    public List<StuffInfo> GetStuffList()
    {
        return RecipeStuff;
    }

    public bool IsPickUpItem()
    {
        return _isPickUp;
    }

    public bool IsHomeState()
    {
        return _state == State.Home;
    }

    public bool IsWaitingToStartState()
    {
        return _state == State.WaitingToStart;
    }

    public bool IsCountdownToStartState()
    {
        return _state == State.CountdownToStart;
    }

    public bool IsGamePlayingState()
    {
        return _state == State.GamePlaying;
    }

    public bool IsGameEndState()
    {
        return _state == State.GameEnd;
    }

    private void UpdateAction()
    {
        for (int i = 0; i < _actionList.Count; i++)
        {
            _actionList[i].Action();
        }
    }

    private void AddItem(Vector3 position, int stuffIndex)
    {
        bool isAdded = false;
        for(int i = 0; i < _objectList.Count; i++)
        {
            if (_objectList[i].ActivateState == false && _objectList[i] is NewItem item)
            {
                _objectList[i].ActivateState = true;
                _objectList[i].SettingSprite(_stuffsImg[stuffIndex].sprite);
                item.SetUp(position, stuffIndex);
                isAdded = true;
                break;
            }
        }
        if(!isAdded)
        {
            _objectList.Add(new NewItem(Instantiate(_itemGO), position, stuffIndex));
            _objectList[_objectList.Count - 1].SettingSprite(_stuffsImg[stuffIndex].sprite);
            _actionList.Add(_objectList[_objectList.Count - 1] as NewItem);
        }
    }
}
