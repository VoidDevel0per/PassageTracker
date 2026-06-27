using BepInEx;
using MonoMod;
using System;
using System.Security.Permissions;
using UnityEngine;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace PassageTrackerMod;


[BepInPlugin("com.alexthedragon.passagetracker", "Passage Tracker", "1.0.3")]
public class PassageTracker : BaseUnityPlugin
{
    // Name is WinState.PassageDisplayName()
    public void OnEnable()
    {
        // ModInit
        On.RainWorld.OnModsInit += OnModsInit;
    }


    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (init) return;

        // Get Room
        On.Player.Update += OnPlayerUpdate;

        On.SlugcatStats.getSlugcatName += SlugcatStatsOngetSlugcatName;

        // Reset Saint/Hunter/Monk Passages at start of cycle
        On.RainWorldGame.ctor += OnRainWorldGameCtor;
        Logger.LogInfo("Hooked Cycle Start");

        // HUD
        On.HUD.HUD.InitSinglePlayerHud += SinglePlayerHudInit;
        On.HUD.HUD.InitMultiplayerHud += MultiplayerHudInit;
        Logger.LogInfo("Hooked HUD");

        // DragonSlayer/Outlaw
        On.PlayerSessionRecord.AddKill += Passage.OnAddKill;
        Logger.LogInfo("Hooked DragonSlayer+Outlaw");


        // Nomad & Wanderer
        On.OverWorld.GateRequestsSwitchInitiation += Passage.OnGateRequestsSwitchInitiation;
        Logger.LogInfo("Hooked Nomad and Wanderer");

        // Saint/Outlaw
        On.PlayerSessionRecord.BreakPeaceful += Passage.OnBreakPeaceful;
        Logger.LogInfo("Hooked Saint+Outlaw");

        // Hunter/Monk
        On.PlayerSessionRecord.AddEat += Passage.OnAddEat;
        Logger.LogInfo("Hooked Hunter+Monk");

        // Chieftain
        On.CreatureCommunities.InfluenceLikeOfPlayer += Passage.OnInfluenceLikeOfPlayer;
        Logger.LogInfo("Hooked Chieftain");

        // Scholar
        On.DataPearl.Update += Passage.OnDataPearlUpdate;
        Logger.LogInfo("Hooked Scholar");

        // Map
        On.HUD.Map.Update += Passage.OnMapUpdate;



        Logger.LogInfo("Hooked succesfully.");

        MachineConnector.SetRegisteredOI("alexthedragon.passagetracker", PassageTrackerOptions.instance);

        maxShowBarAmount = (int)(PassageTrackerOptions.passageDisplayTime.Value * 40f);
        init = true;
    }

    private string SlugcatStatsOngetSlugcatName(On.SlugcatStats.orig_getSlugcatName orig, SlugcatStats.Name i)
    {
        currentScugTimeline = SlugcatStats.SlugcatToTimeline(i);
        Logger.LogInfo($"I got the slugcat's timeline king! We are : {i}");
        orig(i);
        var wawa = i.ToString();
        return wawa;
    }

    public static void OnRainWorldGameCtor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        failedHunter = false;
        failedMonk = false;
        failedSaint = false;
        ateAnything = false;
        killsThisCycle = 0;
        scholarTracker = null;
        dragonslayerTracker = null;
        dragonslayerBATracker = null;
        orig(self, manager);
    }

    public void OnPlayerUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        room = self.room;
    }

    public static void SinglePlayerHudInit(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);
        self.AddPart(new HUDNotchMeter(self, self.fContainers[1]));
        self.AddPart(new HUDFloatMeter(self, self.fContainers[1]));
        self.AddPart(new MapPassageTracker(self, self.fContainers[1]));
    }

    public static void MultiplayerHudInit(On.HUD.HUD.orig_InitMultiplayerHud orig, HUD.HUD self, ArenaGameSession session)
    {
        orig(self, session);
        self.AddPart(new HUDNotchMeter(self, self.fContainers[1]));
        self.AddPart(new HUDFloatMeter(self, self.fContainers[1]));
        self.AddPart(new MapPassageTracker(self, self.fContainers[1]));
    }


    public static WinState.BoolArrayTracker dragonslayerBATracker = null;
    public static WinState.ListTracker scholarTracker, dragonslayerTracker = null;
    public static int maxShowBarAmount;
    public static int showNotchAmount, showFailAmount, showFloatAmount, showWandererAmount, showChieftainAmount = 0;
    public static WinState.EndgameTracker notchObject, floatObject;
    public static FSprite failIcon;
    public static Room room;
    public static bool failedHunter, failedMonk, failedSaint, ateAnything = false;
    public static int killsThisCycle = 0;
    public static SlugcatStats.Timeline currentScugTimeline;

    public static bool showMapPassageTracker = false;

    bool init;
    // WinState.EndgameID.DragonSlayer
}
