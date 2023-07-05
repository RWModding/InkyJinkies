﻿using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using BepInEx.Logging;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace InkyJinkies;

[BepInPlugin(MOD_ID, "Inky Jinkies", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "inkyjinkies";
    public const string ACRONYM = "OWO";
    public static new ManualLogSource Logger { get; private set; } = null!;

    public bool IsInit;
    public bool IsPreInit;
    public bool IsPostInit;
    public bool ExpeditionPatched;

    private void OnEnable()
    {
        Logger = base.Logger;

        On.RainWorld.PreModsInit += RainWorld_PreModsInit;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
    }

    private void RainWorld_PreModsInit(On.RainWorld.orig_PreModsInit orig, RainWorld self)
    {
        orig(self);
  
        try
        {
            if (IsPreInit) return;
            IsPreInit = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            if (IsInit) return;
            IsInit = true;

            BigAcronymFix.Apply();
            Overlay.Apply();
            AllSeeingEye.Apply();
            RunawayRemixMenu.Apply();
            BobMarley.Apply();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            //-- Is done every PostModsInit because it can only run after expedition has been initialized, and it may be initialized mid-game
            if (ModManager.Expedition && !ExpeditionPatched)
            {
                BigAcronymFix.ApplyExpedition();
                ExpeditionPatched = true;
            }
            
            if (IsPostInit) return;
            IsPostInit = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}