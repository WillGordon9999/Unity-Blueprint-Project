//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using System.Reflection;
////using UnityEditor;
//
//public class TestComponent : MonoBehaviour
//{
//    System.Type type;
//        
//    // Start is called before the first frame update
//    void Start()
//    {
//        var lambdaSetter = new Action<GameObject, object>((o, m) => o.active = (bool)m);
//    }
//
//    // Update is called once per frame
//    void Update()
//    {
//        
//    }
//
//    //Testing to see what happens when you change parameters
//    //The result is that it's no longer a message, it becomes a local definition function
//    private void OnCollisionEnter(Collision collision)
//    {
//        
//    }
//
//    private void OnGUI()
//    {
//        //GUI.SetNextControlName("Float");        
//        //result = GUI.TextField(new Rect(Screen.width / 2, Screen.height / 2, 300, 100), RealTimeWindow.result.ToString());
//        //EditorGUI.FocusTextInControl("Float");
//        //toolbarInt = GUI.Toolbar(new Rect(25, 25, 250, 30), toolbarInt, new string[] { "Option 1", "Option 2", "Option 3", "Option 4"});
//    }
//}

//public class RealTimeWindow : EditorWindow
//{
//    public static RealTimeWindow Instance;
//    public static float result;
//
//    public static void OpenWindow()
//    {
//        Debug.Log("Instantiating window");
//        Instance = GetWindow<RealTimeWindow>();
//        Instance.titleContent = new GUIContent("Test");
//    }
//
//    private void OnGUI()
//    {
//        Debug.Log("In OnGUI for window");
//        GUI.SetNextControlName("Test");
//        result = EditorGUI.FloatField(new Rect(Screen.width / 2, Screen.height / 2, 300, 100), result);
//        GUI.FocusControl("Test");
//    }
//
//    private void Update()
//    {       
//    }
//}

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;


/// <summary>
/// A Property or Field accessor object that uses compiled Expressions to access a
/// property or a field. Properties are assumed to have both getter and setter methods
/// for the respective methods to be available. The access modifiers do not matter, as
/// the application must already have a <see cref="PropertyInfo"/> or a
/// <see cref="FieldInfo"/> object to use this class.
/// </summary>
public class CGetterSetter
{
    /// <summary>
    /// Get the helper method signature one time
    /// </summary>
    private static MethodInfo sm_valueAssignerMethod =
        typeof(CGetterSetter)
        .GetMethod("ValueAssigner", BindingFlags.Static | BindingFlags.NonPublic);

    /// <summary>
    /// This is the internal method responsible for assigning one value to a member.
    /// This is required to make this class compiant with .NET 3.5 (Unity3d compatible)
    /// </summary>
    /// <typeparam name="T">The Type of the values to assign</typeparam>
    /// <param name="dest">The destination member</param>
    /// <param name="src">The value to assign</param>
    private static void ValueAssigner<T>(out T dest, T src)
    {
        dest = src;
    }


    /// <summary>
    /// The delegate for getting the value of the member
    /// </summary>
    private Func<object, object> m_getter;

    /// <summary>
    /// The delegate for setting the value of the member
    /// </summary>
    private Action<object, object> m_setter;

    /// <summary>
    /// Get the value of the member on a provided object.
    /// </summary>
    /// <param name="p_obj">The object to query for the member value</param>
    /// <returns>The value of the member on the provided object</returns>
    public object Get(object p_obj)
    {
        return m_getter(p_obj);
    }

    /// <summary>
    /// Set the value on a given object to a given value.
    /// </summary>
    /// <param name="p_obj">The object whose member value to set</param>
    /// <param name="p_value">The value to assign to the member</param>
    public void Set(object p_obj, object p_value)
    {
        m_setter(p_obj, p_value);
    }

    /// <summary>
    /// Construct a new member accessor based on a Reflection MemberInfo- either a
    /// PropertyInfo or a FieldInfo
    /// </summary>
    /// <param name="p_member">
    /// A PropertyInfo or a FieldInfo describing the member to access
    /// </param>
    public CGetterSetter(MemberInfo p_member)
    {
        if (p_member == null)
            throw new ArgumentNullException("Must initialize with a non-null Field or Property");

        MemberExpression exMember = null;

        if (p_member is FieldInfo)
        {
            var fi = p_member as FieldInfo;
            var assignmentMethod = sm_valueAssignerMethod.MakeGenericMethod(fi.FieldType);

            Init(fi.DeclaringType, fi.FieldType,
                _ex => exMember = Expression.Field(_ex, fi), // Create a Field expression, and SAVE that field expression for the Call expression
                (_, _val) => Expression.Call(assignmentMethod, exMember, _val) // We're going to call the static "ValueAssigner" method on this class
            );
        }
        else if (p_member is PropertyInfo)
        {
            var pi = p_member as PropertyInfo;
            var assignmentMethod = pi.GetSetMethod();

            Init(pi.DeclaringType, pi.PropertyType,
                _ex => exMember = Expression.Property(_ex, pi), // Create a Property expression
                (_obj, _val) => Expression.Call(_obj, assignmentMethod, _val) // We're going to call the SetMethod on the PropertyInfo object
            );
        }
        else
        {
            throw new ArgumentException("The member must be either a Field or a Property, not " + p_member.MemberType);
        }
    }


