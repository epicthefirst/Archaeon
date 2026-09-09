using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private string playerName;
    private Vector2 playerCapital;
    private int totalPlayerEconCount;
    private int totalPlayerIndustryCount;
    private int totalPlayerScienceCount;
    private int totalPlayerShipCount;
    private int playerWeaponLevel;
    private int playerMnufacturingLevel;
    private int playerScanningLevel;
    private int playerRangeLevel;
    private List<GameObject> playerCarrierList = new List<GameObject>();
    private List<GameObject> playerStarList = new List<GameObject>();
    public GameInformation.PlayerClass playerClass;

    public int playerMoney = 500;



    public int cycleCount = 0;
    public int tickCount = 0;
    public List<GameObject> playerStars = new List<GameObject>();

    public void AddStar(GameObject star)
    {
        if (playerStars.Contains(star) != true)
        {
            playerStars.Add(star);
            Debug.Log("Added star");
        }
        else
        {
            Debug.LogError("Not supposed to happen");
        }
        
    }
    public void RemoveStar(GameObject star)
    {
        if (playerStars.Contains (star) == true)
        {
            playerStars.Remove(star);
        }
        else
        {
            Debug.LogError("Not supposed to happen either");
        }
    }
    public int NewCycle(int cycleCount)
    {
        this.cycleCount = cycleCount;
        Debug.Log("Cycle:" + cycleCount);
        totalPlayerEconCount = 0;
        foreach (GameObject s in playerStars)
        {
            StarScript ss = s.GetComponent<StarScript>();
            totalPlayerEconCount += ss.EconCount;
            totalPlayerIndustryCount += ss.IndustryCount;
            totalPlayerScienceCount += ss.ScienceCount;
        }
        Debug.Log("New econCount" + totalPlayerEconCount);
        Debug.Log("New industryCount" + totalPlayerIndustryCount);
        Debug.Log("New scienceCount" + totalPlayerScienceCount);
        playerMoney += totalPlayerEconCount * GameInformation.cycleLength;
        return totalPlayerEconCount;
    }
    public int NewTick(int tickCount)
    {
        this.tickCount = tickCount;
        //Debug.Log("Tick:" + tickCount);
        totalPlayerEconCount = 0;
        foreach (GameObject s in playerStars)
        {
            StarScript ss = s.GetComponent<StarScript>();
            totalPlayerEconCount += ss.EconCount;
            totalPlayerIndustryCount += ss.IndustryCount;
            totalPlayerScienceCount += ss.ScienceCount;
        }
        //Debug.Log("New econCount" + totalPlayerEconCount);
        //Debug.Log("New industryCount" + totalPlayerIndustryCount);
        //Debug.Log("New scienceCount" + totalPlayerScienceCount);
        playerMoney += totalPlayerEconCount;
        return totalPlayerEconCount;
    }

    public void newCarrier(GameObject newCarrier)
    {
        playerCarrierList.Add(newCarrier);
    }
    public void removeCarrier(GameObject carrierRemove)
    {
        playerCarrierList.Remove(carrierRemove);
    }
    public int GetCombatLevel()
    {
        return playerWeaponLevel;
    }
    public void test()
    {
        Debug.Log("test success");
    }
}
