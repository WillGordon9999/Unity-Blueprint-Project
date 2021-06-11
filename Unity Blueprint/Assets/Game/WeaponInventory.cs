using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponItem
{
    public GameObject weaponPrefab;
    public GameObject weaponHolder;
    public WeaponInventory.WeaponSlot slot;
}

/*
Current Notes for things to implement:
- Set up Prefabs to have a transform at the handle to properly map onto back
- Set up Right and Left Weapon Holders to use this transform to map weapons onto back
- Get rid of the WeaponType enum in the WeaponData component
- Could possibly send a setactive to holder version of GameObject when weapon gets parented to hand 
may have to remap handle parent back
*/

public class WeaponInventory : MonoBehaviour
{
    public enum WeaponSlot { Right, Left }
    public enum WeaponEquipAnimation { LowFront, LowBack, High }
    public List<WeaponItem> weaponItems;
    public List<Invector.vWeaponHolder> initHolders;
    public Dictionary<string, Invector.vWeaponHolder> holders;
    //public Dictionary<string, List<Invector.vWeaponHolder>> holders;

    public GameObject rightHandWeapon;
    public GameObject rightHandShield;
    public GameObject leftHandWeapon;
    public GameObject leftHandShield;

    public WeaponHolder rightHolder;
    public GameObject rightHolderObject; //Reference to holder object
    public GameObject rightHolderCurrentWeapon; //Actual weapon in slot

    public WeaponHolder leftHolder;
    public GameObject leftHolderObject; //Reference to holder object
    public GameObject leftHolderCurrentWeapon; //Actual weapon in slot
    
    const string leftArm = "LeftArm";
    const string rightArm = "RightArm";
    [SerializeField] int scrollIndex = 0;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        initHolders.AddRange(GetComponentsInChildren<Invector.vWeaponHolder>());
        holders = new Dictionary<string, Invector.vWeaponHolder>();
        //holders = new Dictionary<string, List<Invector.vWeaponHolder>>();
        anim = GetComponent<Animator>();

        foreach(Invector.vWeaponHolder hold in initHolders)
        {
            //WARNING WILL ONLY HAVE ONE SLOT FOR EACH TYPE OF WEAPON
            holders.Add(hold.gameObject.name, hold);
        }

