using System.Collections.Generic;
using HUD;
using UnityEngine;

namespace PassageTrackerMod
{
    internal class MapPassageTracker : HudPart
    {
        private FContainer mptContainer; 
        private Vector2 screenSize;
        private Vector2 multiplier;
        private bool spritesBuilt = false;
        
        private List<FSprite> allSprites = new List<FSprite>();

        public MapPassageTracker(HUD.HUD hud, FContainer container) : base(hud)
        {

            mptContainer = new FContainer();
            container.AddChild(mptContainer);

            if (ModManager.MSC)
                multiplier = new Vector2(0.75f, 0.98f);
            else
                multiplier = new Vector2(0.80f, 0.98f);

            screenSize = hud.rainWorld.screenSize;
        }

        public override void Update()
        {

            mptContainer.isVisible = PassageTracker.showMapPassageTracker;


            if (PassageTracker.showMapPassageTracker && !spritesBuilt)
            {
                BuildSprites();
            }
            
            if (!PassageTracker.showMapPassageTracker && spritesBuilt)
            {
                ClearSprites();
            }
        }

        public override void ClearSprites()
        {
            mptContainer.RemoveAllChildren();
            allSprites.Clear();
            spritesBuilt = false;
        }

        private void BuildSprites()
        {
            ClearSprites(); // just in case qmq

            WinState winState = PassageTracker.room.world.game.rainWorld.progression
                .currentSaveState.deathPersistentSaveData.winState;
            WinState.IntegerTracker survivorTracker =
                winState.GetTracker(WinState.EndgameID.Survivor, true) as WinState.IntegerTracker;

            WinState.EndgameID endgameID = WinState.EndgameID.Survivor;
            bool notch = true;

            for (int trackerNumber = 0; trackerNumber < 14; trackerNumber++)
            {
                WinState.EndgameTracker endgameTracker = winState.GetTracker(endgameID, true);
                
                if (endgameID == WinState.EndgameID.DragonSlayer)
                {
                    if (ModManager.MSC && PassageTracker.dragonslayerTracker != null)
                        endgameTracker = PassageTracker.dragonslayerTracker;
                    else if (!ModManager.MSC && PassageTracker.dragonslayerBATracker != null)
                        endgameTracker = PassageTracker.dragonslayerBATracker;
                }
                else if (endgameID == WinState.EndgameID.Scholar && PassageTracker.scholarTracker != null)
                {
                    endgameTracker = PassageTracker.scholarTracker;
                }

                Vector2 position;
                position.x = (screenSize.x * multiplier.x) + 11f + (22f * trackerNumber);
                position.y = (screenSize.y * multiplier.y) - 13f;

                // Icon
                FSprite icon = new FSprite(endgameID + "A", true);
                icon.SetPosition(position);
                if (endgameTracker.GoalFullfilled)
                    icon.color = Color.yellow;
                mptContainer.AddChild(icon);
                allSprites.Add(icon);

                position.y -= 10f;

                if (notch)
                {
                    Color[] customColors = BuildCustomColors(endgameID, endgameTracker, survivorTracker);
                    int slotCount = customColors.Length;

                    for (int i = 0; i < slotCount; i++)
                    {
                        position.y -= 10f;

                        FSprite bg = new FSprite("haloGlyph-1", true);
                        bg.scaleX = 2f; bg.scaleY = 2f; bg.color = Color.white;
                        bg.SetPosition(position);
                        mptContainer.AddChild(bg);
                        allSprites.Add(bg);

                        FSprite fill = new FSprite("haloGlyph-1", true);
                        fill.color = customColors[i];
                        fill.SetPosition(position);
                        mptContainer.AddChild(fill);
                        allSprites.Add(fill);

                        FSprite glow = new FSprite("Futile_White", true);
                        glow.shader = PassageTracker.room.game.rainWorld.Shaders["FlatLight"];
                        glow.color = customColors[i];
                        glow.SetPosition(position);
                        mptContainer.AddChild(glow);
                        allSprites.Add(glow);
                    }
                }
                else
                {
                    BuildMeterSprites(endgameID, endgameTracker, position);
                }

                notch = false;
                AdvanceEndgameID(trackerNumber, ref endgameID, ref notch);
            }

            spritesBuilt = true;
        }

