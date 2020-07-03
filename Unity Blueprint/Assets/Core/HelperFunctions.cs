using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{    
    public static bool Equals(Var v, Var v2)
    {       
        if (v.type == v2.type)       
            return v.obj.Equals(v2.obj);        
        else
            return false;
    }

    public static bool NotEquals(Var v, Var v2)
    {
        if (v.type == v2.type)
            return !v.obj.Equals(v2.obj);
        else
            return false;
    }

    public static bool LessThan(Var v, Var v2)
    {
        try
        {
            double val = (double)System.Convert.ChangeType(v.obj, typeof(double));
            double val2 = (double)System.Convert.ChangeType(v2.obj, typeof(double));
            return val < val2;
        }
        catch
        {
            return false;
        }

    }

    public static bool GreaterThan(Var v, Var v2)
    {
        try
        {
            double val = (double)System.Convert.ChangeType(v.obj, typeof(double));
            double val2 = (double)System.Convert.ChangeType(v2.obj, typeof(double));
            return val > val2;
        }

        catch
        {
            return false;
        }
    }

    public static bool LessThanOrEqual(Var v, Var v2)
    {
        try
        {
            double val = (double)System.Convert.ChangeType(v.obj, typeof(double));
            double val2 = (double)System.Convert.ChangeType(v2.obj, typeof(double));
            return val <= val2;
        }

        catch
        {
            return false;
        }
    }

    public static bool GreaterThanOrEqual(Var v, Var v2)
    {
        try
        {            
            double val = (double)System.Convert.ChangeType(v.obj, typeof(double));
            double val2 = (double)System.Convert.ChangeType(v2.obj, typeof(double));
            return val >= val2;
        }

        catch
        {
            return false;
        }
    }

    public static bool And(Var v, Var v2)
    {
        try
        {
            return (bool)v.obj && (bool)v2.obj;
        }

        catch
        {
            return false;
        }
    }

    public static bool Or(Var v, Var v2)
    {
        try
        {
            return (bool)v.obj || (bool)v2.obj;
        }

        catch
        {
            return false;
        }
    }
}
