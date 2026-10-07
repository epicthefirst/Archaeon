using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBotBehavior : MonoBehaviour
{
    //Testing
    int stupidCounter = 0;
    List<string> removedCarrierList = new List<string>();


    //General Bot Logic
    public GameInformation.PlayerClass bot;
    public UIManager uIManager;
    public MapGeneration mapGenerationScript;
    public Pathfinder pathfinderScript;
    public int carrierNameIncrement;

    public Research botResearchScript;

    private List<GameObject> carrierList = new List<GameObject>();
    private Pathfinder.MaxObjBinaryHeap carrierSizeHeap = new Pathfinder.MaxObjBinaryHeap(1024); //Change later
    private Pathfinder.MaxObjBinaryHeap idleCarrierHeap = new Pathfinder.MaxObjBinaryHeap(1024); //Change later


    private List<GameObject> ownedStars = new List<GameObject>();
    private Dictionary<int, StarScript> IdToScript = new Dictionary<int, StarScript>();
    private Pathfinder.Graph knownGraph;
    private List<GameObject> targetStars = new List<GameObject>();
    private List<GameObject> candidateStars = new List<GameObject>();


    private System.Random random;
    private int money;

    private Pathfinder.MaxObjBinaryHeap garrisonHeap = new Pathfinder.MaxObjBinaryHeap(1024);
    private Pathfinder.MinObjBinaryHeap econCostHeap = new Pathfinder.MinObjBinaryHeap(1024); //Change later
    private Pathfinder.MinObjBinaryHeap industryCostHeap = new Pathfinder.MinObjBinaryHeap(1024); //Change later
    private Pathfinder.MinObjBinaryHeap scienceCostHeap = new Pathfinder.MinObjBinaryHeap(1024); //Change later

    public void init(GameInformation.PlayerClass bot, List<GameObject> startingStars, System.Random random, MapGeneration mapGenerationScript, Pathfinder pathfinder)
    {
        //Change me later, controls bot's vision
        //knownGraph = mapGenerationScript.graphFullSpeed;

        
        this.random = random;
        this.bot = bot;

        botResearchScript = new Research(bot, 1, 1, 1, 1);

        money = GameInformation.playerMoney;
        this.mapGenerationScript = mapGenerationScript;
        knownGraph = mapGenerationScript.GetGraphFullSpeed();
        this.pathfinderScript = pathfinder;
        CycleEventManager.OnPreTick += preTick;
        CycleEventManager.OnTick += newTick;
        CycleEventManager.OnCycle += newCycle;

        foreach (GameObject star in startingStars)
        {
            if(star == null)
            {
                Debug.LogError("Baaaaaad");
                continue;
            }
            addStar(star);
        }
        //checkStars();

        WakeUp();
    }

    public void WakeUp()
    {
        checkStars();
    }

    public void preTick(object sender, PreTickEvent e)
    {
        stupidCounter++;
        
        if (targetStars.Count < Mathf.CeilToInt(Mathf.Sqrt(ownedStars.Count)) && stupidCounter % 1 == 0)
        {
            checkToExpand();
        }
        
        //if (money > GameInformation.carrierCost)
        //{
        //    checkToExpand();
        //}
    }
    public void newTick(object sender, NewTickEvent e)
    {
        money += 50;
        money += econCostHeap.Size();
    }
    public void newCycle(object sender, NewCycleEvent e)
    {
        //checkStars();
        money += econCostHeap.Size() * 12;
        buyInfrastructure();
        
    }
    public void checkStars()
    {
        Debug.LogError("Checked stars: " + ownedStars.Count);
        econCostHeap = new Pathfinder.MinObjBinaryHeap(1024); //Change me later
        industryCostHeap = new Pathfinder.MinObjBinaryHeap(1024); //Change me later
        scienceCostHeap = new Pathfinder.MinObjBinaryHeap(1024); //Change me later
        garrisonHeap = new Pathfinder.MaxObjBinaryHeap(1024);

        foreach (GameObject star in ownedStars)
        {
            StarScript s = star.GetComponent<StarScript>();
            econCostHeap.Insert(star, s.GetEconPrice());
            industryCostHeap.Insert(star, s.GetIndustryPrice());
            scienceCostHeap.Insert(star, s.GetSciencePrice());

            garrisonHeap.Insert(star, s.GarrisonCount);
            /////////WORK ON ME PLEASE
        }
    }
    public void buyInfrastructure()
    {
        Debug.LogWarning(econCostHeap.Size());
        int allocatedFunds = Mathf.RoundToInt(money / 3);
        Debug.LogWarning(allocatedFunds);
        buyEcon(allocatedFunds);
        buyIndustry(allocatedFunds);
        buyScience(allocatedFunds);
        
    }
    public void buyEcon(int funds)
    {
        Debug.Log(econCostHeap.Size());
        GameObject node;
        while (funds > econCostHeap.Root().value)
        {
            funds -= econCostHeap.Root().value;
            money -= econCostHeap.Root().value;
            node = econCostHeap.Root().node;
            StarScript poppedStarScript = econCostHeap.ExtractRoot().node.GetComponent<StarScript>();
            poppedStarScript.EconCount++;
            econCostHeap.Insert(node, poppedStarScript.GetEconPrice());
        }
    }
    public void buyIndustry(int funds)
    {
        GameObject node;
        while (funds > industryCostHeap.Root().value)
        {
            funds -= industryCostHeap.Root().value;
            money -= industryCostHeap.Root().value;
            node = industryCostHeap.Root().node;
            StarScript poppedStarScript = industryCostHeap.ExtractRoot().node.GetComponent<StarScript>();
            poppedStarScript.IndustryCount++;
            industryCostHeap.Insert(node, poppedStarScript.GetIndustryPrice());
        }
    }
    public void buyScience(int funds)
    {
        GameObject node;
        while (funds > scienceCostHeap.Root().value)
        {
            funds -= scienceCostHeap.Root().value;
            money -= scienceCostHeap.Root().value;
            node = scienceCostHeap.Root().node;
            StarScript poppedStarScript = scienceCostHeap.ExtractRoot().node.GetComponent<StarScript>();
            poppedStarScript.ScienceCount++;
            scienceCostHeap.Insert(node, poppedStarScript.GetSciencePrice());
        }
    }


    public void checkToExpand()
    {
        

        int target = Mathf.CeilToInt(Mathf.Sqrt(ownedStars.Count));
        if (candidateStars.Count < target)
        {
            foreach(GameObject star in ownedStars)
            {
                candidateStars.AddRange(knownGraph.getStarNeighbors(star).Except(ownedStars).Except(candidateStars));
                
            }
        }
        List<GameObject> tempList = new List<GameObject>();
        if (candidateStars.Count < target)
        {
            tempList.AddRange(knownGraph.starList.Except(ownedStars));
        }
        else
        {
            tempList = candidateStars.GetRange(0, target);
        }
        

        foreach(GameObject star in tempList)
        {
            if (idleCarrierHeap.Size() > 0)
            {
                Debug.LogWarning("Sending carrier");
                if (idleCarrierHeap.Root().node == null)
                {
                    Debug.LogError("Carrier going to star: " + star.GetComponent<StarScript>().Name + " from size " + idleCarrierHeap.Root().value + " at tick: " + CycleEventManager.CurrentTick + " did an oopsie");
                    foreach (string obj in removedCarrierList)
                    {
                        Debug.LogError(obj);
                    }

                }
                //Debug.LogError(idleCarrierHeap.Size());
                ShipController tempCarrierScript = idleCarrierHeap.ExtractRoot().node.GetComponent<ShipController>();
                //tempCarrierScript.SetNewWaypoints(pathfinderScript.calculate(knownGraph, knownGraph.findStarIndex(tempCarrierScript.dockedStar), knownGraph.findStarIndex(star)));
                if (star == tempCarrierScript.dockedStar)
                {
                    Debug.LogError("Booo");
                    if (candidateStars.Contains(tempCarrierScript.dockedStar))
                    {
                        Debug.LogError(tempCarrierScript.dockedStar.GetComponent<StarScript>().Name);
                        Debug.LogError("ihtiuhwqeiuf    34i");
                    }
                }

                tempCarrierScript.SetNewWaypoints(pathfinderScript.calculate(knownGraph, knownGraph.findStarIndex(star), knownGraph.findStarIndex(tempCarrierScript.dockedStar)));
                tempCarrierScript.StartJourney();

                candidateStars.RemoveAll(x => x == star);
            }
            //Fix this part later
            else if (money >= GameInformation.carrierCost && garrisonHeap.Root().value > 0 && carrierList.Count <= 2 * Mathf.CeilToInt(Mathf.Sqrt(ownedStars.Count) + 3))
            {
                money -= GameInformation.carrierCost;
                GameObject poppedStar = garrisonHeap.Root().node;
                StarScript poppedStarScript = poppedStar.GetComponent<StarScript>();

                GameObject c = GameObject.Instantiate(GameInformation.shipPrefab, poppedStar.transform.position, Quaternion.identity);
                c.transform.parent = poppedStar.transform;
                ShipController shipController = c.GetComponent<ShipController>();

                poppedStarScript.AttachCarrier(c);
                shipController.dockedStar = poppedStar;
                shipController.Init(carrierNameGenerator(), poppedStar, poppedStarScript.GarrisonCount, bot);
                

                Debug.Log(knownGraph.starList.Count);
                //shipController.SetNewWaypoints(pathfinderScript.calculate(knownGraph, knownGraph.findStarIndex(poppedStar), knownGraph.findStarIndex(star)));
                if (star == poppedStar)
                {
                    Debug.LogError("Booo");
                }

                shipController.SetNewWaypoints(pathfinderScript.calculate(knownGraph, knownGraph.findStarIndex(star), knownGraph.findStarIndex(poppedStar)));
                shipController.StartJourney();

                
                Debug.Log("Created and sent a new carrier from " + poppedStarScript.Name);
                //poppedStarScript.Refresh();

                candidateStars.RemoveAll(x => x == star);
            }
            else
            {
                break;
            }
        }
        







        ////Old////


        //debug.logerror("amount of stars in emprie: " + ownedstars.count);
        //list<gameobject> templist = new list<gameobject>();
        //foreach (gameobject star in ownedstars)
        //{
        //    starscript starscript = star.getcomponent<starscript>();
        //    debug.logerror("checked star");
        //    starscript.refresh();
        //    if (starscript.carriershiptally + starscript.garrisoncount >= 100)
        //    {
        //        if (starscript.garrisoncount >= 100 && money >= gameinformation.carriercost)
        //        {
        //            debug.logerror("passed check");

        //            templist = mapgenerationscript.graphfullspeed.getstarneighbors(star).except(ownedstars).tolist();
        //            templist = mapgenerationscript.graphfullspeed.getstarneighbors(star);

        //            if (templist.count == 0)
        //            {
        //                templist = mapgenerationscript.graphfullspeed.getstarneighbors(star);
        //            }

        //            gameobject chosenstar = templist[random.next(0, templist.count - 1)];



        //            money -= gameinformation.carriercost;
        //            gameobject c = instantiate(gameinformation.shipprefab, star.transform.position, quaternion.identity);
        //            c.transform.parent = star.transform;
        //            shipcontroller shipcontroller = c.getcomponent<shipcontroller>();

        //            star.getcomponent<starscript>().attachcarrier(c);
        //            shipcontroller.dockedstar = star;

        //            shipcontroller.init(carriernamegenerator(), star, 100, bot);
        //            list<gameobject> temp = shipcontroller.getwaypoints();
        //            temp.add(chosenstar);
        //            shipcontroller.setnewwaypoints(temp);
        //            shipcontroller.startjourney();
        //        }
        //        else if (star.getcomponent<starscript>().carriercount > 0)
        //        {
        //            list<gameobject> cl = star.getcomponent<starscript>().carrierlist;
        //            gameobject movedcarrier = null;
        //            for (int i = 0; i < star.getcomponent<starscript>().carriercount; i++)
        //            {
        //                if (cl[i].getcomponent<shipcontroller>().getwaypoints().count == 0)
        //                {
        //                    movedcarrier = cl[i];
        //                    break;
        //                }
        //            }
        //            if (movedcarrier != null)
        //            {
        //                debug.logerror("moving preexisting carrier");
        //                templist = mapgenerationscript.graphfullspeed.getstarneighbors(star).except(ownedstars).tolist();
        //                templist = mapgenerationscript.graphfullspeed.getstarneighbors(star);

        //                if (templist.count == 0)
        //                {
        //                    templist = mapgenerationscript.graphfullspeed.getstarneighbors(star);
        //                }

        //                gameobject chosenstar = templist[random.next(0, templist.count - 1)];

        //                shipcontroller sc = movedcarrier.getcomponent<shipcontroller>();

        //                list<gameobject> temp = sc.getwaypoints();
        //                temp.add(chosenstar);
        //                sc.setnewwaypoints(temp);
        //                sc.startjourney();


        //            }
        //        }
        //        else
        //        {
        //            debug.logerror("bad stuff here");
        //        }
        //    }
        //}

        ///*        mapgenerationscript.graphfullspeed*/
    }

    public string carrierNameGenerator()
    {
        carrierNameIncrement++;
        return bot.name + " " + carrierNameIncrement.ToString();
    }

    public void addCarrier(GameObject carrier)
    {
        if(!carrierList.Contains(carrier))
        {
            ShipController carrierScript = carrier.GetComponent<ShipController>();
            carrierList.Add(carrier);
            carrierSizeHeap.Insert(carrier, carrierScript.ShipCount);

            if(carrierScript.idle)
            {
                idleCarrierHeap.Insert(carrier, carrierScript.ShipCount);
            }
        }
        else
        {
            Debug.LogError("Bad");
        }
        
    }
    public void updateCarrier(GameObject carrier)
    {

        ShipController carrierScript = carrier.GetComponent<ShipController>();

        //carrierSizeHeap.RemoveNode(carrier);
        //carrierSizeHeap.Insert(carrier, carrierScript.ShipCount);

        int sizeIndex = carrierSizeHeap.findKey(carrier);
        //if (sizeIndex < 0)
        //{
        //    Debug.LogWarning("updateCarrier: carrier not in carrierSizeHeap");
        //    return;
        //}
        if(carrierSizeHeap.findKey(carrier) == -1)
        {
            Debug.LogError("I'M THE PROBLEM!");
        }
        carrierSizeHeap.ChangeValueOfObject(carrier, carrierScript.ShipCount);



        //In it already
        if(carrier == null)
        {
            Debug.LogError("Null");
        }
        //Debug.LogError(idleCarrierHeap.Size());
        int index = idleCarrierHeap.findKey(carrier);
        if (index >= 0)
        {
            if (carrierScript.idle)
            {
                idleCarrierHeap.ChangeValueAtIndex(index, carrierScript.ShipCount);
            }
            else
            {
                //Debug.LogError(index);
                idleCarrierHeap.deleteKey(index);
            }
        }
        //Not in it
        else if (carrierScript.idle)
        {
            idleCarrierHeap.Insert(carrier, carrierScript.ShipCount);
        }





    }
    public void removeCarrier(GameObject carrier)
    {
        //Debug.LogError("Removed carrier stationed at star: "+ carrier.GetComponent<ShipController>().dockedStar);


        //removedCarrierList.Add(carrier.GetComponent<ShipController>().idle.ToString());
        //Debug.LogError(carrier.GetComponent<ShipController>().owner.name);
        //Debug.LogError(carrier.GetComponent<ShipController>().dockedStar);

        carrierList.Remove(carrier);
        //Debug.LogError(carrierSizeHeap.Size());
        int q = carrierSizeHeap.findKey(carrier);
        if(q != -1)
        {
            carrierSizeHeap.deleteKey(q);
        }
        else
        {
            Debug.LogError("Unnecessary deletion attempt");
        }
        

        int key = idleCarrierHeap.findKey(carrier);
        if (key >= 0)
        {
            Debug.LogError(idleCarrierHeap.Size());
            idleCarrierHeap.deleteKey(key);
        }
        

        //removedCarrierList.Add(idleCarrierHeap.elements.Count.ToString());
    }
    public void addStar(GameObject star)
    {
        
        if(star == null)
        {
            Debug.LogError("Star is null");
        }
        candidateStars.RemoveAll(x => x == star);
        if (!ownedStars.Contains(star))
        {
            StarScript s = star.GetComponent<StarScript>();
            ownedStars.Add(star);

            garrisonHeap.Insert(star, s.GarrisonCount);
            econCostHeap.Insert(star, s.GetEconPrice());
            industryCostHeap.Insert(star, s.GetIndustryPrice());
            scienceCostHeap.Insert(star, s.GetSciencePrice());

        }
        else
        {
            //Debug.LogError("Updating star");
            updateStar(star);
        }
        //Debug.LogError("Stars in stuff: " + ownedStars.Count);
        if(knownGraph == null)
        {
            Debug.LogError("knownGraph");
        }
        if (candidateStars == null)
        {
            Debug.LogError("knownGraph");
        }

        candidateStars.AddRange(knownGraph.getStarNeighbors(star).Except(ownedStars).Except(candidateStars));
        
    }
    public void updateGarrisonHeap(GameObject star, int newCount)
    {
        if (garrisonHeap.findKey(star) == -1)
        {
            Debug.LogError("I'M THE PROBLEM!");
            Debug.LogError(star.GetComponent<StarScript>().Name);
            Debug.LogError(ownedStars.Find(x => x == star).GetComponent<StarScript>().Name);
            Debug.LogError("garrisonHeap size: " + garrisonHeap.Size() + " vs. ownedStarsCount: " + ownedStars.Count);
            Debug.LogError(garrisonHeap.array.Distinct().Count() - 1);
            //what the fuck is going on here
            if(ownedStars.Find(x => x == star) == null)
            {
                Debug.LogError("Star ain't in here twin");
            }
            Debug.LogError(ownedStars.Find(x => x == star));
        }
        garrisonHeap.ChangeValueOfObject(star, newCount);
    }
    public void updateEconHeap(GameObject star, int newCost)
    {
        econCostHeap.ChangeValueOfObject(star, newCost);
    }
    public void updateIndustryHeap(GameObject star, int newCost)
    {
        industryCostHeap.ChangeValueOfObject(star, newCost);
    }
    public void updateScienceHeap(GameObject star, int newCost)
    {
        scienceCostHeap.ChangeValueOfObject(star, newCost);
    }

    public void updateStar(GameObject star)
    {
        StarScript s = star.GetComponent<StarScript>();

        //garrisonHeap.RemoveNode(star);
        //garrisonHeap.Insert(star, s.GarrisonCount);
        //Debug.LogError(star.GetComponent<StarScript>().Name);
        if (garrisonHeap.findKey(star) == -1)
        {
            Debug.LogError("I'M THE PROBLEM!");
        }
        garrisonHeap.ChangeValueOfObject(star, s.GarrisonCount);

        //econCostHeap.deleteKey(econCostHeap.findKey(star));
        //industryCostHeap.deleteKey(industryCostHeap.findKey(star));
        //scienceCostHeap.deleteKey(scienceCostHeap.findKey(star));


        //econCostHeap.Insert(star, s.GetEconPrice());
        //industryCostHeap.Insert(star, s.GetIndustryPrice());
        //scienceCostHeap.Insert(star, s.GetSciencePrice());

        econCostHeap.ChangeValueOfObject(star, s.GetEconPrice());
        industryCostHeap.ChangeValueOfObject(star, s.GetIndustryPrice());
        scienceCostHeap.ChangeValueOfObject(star, s.GetSciencePrice());

        candidateStars.Remove(star);
        //candidateStars.RemoveAll(x => x == star);

    }
    public void killBot()
    {
        CycleEventManager.OnPreTick -= preTick;
        CycleEventManager.OnTick -= newTick;
        CycleEventManager.OnCycle -= newCycle;
        Debug.Log(bot.name + " has been destroyed!");
    }
    public void removeStar(GameObject star)
    {
        ownedStars.Remove(star);
        candidateStars.Add(star);

        if (ownedStars.Count == 0)
        {
            killBot();
        }

        garrisonHeap.deleteKey(garrisonHeap.findKey(star));
        econCostHeap.deleteKey(econCostHeap.findKey(star));
        industryCostHeap.deleteKey(industryCostHeap.findKey(star));
        scienceCostHeap.deleteKey(scienceCostHeap.findKey(star));

        
    }

}