        private void BuildMeterSprites(WinState.EndgameID endgameID, WinState.EndgameTracker endgameTracker, Vector2 tipPos)
        {
            Vector2 meterTip = tipPos - new Vector2(0, 5f);
            Vector2 meterStart = meterTip - new Vector2(0, 50f);

            float lastFill = 0f, fill = 0f;
            Color meterColor = Color.white;

            try
            {
                ComputeMeterFill(endgameID, endgameTracker, out lastFill, out fill, out meterColor);
            }
            catch { }

            FSprite meterbg = new FSprite("pixel", true)
            {
                scaleX = 2f,
                scaleY = Vector2.Distance(meterStart, meterTip),
                anchorY = 0f,
                color = Color.black,
                x = meterStart.x,
                y = meterStart.y
            };
            mptContainer.AddChild(meterbg);
            allSprites.Add(meterbg);

            float bgScaleY = Vector2.Distance(meterStart, meterTip) * lastFill;
            float fgScaleY = Mathf.Max(0f, Vector2.Distance(meterStart, meterTip) * fill - bgScaleY);

            FSprite meter2 = new FSprite("pixel", true)
            {
                scaleX = 2f, anchorY = 0f,
                scaleY = bgScaleY,
                color = Color.grey,
                x = meterStart.x, y = meterStart.y
            };
            mptContainer.AddChild(meter2);
            allSprites.Add(meter2);

            FSprite meter = new FSprite("pixel", true)
            {
                scaleX = 2f, anchorY = 0f,
                scaleY = fgScaleY,
                color = meterColor,
                x = meterStart.x, y = meterStart.y + bgScaleY
            };
            mptContainer.AddChild(meter);
            allSprites.Add(meter);
        }

        private void ComputeMeterFill(WinState.EndgameID endgameID, WinState.EndgameTracker endgameTracker,
            out float lastFill, out float fill, out Color color)
        {
            lastFill = 0f; fill = 0f; color = Color.green;
            WinState winState = PassageTracker.room.world.game.rainWorld.progression
                .currentSaveState.deathPersistentSaveData.winState;

            if (endgameID == WinState.EndgameID.Outlaw)
            {
                var t = endgameTracker as WinState.IntegerTracker;
                int cur = PassageTracker.killsThisCycle + t.progress;
                lastFill = Mathf.InverseLerp(t.showFrom, t.max, t.progress);
                fill = Mathf.InverseLerp(t.showFrom, t.max, cur);
                color = cur >= t.progress ? Color.green : Color.red;
            }
            else if (endgameID == WinState.EndgameID.Chieftain)
            {
                var t = endgameTracker as WinState.FloatTracker;
                float cur = PassageTracker.room.world.game.session.creatureCommunities
                    .LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0);
                cur = PassageTracker.room.game.StoryCharacter == SlugcatStats.Name.Yellow
                    ? Mathf.InverseLerp(0.42f, 0.9f, cur)
                    : Mathf.InverseLerp(0.1f, 0.8f, cur);
                cur = Mathf.Floor(cur * 20f) / 20f;
                lastFill = Mathf.InverseLerp(t.showFrom, t.max, t.progress);
                fill = Mathf.InverseLerp(t.showFrom, t.max, cur);
                color = cur >= t.progress ? Color.green : Color.red;
            }
            else if (endgameID == WinState.EndgameID.Monk)
            {
                var t = endgameTracker as WinState.IntegerTracker;
                bool failed = PassageTracker.failedMonk && !endgameTracker.GoalFullfilled;
                lastFill = Mathf.InverseLerp(t.showFrom, t.max, t.progress);
                fill = failed ? 0f : Mathf.InverseLerp(t.showFrom, t.max, t.progress + 2);
                color = failed ? Color.red : Color.green;
            }
            else if (endgameID == WinState.EndgameID.Hunter)
            {
                var t = endgameTracker as WinState.IntegerTracker;
                bool failed = PassageTracker.failedHunter && !endgameTracker.GoalFullfilled;
                lastFill = Mathf.InverseLerp(t.showFrom, t.max, t.progress);
                fill = failed ? 0f : Mathf.InverseLerp(t.showFrom, t.max, t.progress + 2);
                color = failed ? Color.red : Color.green;
            }
            else if (endgameID == WinState.EndgameID.Saint)
            {
                var t = endgameTracker as WinState.IntegerTracker;
                bool failed = PassageTracker.failedSaint && !endgameTracker.GoalFullfilled;
                lastFill = Mathf.InverseLerp(t.showFrom, t.max, t.progress);
                fill = failed ? 0f : Mathf.InverseLerp(t.showFrom, t.max, t.progress + 2);
                color = failed ? Color.red : Color.green;
            }
            else if (endgameID == WinState.EndgameID.Friend)
            {
                var t = endgameTracker as WinState.FloatTracker;
                lastFill = Mathf.InverseLerp(t.showFrom, t.max, t.progress);
                fill = 0f; color = Color.white;
            }
            else if (endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Martyr ||
                     endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Mother)
            {
                var t = endgameTracker as WinState.FloatTracker;
                fill = Mathf.InverseLerp(t.showFrom, t.max, t.progress);
                color = Color.white;
            }
        }

