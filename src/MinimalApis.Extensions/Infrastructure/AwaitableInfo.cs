﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Taken from https://github.com/dotnet/aspnetcore

#nullable disable

using System.Reflection;
using System.Runtime.CompilerServices;

namespace MinimalApis.Extensions.Infrastructure;

internal readonly struct AwaitableInfo
{
    public Type AwaiterType { get; }
    public PropertyInfo AwaiterIsCompletedProperty { get; }
    public MethodInfo AwaiterGetResultMethod { get; }
    public MethodInfo AwaiterOnCompletedMethod { get; }
    public MethodInfo AwaiterUnsafeOnCompletedMethod { get; }
    public Type ResultType { get; }
    public MethodInfo GetAwaiterMethod { get; }

    public AwaitableInfo(
        Type awaiterType,
        PropertyInfo awaiterIsCompletedProperty,
        MethodInfo awaiterGetResultMethod,
        MethodInfo awaiterOnCompletedMethod,
        MethodInfo awaiterUnsafeOnCompletedMethod,
        Type resultType,
        MethodInfo getAwaiterMethod)
    {
        AwaiterType = awaiterType;
        AwaiterIsCompletedProperty = awaiterIsCompletedProperty;
        AwaiterGetResultMethod = awaiterGetResultMethod;
        AwaiterOnCompletedMethod = awaiterOnCompletedMethod;
        AwaiterUnsafeOnCompletedMethod = awaiterUnsafeOnCompletedMethod;
        ResultType = resultType;
        GetAwaiterMethod = getAwaiterMethod;
    }

    public static Type GetMethodReturnType(MethodInfo method)
    {
        if (AwaitableInfo.IsTypeAwaitable(method.ReturnType, out var awaitableInfo))
        {
            return awaitableInfo.ResultType;
        }

        return method.ReturnType;
    }

    public static Type GetParameterType(ParameterInfo parameter)
    {
        if (AwaitableInfo.IsTypeAwaitable(parameter.ParameterType, out var awaitableInfo))
        {
            return awaitableInfo.ResultType;
        }

        return parameter.ParameterType;
    }

    public static bool IsTypeAwaitable(Type type, out AwaitableInfo awaitableInfo)
    {
        // Based on Roslyn code: http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/Shared/Extensions/ISymbolExtensions.cs,db4d48ba694b9347

        // Awaitable must have method matching "object GetAwaiter()"
        var getAwaiterMethod = type.GetRuntimeMethods().FirstOrDefault(m =>
            m.Name.Equals("GetAwaiter", StringComparison.OrdinalIgnoreCase)
            && m.GetParameters().Length == 0
            && m.ReturnType != null);
        if (getAwaiterMethod == null)
        {
            awaitableInfo = default;
            return false;
        }

        var awaiterType = getAwaiterMethod.ReturnType;

        // Awaiter must have property matching "bool IsCompleted { get; }"
        var isCompletedProperty = awaiterType.GetRuntimeProperties().FirstOrDefault(p =>
            p.Name.Equals("IsCompleted", StringComparison.OrdinalIgnoreCase)
            && p.PropertyType == typeof(bool)
            && p.GetMethod != null);
        if (isCompletedProperty == null)
        {
            awaitableInfo = default;
            return false;
        }

        // Awaiter must implement INotifyCompletion
        var awaiterInterfaces = awaiterType.GetInterfaces();
        var implementsINotifyCompletion = awaiterInterfaces.Any(t => t == typeof(INotifyCompletion));
        if (!implementsINotifyCompletion)
        {
            awaitableInfo = default;
            return false;
        }

        // INotifyCompletion supplies a method matching "void OnCompleted(Action action)"
        var onCompletedMethod = typeof(INotifyCompletion).GetRuntimeMethods().Single(m =>
            m.Name.Equals("OnCompleted", StringComparison.OrdinalIgnoreCase)
            && m.ReturnType == typeof(void)
            && m.GetParameters().Length == 1
            && m.GetParameters()[0].ParameterType == typeof(Action));

        // Awaiter optionally implements ICriticalNotifyCompletion
        var implementsICriticalNotifyCompletion = awaiterInterfaces.Any(t => t == typeof(ICriticalNotifyCompletion));
        MethodInfo unsafeOnCompletedMethod;
        if (implementsICriticalNotifyCompletion)
        {
            // ICriticalNotifyCompletion supplies a method matching "void UnsafeOnCompleted(Action action)"
            unsafeOnCompletedMethod = typeof(ICriticalNotifyCompletion).GetRuntimeMethods().Single(m =>
                m.Name.Equals("UnsafeOnCompleted", StringComparison.OrdinalIgnoreCase)
                && m.ReturnType == typeof(void)
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType == typeof(Action));
        }
        else
        {
            unsafeOnCompletedMethod = null;
        }

        // Awaiter must have method matching "void GetResult" or "T GetResult()"
        var getResultMethod = awaiterType.GetRuntimeMethods().FirstOrDefault(m =>
            m.Name.Equals("GetResult")
            && m.GetParameters().Length == 0);
        if (getResultMethod == null)
        {
            awaitableInfo = default;
            return false;
        }

        awaitableInfo = new(
            awaiterType,
            isCompletedProperty,
            getResultMethod,
            onCompletedMethod,
            unsafeOnCompletedMethod,
            getResultMethod.ReturnType,
            getAwaiterMethod);
        return true;
    }
}
