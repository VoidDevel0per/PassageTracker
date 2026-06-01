
using System.Collections.Generic;
using System.Text.RegularExpressions;
using On;

namespace PassageTrackerMod;

public class Passage
{
    //DragonSlayer+Outlaw
    public static void OnAddKill(On.PlayerSessionRecord.orig_AddKill orig, PlayerSessionRecord self, Creature victim)
	{
		orig(self, victim);

        DragonslayerTracking(self, victim);

        if (!(victim.Template.countsAsAKill > 0))
            return;

        //Outlaw
        HUDFloatMeter.init = true;
        PassageTracker.killsThisCycle++;
        WinState winState = PassageTracker.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
        WinState.IntegerTracker outlawTracker = winState.GetTracker(WinState.EndgameID.Outlaw, true) as WinState.IntegerTracker;
        ShowPassage(WinState.EndgameID.Outlaw, outlawTracker);
    }

    //Saint
    public static void OnBreakPeaceful(On.PlayerSessionRecord.orig_BreakPeaceful orig, PlayerSessionRecord self, Creature victim)
    {
        orig(self, victim);
        if (PassageTracker.failedSaint)
            return;

        if (victim.Template.countsAsAKill > 0)
        {
            PassageTracker.failedSaint = true;
            FailPassage(WinState.EndgameID.Saint);
        }
    }

    //Hunter/Monk
    public static void OnAddEat(On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenObject)
    {
        orig(self, eatenObject);
        if (eatenObject is KarmaFlower || eatenObject is Mushroom)
        {
            return;
        }
        PassageTracker.ateAnything = true;
        if (eatenObject is Creature || eatenObject.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.JellyFish || eatenObject.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg)
        {
            if (!PassageTracker.failedMonk)
                FailPassage(WinState.EndgameID.Monk);
            PassageTracker.failedMonk = true;

            return;
        }
        if (!PassageTracker.failedHunter)
            FailPassage(WinState.EndgameID.Hunter);

        PassageTracker.failedHunter = true;
    }
    

    //Nomad
    public static void OnGateRequestsSwitchInitiation(On.OverWorld.orig_GateRequestsSwitchInitiation orig, OverWorld self, RegionGate reportBackToGate)
    {
        orig(self, reportBackToGate);

        if (self.game.IsArenaSession)
            return;
        WinState winState = self.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
        
        //Wanderer
        WinState.BoolArrayTracker bat = winState.GetTracker(WinState.EndgameID.Traveller, true) as WinState.BoolArrayTracker;
        List<string> storyRegions = SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot);
        int q = 0;
        foreach (bool regionslept in bat.lastShownProgress)
        {
            if (q < storyRegions.Count && storyRegions[q] == self.activeWorld.name)
            {
                if (!regionslept)
                {
                    PassageTracker.showWandererAmount = PassageTracker.maxShowBarAmount;
                    ShowPassage(WinState.EndgameID.Traveller, bat);
                }

                break;
            }
            q++;
        }
        
        //Nomad
        if (!ModManager.MSC)
            return;

        WinState winstate = self.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
        WinState.IntegerTracker survivorTracker = winstate.GetTracker(WinState.EndgameID.Survivor, true) as WinState.IntegerTracker;

        if (!survivorTracker.GoalAlreadyFullfilled)
            return;

        AbstractRoom abstractRoom = reportBackToGate.room.abstractRoom;

        string text = self.activeWorld.name;
        text = Region.GetVanillaEquivalentRegionAcronym(text);
        string[] array = Regex.Split(abstractRoom.name, "_");
        string text2 = "ERROR!";
        if (array.Length == 3)
        {
            for (int i = 1; i < 3; i++)
            {
                if (array[i] != text)
                {
                    text2 = array[i];
                    break;
                }
            }
        }

