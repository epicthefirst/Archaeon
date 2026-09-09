using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class StarScript : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick (PointerEventData eventData)
    {
        uIManager.InitUI(gameObject);
        string text = "Start, " + planetTimings.Count + ":";
        foreach (Tuple<int, int> tu in planetTimings)
        {
            text = text + tu.Item1 + ", " + tu.Item2 + ";";
        }
        Debug.Log(text);
    }





    //ORBITS + PLANETS QUALITY
    //Numbers to mess with
    public static float distanceIncrease = 1.5f;
    public int qualityMultiplier;




    public int selfId;
    public string Name;
    public List<int> planetList;
    public List<Tuple<int, int>> planetTimings;
    public GameObject canvas;
    private TextMeshPro ships;
    private GameObject go;
    public GameInformation.PlayerClass owner;
    private int shipCountDisplay = 0;
    public int GarrisonCount = 0;
    public int CarrierShipTally;
    public int EconCount = 0;
    public int EconPrice;
    public int IndustryCount = 0;
    public int IndustryPrice;
    public int ScienceCount = 0;
    public int SciencePrice;
    public int GasCount = 0;
    public int PlanetaryCount = 0;
    public int HabitableCount = 0;
    public int Range = 2;
    public int CarrierCount;
    public bool isAwake = false;
    public StarData star;
    private int tick;
    private int cycle;

    public LineRenderer orbitMaker;
    public GameObject insidePolygon;
    public GameObject borderPolygon;
    private GameObject starSpawn;
    //private Dictionary<int, Material[]> materialDictionary;
    public Material MainMaterial;
    public Material SecondaryMaterial;
    public List<GameObject> CarrierList = new List<GameObject>();
    public List<GameObject> maneuverCarrierList = new List<GameObject>();

    private TextMeshPro shipText;
    private TextMeshPro scienceText;
    private TextMeshPro industryText;
    private TextMeshPro econText;

    private UIManager uIManager;

    private List<GameObject> planetObjectList = new List<GameObject>();
    private List<GameObject> orbitObjectList;
    public GameObject[] planetArray;

    public GameObject planet;
    Dictionary<int, int> slingshotWindowDurations;




    public Vector3[] array = new Vector3[10];

    //Battle
    public bool isGoingToFight = false;

    List<FightingEntity> participatingFactions = new();
    Dictionary<GameInformation.PlayerClass, List<ShipController>> invadingShips = new();


    public void Initialize(int Id, string Name, List<int> planetList, List<Tuple<int,int>> PlanetTimings, int Range, GameInformation.PlayerClass owner, GameObject canvas, int GarrisonCount, GameObject[] planetArray, int qualityMultiplier, Dictionary<int, int> slingshotWindowDurations)
    {

        this.owner = owner;
        this.selfId = Id;
        this.planetTimings = PlanetTimings;
        this.Name = Name;
        this.planetList = planetList;
        this.Range = Range;
        this.canvas = canvas;
        this.GarrisonCount = GarrisonCount;
        this.planetArray = planetArray;
        this.qualityMultiplier = qualityMultiplier;
        this.slingshotWindowDurations = slingshotWindowDurations;


        if (owner == null)
        {
            MainMaterial = OwnerColourScript.Instance.GetMainMaterial(0);
            SecondaryMaterial = OwnerColourScript.Instance.GetSecondaryMaterial(0);
        }
        else
        {
            MainMaterial = owner.primaryMaterial;
            SecondaryMaterial = owner.secondaryMaterial;
        }
        //Refresh();
    }



    private void WakeUp()
    {
        if (!isAwake){
            CycleEventManager.OnTick -= thisNewTick;
            CycleEventManager.OnTick += thisNewTick;

            //CycleEventManager.OnPreTick -= preTick;
            //CycleEventManager.OnPreTick += preTick;
        }
        isAwake = true;
    }



    private void Start()
    {


        uIManager = canvas.GetComponent<UIManager>();

        CycleEventManager.OnTick -= thisNewTick;
        CycleEventManager.OnTick += thisNewTick;

        /*Debug.Log(gameObject.transform.position);*/

        if (planetList.Count == planetTimings.Count)
        {
            drawOrbit(qualityMultiplier);
            drawPlanets(qualityMultiplier);
        }
        else
        {
            Debug.LogError("YO THERE'S AN ERROR HERE");
        }
        /*gameObject.GetComponent<Orbits>().init(planetList, planetTimings, tick);*/
        //Create the 3 infrastructure indicators

        GameObject econInfrastructure = new GameObject("econInfrastructure");
        econInfrastructure.transform.SetParent(gameObject.transform);
        econInfrastructure.transform.position = gameObject.transform.position;
        econText = econInfrastructure.AddComponent<TextMeshPro>();
        econText.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        econText.transform.position += new Vector3(2, 3);
        econText.verticalAlignment = VerticalAlignmentOptions.Middle;
        econText.enableWordWrapping = false;
        econText.fontSize = 12;
        econText.color = Color.green;

        GameObject industryInfrastructure = new GameObject("industryInfrastructure");
        industryInfrastructure.transform.SetParent(gameObject.transform);
        industryInfrastructure.transform.position = gameObject.transform.position;
        industryText = industryInfrastructure.AddComponent<TextMeshPro>();
        industryText.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        industryText.transform.position += new Vector3(5, 3);
        industryText.verticalAlignment = VerticalAlignmentOptions.Middle;
        industryText.enableWordWrapping = false;
        industryText.fontSize = 12;
        industryText.color = Color.red;

        GameObject scienceInfrastructure = new GameObject("scienceInfrastructure");
        scienceInfrastructure.transform.SetParent(gameObject.transform);
        scienceInfrastructure.transform.position = gameObject.transform.position;
        scienceText = scienceInfrastructure.AddComponent<TextMeshPro>();
        scienceText.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        scienceText.transform.position += new Vector3(8, 3);
        scienceText.verticalAlignment = VerticalAlignmentOptions.Middle;
        scienceText.enableWordWrapping = false;
        scienceText.fontSize = 12;
        scienceText.color = Color.blue;


        // Create the Text GameObject.
        GameObject go = new GameObject("shipCountDisplay");
        go.transform.parent = gameObject.transform;
        go.transform.position = gameObject.transform.position;
        shipText = go.AddComponent<TextMeshPro>();
        shipText.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 3);
        shipText.transform.position += new Vector3(26, 1, 0);
        shipText.verticalAlignment = VerticalAlignmentOptions.Middle;
        shipText.enableWordWrapping = false;
        shipText.text = shipCountDisplay.ToString();
        shipText.fontSize = 24;
        shipText.color = Color.white;
        

        // Make the name gameobject
        GameObject go2 = new GameObject("starNameDisplay");
        go2.transform.parent = gameObject.transform;
        go2.transform.position = gameObject.transform.position;
        TextMeshPro starNameDisplay = go2.AddComponent<TextMeshPro>();
        starNameDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(6, 0.5f);
        starNameDisplay.transform.position += new Vector3(5, -1, 0);
        starNameDisplay.text = Name;
        starNameDisplay.enableWordWrapping = false;
        starNameDisplay.fontSize = 12;

        //Demo
        if (GameInformation.demoMode)
        {
            Destroy(shipText);
            Destroy(econText);
            Destroy(industryText);
            Destroy(scienceText);
            Destroy(starNameDisplay);
        }

        Refresh();
    }
    //private void preTick(object sender, PreTickEvent e)
    //{
    //    if (IndustryCount != 0)
    //    {
    //        GarrisonCount += IndustryCount;

    //        //Debug.LogError(gameObject);

    //        owner.SimpleUpdateStarOfOwner(gameObject, GarrisonCount);
    //        //owner.UpdateStarOfOwner(gameObject);
    //        Refresh();
    //    }
    //}
    private void thisNewTick(object sender, NewTickEvent e)
    {
        //Beware of race conditions
        if (IndustryCount != 0)
        {
            GarrisonCount += IndustryCount;

            //Debug.LogError(gameObject);
            //if (tick >= 130)
            //{
            //    Debug.LogError(owner.name);
            //}

            owner.SimpleUpdateStarOfOwner(gameObject, GarrisonCount);
            //owner.UpdateStarOfOwner(gameObject);
            Refresh();
        }
        tick = e.CurrentTick;
        updatePlanets();
    }

    public class FightingEntity
    {
        public int shipCount = 0;

        public int baseCombatStrength = 0;
        public int combatBonusSelf = 0;
        public int combatBonusToOtherAllies = 0;
        public int combatBonusToOtherEnemies = 0;
        public int combatDebuffSelf = 0;
        public int combatDebuffOthers = 0;
        public GameInformation.PlayerClass entity;
        public List<FightingEntity> allies = new();

        public void FightingEntity(GameInformation.PlayerClass Entity)
        {
            this.entity = Entity;
            baseCombatStrength = entity.GetCombatLevel();
        }

        public void SetShipCount(int count)
        {
            shipCount = count;
        }

        public void SetCombatBonusSelf(int bonus)
        {
            if(bonus > combatBonusSelf)
            {
                combatBonusSelf = bonus;
            }
        }

        public void SetCombatDebuffSelf(int debuff)
        {
            if(debuff > SetCombatDebuffSelf)
            {
                SetCombatDebuffSelf = debuff;
            }
        }

        //Need all fighting entities established before doing these
        public void SetCombatBonusToOtherAllies(int debuff)
        {
            if(debuff > SetCombatDebuffSelf)
            {
                SetCombatDebuffSelf = debuff;
            }
        }
    }

    public void Fight(object sender, FightTickEvent e)
    {
        if(owner == null && invadingShips.Keys.Count == 1) //When most of this function is useless
        {
            foreach (GameInformation.PlayerClass key in invadingShips.Keys)
            {

                owner = key;
                if(key == null)
                {
                    Debug.LogError("BADDDDD");
                }
                foreach (ShipController carrier in invadingShips[key])
                {
                    AttachCarrier(carrier.gameObject);
                }


            }
            owner.AddStarToOwner(gameObject);
            EndFight();
            Refresh();
            return;
        }



        




        //Sorta have implementation for multiple factions
        //Want to make fighting even, if 2 players attack an occupied planet, all 3 should fight eachother

        Dictionary<FightingEntity, int> combatRatios = new();

        

        int tally = 0;
        int tempTally = 0;
        int bestTally = 0;

        FightingEntity best = null;
        participatingFactions = new();
        
        foreach (GameInformation.PlayerClass key in invadingShips.Keys)
        {
            participatingFactions.add(new FightingEntity(key));

            foreach(ShipController carrier in invadingShips[key])
            {
                tempTally += carrier.ShipCount;
            }

            tally += tempTally;

            //Determine largest army
            if(tempTally > bestTally)
            {
                best = key;
            }
        }
        if (tally > GarrisonCount + CarrierShipTally) //Defenders lost
        {
            GarrisonCount = 0;

            for (int i = CarrierList.Count - 1; i >= 0; i--) //Go backwards cuz that's tuff (Changing list while iterating through it)
            {

                CarrierList[i].GetComponent<ShipController>().DestroyCarrier();
                CarrierList.RemoveAt(i);

            }

            if (owner != null)
            {
                owner.RemoveStarFromOwner(gameObject);
            }
            owner = best;

            ShipDeathAlgorithm(tally);

//

            GarrisonCount = 0;
            EconCount = 0;
            IndustryCount = 0;
            ScienceCount = 0;
            
            //canvas.GetComponent<UIManager>().playerStars.Add(gameObject);

            owner.AddStarToOwner(gameObject);


        }
        else //Defenders won
        {
            if (GarrisonCount >= tally)
            {
                GarrisonCount -= tally;

                foreach (GameInformation.PlayerClass key in invadingShips.Keys)
                {
                    foreach (ShipController carrier in invadingShips[key])
                    {
                        carrier.DestroyCarrier();
                    }
                }
            }
            else //If GarrisonCount < Tally < (GarrisonCount + CarrierShipTally)
            {
                
                int friendlyShipsToKill = tally - GarrisonCount;
                GarrisonCount = 0;

                foreach (GameInformation.PlayerClass key in invadingShips.Keys)
                {
                    foreach (ShipController carrier in invadingShips[key])
                    {
                        carrier.DestroyCarrier(); //Destroy all invading ships
                    }
                }

                float ratio = friendlyShipsToKill / CarrierShipTally;
                int trueKillTally = 0;
                for(int i = CarrierList.Count - 1; i >= 0; i--) //Go backwards cuz that's tuff (Changing list while iterating through it)
                {
                    ShipController sc = CarrierList[i].GetComponent<ShipController>();
                    //Overestimates loses, mayyybe switch calculation to something more robust later
                    int loss = Mathf.CeilToInt(sc.ShipCount * ratio);
                    sc.ShipCount -= loss;
                    if (loss == sc.ShipCount)
                    {
                        sc.DestroyCarrier(); //Change if we want carriers to survive a winning battle
                        CarrierList.RemoveAt(i);
                    }
                    trueKillTally += loss;
                }
            }

        }

        EndFight();
        Refresh();
    }
    
    private void EndFight()
    {
        isGoingToFight = false;
        invadingShips = new();
        CycleEventManager.FightTick -= Fight;
        owner.UpdateStarOfOwner(gameObject);
    }

    public int ShipDeathAlgorithm(int offensiveTally)
    {
        float ratio = (GarrisonCount + CarrierShipTally) / offensiveTally;
        int trueKillTally = 0;
        foreach (GameInformation.PlayerClass key in invadingShips.Keys)
        {
            foreach (ShipController carrier in invadingShips[key])
            {
                //Overestimates loses, mayyybe switch calculation to something more robust later
                int loss = Mathf.CeilToInt(carrier.ShipCount * ratio);
                carrier.ShipCount -= loss;
                if (loss == carrier.ShipCount)
                {
                    carrier.DestroyCarrier(); //Change if we want carriers to survive a winning battle
                }
                else
                {
                    AttachCarrier(carrier.gameObject);
                }
                trueKillTally += loss;
            }
        }
        return trueKillTally;
    }

    public void ShipInbound(int shipShipCount, GameInformation.PlayerClass shipOwner, GameObject carrier)
    {
        
        WakeUp();
        if (shipOwner == owner)
        {

            AttachCarrier(carrier);
            //Refresh();
        }
        else
        {
            isGoingToFight = true;
            CycleEventManager.FightTick -= Fight;
            CycleEventManager.FightTick += Fight;
            if (invadingShips.ContainsKey(shipOwner))
            {
                invadingShips[shipOwner].Add(carrier.GetComponent<ShipController>());
            }
            else
            {
                invadingShips.Add(shipOwner, new List<ShipController>() { carrier.GetComponent<ShipController>() });
            }
        }
        //else if (owner == null)
        //{


        //    owner = shipOwner;
        //    //canvas.GetComponent<UIManager>().playerStars.Add(gameObject);
        //    //Come back to this
        //    //owner.playerScript.AddStar(gameObject);
        //    AttachCarrier(carrier);
        //    owner.AddStarToOwner(gameObject);
        //    Refresh();
        //}
        //else
        //{
        //    if (shipShipCount > GarrisonCount + CarrierShipTally)
        //    {
        //        GarrisonCount = 0;
        //        foreach (GameObject c in CarrierList)
        //        {
        //            c.GetComponent<ShipController>().DestroyCarrier();
        //        }
        //        //WORK ON THIS LATER
        //        owner = shipOwner;
        //        carrier.GetComponent<ShipController>().ShipCount -= GarrisonCount;
        //        GarrisonCount = 0;
        //        EconCount = 0;
        //        IndustryCount = 0;
        //        ScienceCount = 0;
        //        AttachCarrier(carrier);
        //        //canvas.GetComponent<UIManager>().playerStars.Add(gameObject);

        //        owner.AddStarToOwner(gameObject);
        //        // Do some stuff here
        //        Refresh();
        //    }
        //    else
        //    {
        //        if(GarrisonCount > shipShipCount)
        //        {
        //            GarrisonCount -= shipShipCount;
        //        }
        //        else
        //        {
        //            GarrisonCount = 0;
        //            //WORK ON ME, NEED ALGORITHM TO DECIDE SHIP DEATH
        //        }
                
        //        //Debug.LogError("Destroying carrier");
        //        carrier.GetComponent<ShipController>().DestroyCarrier();
        //        Refresh();
        //    }
        //}
      
        

    }
    public void AttachCarrier(GameObject carrier)
    {
        CarrierList.Add(carrier);
        CarrierCount += 1;
        PolygonRefresh();
        Refresh();
    }
    public void DetachCarrier(GameObject carrier)
    {
        if (CarrierList.Contains(carrier))
        {
            CarrierList.Remove(carrier);
            CarrierCount -= 1;

            //Debug.LogError("Dettached carrier");
        }

        PolygonRefresh();
        Refresh();
    }
    public void ReduceShipCount(int shipCountReduction)
    {
        GarrisonCount -= shipCountReduction;
        owner.SimpleUpdateStarOfOwner(gameObject, GarrisonCount);
        PolygonRefresh();
        Refresh();
    }

    public bool isGoingToSlingshot(int timeLeft)
    {
        for (int i = 0; i < planetList.Count; i++)
        {
/*            float orbitProgress = (1 / (float)planetTimings[i].Item2) * ((tick + planetTimings[i].Item1) % planetTimings[i].Item2);

            int orbitProgressInt = ((tick + planetTimings[i].Item1) % planetTimings[i].Item2);*/

            if ((timeLeft + tick + planetTimings[i].Item1 + slingshotWindowDurations[i] - 1) % planetTimings[i].Item2 <= (slingshotWindowDurations[i]-1))
            {
                Debug.Log("Slingshot-able, " + i + " : " + (timeLeft + tick + 1 + planetTimings[i].Item1) % planetTimings[i].Item2);
                Debug.Log(planetTimings[i].Item1 + " : " + planetTimings[i].Item2);
                return true;
            }
        }
        Debug.Log("Not slingshot-able");
        return false;
    }
    public void startSlingshot(GameObject carrier)
    {
        maneuverCarrierList.Add(carrier);
        PolygonRefresh();
        Refresh();
    }

    public void Refresh()
    {

        CarrierShipTally = 0;
        foreach (GameObject carrier in CarrierList) 
        {
            if (carrier == null)
            {
                Debug.LogError("carrier is null");
            }
            CarrierShipTally += carrier.GetComponent<ShipController>().ShipCount;
        }
        shipCountDisplay = GarrisonCount + CarrierShipTally;



        if (shipText != null)
        {
            shipText.text = shipCountDisplay.ToString() + (CarrierCount == 0 ? null : "/" + CarrierCount)+ (maneuverCarrierList.Count == 0 ? null : "!" + maneuverCarrierList.Count);
        }
        if (owner != null)
        {
            econText.text = EconCount.ToString();
            industryText.text = IndustryCount.ToString();
            scienceText.text = ScienceCount.ToString();
        }
        if (owner == null)
        {
            MainMaterial = OwnerColourScript.Instance.GetMainMaterial(0);
            SecondaryMaterial = OwnerColourScript.Instance.GetSecondaryMaterial(0);
        }
        else
        {
            MainMaterial = owner.primaryMaterial;
            SecondaryMaterial = owner.secondaryMaterial;
        }

    }
    public void ReCountPlanets()
    {
        PlanetaryCount = 0;
        GasCount = 0;
        HabitableCount = 0;
        for (int i = 0; i < planetList.Count; i++)
        {
            switch (planetList[i])
            {
                case 0:
                    //Planetary
                    PlanetaryCount++;
                    break;
                case 1:
                    //Gas
                    GasCount++;
                    break;
                case 2:
                    //Habitable
                    HabitableCount++;
                    break;

            }
        }
    }
    public void PolygonRefresh()
    {
        if (owner == null)
        {
            insidePolygon.GetComponent<MeshRenderer>().material = OwnerColourScript.Instance.GetSecondaryMaterial(0);
            borderPolygon.GetComponent<MeshRenderer>().material = OwnerColourScript.Instance.GetMainMaterial(0);
        }
        else
        {
            //Debug.LogError("Owner is not null");
            insidePolygon.GetComponent<MeshRenderer>().material = owner.secondaryMaterial;
            borderPolygon.GetComponent<MeshRenderer>().material = owner.primaryMaterial;
        }
    }
    public void SendPolygon(GameObject insidePolygon, GameObject borderPolygon)
    {
        this.insidePolygon = insidePolygon;
        this.borderPolygon = borderPolygon;
        //this.materialDictionary = materialDictionary;
        PolygonRefresh();
    }






    //
    //
    //ORBITS AND STUFF
    //
    //


    void drawOrbit(int steps)
    {
        steps = steps * 15;
        for (int j = 0; j < planetList.Count; j++)
        {
            float radius = distanceIncrease * (j + 1);
            GameObject orbitObject = new GameObject("Orbit");
            orbitObject.transform.SetParent(gameObject.transform);
            LineRenderer orbitMaker = orbitObject.AddComponent<LineRenderer>();
            orbitMaker.material = new Material(Shader.Find("Sprites/Default"));

            //bool isPlanetary = orbitType == 1;

            //orbitMaker.startColor = isPlanetary ? Color.red : Color.gray;
            //orbitMaker.endColor = isPlanetary ? Color.red : Color.gray;
            //orbitMaker.startWidth = isPlanetary ? 0.5f : 0.3f;
            //orbitMaker.endWidth = isPlanetary ? 0.5f : 0.3f;


            switch (planetList[j])
            {
                case 0:
                    //Planetary
                    orbitMaker.startColor = Color.gray;
                    orbitMaker.endColor = Color.gray;
                    orbitMaker.startWidth = 0.3f;
                    orbitMaker.endWidth = 0.3f;
                    break;
                case 1:
                    //Gas
                    orbitMaker.startColor = Color.red;
                    orbitMaker.endColor = Color.red;
                    orbitMaker.startWidth = 0.5f;
                    orbitMaker.endWidth = 0.5f;
                    break;
                case 2:
                    //Habitable
                    orbitMaker.startColor = Color.green;
                    orbitMaker.endColor = Color.green;
                    orbitMaker.startWidth = 0.3f;
                    orbitMaker.endWidth = 0.3f;
                    break;
            }


            
            orbitMaker.positionCount = (steps * 6 / 10) + 1;

            float cutStartAngle = 5 * Mathf.PI / 10;
            for (int i = 0; i < (steps * 6 / 10) + 1; i++)
            {
                float circumferenceProgress = (float)i / steps;

                float currentRadian = (circumferenceProgress * 2 * Mathf.PI) + cutStartAngle;

                float xScaled = Mathf.Cos(currentRadian);
                float yScaled = Mathf.Sin(currentRadian);

                float x = xScaled * radius;
                float y = yScaled * radius;

                Vector2 position = new Vector2(x, y) + gameObject.transform.position.ConvertTo<Vector2>();

                orbitMaker.SetPosition(i, position);
            }


            //// Useless now
            //orbitMaker.SetPosition(steps, orbitMaker.GetPosition(0));
            orbitMaker.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
    private void drawPlanets(int steps)
    {
        foreach (GameObject pl in planetObjectList)
        {
            Destroy(pl);
        }
        for (int i = 0; i < planetList.Count; i++)
        {
            
            float orbitalRadius = distanceIncrease * (i + 1);
            /*            GameObject planetObject = new GameObject("Planet");
                        planetObject.transform.SetParent(gameObject.transform);
                        LineRenderer planetMaker = planetObject.AddComponent<LineRenderer>();
                        planetMaker.material = new Material(Shader.Find("Sprites/Default"));
                        planetObjectList.Add(planetObject);*/

            //List<int> planetList, List< int > planetTimings

            //orbitProgress ranges from 0-1, dont forget to offset and reverse direction!!!!!!!
            float orbitProgress = (1 / (float)planetTimings[i].Item2) * ((tick + planetTimings[i].Item1) % planetTimings[i].Item2);
            /*        float orbitProgress = 0.25f;*/
            float orbitRadians = (0.25f - orbitProgress) * 2 * Mathf.PI;
            float xTimingAdjust = Mathf.Cos(orbitRadians) * orbitalRadius;
            float yTimingAdjust = Mathf.Sin(orbitRadians) * orbitalRadius;



            switch (planetList[i])
            {
                case 0:
                    //Terrestrial
                    planet = Instantiate(planetArray[0], gameObject.transform, false);
                    break;
                case 1:
                    //Gas
                    planet = Instantiate(planetArray[1], gameObject.transform, false);
                    break;
                case 2:
                    //Habitable
                    planet = Instantiate(planetArray[2], gameObject.transform, false);
                    break;
            }

            planet.transform.localPosition = new Vector3(xTimingAdjust, yTimingAdjust);
            planet.SetActive(true);
            planetObjectList.Add(planet);
            //0.25f is to offset it
            planetTransparency(0.25f - orbitProgress, planet.GetComponent<MeshRenderer>());

            /*            planetMaker.positionCount = steps+4;

                        for (int j = 0; j < steps+4; j++)
                        {
                            float circumferenceProgress = (float)j / steps;

                            float currentRadian = (circumferenceProgress * 2 * Mathf.PI);

                            float xScaled = Mathf.Cos(currentRadian);
                            float yScaled = Mathf.Sin(currentRadian);

                            float x = xScaled * radius;
                            float y = yScaled * radius;

                            Vector2 position = new Vector2(x, y) + gameObject.transform.position.ConvertTo<Vector2>() + new Vector2(xTimingAdjust, yTimingAdjust);

                            planetMaker.SetPosition(j, position);

                        }*/
            /*            planetMaker.SetPosition(steps + 1, planetMaker.GetPosition(1));*/
            /*            planetMaker.SetPosition(steps-1, planetMaker.GetPosition(0));
                        planetMaker.SetPosition(steps, planetMaker.GetPosition(1));
                        planetMaker.SetPosition(steps+1, planetMaker.GetPosition(2));
                        planetMaker.SetPosition(steps+2, planetMaker.GetPosition(3));*/
        }
    }

    public void planetTransparency(float orbitProgress, MeshRenderer MR)
    {
        var color = MR.material.color;
        if (orbitProgress < 0.25f & orbitProgress > -0.15f)
        {
            color.a = 0.4f;
            MR.material.color = color;
        }
        else
        {
            color.a = 1f;
            MR.material.color = color;
        }
    }

    public void updatePlanets()
    {
        for (int i = 0; i < planetObjectList.Count; i++)
        {
            float orbitalRadius = distanceIncrease * (i + 1);
            float orbitProgress = (1 / (float)planetTimings[i].Item2) * ((tick + planetTimings[i].Item1) % planetTimings[i].Item2);

            planetTransparency(0.25f - orbitProgress, planetObjectList[i].GetComponent<MeshRenderer>());

            /*        float orbitProgress = 0.25f;*/
            float orbitRadians = (0.25f - orbitProgress) * 2 * Mathf.PI;
            float xTimingAdjust = Mathf.Cos(orbitRadians) * orbitalRadius;
            float yTimingAdjust = Mathf.Sin(orbitRadians) * orbitalRadius;

            planetObjectList[i].transform.localPosition = new Vector3(xTimingAdjust, yTimingAdjust);
        }
    }

    public int GetEconPrice()
    {
        return (50 * (EconCount + 1)) / (PlanetaryCount + 1 + HabitableCount * 2);
    }

    public int GetIndustryPrice()
    {
        return (100 * (IndustryCount + 1)) / (GasCount + 1 + HabitableCount * 2);
    }
    public int GetSciencePrice()
    {
        return (200 * (ScienceCount + 1)) / (1 + HabitableCount * 5);
    }
    



}
