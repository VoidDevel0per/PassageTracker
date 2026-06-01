using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using HUD;
using Menu;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;

namespace PassageTrackerMod
{
    internal class HUDNotchMeter : HudPart
    {
        public HUDNotchMeter(HUD.HUD hud, FContainer container) : base(hud)
        {
            this.container = container;
            Vector2 screenSize = hud.rainWorld.options.ScreenSize;
            pos = new Vector2(screenSize.x / 2, screenSize.y / 2);
        }

        public override void Update()
        {

            if (PassageTracker.showWandererAmount > 0)
            {
                PassageTracker.showWandererAmount--;
            }

            if (PassageTracker.showNotchAmount <= 0 && PassageTracker.showFailAmount <= 0)
                return;

            try
            {
                foreach (AbstractCreature abstractCreature in PassageTracker.room.game.Players)
                {
                    if (abstractCreature.realizedCreature is Player player)
                    {
                        pos = player.mainBodyChunk.pos - player.room.world.game.cameras[0].pos + new Vector2(0, 10);
                    }
                }
            }
            catch
            {
                pos = new Vector2(-200, 0); //shhhhhhh don't tell anyone
            }


            if (PassageTracker.showNotchAmount <= 0)
                return;

            PassageTracker.showNotchAmount--;

            if (init)
            {
                if (fsprite != null)
                {
                    foreach (FSprite f in fsprite)
                    {
                        container.RemoveChild(f);
                    }
                    container.RemoveChild(icon);
                    container.RemoveChild(travellerIcon);
                }
                init = false;

                if(PassageTracker.notchObject is WinState.ListTracker)
                { 
                    WinState.ListTracker listTracker = (WinState.ListTracker)PassageTracker.notchObject;
                    fsprite = new FSprite[listTracker.totItemsToWin, 4];
                }
                else if (PassageTracker.notchObject is WinState.BoolArrayTracker)
                {
                    WinState.BoolArrayTracker boolArrayTracker = (WinState.BoolArrayTracker)PassageTracker.notchObject;
                    fsprite = new FSprite[boolArrayTracker.progress.Length, 4];
                }
                else
                {
                    WinState.IntegerTracker integerTracker = (WinState.IntegerTracker)PassageTracker.notchObject;
                    fsprite = new FSprite[integerTracker.max - integerTracker.showFrom, 4];
                }


                for (int i = 0; i < fsprite.GetLength(0); i++)
                {
                    fsprite[i, 0] = new FSprite("JetFishEyeA", true);
                    fsprite[i, 1] = new FSprite("haloGlyph-1", true);
                    fsprite[i, 2] = new FSprite("haloGlyph-1", true);
                    fsprite[i, 3] = new FSprite("Futile_White", true);
                    fsprite[i, 3].shader = PassageTracker.room.game.rainWorld.Shaders["FlatLight"];
                }

                icon = new FSprite(PassageTracker.notchObject.ID + "A", true);

                if (PassageTracker.notchObject.ID == WinState.EndgameID.DragonSlayer)
                {
                    if (ModManager.MSC)
                    {
                        WinState.ListTracker listTracker = (WinState.ListTracker)PassageTracker.notchObject;
                        customColors = new Color[listTracker.totItemsToWin];

                        for (int k = 0; k < customColors.Length; k++)
                        {
                            if (k < listTracker.myLastList.Count)
                            {
                                customColors[k] = (StaticWorld.GetCreatureTemplate(WinState.lizardsOrder[listTracker.myLastList[k]]).breedParameters as LizardBreedParams).standardColor;
                            }
                            else
                            {
                                customColors[k] = Color.black;
                            }
                        }
                    }
                    else
                    {
                        WinState.BoolArrayTracker boolArrayTracker = (WinState.BoolArrayTracker)PassageTracker.notchObject;
                        customColors = new Color[boolArrayTracker.progress.Length];
                        
                        for (int k = 0; k < customColors.Length; k++)
                        {
                            customColors[k] = (StaticWorld.GetCreatureTemplate(WinState.lizardsOrder[k]).breedParameters as LizardBreedParams).standardColor;
                        }
                    }
                }
                else if(PassageTracker.notchObject.ID == WinState.EndgameID.Traveller)
                {
                    WinState.BoolArrayTracker boolArrayTracker = (WinState.BoolArrayTracker)PassageTracker.notchObject;
                    customColors = new Color[boolArrayTracker.progress.Length];
                    for (int m = 0; m < customColors.Length; m++)
                    {
                        customColors[m] = Region.RegionColor(SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot)[m]);
                    }
                }
                else if(PassageTracker.notchObject.ID == MoreSlugcats.MoreSlugcatsEnums.EndgameID.Nomad)
                {
                    WinState.ListTracker listTracker = (WinState.ListTracker)PassageTracker.notchObject;
                    customColors = new Color[listTracker.totItemsToWin];
                    List<string> fullRegionOrder = Region.GetFullRegionOrder();

                    if (listTracker.myList.Count > 0)
                    {
                        for (int num2 = 0; num2 < listTracker.totItemsToWin; num2++)
                        {
                            if (num2 < listTracker.myList.Count)
                            {
                                customColors[num2] = Region.RegionColor(fullRegionOrder[listTracker.myList[num2]]);
                            }
                            else
                            {
                                customColors[num2] = Color.black;
                            }
                        }
                    }                    
                }
                else if(PassageTracker.notchObject.ID == WinState.EndgameID.Scholar)
                {
                    WinState.ListTracker listTracker = (WinState.ListTracker)PassageTracker.notchObject;
                    customColors = new Color[listTracker.totItemsToWin];
                    for (int l = 0; l < listTracker.myLastList.Count; l++)
                    {
                        string entry = ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(listTracker.myLastList[l]);
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
                }
                draw = true;
            }

            if (PassageTracker.showNotchAmount == 0)
            {
                foreach (FSprite f in fsprite)
                {
                    container.RemoveChild(f);
                }
                container.RemoveChild(icon);
                container.RemoveChild(travellerIcon);

                PassageTracker.showWandererAmount = 0;
                draw = false;
            }
        }

