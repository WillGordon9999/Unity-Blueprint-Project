using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ClassAccess { All, None, Partial };

[Serializable]
public class MemberData
{
    public string name; //should possibly include pass In parameters
    public enum MemberType { Field, Property, Function, Constructor };
    public MemberType memberType;

    public enum AccessType { Get, Set, Both, None };
    public AccessType access;

    public int metaDataToken;

    public MemberData(string theName, MemberType memType, AccessType accessType, int metaData)
    {
        name = theName;
        memberType = memType;
        access = accessType;
        metaDataToken = metaData;
    }

}


[Serializable]
public class GameClass
{
    public string className;
    public ClassAccess access;
    public List<MemberData> members;

    public GameClass(string name, ClassAccess classAccess)
    {
        className = name;
        access = classAccess;
        members = new List<MemberData>();
    }    
}

public class GameCodebase : ScriptableObject
{
    public List<GameClass> classes;
}
