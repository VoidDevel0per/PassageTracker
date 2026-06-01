using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using HUD;
using Menu;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


namespace PassageTrackerMod
{
    internal class MapPassageTracker : HudPart
    {
        public MapPassageTracker(HUD.HUD hud, FContainer container) : base(hud)
        {
            this.container = container;
            //screenSize = hud.rainWorld.options.ScreenSize;

            if(ModManager.MSC)
            {
                multiplier = new Vector2(0.75f, 0.98f);
            }
            else
            {
                multiplier = new Vector2(0.80f, 0.98f);
            }
            screenSize = hud.rainWorld.screenSize;
        }

        public override void Update()
        {
            if (!PassageTracker.showMapPassageTracker)
            {
                return;
            }

            //WinState.EndgameID.Survivor;
            //WinState.EndgameID.Monk; //Survivor
            //WinState.EndgameID.Hunter; //Survivor
            //WinState.EndgameID.Saint; //Survivor
            //WinState.EndgameID.Outlaw; //Survivor
            //WinState.EndgameID.Chieftain; //Survivor* (Scavenger rep viewed seperately)
            //WinState.EndgameID.Traveller; //Passage progress w/o survivor
            //WinState.EndgameID.DragonSlayer; //Passage progress w/o survivor
            //WinState.EndgameID.Friend; //Passage progress w/o survivor
            //WinState.EndgameID.Scholar; //The Mark + Moon or other thingies

            //MoreSlugcats.MoreSlugcatsEnums.EndgameID.Martyr; //Passage progress w/o survivor
            //MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad; //Survivor
            //MoreSlugcats.MoreSlugcatsEnums.EndgameID.Pilgrim; //Passage progress w/o survivor
            //MoreSlugcats.MoreSlugcatsEnums.EndgameID.Mother; //Passage progress w/o survivor
        }