        public override void Draw(float timeStacker)
        {
            Vector2 position = pos;
            if(PassageTracker.showFailAmount > 0)
            {
                PassageTracker.showFailAmount--;


                PassageTracker.failIcon.alpha = Mathf.Lerp(0f, 1f, PassageTracker.showFailAmount / 200f);
                PassageTracker.failIcon.color = Color.red;

                PassageTracker.failIcon.SetPosition(position.x - 20f, position.y + 20f);

                if(PassageTracker.showWandererAmount > 0)
                    position.x += 10f;

                container.AddChild(PassageTracker.failIcon);
            }
            if (PassageTracker.showNotchAmount <= 0 || !draw)
                return;

            if (PassageTracker.showWandererAmount > 0)
            {
                if(PassageTracker.notchObject.ID != WinState.EndgameID.Traveller)
                {
                    travellerIcon.SetPosition(position.x - 10f, position.y + 20f);
                    position.x += 10f;
                    container.AddChild(travellerIcon);
                }
                else
                {
                    travellerIcon.SetPosition(position.x, position.y + 20f);
                    container.AddChild(travellerIcon);
                    return;
                }
            }
            
            if (PassageTracker.notchObject is WinState.ListTracker)
            {
                for (int i = 0; i < fsprite.GetLength(0); i++)
                {
                    fsprite[i, 2].color = customColors[i];
                    fsprite[i, 3].color = customColors[i];

                    position.y = pos.y + (i * 10f);

                    fsprite[i, 2].SetPosition(position);
                    fsprite[i, 3].SetPosition(position);

                    container.AddChild(fsprite[i, 2]);
                    container.AddChild(fsprite[i, 3]);
                }
            }
            else if (PassageTracker.notchObject is WinState.BoolArrayTracker boolArrayTracker)
            {
                for (int i = 0; i < fsprite.GetLength(0); i++)
                {
                    if (boolArrayTracker.lastShownProgress[i])
                    {
                        fsprite[i, 2].color = customColors[i];
                        fsprite[i, 3].color = customColors[i];
                    }
                    else
                    {
                        fsprite[i, 2].color = Color.black;
                        fsprite[i, 3].color = Color.black;
                    }

                    position.y = pos.y + (i * 10f);

                    fsprite[i, 2].SetPosition(position);
                    fsprite[i, 3].SetPosition(position);

                    container.AddChild(fsprite[i, 2]);
                    container.AddChild(fsprite[i, 3]);
                }
            }
            else //IntegerTracker
            {
                Debug.Log("PassageTracker: Notch IntegerTracker UNUSED!!!");
            }

            position.y = pos.y + ((fsprite.GetLength(0) + 1) * 10f);
            
            
            icon.SetPosition(position);
            container.AddChild(icon);
        }


        public static bool init = true;
        public FContainer container;
        public FSprite[,] fsprite;
        public FSprite icon;
        public Color[] customColors;

        private FSprite travellerIcon = new FSprite("TravellerA", true);
        private Vector2 pos;
        public bool draw = false;
    }
}