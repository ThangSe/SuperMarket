using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObject
{
    bool ActivateState
    {
        get;
        set;
    }
    void SettingSprite(Sprite sprite);
}