        public override void Draw(float timeStacker)
        {
            if (!PassageTracker.showMapPassageTracker)
            {
                try
                {
                    if (!init)
                    {
                        //container.RemoveAllChildren();
                        int count = 0;
                        while(count < container.GetChildCount())
                        {
                            FNode child = container.GetChildAt(count);
                            if (child is FSprite fsprite)
                            {
                                if (fsprite.element.name == elementName && fsprite.y > 200f)
                                {
                                    container.RemoveChild(fsprite); count--;                                    
                                }
                            }
                            count++;
                        }                        
                    }
                }
                catch(Exception e)
                {
                }
                init = true;

                return;
            }

            //container.AddChild(ceil);
            //container.AddChild(floor);
            //container.AddChild(lWall);
            //container.AddChild(rWall);

            #region passagesContained

            if (init)
            {
                init = false;

                WinState winState = PassageTracker.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
                WinState.EndgameID endgameID = WinState.EndgameID.Survivor;
                WinState.EndgameTracker endgameTracker;
                WinState.IntegerTracker survivorTracker;
                bool notch = true;
                survivorTracker = winState.GetTracker(endgameID, true) as WinState.IntegerTracker;

                for (int trackerNumber = 0; trackerNumber < 14; trackerNumber++) //<14 before
                {
                    endgameTracker = winState.GetTracker(endgameID, true);
                    if(endgameID == WinState.EndgameID.DragonSlayer)
                    {
                        if (ModManager.MSC)
                        {
                            if(PassageTracker.dragonslayerTracker != null)
                            {
                                endgameTracker = PassageTracker.dragonslayerTracker;
                            }
                        }
                        else
                        {
                            if (PassageTracker.dragonslayerBATracker != null)
                            {
                                endgameTracker = PassageTracker.dragonslayerBATracker;
                            }
                        }
                    }
                    else if (endgameID == WinState.EndgameID.Scholar)
                    {
                        if (PassageTracker.scholarTracker != null)
                        {
                            endgameTracker = PassageTracker.scholarTracker;
                        }
                    }
                    icon = new FSprite(endgameID + "A", true);
                    icon.element.name = elementName;

                    if (endgameTracker.GoalFullfilled)
                    {
                        icon.color = Color.yellow;
                    }

                    if (notch)
                    {
                        if (endgameTracker is WinState.ListTracker listTracker)
                        {
                            notchSprites = new FSprite[listTracker.totItemsToWin, 4];
                            customColors = new Color[listTracker.totItemsToWin];
                        }
                        else if (endgameTracker is WinState.BoolArrayTracker boolArrayTracker)
                        {
                            notchSprites = new FSprite[boolArrayTracker.progress.Length, 4];
                            customColors = new Color[boolArrayTracker.progress.Length];
                        }
                        else
                        {
                            WinState.IntegerTracker integerTracker = (WinState.IntegerTracker)endgameTracker;
                            notchSprites = new FSprite[integerTracker.max - integerTracker.showFrom, 4];
                            customColors = new Color[integerTracker.max - integerTracker.showFrom];
                        }

                        for (int i = 0; i < notchSprites.GetLength(0); i++)
                        {
                            notchSprites[i, 0] = new FSprite("JetFishEyeA", true);
                            notchSprites[i, 1] = new FSprite("haloGlyph-1", true);                             
                            notchSprites[i, 2] = new FSprite("haloGlyph-1", true);
                            notchSprites[i, 3] = new FSprite("Futile_White", true);
                            notchSprites[i, 3].shader = PassageTracker.room.game.rainWorld.Shaders["FlatLight"];

                            for(int j = 0; j < 4; j++)
                            {
                                notchSprites[i, j].element.name = elementName;
                            }
                        }

                        try
                        {
                            if (endgameID == WinState.EndgameID.DragonSlayer)
                            {
                                if (ModManager.MSC)
                                {
                                    WinState.ListTracker dragonSlayerTracker = (WinState.ListTracker)endgameTracker;
                                    customColors = new Color[dragonSlayerTracker.totItemsToWin];

                                    for (int k = 0; k < customColors.Length; k++)
                                    {
                                        if (k < dragonSlayerTracker.myLastList.Count)
                                        {
                                            customColors[k] = (StaticWorld.GetCreatureTemplate(WinState.lizardsOrder[dragonSlayerTracker.myLastList[k]]).breedParameters as LizardBreedParams).standardColor;
                                        }
                                        else
                                        {
                                            customColors[k] = Color.black;
                                        }
                                    }
                                }
                                else
                                {
                                    WinState.BoolArrayTracker boolArrayTracker = (WinState.BoolArrayTracker)endgameTracker;
                                    customColors = new Color[boolArrayTracker.progress.Length];

                                    for (int k = 0; k < customColors.Length; k++)
                                    {
                                        if (boolArrayTracker.progress[k])
                                        {
                                            customColors[k] = (StaticWorld.GetCreatureTemplate(WinState.lizardsOrder[k]).breedParameters as LizardBreedParams).standardColor;
                                        }
                                        else
                                        {
                                            customColors[k] = Color.black;
                                        }
                                    }
                                }
                            }
                            else if (endgameID == WinState.EndgameID.Traveller)
                            {
                                WinState.BoolArrayTracker travellerTracker = (WinState.BoolArrayTracker)endgameTracker;
                                customColors = new Color[travellerTracker.progress.Length];
                                for (int m = 0; m < customColors.Length; m++)
                                {
                                    if (travellerTracker.progress[m])
                                    {
                                        customColors[m] = Region.RegionColor(SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot)[m]);
                                    }
                                    else
                                    {
                                        customColors[m] = Color.black;
                                    }
                                }
                            }
                            else if (endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad)
                            {
                                WinState.ListTracker nomadTracker = (WinState.ListTracker)endgameTracker;
                                customColors = new Color[nomadTracker.totItemsToWin];
                                List<string> fullRegionOrder = Region.GetFullRegionOrder();

                                for (int num2 = 0; num2 < nomadTracker.totItemsToWin; num2++)
                                {
                                    if (num2 < nomadTracker.myList.Count)
                                    {
                                        customColors[num2] = Region.RegionColor(fullRegionOrder[nomadTracker.myList[num2]]);
                                    }
                                    else
                                    {
                                        customColors[num2] = Color.black;
                                    }
                                }
                                
                            }
                            else if (endgameID == WinState.EndgameID.Scholar)
                            {
                                WinState.ListTracker scholarTracker = (WinState.ListTracker)endgameTracker;
                                customColors = new Color[scholarTracker.totItemsToWin];
                                for (int l = 0; l < scholarTracker.myLastList.Count; l++)
                                {
                                    string entry = ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(scholarTracker.myLastList[l]);
                                    Color color = DataPearl.UniquePearlMainColor(DataPearl.AbstractDataPearl.DataPearlType.Misc);
                                    Color? color2 = null;
                                    if (entry != null)
                                    {
                                        DataPearl.AbstractDataPearl.DataPearlType pearlType = new DataPearl.AbstractDataPearl.DataPearlType(entry, false);
                                        color = DataPearl.UniquePearlMainColor(pearlType);
                                        color2 = DataPearl.UniquePearlHighLightColor(pearlType);
                                    }
                                    if (color2 != null)
                                    {
                                        customColors[l] = Color.Lerp(color, color2.Value, 0.4f);
                                    }
                                    else
                                    {
                                        customColors[l] = color; 
                                    }
                                    customColors[l] = RWCustom.Custom.Saturate(customColors[l], 0.2f);
                                }
                                for(int i = scholarTracker.myLastList.Count; i < scholarTracker.totItemsToWin; i++)
                                {
                                    customColors[i] = Color.black;
                                }
                            }
                            else if (endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Pilgrim)
                            {
                                WinState.BoolArrayTracker boolArrayTracker = endgameTracker as WinState.BoolArrayTracker;
                                customColors = new Color[boolArrayTracker.progress.Length];
                                List<string> slugcatStoryRegions = SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot);
                                for (int n = 0; n < slugcatStoryRegions.Count; n++)
                                {
                                    if (boolArrayTracker.progress[n])
                                    {
                                        if (World.CheckForRegionGhost(RainWorld.lastActiveSaveSlot, slugcatStoryRegions[n]))
                                        {
                                            customColors[n] = Region.RegionColor(slugcatStoryRegions[n]);
                                        }
                                    }
                                    else
                                    {
                                        customColors[n] = Color.black;
                                    }
                                }
                            } 
                            else if (endgameID == WinState.EndgameID.Survivor)
                            {
                                int i = 0;
                                for (; i < survivorTracker.progress; i++)
                                {
                                    customColors[i] = Color.white;
                                }
                                for (; i < survivorTracker.max; i++)
                                {
                                    customColors[i] = Color.black;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    else
                    {                        

                    }

                    Vector2 position;

                    position.x = (screenSize.x * multiplier.x) + 11f + (22f * trackerNumber);
                    position.y = (screenSize.y * multiplier.y) - 13f;

                    icon.SetPosition(position);
                    container.AddChild(icon);

                    position.y -= 10f;

                    if (notch) 
                    { 
                        for (int i = 0; i < notchSprites.GetLength(0); i++)
                        {
                            position.y -= 10f;

                            notchSprites[i, 1].scaleX = 2f;
                            notchSprites[i, 1].scaleY = 2f;
                            notchSprites[i, 1].color = Color.white;
                            notchSprites[i, 2].color = customColors[i];
                            notchSprites[i, 3].color = customColors[i];

                            notchSprites[i, 1].SetPosition(position);
                            notchSprites[i, 2].SetPosition(position);
                            notchSprites[i, 3].SetPosition(position);

                            container.AddChild(notchSprites[i, 1]);
                            container.AddChild(notchSprites[i, 2]);
                            container.AddChild(notchSprites[i, 3]);
                        }
                    }
                    else
                    {
                        position.y -= 5f;
                        meterTip = position;
                        meterStart = meterTip - new Vector2(0, 50f);
                            
                        meter = new FSprite("pixel", true)
                        {
                            scaleX = 2f,
                            anchorY = 0f,
                        };
                        meter.element.name = elementName;

                        meter2 = new FSprite("pixel", true)
                        {
                            scaleX = 2f,
                            anchorY = 0f,
                        };
                        meter2.element.name = elementName;

                        meterbg = new FSprite("pixel", true)
                        {
                            scaleX = 2f,
                            scaleY = Vector2.Distance(meterStart, meterTip) * 1f,
                            anchorY = 0f,
                            color = Color.black,
                        };
                        meterbg.element.name = elementName;

                        if (endgameID == WinState.EndgameID.Outlaw)
                        {
                            WinState.IntegerTracker integerTracker = endgameTracker as WinState.IntegerTracker;
                            int currentProgress = PassageTracker.killsThisCycle + integerTracker.progress;

                            lastFill = Mathf.InverseLerp(integerTracker.showFrom, integerTracker.max, integerTracker.progress);
                            fill = Mathf.InverseLerp(integerTracker.showFrom, integerTracker.max, currentProgress);

                            try
                            {
                                if (currentProgress > integerTracker.progress)
                                {
                                    meter.color = Color.green;
                                    meter2.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                    meter.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                                }
                                else if (currentProgress < integerTracker.progress)
                                {
                                    meter.color = Color.red;
                                    meter.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                    meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                                }
                                else
                                {
                                    meter.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                    meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                                }

                                meter.scaleY -= meter2.scaleY;
                            }
                            catch (Exception e)
                            {
                            }
                        }
                        else if (endgameID == WinState.EndgameID.Chieftain)
                        {
                            try
                            {
                                WinState.FloatTracker chieftainTracker = endgameTracker as WinState.FloatTracker;

                                float currentLike = PassageTracker.room.world.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0);

                                //If num4 isn't inside InverseLerp, take closest (lower -> 0.42, higher -> 0.9)
                                if (PassageTracker.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
                                {
                                    currentLike = Mathf.InverseLerp(0.42f, 0.9f, currentLike);
                                }
                                else
                                {
                                    currentLike = Mathf.InverseLerp(0.1f, 0.8f, currentLike);
                                }
                                currentLike = Mathf.Floor(currentLike * 20f) / 20f;


                                lastFill = Mathf.InverseLerp(chieftainTracker.showFrom, chieftainTracker.max, chieftainTracker.progress);
                                fill = Mathf.InverseLerp(chieftainTracker.showFrom, chieftainTracker.max, currentLike);

                                try
                                {
                                    if (currentLike > chieftainTracker.progress)
                                    {
                                        meter.color = Color.green;
                                        meter2.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                        meter.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                                    }
                                    else if (currentLike < chieftainTracker.progress)
                                    {
                                        meter.color = Color.red;
                                        meter.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                        meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                                    }
                                    else
                                    {
                                        meter.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                        meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                                    }

                                    meter.scaleY -= meter2.scaleY;
                                }
                                catch (Exception e)
                                {
                                }
                            }
                            catch (Exception e)
                            {
                            }
                        }
                        else if (endgameID == WinState.EndgameID.Monk)
                        {
                            WinState.IntegerTracker monkTracker = endgameTracker as WinState.IntegerTracker;

                            if ((PassageTracker.failedMonk || !(!PassageTracker.ateAnything && !(monkTracker.progress > 0))) && !endgameTracker.GoalFullfilled)
                            {
                                lastFill = Mathf.InverseLerp(monkTracker.showFrom, monkTracker.max, monkTracker.progress);
                                fill = Mathf.InverseLerp(monkTracker.showFrom, monkTracker.max, 0);

                                meter.color = Color.red;
                                meter.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            }
                            else
                            {
                                lastFill = Mathf.InverseLerp(monkTracker.showFrom, monkTracker.max, monkTracker.progress);
                                fill = Mathf.InverseLerp(monkTracker.showFrom, monkTracker.max, monkTracker.progress + 2);

                                meter.color = Color.green;
                                meter2.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                meter.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            }

                            meter.scaleY -= meter2.scaleY;
                        }
                        else if (endgameID == WinState.EndgameID.Hunter)
                        {
                            WinState.IntegerTracker hunterTracker = endgameTracker as WinState.IntegerTracker;

                            if ((PassageTracker.failedHunter || !(!PassageTracker.ateAnything && !(hunterTracker.progress > 0))) && !endgameTracker.GoalFullfilled)
                            {
                                lastFill = Mathf.InverseLerp(hunterTracker.showFrom, hunterTracker.max, hunterTracker.progress);
                                fill = Mathf.InverseLerp(hunterTracker.showFrom, hunterTracker.max, 0);

                                meter.color = Color.red;
                                meter.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            }
                            else
                            {
                                lastFill = Mathf.InverseLerp(hunterTracker.showFrom, hunterTracker.max, hunterTracker.progress);
                                fill = Mathf.InverseLerp(hunterTracker.showFrom, hunterTracker.max, hunterTracker.progress + 2);

                                meter.color = Color.green;
                                meter2.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                meter.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            }

                            meter.scaleY -= meter2.scaleY;
                        }
                        else if (endgameID == WinState.EndgameID.Saint)
                        {
                            WinState.IntegerTracker saintTracker = endgameTracker as WinState.IntegerTracker;

                            if (PassageTracker.failedSaint && !endgameTracker.GoalFullfilled)
                            {
                                lastFill = Mathf.InverseLerp(saintTracker.showFrom, saintTracker.max, saintTracker.progress);
                                fill = Mathf.InverseLerp(saintTracker.showFrom, saintTracker.max, 0);

                                meter.color = Color.red;
                                meter.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            }
                            else
                            {
                                lastFill = Mathf.InverseLerp(saintTracker.showFrom, saintTracker.max, saintTracker.progress);
                                fill = Mathf.InverseLerp(saintTracker.showFrom, saintTracker.max, saintTracker.progress + 2);

                                meter.color = Color.green;
                                meter2.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                                meter.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            }

                            meter.scaleY -= meter2.scaleY;
                        }
                        else if (endgameID == WinState.EndgameID.Friend)
                        {
                            WinState.FloatTracker friendTracker = endgameTracker as WinState.FloatTracker;

                            lastFill = Mathf.InverseLerp(friendTracker.showFrom, friendTracker.max, friendTracker.progress);
                            meter2.scaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
                            meter.scaleY = Vector2.Distance(meterStart, meterTip) * 0;

                            meter.scaleY -= meter2.scaleY;
                        }
                        else if (endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Martyr)
                        {
                            WinState.FloatTracker martyrTracker = endgameTracker as WinState.FloatTracker;

                            fill = Mathf.InverseLerp(martyrTracker.showFrom, martyrTracker.max, martyrTracker.progress);
                            meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            meter.scaleY = Vector2.Distance(meterStart, meterTip) * 0;

                            meter.scaleY -= meter2.scaleY;
                        }
                        else if (endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Mother)
                        {
                            WinState.FloatTracker motherTracker = endgameTracker as WinState.FloatTracker;

                            fill = Mathf.InverseLerp(motherTracker.showFrom, motherTracker.max, motherTracker.progress);
                            meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                            meter.scaleY = Vector2.Distance(meterStart, meterTip) * 0;

                            meter.scaleY -= meter2.scaleY;
                        }

                        meter.x = meterStart.x;
                        meter.y = meterStart.y + meter2.scaleY;

                        meter2.x = meterStart.x;
                        meter2.y = meterStart.y;

                        meterbg.x = meterStart.x;
                        meterbg.y = meterStart.y;

                        container.AddChild(meterbg);
                        container.AddChild(meter2);
                        container.AddChild(meter);
                    }

                    notch = false;

                    switch (trackerNumber)
                    {
                        //WinState.EndgameID.Survivor; //Always [Notch]
                        case 0:
                            endgameID = WinState.EndgameID.Monk; //Survivor [FLOAT]
                            break;
                        case 1:
                            endgameID = WinState.EndgameID.Hunter; //Survivor [FLOAT]
                            break;
                        case 2:
                            endgameID = WinState.EndgameID.Saint; //Survivor [FLOAT]
                            break;
                        case 3:
                            endgameID = WinState.EndgameID.Outlaw; //Survivor [FLOAT]
                            break;
                        case 4:
                            endgameID = WinState.EndgameID.Chieftain; //Survivor* (Scavenger rep viewed seperately) [FLOAT]
                            break;
                        case 5:
                            endgameID = WinState.EndgameID.Traveller; //Passage progress w/o survivor [Notch]
                            notch = true;
                            break;
                        case 6:
                            endgameID = WinState.EndgameID.DragonSlayer; //Passage progress w/o survivor [Notch]
                            notch = true;
                            break;
                        case 7:
                            endgameID = WinState.EndgameID.Friend; //Passage progress w/o survivor [Float]
                            break;
                        case 8:
                            endgameID = WinState.EndgameID.Scholar; //The Mark + Moon or other thingies [Notch]
                            notch = true;
                            break;
                        case 9:
                            endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Martyr; //Passage progress w/o survivor [Float]
                            break;
                        case 10:
                            endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad; //Survivor [Notch]
                            notch = true;
                            break;
                        case 11:
                            endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Pilgrim; //Passage progress w/o survivor [Notch]
                            notch = true;
                            break;
                        case 12:
                            endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Mother; //Passage progress w/o survivor [FLOAT]
                            break;
                        case 13:

                            break;
                        case 14:

                            break;
                        default:
                            break;
                    }
                }

               
            }
           
            #endregion passagesContained
        }

        public string elementName = "PassageTracker";

        public FSprite meter, meter2, meterbg;
        public Vector2 meterStart;
        public Vector2 meterTip;
        public float lastFill, fill = 0f;

        public bool init = true;
        public FSprite icon;
        public FSprite[,] notchSprites;
        public FContainer container;
        public Vector2 screenSize;// = new Vector2(1920, 1080);
        public Vector2 multiplier = new Vector2(0.75f, 0.98f);
        public Color[] customColors;
    }
}