        //The problem with the list is how do you deduce where to put what weapon?
        //foreach(Invector.vWeaponHolder hold in initHolders)
        //{
        //    if (!holders.ContainsKey(hold.equipPointName))
        //    {
        //        holders.Add(hold.equipPointName, new List<Invector.vWeaponHolder>());
        //        holders[hold.equipPointName].Add(hold);
        //    }
        //
        //    else
        //    {
        //        holders[hold.equipPointName].Add(hold);
        //    }
        //}

    }

    public void SetScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0.0f)
            scrollIndex++;

        if (scroll < 0.0f)
            scrollIndex--;

        if (scrollIndex >= weaponItems.Count)
            scrollIndex = 0;

        if (scrollIndex < 0)
            scrollIndex = weaponItems.Count - 1;

    }

    public void EquipWeaponScroll(WeaponSlot slot)
    {
        EquipWeapon(slot, weaponItems[scrollIndex]);
    }
    
    //This only spawns the weapon on the weapon slot, use DrawWeapon to actually wield it
    public void EquipWeapon(WeaponSlot slot, WeaponItem item)
    {
        //Get Weapon Data
        WeaponData data = item.weaponPrefab.GetComponent<WeaponData>();

        //Check Sides
        if (slot == WeaponSlot.Right)
        {
            //Check if the weapon already exists
            GameObject weapon;
            if (rightHolder.weapons.TryGetValue(data, out weapon))
            {
                //Do some set active stuff here
                if (rightHolderCurrentWeapon != null)                                    
                    rightHolderCurrentWeapon.SetActive(false);

                //Enable new Weapon if it exists
                weapon.SetActive(true);
                rightHolderCurrentWeapon = weapon;
                
                //Get Holder transform on Weapon Prefab and set it to the parent
                Transform handle = weapon.GetComponent<WeaponData>().handleTransform;
                handle.parent = null;
                weapon.transform.parent = handle;
                handle.position = rightHolderObject.transform.position;
                handle.rotation = rightHolderObject.transform.rotation;

                //Be sure to reset the handle transform to it's original location
                weapon.transform.parent = null;
                handle.parent = weapon.transform;
                weapon.transform.parent = rightHolderObject.transform;
            }

            else
            {
                //Do some set active stuff here
                if (rightHolderCurrentWeapon != null)
                    rightHolderCurrentWeapon.SetActive(false);

                weapon = Instantiate(item.weaponPrefab);
                //Get Holder transform on Weapon Prefab and set it to the parent
                Transform handle = weapon.GetComponent<WeaponData>().handleTransform;
                handle.parent = null;
                weapon.transform.parent = handle;
                handle.position = rightHolderObject.transform.position;
                handle.rotation = rightHolderObject.transform.rotation;

                //Be sure to reset the handle transform to it's original location
                weapon.transform.parent = null;
                handle.parent = weapon.transform;
                weapon.transform.parent = rightHolderObject.transform;

                //handle.parent = rightHolderObject.transform;

                rightHolderCurrentWeapon = weapon;
            }
        }

        if (slot == WeaponSlot.Left)
        {
            //Check if the weapon already exists
            GameObject weapon;
            if (leftHolder.weapons.TryGetValue(data, out weapon))
            {
                //Do some set active stuff here
                if (leftHolderCurrentWeapon != null)
                    leftHolderCurrentWeapon.SetActive(false);

                //Enable new Weapon if it exists
                weapon.SetActive(true);
                leftHolderCurrentWeapon = weapon;

                //Get Holder transform on Weapon Prefab and set it to the parent
                Transform handle = weapon.GetComponent<WeaponData>().handleTransform;
                handle.parent = null;
                weapon.transform.parent = handle;
                handle.position = leftHolderObject.transform.position;
                handle.rotation = leftHolderObject.transform.rotation;

                //Be sure to reset the handle transform to it's original location
                weapon.transform.parent = null;
                handle.parent = weapon.transform;
                weapon.transform.parent = leftHolderObject.transform;
            }

            else
            {
                //Do some set active stuff here
                if (leftHolderCurrentWeapon != null)
                    leftHolderCurrentWeapon.SetActive(false);

                weapon = Instantiate(item.weaponPrefab);
                //Get Holder transform on Weapon Prefab and set it to the parent
                Transform handle = weapon.GetComponent<WeaponData>().handleTransform;
                handle.parent = null;
                weapon.transform.parent = handle;
                handle.position = leftHolderObject.transform.position;
                handle.rotation = leftHolderObject.transform.rotation;

                //Be sure to reset the handle transform to it's original location
                weapon.transform.parent = null;
                handle.parent = weapon.transform;
                weapon.transform.parent = leftHolderObject.transform;

                //handle.parent = rightHolderObject.transform;

                leftHolderCurrentWeapon = weapon;
            }
        }
    }

    public void OldEquipWeapon(WeaponSlot slot, WeaponItem item)
    {
        WeaponData data = item.weaponPrefab.GetComponent<WeaponData>();

        if (data != null)
        {
            Invector.vWeaponHolder hold;

            string str = $"{slot.ToString()}Holder@{data.weaponType.ToString()}";

            //print("Weapon Holder String: " + str);

            //FIX THIS

            if (holders.TryGetValue(str, out hold))
            {
                hold.SetActiveHolder(true);
                hold.SetActiveWeapon(true);
            }

            //data.Init();
            //int i = Random.Range(0, initHolders.Count);
            ////11 is the GreatSword
            //initHolders[i].weaponObject.GetComponent<MeshFilter>().mesh = data.weaponMesh.sharedMesh;
            //initHolders[i].weaponObject.GetComponent<MeshRenderer>().materials = data.weaponMats;
            //initHolders[i].holderObject.GetComponent<MeshFilter>().mesh = data.holderMesh.sharedMesh;
            //initHolders[i].holderObject.GetComponent<MeshRenderer>().materials = data.holderMats;
            //initHolders[i].SetActiveHolder(true);
            //initHolders[i].SetActiveWeapon(true);
        }

        else
            Debug.LogError($"WeaponInventory Equip Weapon: {item.weaponPrefab.name} does not have a WeaponData component!");
    }

    public void DrawWeaponScroll(WeaponSlot slot)
    {
        DrawWeapon(slot, weaponItems[scrollIndex]);
    }

    public void DrawWeapon(WeaponSlot slot, WeaponItem item)
    {
        WeaponData data = item.weaponPrefab.GetComponent<WeaponData>();

        if (slot == WeaponSlot.Right)
        {
            anim.SetInteger("EquipAnimID", (int)data.animID);
            anim.SetBool("IsEquipping", true);
            //StartCoroutine(WaitOnEquip(slot, data.meleeType, item));
            StartCoroutine(WaitOnEquip(slot, data, item));
        }

        if (slot == WeaponSlot.Left)
        {
            anim.SetInteger("EquipAnimID", (int)data.animID);
            anim.SetBool("IsEquipping", true);
            anim.SetBool("FlipEquip", true);
            //StartCoroutine(WaitOnEquip(slot, data.meleeType, item));
            StartCoroutine(WaitOnEquip(slot, data, item));
        }
    }

    public void DrawWeaponEvent()
    {
        print("DrawWeaponEvent called");
    }

    IEnumerator WaitOnEquip(WeaponSlot slot, WeaponData data, WeaponItem item)
    {
        //Make sure animator is actually in an equip animation state
        while (!anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndex("UpperBody")).IsTag("Equip"))
            yield return null;

        while (anim.GetBool("IsEquipping"))
        {            
            float time = anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndex("UpperBody")).normalizedTime;
            time = Mathf.Clamp(time, 0.0f, 1.0f);
            
            if (time >= data.normalizedEquipAnimationTime)
            {
                print("At Normalized Equip Time Continuing");
                break;
            }

            yield return null;
        }

        if (slot == WeaponSlot.Right)
        {
            if (data.weaponType == WeaponData.WeaponType.Melee)
            {
                rightHolderCurrentWeapon.transform.parent = rightHandWeapon.transform;
                rightHolderCurrentWeapon.transform.position = rightHandWeapon.transform.position;
                rightHolderCurrentWeapon.transform.rotation = rightHandWeapon.transform.rotation;
                //If there is a sheath/scabbard mesh unparent from weapon and parent to rightHolder
            }

            if (data.weaponType == WeaponData.WeaponType.Shield)
            {
                //For later
            }

            if (data.weaponType == WeaponData.WeaponType.Gun)
            {
                //For later
            }
        }

        if (slot == WeaponSlot.Left)
        {
            if (data.weaponType == WeaponData.WeaponType.Melee)
            {
                leftHolderCurrentWeapon.transform.parent = leftHandWeapon.transform;
            }

            if (data.weaponType == WeaponData.WeaponType.Shield)
            {
                //For later
            }

            if (data.weaponType == WeaponData.WeaponType.Gun)
            {
                //For later
            }
        }
    }

    IEnumerator WaitOnEquip(WeaponSlot slot, WeaponData.MeleeType meleeType,  WeaponItem item)
    {
        while (anim.GetBool("IsEquipping"))
        {
            yield return new WaitForEndOfFrame(); //I think this works well
        }

        if (slot == WeaponSlot.Right)
        {
            if (meleeType == WeaponData.MeleeType.Defense)
            {
                GameObject clone = Instantiate(item.weaponPrefab);
                clone.transform.parent = rightHandShield.transform;
                clone.transform.position = rightHandShield.transform.position;
                //need to get weapon holder to set mesh inactive
            }

            else
            {
                GameObject clone = Instantiate(item.weaponPrefab);
                clone.transform.parent = rightHandWeapon.transform;
                clone.transform.position = rightHandWeapon.transform.position;
                //need to get weapon holder to set mesh inactive

            }
        }

        if (slot == WeaponSlot.Left)
        {
            if (meleeType == WeaponData.MeleeType.Defense)
            {
                GameObject clone = Instantiate(item.weaponPrefab);
                clone.transform.parent = leftHandShield.transform;
                clone.transform.position = leftHandShield.transform.position;
                //need to get weapon holder to set mesh inactive
            }

            else
            {
                GameObject clone = Instantiate(item.weaponPrefab);
                clone.transform.parent = leftHandWeapon.transform;
                clone.transform.position = leftHandWeapon.transform.position;
                //need to get weapon holder to set mesh inactive
            }
        }

    }
}
