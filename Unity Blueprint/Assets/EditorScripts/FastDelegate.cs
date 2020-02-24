/*
    The MIT License (MIT)

    Copyright (c) 2015 coder0xff
    
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 */

//Source: https://github.com/coder0xff/FastDelegate.Net/blob/master/FastDelegate.Net/FastDelegate.cs

using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System;

//namespace FastDelegate.Net
public static class MethodInfoExtensions
{
    private static Func<object, object[], object> CreateForNonVoidInstanceMethod(MethodInfo method)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            Expression.Convert(instanceParameter, method.DeclaringType),
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(
            Expression.Convert(call, typeof(object)),
            instanceParameter,
            argumentsParameter);

        return lambda.Compile();
    }

    private static Func<object[], object> CreateForNonVoidStaticMethod(MethodInfo method)
    {
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(
            Expression.Convert(call, typeof(object)),
            argumentsParameter);

        return lambda.Compile();
    }

    private static Action<object, object[]> CreateForVoidInstanceMethod(MethodInfo method)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            Expression.Convert(instanceParameter, method.DeclaringType),
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(
            call,
            instanceParameter,
            argumentsParameter);

        return lambda.Compile();
    }

    private static Action<object[]> CreateForVoidStaticMethod(MethodInfo method)
    {
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Action<object[]>> lambda = Expression.Lambda<Action<object[]>>(
            call,
            argumentsParameter);

        return lambda.Compile();
    }

    private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
    {
        return method.GetParameters().Select((parameter, index) =>
            Expression.Convert(
                Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).Cast<Expression>().ToArray();
    }

    public static Func<object, object[], object> Bind(this MethodInfo method)
    {
        if (method.IsStatic)
        {
            if (method.ReturnType == typeof(void))
            {
                Action<object[]> wrapped = CreateForVoidStaticMethod(method);
                return (target, parameters) => {
                    wrapped(parameters);
                    return (object)null;
                };
            }
            else
            {
                Func<object[], object> wrapped = CreateForNonVoidStaticMethod(method);
                return (target, parameters) => wrapped(parameters);
            }
        }
        if (method.ReturnType == typeof(void))
        {
            Action<object, object[]> wrapped = CreateForVoidInstanceMethod(method);
            return (target, parameters) => {
                wrapped(target, parameters);
                return (object)null;
            };
        }
        else
        {
            Func<object, object[], object> wrapped = CreateForNonVoidInstanceMethod(method);
            return wrapped;
        }
    }

    public static Type LambdaType(this MethodInfo method)
    {
        if (method.ReturnType == typeof(void))
        {
            Type actionGenericType;
            switch (method.GetParameters().Length)
            {
                case 0:
                    return typeof(Action);
                case 1:
                    actionGenericType = typeof(Action<>);
                    break;
                case 2:
                    actionGenericType = typeof(Action<,>);
                    break;
                case 3:
                    actionGenericType = typeof(Action<,,>);
                    break;
                case 4:
                    actionGenericType = typeof(Action<,,,>);
                    break;
#if NET_FX_4 //See #define NET_FX_4 as the head of this file
                    case 5:
                        actionGenericType = typeof(Action<,,,,>);
                        break;
                    case 6:
                        actionGenericType = typeof(Action<,,,,,>);
                        break;
                    case 7:
                        actionGenericType = typeof(Action<,,,,,,>);
                        break;
                    case 8:
                        actionGenericType = typeof(Action<,,,,,,,>);
                        break;
                    case 9:
                        actionGenericType = typeof(Action<,,,,,,,,>);
                        break;
                    case 10:
                        actionGenericType = typeof(Action<,,,,,,,,,>);
                        break;
                    case 11:
                        actionGenericType = typeof(Action<,,,,,,,,,,>);
                        break;
                    case 12:
                        actionGenericType = typeof(Action<,,,,,,,,,,,>);
                        break;
                    case 13:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,>);
                        break;
                    case 14:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,>);
                        break;
                    case 15:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,>);
                        break;
                    case 16:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,,>);
                        break;
#endif
                default:
                    throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
            }
            return actionGenericType.MakeGenericType(method.GetParameters().Select(_ => _.ParameterType).ToArray());
        }
        Type functionGenericType;
        switch (method.GetParameters().Length)
        {
            case 0:
                return typeof(Func<>);
            case 1:
                functionGenericType = typeof(Func<,>);
                break;
            case 2:
                functionGenericType = typeof(Func<,,>);
                break;
            case 3:
                functionGenericType = typeof(Func<,,,>);
                break;
            case 4:
                functionGenericType = typeof(Func<,,,,>);
                break;
#if NET_FX_4 //See #define NET_FX_4 as the head of this file
                case 5:
                    funcGenericType = typeof(Func<,,,,,>);
                    break;
                case 6:
                    funcGenericType = typeof(Func<,,,,,,>);
                    break;
                case 7:
                    funcGenericType = typeof(Func<,,,,,,,>);
                    break;
                case 8:
                    funcGenericType = typeof(Func<,,,,,,,,>);
                    break;
                case 9:
                    funcGenericType = typeof(Func<,,,,,,,,,>);
                    break;
                case 10:
                    funcGenericType = typeof(Func<,,,,,,,,,,>);
                    break;
                case 11:
                    funcGenericType = typeof(Func<,,,,,,,,,,,>);
                    break;
                case 12:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,>);
                    break;
                case 13:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,>);
                    break;
                case 14:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,,>);
                    break;
                case 15:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,,,>);
                    break;
                case 16:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,,,,>);
                    break;
#endif
            default:
                throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
        }
        var parametersAndReturnType = new Type[method.GetParameters().Length + 1];
        method.GetParameters().Select(_ => _.ParameterType).ToArray().CopyTo(parametersAndReturnType, 0);
        parametersAndReturnType[parametersAndReturnType.Length - 1] = method.ReturnType;
        return functionGenericType.MakeGenericType(parametersAndReturnType);
    }
}