        text2 = Region.GetProperRegionAcronym(self.game.IsStorySession ? self.game.StoryCharacter : null, text2);
        if (ModManager.MSC)
        {
            WinState.ListTracker nomadTracker = self.game.GetStorySession.saveState.deathPersistentSaveData.winState.GetTracker(MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad, true) as WinState.ListTracker;
            if (!nomadTracker.GoalAlreadyFullfilled)
            {
                if (nomadTracker.myList.Count == 0 || nomadTracker.myList[nomadTracker.myList.Count - 1] != self.GetRegion(text2).regionNumber)
                {
                    ShowPassage(MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad, nomadTracker);
                }
            }
            
        }
    }

    //DragonSlayer
    public static void DragonslayerTracking(PlayerSessionRecord self, Creature victim)
    {
        if (victim.Template.IsLizard)
        {
            WinState winState = victim.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;

            if (ModManager.MSC)
            {
                if(PassageTracker.dragonslayerTracker == null)
                {
                    WinState.ListTracker listTracker = winState.GetTracker(WinState.EndgameID.DragonSlayer, true) as WinState.ListTracker;
                    PassageTracker.dragonslayerTracker = new WinState.ListTracker(listTracker.ID, listTracker.totItemsToWin);
                    foreach(int i in listTracker.myLastList)
                    {
                        PassageTracker.dragonslayerTracker.myLastList.Add(i);
                    }
                }

                int k = 0;

                if (PassageTracker.dragonslayerTracker.myLastList.Count > 0)
                {
                    for (int count = 0; count < PassageTracker.dragonslayerTracker.myLastList.Count; count++)
                    {
                        while (k < WinState.lizardsOrder.Length)
                        {
                            if (victim.Template.type == WinState.lizardsOrder[k]) //If GreenLizard == GreenLizard
                            {
                                if (PassageTracker.dragonslayerTracker.myLastList[count] == k)
                                {
                                    return;
                                }
                                break;
                            }
                            else k++;
                        }
                    }
                }
                else
                {
                    while (k < WinState.lizardsOrder.Length)
                    {
                        if (victim.Template.type == WinState.lizardsOrder[k]) //If GreenLizard == GreenLizard
                            break;

                        k++;
                    }
                } 

                PassageTracker.dragonslayerTracker.myLastList.Add(k);

                ShowPassage(WinState.EndgameID.DragonSlayer, PassageTracker.dragonslayerTracker); 
            }
            else
            {
                if(PassageTracker.dragonslayerBATracker == null)
                {
                    WinState.BoolArrayTracker bat = winState.GetTracker(WinState.EndgameID.DragonSlayer, true) as WinState.BoolArrayTracker;

                    PassageTracker.dragonslayerBATracker = new WinState.BoolArrayTracker(bat.ID, 6);
                    int i = 0;
                    foreach(bool a in bat.lastShownProgress)
                    {
                        PassageTracker.dragonslayerBATracker.lastShownProgress[i] = a;
                        i++;
                    }
                }

                int k = 0;
                while (k < WinState.lizardsOrder.Length)
                {
                    if (self.kills[self.kills.Count - 1].symbolData.critType == WinState.lizardsOrder[k])
                    {
                        if(k < PassageTracker.dragonslayerBATracker.lastShownProgress.Length)
                        {
                            if (PassageTracker.dragonslayerBATracker.lastShownProgress[k])
                            {
                                return;
                            }
                        }
                        break;
                    }
                    else k++;
                }
                PassageTracker.dragonslayerBATracker.lastShownProgress[k] = true;

                ShowPassage(WinState.EndgameID.DragonSlayer, PassageTracker.dragonslayerBATracker); //Add a hud element to show we got a pip
            }
        }
    }

    //Chieftain
    public static void OnInfluenceLikeOfPlayer(On.CreatureCommunities.orig_InfluenceLikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID commID, int region, int playerNumber, float influence, float interRegionBleed, float interCommunityBleed)
    {
        orig(self, commID, region, playerNumber, influence, interRegionBleed, interCommunityBleed);
        if (commID != CreatureCommunities.CommunityID.Scavengers || PassageTracker.room.world.game.IsArenaSession)
            return;

        WinState winState = PassageTracker.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
        WinState.FloatTracker ft = winState.GetTracker(WinState.EndgameID.Chieftain, true) as WinState.FloatTracker;

        ShowPassage(WinState.EndgameID.Chieftain, ft);
    }

    //Scholar
    public static void OnDataPearlUpdate(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        if (self.grabbedBy.Count > 0)
        {
            if (self.grabbedBy[0].grabber is Player && !self.uniquePearlCountedAsPickedUp)
            {
                bool flag;
                if (ModManager.MSC)
                {
                    flag = (DataPearl.PearlIsNotMisc(self.AbstractPearl.dataPearlType) && self.room.game.session is StoryGameSession && SlugcatStats.PearlsGivePassageProgress(self.room.game.session as StoryGameSession));
                }
                else
                {
                    flag = ((int)self.AbstractPearl.dataPearlType >= 2 && ((self.room.game.session is StoryGameSession && (self.room.game.session as StoryGameSession).saveState.miscWorldSaveData.EverMetMoon && (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark && (self.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.SpeakingTerms) || self.room.game.StoryCharacter == SlugcatStats.Name.Red) && self.room.game.GetStorySession.playerSessionRecords != null);
                }
                if (flag)
                {
                    WinState winState = PassageTracker.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;

                    if(PassageTracker.scholarTracker == null)
                    {
                        WinState.ListTracker listTracker = winState.GetTracker(WinState.EndgameID.Scholar, true) as WinState.ListTracker;
                        PassageTracker.scholarTracker = new WinState.ListTracker(listTracker.ID, listTracker.totItemsToWin);
                        foreach (int i in listTracker.myLastList)
                        {
                            PassageTracker.scholarTracker.myLastList.Add(i);
                        }
                    }
                    
                    foreach(int i in PassageTracker.scholarTracker.myLastList)
                    {
                        if (i == (int)self.AbstractPearl.dataPearlType)
                        {
                            orig(self, eu);
                            return;
                        }
                    }

                    PassageTracker.scholarTracker.myLastList.Add((int)self.AbstractPearl.dataPearlType);

                    ShowPassage(WinState.EndgameID.Scholar, PassageTracker.scholarTracker);
                }
            }
        }
        orig(self, eu);
    }

    //Map
    public static void OnMapUpdate(On.HUD.Map.orig_Update orig, HUD.Map self)
    {
        orig(self);
        if((self.fade > 0f && self.lastFade > 0f))
        {
            PassageTracker.showMapPassageTracker = true;
        }
        else
        {
            PassageTracker.showMapPassageTracker = false;
        }
    }

    public static void ShowPassage(WinState.EndgameID gameID, WinState.EndgameTracker type)
    {
        if ((gameID == WinState.EndgameID.Traveller && PassageTracker.showNotchAmount > 0) || PassageTracker.room.world.game.IsArenaSession)
            return;

        WinState winState = PassageTracker.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
        WinState.IntegerTracker survivorTracker = winState.GetTracker(WinState.EndgameID.Survivor, true) as WinState.IntegerTracker;

        if(gameID != WinState.EndgameID.Chieftain)
        {
            if (gameID != MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad)
            {
                if (winState.GetTracker(gameID, true).GoalFullfilled)
                    return;
            }
            else
            {
                if (winState.GetTracker(gameID, true).GoalAlreadyFullfilled) //.GoalAlreadyFullfilled = completed this cycle
                    return;
            }
        }

        if(gameID == WinState.EndgameID.Outlaw || gameID == WinState.EndgameID.Chieftain)
        {
            if (!survivorTracker.GoalAlreadyFullfilled)
                return;
        }
        else
        {
            if (PassageTrackerOptions.playPassageSound.Value)
            {
                PassageTracker.room.PlaySound(SoundID.HUD_Karma_Reinforce_Bump, 0f, .75f, 1f);
            }
        }

        if(gameID == WinState.EndgameID.Outlaw || gameID == WinState.EndgameID.Chieftain)
        {
            PassageTracker.showFloatAmount = PassageTracker.maxShowBarAmount;
            PassageTracker.floatObject = type;
            HUDFloatMeter.init = true;
            return;
        }

        PassageTracker.showNotchAmount = PassageTracker.maxShowBarAmount;
        PassageTracker.notchObject = type;
        HUDNotchMeter.init = true; 
    }

    public static void FailPassage(WinState.EndgameID gameid)
    {
        WinState winState = PassageTracker.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
        WinState.IntegerTracker survivorTracker = winState.GetTracker(WinState.EndgameID.Survivor, true) as WinState.IntegerTracker;

        WinState.IntegerTracker integerTracker = winState.GetTracker(gameid, true) as WinState.IntegerTracker;

        if (!survivorTracker.GoalAlreadyFullfilled || integerTracker.GoalAlreadyFullfilled || PassageTracker.room.world.game.IsArenaSession)
            return;

        if (PassageTracker.showFailAmount > 0)
            PassageTracker.failIcon.isVisible = false;

        SoundID sound = MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Core_Removed;

        if(PassageTrackerOptions.playFailSound.Value)
        {
            PassageTracker.room.PlaySound(sound, 0f, 1f, 1f);
        }
        
        PassageTracker.failIcon = new FSprite(gameid.ToString() + "A", true);
        PassageTracker.showFailAmount = 200;
    }
}
