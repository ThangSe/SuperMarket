using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings SO", menuName = "ScriptableObjects/GameSettingRefsSO")]
public class SOGameSettings : ScriptableObject
{
    public SOFloatVariable
        CountdownToStartTime, CountdownToStartTimeMax,
        TurnPlayLeft, TurnPlayMax,
        TimePlayingLeft, TimePlayingMax;
    public Vector3 StartingPosition;
    public enum StuffTag
    {
        Stuff0 = 0,
        Stuff1 = 1,
        Stuff2 = 2,
        Stuff3 = 3,
        Stuff4 = 4,
        Stuff5 = 5,
        Stuff6 = 6,
        Stuff7 = 7,
        Stuff8 = 8,
        Stuff9 = 9,
        Stuff10 = 10,
        Stuff11 = 11,
        Stuff12 = 12,
        Stuff13 = 13,
        Stuff14 = 14,
        Stuff15 = 15,
        Stuff16 = 16,
        Stuff17 = 17,
        Stuff18 = 18,
        Stuff19 = 19,
        Stuff20 = 20,
        Stuff21 = 21,
        Stuff22 = 22,
        Stuff23 = 23,
        Stuff24 = 24,
        Stuff25 = 25,
        Stuff26 = 26,
        Stuff27 = 27,
        Stuff28 = 28,
        Stuff29 = 29,
        Stuff30 = 30,
        Stuff31 = 31,
        Stuff32 = 32,
        Stuff33 = 33,
        Stuff34 = 34,
        Stuff35 = 35,
        Stuff36 = 36,
        Stuff37 = 37,
        Stuff38 = 38,
    }
}
