
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Research : MonoBehaviour
{
    public GameInformation.PlayerClass Owner;

    private int playerWeaponLevel;
    private int playerMnufacturingLevel;
    private int playerScanningLevel;
    private int playerRangeLevel;

    public Research(GameInformation.PlayerClass owner, int initWeapons, int initManu, int initScanning, int initRange)
    {
        Owner = owner;

        playerWeaponLevel = initWeapons;
        playerMnufacturingLevel = initManu;
        playerScanningLevel = initScanning;
        playerRangeLevel = initRange;
    }

    public int GetWeaponsLevel()
    {
        return playerWeaponLevel;
    }

    public int GetManufacturingLevel()
    {
        return playerMnufacturingLevel;
    }

    public int GetScanningLevel()
    {
        return playerScanningLevel;
    }

    public int GetRangeLevel()
    {
        return playerScanningLevel;
    }
}
