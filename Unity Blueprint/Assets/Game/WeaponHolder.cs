using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public Dictionary<WeaponData, GameObject> weapons;
    public WeaponInventory.WeaponSlot slot;
    // Start is called before the first frame update
    void Start()
    {
        weapons = new Dictionary<WeaponData, GameObject>();
        WeaponInventory inventory = transform.root.GetComponent<WeaponInventory>();

        if (slot == WeaponInventory.WeaponSlot.Right)
        {
            inventory.rightHolder = this;
            inventory.rightHolderObject = gameObject;
        }

        if (slot == WeaponInventory.WeaponSlot.Left)
        {
            inventory.leftHolder = this;
            inventory.leftHolderObject = gameObject;
        }

    }   
}
