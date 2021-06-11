using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponData : MonoBehaviour
{
    //Do away with this
    //public enum WeaponType { Sword, GreatSword, Shield, GreatShield, Axe, Katana, GreatKatana, Gun }
    public enum WeaponType { Melee, Shield, Gun }
    public enum MeleeType { Attack, Defense, AttackAndDefense }
    public enum MoveSetType { Unarmed, OneHand, TwoHand }

    [Header("Type Settings")]
    public WeaponType weaponType = WeaponType.Melee;
    public MeleeType meleeType = MeleeType.Attack;
    public WeaponInventory.WeaponEquipAnimation animID;
    public bool twoHandItem = false;

    [Header("Attack Settings")]
    public bool useHeavyAttack = true;
    [Tooltip("Trigger a Attack Animation")]
    public int attackID; //not currently used
    [Tooltip("This determines which animations to use based on WeaponType")]
    public int movesetID;
    [Tooltip("My more readable version of the move set system")]
    public MoveSetType moveSetType;

    [Header("* Third Person Controller Only *")]
    [Tooltip("How much stamina will be consumed when attack")]
    public float staminaCost;
    [Tooltip("How much time the stamina will wait to start recover")]
    public float staminaRecoveryDelay;

    [Header("Defense Settings")]
    [Range(0, 100)]
    public int defenseRate = 100;
    [Range(0, 180)]
    public float defenseRange = 90;
    [Tooltip("Trigger a Defense Animation")]
    public int defenseID;
    [Tooltip("What recoil animation will trigger")]
    public int recoilID;
    [Tooltip("Can break the opponent attack, will trigger a recoil animation")]
    public bool breakAttack;

    [Tooltip("The transform of the handle to use to snap the object into the right place")]
    public Transform handleTransform;
    [Tooltip("The normalized time in the animation where the weapon should actually be parented to the hand")]
    public float normalizedEquipAnimationTime = 0.5f;

    public MeshFilter weaponMesh;
    [HideInInspector] public Material[] weaponMats;

    public MeshFilter holderMesh;
    [HideInInspector] public Material[] holderMats;
    [HideInInspector] public BoxCollider hitBox;

    // Start is called before the first frame update
    void Start()
    {        
        hitBox = GetComponentInChildren<BoxCollider>();
        weaponMats = weaponMesh.GetComponent<MeshRenderer>().materials;
        holderMats = holderMesh.GetComponent<MeshRenderer>().materials;        
    }

    public void Init()
    {
        hitBox = GetComponentInChildren<BoxCollider>();
        weaponMats = weaponMesh.GetComponent<MeshRenderer>().sharedMaterials;
        holderMats = holderMesh.GetComponent<MeshRenderer>().sharedMaterials;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
