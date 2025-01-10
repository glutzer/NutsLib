using MareLib;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace Equimancy;

/// <summary>
/// Manages spell visual effects.
/// </summary>
[GameSystem(forSide = EnumAppSide.Client)]
public class FXManager : GameSystem
{
    private static readonly Dictionary<string, IFXType> fxTypes = new();

    public FXManager(bool isServer, ICoreAPI api) : base(isServer, api)
    {

    }

    public override void OnAssetsLoaded()
    {
        RegisterAllFX();
    }

    public static T GetFX<T>() where T : IFXType
    {
        return (T)fxTypes[InnerClass<T>.Name];
    }

    private static void RegisterAllFX()
    {
        (Type, FXAttribute)[] types = AttributeUtilities.GetAllAnnotatedClasses<FXAttribute>();

        // Need to set constructor params here later.
        foreach ((Type, FXAttribute) type in types)
        {
            fxTypes[type.Item1.Name] = (IFXType)Activator.CreateInstance(type.Item1)!;
        }
    }

    public override void OnClose()
    {
        foreach (KeyValuePair<string, IFXType> item in fxTypes)
        {
            item.Value.OnClosing();
        }
        fxTypes.Clear();
    }
}