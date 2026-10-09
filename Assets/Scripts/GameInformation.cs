using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class GameInformation
{

    public static bool demoMode = false;

    public static int playerCount;
    public static List<PlayerClass> playerList = new List<PlayerClass>();

    public static int seed;

    public static int tickCounter = 0;
    public static int playerMoney = 500;
    public static int cycleLength = 12; // Change this in the future
    public static int carrierCost = 25; //This too

    public static int qualityMultiplier = 4;
    public static int numberOfStars;
    public static int numberOfCircles = 5;
    public static double offset = 60d;
    public static int minRandOffset = 10;
    public static int maxRandOffset = 15;

    public static string gameMode = "Normal";
    public static int NumberOfBots;



    public static GameObject shipPrefab;

    public static void init(int PlayerMoney, int CycleLength, int NumberOfCircles, GameObject ShipPrefab, string GameMode, int DesiredNumberOfBots, int seedInput)
    {
        Debug.LogWarning("Initialising GameInformation.");
        Debug.LogWarning("Player money: " + PlayerMoney);
        Debug.LogWarning("Cycle length: " + CycleLength);
        Debug.LogWarning("Number of circles: " + NumberOfCircles);
        Debug.LogWarning("Gamemode: " + GameMode);
        playerMoney = PlayerMoney;
        cycleLength = CycleLength;
        numberOfCircles = NumberOfCircles;
        shipPrefab = ShipPrefab;
        gameMode = GameMode;
        NumberOfBots = DesiredNumberOfBots;
        seed = seedInput;

        
    }

    public static int GetQualityMultiplier()
    {
        return qualityMultiplier;
    }

    public static void AddPlayer(PlayerClass player)
    {
        if (playerList.Contains(player))
        {
            Debug.LogError("Already in list");
            return;
        }
        else
        {
            playerList.Add(player);
        }
    }
    public static PlayerClass GetPlayerByID(int playerNumberID)
    {
        return playerList[playerNumberID];
    }
    public static PlayerClass GetPlayerByName(string name)
    {
        foreach (PlayerClass player in playerList)
        {
            if (player.name.Equals(name)){
                return player;
            }
        }
        return null;
    }

    public static void WakeUpPlayers()
    {
        foreach(PlayerClass player in playerList)
        {
            if (player.isBot)
            {
                player.botScript.WakeUp();
            }
        }
    }


    public class PlayerClass
    {
        public string name;
        public string description;

        //Playerscript or botscript
        public PlayerScript playerScript;
        public EnemyBotBehavior botScript;
        public bool isBot;

        public List<PlayerClass> Allies;

        public Color primaryColour;
        public Color secondaryColour;
        public Material primaryMaterial;
        public Material secondaryMaterial;




        public PlayerClass(string name, bool isBot, PlayerScript playerScript, EnemyBotBehavior enemyBotBehavior, Color primaryColour, Color secondaryColour, Material primaryMaterial, Material secondaryMaterial)
        {
            this.name = name;
            this.isBot = isBot;
            if (isBot)
            {
                this.botScript = enemyBotBehavior;
                Debug.Log("Clanker made");
            }
            else
            {
                this.playerScript = playerScript;
            }
                
            this.primaryColour = primaryColour;
            this.secondaryColour = secondaryColour;
            this.primaryMaterial = primaryMaterial;
            this.secondaryMaterial = secondaryMaterial;



        }
        int test = 0;
        public void AddCarrierToOwner(GameObject carrier)
        {
            //test++;
            //Debug.LogError(test);   
            if (!isBot)
            {
                playerScript.newCarrier(carrier);
            }
            else
            {
                botScript.addCarrier(carrier);
            }
        }
        public void UpdateCarrierOfOwner(GameObject carrier)
        {
            //test++;
            //Debug.LogError(test);   
            if (!isBot)
            {
                return;
            }
            else
            {
                botScript.updateCarrier(carrier);
            }
        }
        public void RemoveCarrierFromOwner(GameObject carrier)
        {
            if (!isBot)
            {
                playerScript.removeCarrier(carrier);
            }
            else
            {
                botScript.removeCarrier(carrier);
            }
        }
        public void AddStarToOwner(GameObject star)
        {
            if (!isBot)
            {
                playerScript.AddStar(star);
            }
            else
            {
                //Debug.LogError("Adding star");
                botScript.addStar(star);
            }
        }
        public void RemoveStarFromOwner(GameObject star)
        {
            if (!isBot)
            {
                playerScript.RemoveStar(star);
            }
            else
            {
                //Debug.LogError("Adding star");
                botScript.removeStar(star);
            }
        }

        public void UpdateStarOfOwner(GameObject star)
        {
            if (!isBot)
            {
///////////////////////////////
            }
            else
            {
                botScript.addStar(star);
            }
        }

        public void SimpleUpdateStarOfOwner(GameObject star, int garrisonCount)
        {
            if (!isBot)
            {
                ///////////////////////////////
            }
            else
            {
                botScript.updateGarrisonHeap(star, garrisonCount);
            }
        }

        public Research GetResearchScript()
        {
            if(!isBot)
            {
                return playerScript.playerResearchScript;
            }
            else
            {
                return botScript.botResearchScript;
            }
        }

    }
}
