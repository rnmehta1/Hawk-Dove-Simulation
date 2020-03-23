using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class HawkDoveSim : MonoBehaviour
{
   
    public int numDoves = 100;
    int numHawks = 100;
    int numFood = 100;
    int rounds = 100;
    int startingEnergy = 100;
    int reprodThresh = 140;
    int energyForLiving = 20;
    int baseEnergy = 2;
    int costBluff = 10;
    int energyLoss = 100;
    int currRound = 1;
    public bool clicked = false;
    string csv ="";
    string filePath = "";
    public Button StartButton;
    public Button NextStepButton;


    List<GameObject> gameObjects = new List<GameObject>();

    public PopulateFields populate;
    public RandomGenerator rng;

    static Vector3 getRandomPos()
    {
        float xpos = (float)(UnityEngine.Random.Range(-6 * 10, 5 * 10)) / 10;
        float ypos = (float)(UnityEngine.Random.Range(-5 * 10, 5 * 10)) / 10;
        return new Vector3(xpos, ypos, 0);
    }

    class Agent
    {
        public int id = 0;
        public string agent_type = "";
        public string status = "active";
        public int energy = 100;
        public Vector3 pos = getRandomPos();
    }

    class Food
    {
       public int exp = 0;
       public Vector3 pos = getRandomPos();
    }

    List<Food> foods = new List<Food>();
    List<Agent> agents = new List<Agent>();

    // Start is called before the first frame update
    void Start()
    {
        init();
        DisplayObjects();
    }

    private static readonly DateTime Jan1st1970 = new DateTime
    (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long CurrentTimeMillis()
    {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

    public void startSimulation()
    {
        clicked = true;
    }

    //To count number of hawks and doves
    public int getAgentsbyType(string type)
    {
        int count = 0;
        for(int i = 0; i < agents.Count; i++)
        {
            if (agents[i].agent_type == type){
                count += 1;
            }
        }
        return count;
    }

    //Reproduce agents if energy is above 140
    (int, int) breed()
    {
        int hawkBabies = 0, doveBabies = 0;
        List<Agent> temp = new List<Agent>();
        Agent babyA;

        for (int i=0;i<agents.Count;i++)
        {
            if (agents[i].energy >= reprodThresh)
            {
                babyA = new Agent
                {
                    agent_type = agents[i].agent_type,
                    energy = agents[i].energy / 2
                };

                agents[i].energy /= 2;

                if (agents[i].agent_type == "dove")
                {
                    doveBabies += 1;
                } else if(agents[i].agent_type=="hawk")
                {
                    hawkBabies += 1;
                }

                temp.Add(babyA);
            }
        }
        foreach(Agent a in temp)
        {
            agents.Add(a);
        }

        return (hawkBabies, doveBabies);
    }

    
    void deleteFood()
    {
        System.Random r = new System.Random();
        List<Food> temp = new List<Food>();
        int pos = r.Next(0, foods.Count);
        for (int i = 0; i < foods.Count; i++)
        {
            if(i!=pos)
            temp.Add(foods[i]);
        }
        foods.Clear();
        foreach (Food f in temp)
        {
            foods.Add(f);
        }

    }

    void updateFood()
    {
        List<Food> temp = new List<Food>();

        foreach (Food f in foods)
        {
            f.exp--;
            if (f.exp > 0)
            {
                temp.Add(f);
            }
        }
        foods.Clear();
        foreach(Food f in temp)
        {
            foods.Add(f);
        }
    }

    void addFood()
    {
        for (int i = 0; i < 100; i++)
        {
            Food wholeFood = new Food();
            wholeFood.exp = 5;
            foods.Add(wholeFood);
        }
    }

    void compete(Agent agent, Agent nemesis, int food)
    {
        System.Random r = new System.Random();
        int win = r.Next(0,2);

        if (numFood>0) {
            if (agent == null && nemesis != null)
            {
                nemesis.energy += food;
            } else if (agent != null && nemesis == null)
            {
                agent.energy += food;
            }
            else if (agent.agent_type == "hawk" && nemesis.agent_type == "hawk")
            {
                
                if (win == 0)
                {
                    agent.energy += food;
                    nemesis.energy -= energyLoss;
                } else
                {
                    nemesis.energy += food;
                    agent.energy -= energyLoss;
                }
            } else if (agent.agent_type == "hawk" && nemesis.agent_type == "dove")
            {
                agent.energy += food;
            } else if (agent.agent_type == "dove" && nemesis.agent_type == "hawk")
            {
                nemesis.energy += food;
            } else if (agent.agent_type == "dove" && nemesis.agent_type == "dove")
            {
                if (win == 0)
                {
                    agent.energy = agent.energy + food - costBluff;
                    nemesis.energy -= costBluff;
                }
                else
                {
                    nemesis.energy = nemesis.energy + food - costBluff;
                    agent.energy -= costBluff;
                }
            }
            deleteFood();
            numFood = foods.Count;
        }
        if (nemesis!=null)
        nemesis.status = "asleep";
        if(agent!=null)
        agent.status = "asleep";
    }

    //Kill agents if energy is less than 20
    (int, int) cull()
    {
        List<Agent> temp = new List<Agent>();
        int deadHawks = 0;
        int deadDoves = 0;
        for(int i =0; i<agents.Count;i++)
        {
            if (agents[i].energy < energyForLiving)
            {
                if (agents[i].agent_type == "hawk") deadHawks += 1;
                if (agents[i].agent_type == "dove") deadDoves += 1;
                //delete agent
                //agents.RemoveAt(i);
                //i--;
            }
            else
            {
                temp.Add(agents[i]);
            }
        }
        agents.Clear();
        foreach(Agent a in temp)
        {
            agents.Add(a);
        }
        return (deadHawks, deadDoves);
    }

    Tuple<Agent,Agent> getRandomAgent()
    {
        Agent agent = null;
        Agent nemesis=null;
        System.Random r = new System.Random();
        List<Agent> active_agents= new List<Agent>();
        foreach(Agent v in agents)
        {
            if (v.status == "active")
            {
                active_agents.Add(v);
            }
        }

        if (active_agents.Count == 0)
        {
            return Tuple.Create(agent, nemesis);
        }
        if (active_agents.Count == 1)
        {
            return Tuple.Create(active_agents[0], nemesis);
        }
        agent = active_agents[r.Next(0, active_agents.Count)];
        while (nemesis == null)
        {
            Agent n = active_agents[r.Next(0, active_agents.Count)];
            if (n != agent)
            {
                nemesis = n;
            }
        }

        return Tuple.Create(agent, nemesis);
    }

    //Initialise all objects
    public void init()
    {
        for(int i=0; i < numDoves; i++)
        {
            Agent a = new Agent();
            a.agent_type = "dove";
            agents.Add(a);
        }
        for(int i=0;i < numHawks; i++)
        {
            Agent a = new Agent();
            a.agent_type = "hawk";
            agents.Add(a);
        }
        for (int i = 0; i < numFood; i++)
        {
            Food wholeFood = new Food();
            wholeFood.exp = 5;
            foods.Add(wholeFood);
        }


        DateTime now = DateTime.Now;
        Debug.Log("date:" + now.Month+" "+now.Year);
        filePath = "run" + now.Day.ToString() + now.Month + now.Year+now.Hour+now.Minute+now.Second+".csv";
        csv = "Generation,Hawks,Doves,Food,HawkBabies,DoveBabies,DeadHawks,DeadDoves\n";
        File.WriteAllText(filePath, csv);
        csv = "0,100,100,100,0,0,0,0\n";
        File.AppendAllText(filePath, csv);
    }

    public void DisplayObjects()
    {

        for (int i = 0; i < gameObjects.Count; i++)
        {
            Destroy(gameObjects[i]);
        }

        gameObjects.Clear();

        rng = GetComponent<RandomGenerator>();
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i].agent_type == "hawk")
            {
                GameObject go = Instantiate(rng.hawk, agents[i].pos, Quaternion.identity);
                gameObjects.Add(go);
            }
            if (agents[i].agent_type == "dove")
            {
                GameObject go = Instantiate(rng.dove, agents[i].pos, Quaternion.identity);
                gameObjects.Add(go);
            }
        }
        for (int i = 0; i < foods.Count; i++)
        {
            GameObject go = Instantiate(rng.food, foods[i].pos, Quaternion.identity);
            gameObjects.Add(go);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currRound > 100 || !clicked)
        {
            NextStepButton.interactable = true;
            return;
        }

        populate = GetComponent<PopulateFields>();

        numFood = foods.Count;

        int tempfood = numFood;
        updateFood();


        foreach (Agent a in agents)
        {
            a.status = "active";
        }

        int food = 50;
        while (true)
        {

            populate.putText(currRound.ToString(), "InputGenerationRound");
            Agent nemesis, agent;
            (agent, nemesis) = getRandomAgent();
            if (agent == null && nemesis == null)
            {
                break;
            }
            compete(agent, nemesis, food);
        }
        foreach (Agent a in agents)
        {
            a.energy -= baseEnergy;
        }


        int deadHawks, deadDoves;
        (deadHawks, deadDoves) = cull();
        int hawkBabies, doveBabies;
        (hawkBabies, doveBabies) = breed();
        //Debug.Log("Time:" + CurrentTimeMillis() + " ,Round :" + currRound + ", Hawks:" + hawks +
        //"Doves:" + doves + ", new Hawks:" + hawkBabies +
        //", new doves:" + doveBabies + "dead hawks:" + deadHawks +
        //", dead doves:" + deadDoves + ", food:" + tempfood);


        int doves = getAgentsbyType("dove");
        int hawks = getAgentsbyType("hawk");
        populate.putText(hawks.ToString(), "InputHawk");
        populate.putText(doves.ToString(), "InputDove");

        addFood();
        numFood = foods.Count;
        populate.putText(numFood.ToString(), "InputFood");
        csv = currRound.ToString() + "," + hawks.ToString() + "," + doves.ToString() + "," + numFood.ToString() + ","
            + hawkBabies.ToString() + "," + doveBabies.ToString() + "," + deadHawks.ToString() + "," + deadDoves.ToString() + "\n";

        File.AppendAllText(filePath, csv);

        currRound += 1;

        DisplayObjects();

        // Sleep for a second before the next round.
        System.Threading.Thread.Sleep(250);
    }

    public void next_step()
    {
        populate = GetComponent<PopulateFields>();

        numFood = foods.Count;

        int tempfood = numFood;
        updateFood();


        foreach (Agent a in agents)
        {
            a.status = "active";
        }

        int food = 50;
        while (true)
        {

            populate.putText(currRound.ToString(), "InputGenerationRound");
            Agent nemesis, agent;
            (agent, nemesis) = getRandomAgent();
            if (agent == null && nemesis == null)
            {
                break;
            }
            compete(agent, nemesis, food);
        }
        foreach (Agent a in agents)
        {
            a.energy -= baseEnergy;
        }

        int doves = getAgentsbyType("dove");
        int hawks = getAgentsbyType("hawk");

        int deadHawks, deadDoves;
        (deadHawks, deadDoves) = cull();
        int hawkBabies, doveBabies;
        (hawkBabies, doveBabies) = breed();
        Debug.Log("Time:" + CurrentTimeMillis() + " ,Round :" + currRound + ", Hawks:" + hawks +
        "Doves:" + doves + ", new Hawks:" + hawkBabies +
        ", new doves:" + doveBabies + "dead hawks:" + deadHawks +
        ", dead doves:" + deadDoves + ", food:" + tempfood);

        populate.putText(hawks.ToString(), "InputHawk");
        populate.putText(doves.ToString(), "InputDove");

        addFood();
        numFood = foods.Count;
        populate.putText(numFood.ToString(), "InputFood");
        csv = currRound.ToString() + "," + hawks.ToString() + "," + doves.ToString() + "," + numFood.ToString() + ","
            + hawkBabies.ToString() + "," + doveBabies.ToString() + "," + deadHawks.ToString() + "," + deadDoves.ToString() + "\n";

        File.AppendAllText(filePath, csv);

        currRound += 1;

        DisplayObjects();

    }


    public void stop_clicked()
    {
        #if UNITY_EDITOR
        EditorApplication.isPaused = true;
        #endif
    }
}
