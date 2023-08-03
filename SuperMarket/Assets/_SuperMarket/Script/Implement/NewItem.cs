using System;
using UnityEngine;

public class NewItem : IObject, IAction
{
    public static event EventHandler<OnCorrectItemEventArgs> OnCorrectItem;
    public static event EventHandler OnWrongItem;
    public class OnCorrectItemEventArgs: EventArgs
    {
        public string stuffTag;
    }
    private GameObject _gameObject;
    private bool _inCart;
    public static int sortingOrder;
    public NewItem(GameObject newGameObject, Vector3 position, int stuffIndex)
    {
        _gameObject = newGameObject;
        _gameObject.transform.position = position;
        _gameObject.tag = "Stuff" + stuffIndex;
        sortingOrder++;
        _gameObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
    }
    public bool ActivateState { 
        get =>  _gameObject.activeSelf; 
        set => _gameObject.SetActive(value); 
    }

    public void Action()
    {
        if (_gameObject.activeSelf && MainManager.Instance.IsPickUpItem() && !_inCart)
        {
            _gameObject.transform.position = MainManager.Instance.GetMouseWorldPosition();
        } 
        else if (_gameObject.activeSelf && !MainManager.Instance.IsPickUpItem() && !_inCart)
        {
            if (_gameObject.transform.position.x < MainManager.Instance.GetSizeCart().widthCartMax &&
                _gameObject.transform.position.x > MainManager.Instance.GetSizeCart().widthCartMin &&
                _gameObject.transform.position.y < MainManager.Instance.GetSizeCart().heightCartMax &&
                _gameObject.transform.position.y > MainManager.Instance.GetSizeCart().heightCartMin)
            {
                bool isCorrect = false;
                for(int i = 0; i < MainManager.Instance.GetStuffList().Count;i++)
                {
                    if (MainManager.Instance.GetStuffList()[i].StuffTag == _gameObject.tag && MainManager.Instance.GetStuffList()[i].Amount > 0)
                    {
                        _inCart = true;
                        _gameObject.transform.parent = MainManager.Instance.GetSizeCart().cartTransform;
                        Vector3 randomPosInCart = new Vector3(UnityEngine.Random.Range(MainManager.Instance.GetSizeCart().widthCartMin + 1f + _gameObject.GetComponent<SpriteRenderer>().bounds.size.x / 2, MainManager.Instance.GetSizeCart().widthCartMax - 1f - _gameObject.GetComponent<SpriteRenderer>().bounds.size.x / 2),
                                                              MainManager.Instance.GetSizeCart().heightCartMin + 1.5f + _gameObject.GetComponent<SpriteRenderer>().bounds.size.y / 2);
                        _gameObject.transform.position = randomPosInCart;
                        OnCorrectItem?.Invoke(this, new OnCorrectItemEventArgs
                        {
                            stuffTag = _gameObject.tag
                        });
                        isCorrect = true;
                        break;
                    }
                }
                if(!isCorrect)
                {
                    OnWrongItem?.Invoke(this, EventArgs.Empty);
                    ActivateState = false;
                }

            } else
            {
                ActivateState = false;
            }      
        }
    }

    public void SetUp(Vector3 position, int stuffIndex)
    {
        _gameObject.transform.parent = null;
        _gameObject.transform.position = position;
        _gameObject.tag = "Stuff" + stuffIndex;
        sortingOrder++;
        _gameObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        _inCart = false;
    }

    public void SettingSprite(Sprite sprite)
    {
        _gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }
}
