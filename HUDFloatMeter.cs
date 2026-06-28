using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
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
    internal class HUDFloatMeter : HudPart
    {
        public HUDFloatMeter(HUD.HUD hud, FContainer container) : base(hud)
        {
            this.container = container;
        }

        public override void Update()
        {
            if (PassageTracker.showFloatAmount <= 0)
                return;

            PassageTracker.showFloatAmount--;

            Vector2 offset = new();

            if (PassageTracker.showNotchAmount > 0)
            {
                offset += new Vector2(-20f, -30f);
            }

            try
            {
                foreach (AbstractCreature abstractCreature in PassageTracker.room.game.Players)
                {
                    if (abstractCreature.realizedCreature is Player player)
                    {
                        meterStart = offset + player.mainBodyChunk.pos - player.room.world.game.cameras[0].pos + new Vector2(0, 30f);
                        meterTip = meterStart + new Vector2(0, 50f);
                    }
                }
            }
            catch
            {
                meterStart = new Vector2(-200, 0); // shhhhhhh don't tell anyone
                meterTip = meterStart;
            }

            if (init)
            {
                init = false;


                if (meter != null)
                {
                    if (PassageTracker.floatObject.ID == WinState.EndgameID.Outlaw)
                    {
                        WinState.IntegerTracker integerTracker = PassageTracker.floatObject as WinState.IntegerTracker;
                        int currentProgress = PassageTracker.killsThisCycle + integerTracker.progress;

                        if (currentProgress < 2)
                            return;
                    }
                    container.RemoveChild(icon);
                    container.RemoveChild(meter);
                    container.RemoveChild(meter2);
                    container.RemoveChild(meterbg);
                }

                icon = new FSprite(PassageTracker.floatObject.ID + "A", true);

                meter = new FSprite("pixel", true)
                {
                    scaleX = 2f,
                    anchorY = 0f
                };
                meter2 = new FSprite("pixel", true)
                {
                    scaleX = 2f,
                    anchorY = 0f
                };
                meterbg = new FSprite("pixel", true)
                {
                    scaleX = 2f,
                    scaleY = Vector2.Distance(meterStart, meterTip) * 1f,
                    anchorY = 0f,
                    color = Color.black
                };

                if (PassageTracker.floatObject.ID == WinState.EndgameID.Outlaw)
                {
                    WinState.IntegerTracker integerTracker = PassageTracker.floatObject as WinState.IntegerTracker;
                    int currentProgress = PassageTracker.killsThisCycle + integerTracker.progress;

                    if (currentProgress < 2)
                    {
                        PassageTracker.showFloatAmount = 0;
                        return;
                    }

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
                            meter2.scaleY = Vector2.Distance(meterStart, meterTip) * fill;
                        }

                        meter.scaleY -= meter2.scaleY;
                    }
                    catch (Exception e)
                    {
                    }
                }
                else if (PassageTracker.floatObject.ID == WinState.EndgameID.Chieftain)
                {
                    try
                    {
                        WinState.FloatTracker chieftainTracker = PassageTracker.floatObject as WinState.FloatTracker;

                        WinState winState = PassageTracker.room.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.winState;
                        WinState.IntegerTracker survivorTracker = winState.GetTracker(WinState.EndgameID.Survivor, true) as WinState.IntegerTracker;

                        if (!survivorTracker.GoalAlreadyFullfilled)
                            return;

                        float currentLike = PassageTracker.room.world.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0);

                        // If num4 isn't inside InverseLerp, take closest (lower -> 0.42, higher -> 0.9)
                        if (PassageTracker.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
                        {
                            currentLike = Mathf.InverseLerp(0.42f, 0.9f, currentLike); // between 0.42 and 0.9 : num4
                        }
                        else
                        {
                            currentLike = Mathf.InverseLerp(0.1f, 0.8f, currentLike); // between 0.1 and 0.8 : num4
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
                draw = true;
            }

            if (PassageTracker.showFloatAmount == 0)
            {
                container.RemoveChild(icon);
                container.RemoveChild(meter);
                container.RemoveChild(meter2);
                container.RemoveChild(meterbg);
                draw = false;
            }
        }

        public override void Draw(float timeStacker)
        {
            if (PassageTracker.showFloatAmount <= 0 || !draw)
                return;

            /*
            if(PassageTracker.floatObject.ID == WinState.EndgameID.Outlaw)
            {
                WinState.IntegerTracker integerTracker = PassageTracker.floatObject as WinState.IntegerTracker;
                int currentProgress = PassageTracker.killsThisCycle + integerTracker.progress;

                if (currentProgress != PassageTracker.currentOutlawProgress)
                {
                    init = true;
                }
            }
            */


            icon.SetPosition(meterTip + new Vector2(0, 20f));

            meter.x = meterStart.x;
            meter.y = meterStart.y + meter2.scaleY;

            meter2.x = meterStart.x;
            meter2.y = meterStart.y;

            meterbg.x = meterStart.x;
            meterbg.y = meterStart.y;

            container.AddChild(icon);
            container.AddChild(meterbg);
            container.AddChild(meter2);
            container.AddChild(meter);
        }

        public FSprite meter, meter2, meterbg;
        public Vector2 meterStart = new(0f, 0f);
        public Vector2 meterTip = new(0f, 20f);
        public float lastFill, fill = 0f;
        public static bool init = true;
        public FContainer container;
        public FSprite icon;
        public bool draw = false;
    }
}