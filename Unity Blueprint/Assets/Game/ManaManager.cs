using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RepeatOptions { Use, Reset, Toggle }

public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance { get { return mInstance; } set { } }
    private static ManaManager mInstance;

    public int maxMana = 100;
    public int currentMana = 100;
    public int regenRate = 1;
    public float regenTime = 1.0f; //second    
    StateManager stateManager;

    public bool debugMode = false;

    class CostRepeater
    {
        public int cost;
        public System.Action revert;
        public Coroutine coroutine;
        public CostRepeater(int amount, System.Action func, Coroutine coroutineRef)
        {
            cost = amount;
            revert = func;
            coroutine = coroutineRef;
        }
    }

    Dictionary<string, CostRepeater> costRepeaters;

    //Debug Testing
    Rect display = new Rect(0, 0, 200, 50);
    public int baseCost = 10;
    public Vector3 translation = new Vector3(0.0f, 0.0f, 5.0f);

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;

        stateManager = GetComponent<StateManager>();
        costRepeaters = new Dictionary<string, CostRepeater>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Regen");       
    }

    private void OnGUI()
    {
        if (Time.timeScale > 0.0f)
            GUI.TextField(display, currentMana.ToString());
    }

    IEnumerator Regen()
    {
        while (true)
        {
            if (currentMana < maxMana)
                currentMana += regenRate;

            if (currentMana > maxMana)
                currentMana = maxMana;

            yield return new WaitForSeconds(regenTime);
        }
    }
   
    IEnumerator CostPerSecond(string name)
    {
        CostRepeater repeater;
        while (!costRepeaters.TryGetValue(name, out repeater))
        {
            //print("coroutine waiting on repeater reference");
            yield return null;
        }

        //print("Coroutine got repeater reference");

        while (currentMana >= repeater.cost)
        {
            if (!debugMode)
            {
                currentMana -= repeater.cost;

                if (currentMana < 0)
                {
                    currentMana = 0;
                    break;
                }
            }

            yield return new WaitForSeconds(1.0f);
        }

        repeater.revert();
        costRepeaters.Remove(name);
    }
       
    public bool ApplyCost(int cost)
    {
        if (currentMana >= cost)
        {
            currentMana -= cost;
            return true;
        }
        return false;
    }

    public bool CheckCost(int cost)
    {
        if (currentMana >= cost)
            return true;
        else
            return false;
    }

    public void SetCostPerSecond(string name, int cost, System.Action revert)
    {
        CostRepeater repeater;
        if (costRepeaters.TryGetValue(name, out repeater))
        {
            repeater.cost = cost;
        }
        else
        {
            Coroutine enumerator = StartCoroutine("CostPerSecond", name);
            costRepeaters[name] = new CostRepeater(cost, revert, enumerator);
        }
    }

   
    //Return true to indicate you need to start it
    public bool CheckToggle(string name)
    {
        CostRepeater cost;

        if (costRepeaters.TryGetValue(name, out cost))
        {
            StopCostPerSecond(name);
            return false;
        }

        else
            return true;
    }

    public void StopCostPerSecond(string name)
    {
        CostRepeater repeater;
        if (costRepeaters.TryGetValue(name, out repeater))
        {
            StopCoroutine(repeater.coroutine);
            repeater.revert();
            costRepeaters.Remove(name);
        }
    }

    // Update is called once per frame
    void Update()
    {     
    }
}