        private Color[] BuildCustomColors(WinState.EndgameID endgameID,
            WinState.EndgameTracker endgameTracker, WinState.IntegerTracker survivorTracker)
        {
            try
            {
                if (endgameID == WinState.EndgameID.Survivor)
                {
                    var t = endgameTracker as WinState.IntegerTracker;
                    Color[] c = new Color[t.max - t.showFrom];
                    for (int i = 0; i < c.Length; i++)
                        c[i] = i < survivorTracker.progress ? Color.white : Color.black;
                    return c;
                }
                if (endgameID == WinState.EndgameID.DragonSlayer)
                {
                    if (ModManager.MSC)
                    {
                        var t = endgameTracker as WinState.ListTracker;
                        Color[] c = new Color[t.totItemsToWin];
                        for (int k = 0; k < c.Length; k++)
                            c[k] = k < t.myLastList.Count
                                ? (StaticWorld.GetCreatureTemplate(WinState.lizardsOrder[t.myLastList[k]]).breedParameters as LizardBreedParams).standardColor
                                : Color.black;
                        return c;
                    }
                    else
                    {
                        var t = endgameTracker as WinState.BoolArrayTracker;
                        Color[] c = new Color[t.progress.Length];
                        for (int k = 0; k < c.Length; k++)
                            c[k] = t.progress[k]
                                ? (StaticWorld.GetCreatureTemplate(WinState.lizardsOrder[k]).breedParameters as LizardBreedParams).standardColor
                                : Color.black;
                        return c;
                    }
                }
                if (endgameID == WinState.EndgameID.Traveller)
                {
                    var t = endgameTracker as WinState.BoolArrayTracker;
                    Color[] c = new Color[t.progress.Length];
                    var regions = SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot);
                    for (int m = 0; m < c.Length; m++)
                        c[m] = t.progress[m] ? Region.RegionColor(regions[m]) : Color.black;
                    return c;
                }
                if (endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad)
                {
                    var t = endgameTracker as WinState.ListTracker;
                    Color[] c = new Color[t.totItemsToWin];
                    var fullOrder = Region.GetFullRegionOrder();
                    for (int n = 0; n < c.Length; n++)
                        c[n] = n < t.myList.Count ? Region.RegionColor(fullOrder[t.myList[n]]) : Color.black;
                    return c;
                }
                if (endgameID == WinState.EndgameID.Scholar)
                {
                    var t = endgameTracker as WinState.ListTracker;
                    Color[] c = new Color[t.totItemsToWin];
                    for (int l = 0; l < t.myLastList.Count; l++)
                    {
                        string entry = ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(t.myLastList[l]);
                        Color col = DataPearl.UniquePearlMainColor(DataPearl.AbstractDataPearl.DataPearlType.Misc);
                        Color? col2 = null;
                        if (entry != null)
                        {
                            var pt = new DataPearl.AbstractDataPearl.DataPearlType(entry, false);
                            col = DataPearl.UniquePearlMainColor(pt);
                            col2 = DataPearl.UniquePearlHighLightColor(pt);
                        }
                        c[l] = RWCustom.Custom.Saturate(
                            col2 != null ? Color.Lerp(col, col2.Value, 0.4f) : col, 0.2f);
                    }
                    for (int i = t.myLastList.Count; i < t.totItemsToWin; i++)
                        c[i] = Color.black;
                    return c;
                }
                if (endgameID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Pilgrim)
                {
                    var t = endgameTracker as WinState.BoolArrayTracker;
                    Color[] c = new Color[t.progress.Length];
                    var regions = SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot);
                    for (int n = 0; n < regions.Count; n++)
                        c[n] = t.progress[n] && World.CheckForRegionGhost(RainWorld.lastActiveSaveSlot, regions[n])
                            ? Region.RegionColor(regions[n]) : Color.black;
                    return c;
                }
            }
            catch { }
            
            return new Color[] { Color.black };
        }

        private void AdvanceEndgameID(int trackerNumber, ref WinState.EndgameID endgameID, ref bool notch)
        {
            notch = false;
            switch (trackerNumber)
            {
                case 0: endgameID = WinState.EndgameID.Monk; break;
                case 1: endgameID = WinState.EndgameID.Hunter; break;
                case 2: endgameID = WinState.EndgameID.Saint; break;
                case 3: endgameID = WinState.EndgameID.Outlaw; break;
                case 4: endgameID = WinState.EndgameID.Chieftain; break;
                case 5: endgameID = WinState.EndgameID.Traveller; notch = true; break;
                case 6: endgameID = WinState.EndgameID.DragonSlayer; notch = true; break;
                case 7: endgameID = WinState.EndgameID.Friend; break;
                case 8: endgameID = WinState.EndgameID.Scholar; notch = true; break;
                case 9: endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Martyr; break;
                case 10: endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad; notch = true; break;
                case 11: endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Pilgrim; notch = true; break;
                case 12: endgameID = MoreSlugcats.MoreSlugcatsEnums.EndgameID.Mother; break;
            }
        }

        public override void Draw(float timeStacker) { }
    }
}