    /// <summary>
    /// Internal initialization routine. The difference between Field and Property
    /// access is extremely similar, but just different enough to require the two
    /// delegates back into the calling routine provide the specialized information.
    /// </summary>
    /// <param name="p_objectType">
    /// The Type of the objects that will have this member accessed
    /// </param>
    /// <param name="p_valueType">The Type of the member</param>
    /// <param name="p_fnGetMember">
    /// A delegate that returns the correct Expression for the member- either
    /// <see cref="Expression.Property"/> or <see cref="Expression.Field"/>
    /// </param>
    /// <param name="p_fnMakeCallExpression">
    /// Get a method that actually calls the Assignment function appropriate for the
    /// MemberType. The order of the parameters for Fields vs Properties is slightly
    /// different, as the Field assignment is static while the Property assignment is an
    /// instance method.
    /// </param>
    private void Init(
        Type p_objectType,
        Type p_valueType,
        Func<Expression, MemberExpression> p_fnGetMember,
        Func<Expression, Expression, MethodCallExpression> p_fnMakeCallExpression)
    {
        var exObjParam = Expression.Parameter(typeof(object), "theObject");
        var exValParam = Expression.Parameter(typeof(object), "theProperty");

        var exObjConverted = Expression.Convert(exObjParam, p_objectType);
        var exValConverted = Expression.Convert(exValParam, p_valueType);

        Expression exMember = p_fnGetMember(exObjConverted);

        Expression getterMember = p_valueType.IsValueType ? Expression.Convert(exMember, typeof(object)) : exMember;
        m_getter = Expression.Lambda<Func<object, object>>(getterMember, exObjParam).Compile();

        Expression exAssignment = p_fnMakeCallExpression(exObjConverted, exValConverted);
        m_setter = Expression.Lambda<Action<object, object>>(exAssignment, exObjParam, exValParam).Compile();
    }
}

#if false // The following code was refactored because of the extreme similarities between the methods.
        public CGenGetterSetter( MemberInfo p_member )
        {
            if (p_member == null)
                throw new ArgumentNullException( "Must initialize with a non-null Field or Property" );

            if (p_member is FieldInfo)
                InitAsField( p_member as FieldInfo );
            else if (p_member is PropertyInfo)
                InitAsProperty( p_member as PropertyInfo );
            else
                throw new ArgumentException( "The member must be either a Field or a Property, not " + p_member.MemberType );
        }


        private void InitAsProperty( PropertyInfo p_propertyInfo )
        {
            var objType = p_propertyInfo.DeclaringType;
            var valType = p_propertyInfo.PropertyType;

            var assignmentMethod = p_propertyInfo.GetSetMethod();



            var exObjParam = Expression.Parameter( typeof( object ), "theObject" );
            var exValParam = Expression.Parameter( typeof( object ), "theProperty" );

            var exObjConverted = Expression.Convert( exObjParam, objType );
            var exValConverted = Expression.Convert( exValParam, valType );

            /**/
            Expression exMember = Expression.Property( exObjConverted, p_propertyInfo );

            Expression getterMember = valType.IsValueType ? Expression.Convert( exMember, typeof( object ) ) : exMember;
            m_getter = Expression.Lambda<Func<object, object>>( getterMember, exObjParam ).Compile();

            /**/
            Expression exAssignment = Expression.Call( exObjConverted, assignmentMethod, exValConverted );
            m_setter = Expression.Lambda<Action<object, object>>( exAssignment, exObjParam, exValParam ).Compile();
        }

        private void InitAsField( FieldInfo p_fieldInfo )
        {
            var objType = p_fieldInfo.DeclaringType;
            var valType = p_fieldInfo.FieldType;

            var assignmentMethod = typeof( CGenGetterSetter )
                            .GetMethod( "ValueAssigner", BindingFlags.Static | BindingFlags.NonPublic )
                            .MakeGenericMethod( valType );



            var exObjParam = Expression.Parameter( typeof( object ), "theObject" );
            var exValParam = Expression.Parameter( typeof( object ), "theProperty" );

            var exObjConverted = Expression.Convert( exObjParam, objType );
            var exValConverted = Expression.Convert( exValParam, valType );

            /**/
            Expression exMember = Expression.Field( exObjConverted, p_fieldInfo );

            Expression getterMember = valType.IsValueType ? Expression.Convert( exMember, typeof( object ) ) : exMember;
            m_getter = Expression.Lambda<Func<object, object>>( getterMember, exObjParam ).Compile();

            /**/
            var exAssignment = Expression.Call( assignmentMethod, exMember, exValConverted );
            m_setter = Expression.Lambda<Action<object, object>>( exAssignment, exObjParam, exValParam ).Compile();
#endif