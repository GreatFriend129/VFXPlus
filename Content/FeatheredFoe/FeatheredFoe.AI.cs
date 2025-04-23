using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils.Cil;
using rail;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using VFXPlus.Common;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPLus.Common;

namespace VFXPlus.Content.FeatheredFoe
{
    public partial class FeatheredFoe : ModNPC
    {
        Vector2 basicAttackPoint = Vector2.Zero;
        public void BasicAttack()
        {
            //Move to point
            //Stop
            //Shoot 
            //Repeat
            
            if (substate == 0)
            {
                if (timer == 0)
                {
                    basicAttackPoint = Main.rand.NextVector2CircularEdge(550, 250);
                }

                //BasicMovementVariant1(player.Center + basicAttackPoint, 0.06f, 22, 2, 0.1f, 60);
                
                //BasicMovementVariant2(player.Center + basicAttackPoint);
                
                BasicMovementVariant3(player.Center + basicAttackPoint, 3f, 270f);

                if (timer == 100)
                {
                    timer = -1;
                    substate++;
                }
            }
            else if (substate == 1)
            {
                //NPC.velocity = Vector2.Zero;

                //BasicMovementVariant1(player.Center + basicAttackPoint); // A bit jittery, follows point not 100%, tends to overshoot then rebound (sometimes too much)

                //BasicMovementVariant2(player.Center + basicAttackPoint); A bit static to reach dest, but very quick


                BasicMovementVariant3(player.Center + basicAttackPoint, 3f, 270f);


                if (timer == 60 || timer == 185)
                {
                    Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    for (int iaa = -3; iaa < 4; iaa++)
                    {
                        Vector2 vel = toPlayer.RotatedBy(iaa * MathHelper.PiOver4 * 1f) * 15f;

                        float curvePower = iaa * 0.02f; //0.04

                        int curveFeather = Projectile.NewProjectile(null, NPC.Center, vel, ModContent.ProjectileType<CurvingFeather>(), 2, 0, Main.myPlayer);

                        if (Main.projectile[curveFeather].ModProjectile is CurvingFeather cf)
                        {
                            cf.curveValue = curvePower;
                            cf.accelTime = 36;
                        }

                        Main.projectile[curveFeather].hostile = true;
                        Main.projectile[curveFeather].extraUpdates = 0; //2

                    }
                }

                if (timer == 300) //100
                {
                    substate = 1;
                    timer = -1;
                }
            }

            Dust.NewDustPerfect(player.Center + basicAttackPoint, ModContent.DustType<StillDust>(), Velocity: Vector2.Zero, newColor: Color.Black, Scale: 2f);
        }

        public void SwoopFeatherBehind()
        {

        }


        bool drawTriSpinStar = false; //For drawing triStar
        float justShotTristarPower = 0f; //^
        float triSpinStarAngle = 0f; //^
        int triSpinDirection = 1; //Which way does the angle spin
        int triSpinSide = 1; //Which xSide should FF go to
        public void TriSpin()
        {
            //To hopefully fix a weird NaN case that I can't seem to replicate anymore
            if (NPC.Center.HasNaNs())
            {
                Main.NewText("NPC.Center has NaNs! | " + Main.timeForVisualEffects);
                return;
            }
            if (player.Center.HasNaNs())
            {
                Main.NewText("player.Center has NaNs! | " + Main.timeForVisualEffects);
                return;
            }

            Vector2 dir = new Vector2(300f * triSpinSide, -100);

            float timeBeforeShot = 60;
            float timeForShot = 95;


            //Once done with shooting
            if (timer >= timeBeforeShot + timeForShot)
            {
                windOverlayOpacityGoal = 0f;
                doPassiveWindParticles = false;

                NPC.velocity.X *= 0.95f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + dir) * (NPC.Distance(player.Center + dir) / 15), 0.2f); //high lerpval gives less overshoot
            }
            //Shooting
            else if (timer >= timeBeforeShot)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + dir) * (NPC.Distance(player.Center + dir) / 150), 0.05f);
                NPC.velocity *= 0.92f;


                float angleRange = MathHelper.TwoPi * 0.3f;

                float progress = (float)(timer - timeBeforeShot) / timeForShot;
                float shotAngle = MathHelper.Lerp(-angleRange / 2f * triSpinDirection, angleRange / 2f * triSpinDirection, progress);
                if (timer % 5 == 0)
                {
                    Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

                    Vector2 vel1 = toPlayer.RotatedBy(shotAngle) * 10f;
                    Vector2 vel2 = toPlayer.RotatedBy(shotAngle + MathHelper.TwoPi * 0.33f) * 10f;
                    Vector2 vel3 = toPlayer.RotatedBy(shotAngle + MathHelper.TwoPi * 0.66f) * 10f;

                    triSpinStarAngle = vel1.ToRotation();

                    Projectile proj1 = Projectile.NewProjectileDirect(null, NPC.Center, vel1, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);
                    Projectile proj2 = Projectile.NewProjectileDirect(null, NPC.Center, vel2, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);
                    Projectile proj3 = Projectile.NewProjectileDirect(null, NPC.Center, vel3, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);

                    for (int m = 0; m < 3; m++)
                    {
                        Vector2 vel;
                        if (m == 0) 
                            vel = vel1;
                        else if (m == 1)
                            vel = vel2;
                        else
                            vel = vel3;

                        for (int i = 0; i < 2 + Main.rand.Next(0, 3); i++)
                        {
                            Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                            Dust dust = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<GlowPixelCross>(), vel * 0.5f + randomStart, newColor: new Color(30, 90, 255) * 1f, Scale: Main.rand.NextFloat(0.35f, 0.65f));

                            dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                                rotPower: 0.15f, preSlowPower: 0.95f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.88f, shouldFadeColor: false);
                        }
                    }

                    justShotTristarPower = 1f;
                }

                windOverlayOpacityGoal = 0.1f;
                windOverlayRotation = (player.Center - NPC.Center).ToRotation();

                doPassiveWindParticles = true;
                passiveWindParticleDirection = (player.Center - NPC.Center).ToRotation();

                triSpinStarAngle = (player.Center - NPC.Center).ToRotation() + shotAngle;
                drawTriSpinStar = true;
            }
            //Before shooting 
            else
            {
                float prog = (float)timer / (float)timeBeforeShot;

                dir.Y += MathHelper.Lerp(-333f, 0f, Easings.easeOutQuint(prog));

                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + dir) * (NPC.Distance(player.Center + dir) / 18), 0.7f);
            }

            if (timer == 180 - 25)
            {
                NPC.rotation = 0;
                timer = -1;

                triSpinDirection = Main.rand.NextBool() ? 1 : -1;
                triSpinSide *= -1;
            }

            justShotTristarPower = Math.Clamp(MathHelper.Lerp(justShotTristarPower, -0.5f, 0.1f), 0f, 100f);
        }

        public void MartletOrbitFeather()
        {
            
        }

        public void CircleBurstFeather()
        {
            NPC.velocity = Vector2.Zero;

            if (timer >= 60)
            {
                float shotRot = (player.Center - NPC.Center).ToRotation();

                for (int k = 0; k < 9; k++)
                {
                    if (timer == 60 + k * 5)
                    {
                        float rot = (k - 4) / 10f; 

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int a = Projectile.NewProjectile(null, NPC.Center, new Vector2(12f, 0).RotatedBy(shotRot + rot), ModContent.ProjectileType<StopAndStartFeather>(), 1, 0);
                            //(Main.projectile[a].ModProjectile as CurvingFeather).curveValue = rot * -0.04f;

                            int b = Projectile.NewProjectile(null, NPC.Center, new Vector2(12f, 0).RotatedBy(shotRot - rot), ModContent.ProjectileType<StopAndStartFeather>(), 1, 0);
                            //(Main.projectile[b].ModProjectile as CurvingFeather).curveValue = rot * 0.04f;

                        }
                    }
                }
            }

            if (timer == 180)
                timer = -1;

        }

        public void SwirlFeather()
        {

        }

        bool isX = true;
        float cornerTravelGoalRot = 0f;
        public void CornerTravelShot()
        {
            //Start at top left of star
            Vector2 startPoint = new Vector2(350f, 0f).RotatedBy(cornerTravelGoalRot);

            Vector2 goal = player.Center + startPoint;

            BasicMovementVariant3(goal, 8f, 570f);


            if (timer > 20 && timer < 70)
            {
                windOverlayOpacityGoal = 0.15f;
                windOverlayRotation = startPoint.ToRotation() + MathHelper.Pi;

                doPassiveWindParticles = true;
                passiveWindParticleDirection = startPoint.ToRotation() + MathHelper.Pi;

                if (timer % 6 == 0)
                {
                    Vector2 vel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.75f) * 10f; //15

                    Projectile spinShot = Projectile.NewProjectileDirect(null, NPC.Center, vel, ModContent.ProjectileType<SpinShotFeather>(), 10, 5);

                    (spinShot.ModProjectile as SpinShotFeather).targetPlayer = player.whoAmI;

                    SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_66") with { Pitch = .60f, MaxInstances = 1, Volume = 0.5f, PitchVariance = 0.2f };
                    SoundEngine.PlaySound(style2, NPC.Center);

                    for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++)
                    {
                        Vector2 randomStart = Main.rand.NextVector2Circular(2f, 2f) * 1f;
                        Dust dust = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<GlowPixelCross>(), vel * 0.5f + randomStart, newColor: new Color(40, 125, 255), Scale: Main.rand.NextFloat(0.35f, 0.65f));
                        //dust.velocity += Projectile.velocity * 0.25f;

                        dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                            rotPower: 0.15f, preSlowPower: 0.95f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.88f, shouldFadeColor: false);
                    }
                }
            }
            else
            {
                doPassiveWindParticles = true;
                windOverlayOpacityGoal = 0f;
            }

            if (timer == 100)
            {
                if (isX)
                    cornerTravelGoalRot = player.velocity.Y > 0f ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                else
                    cornerTravelGoalRot = player.velocity.X > 0f ? 0f : MathHelper.Pi;
                isX = !isX;

                substate++;
                timer = -1;
            }

            if (NPC.velocity.Length() > 12f)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);

                Color innerCol = Color.Lerp(Color.LightSkyBlue, Color.Tan, 0.5f);


                Dust p = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), ModContent.DustType<WindLine>(), vel + NPC.velocity * 0.5f,
                    newColor: Color.SkyBlue, Scale: 0.75f);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.75f, 0.9f, 0.35f, true);
                //wlb.randomVelRotatePower = 0.2f;
                wlb.drawWhiteCore = false;
                p.customData = wlb;
            }

        }

        public void CornerTravelAggressive()
        {
            //Have him choose whether to go left or right based on ur x vel and whether he goes up or down based on y vel


            float incrementAmount = MathHelper.ToRadians(360f / 4f);

            //Start at top left of star
            Vector2 startPoint = new Vector2(0f, -350f).RotatedBy(MathHelper.ToRadians(0));

            Vector2 goal = player.Center + startPoint.RotatedBy(incrementAmount * substate * 1f);

            BasicMovementVariant3(goal, 8f, 570f);

            if (timer > 20 && timer < 70 && timer % 6 == 0)
            {
                Vector2 vel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.25f) * 15f;//25 // Main.rand.NextVector2CircularEdge(8f, 8f);// + player.velocity;

                Projectile spinShot = Projectile.NewProjectileDirect(null, NPC.Center, vel, ModContent.ProjectileType<SpinShotFeather>(), 10, 5);

                (spinShot.ModProjectile as SpinShotFeather).targetPlayer = player.whoAmI;

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_66") with { Pitch = .60f, MaxInstances = 1, Volume = 0.5f, PitchVariance = 0.2f };
                SoundEngine.PlaySound(style2, NPC.Center);

                for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++)
                {
                    Vector2 randomStart = Main.rand.NextVector2Circular(2f, 2f) * 1f;
                    Dust dust = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<GlowPixelCross>(), vel * 0.5f + randomStart, newColor: new Color(40, 125, 255), Scale: Main.rand.NextFloat(0.35f, 0.65f));
                    //dust.velocity += Projectile.velocity * 0.25f;

                    dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.15f, preSlowPower: 0.95f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.88f, shouldFadeColor: false);
                }
            }

            if (timer == 100)
            {
                substate++;
                timer = -1;
            }

            if (NPC.velocity.Length() > 12f)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);

                Color innerCol = Color.Lerp(Color.LightSkyBlue, Color.Tan, 0.5f);


                Dust p = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), ModContent.DustType<WindLine>(), vel + NPC.velocity * 0.5f,
                    newColor: Color.SkyBlue, Scale: 0.75f);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.75f, 0.9f, 0.35f, true);
                //wlb.randomVelRotatePower = 0.2f;
                wlb.drawWhiteCore = false;
                p.customData = wlb;
            }

        }

        Vector2 storedOffscreenDashEndPos = Vector2.Zero;
        Vector2 storedOffscreenDashStartPos = Vector2.Zero;
        float offscreenDashDir = 0f;
        public void OffscreenDash() 
        {
            float offscreenDashDistance = 1000f;
            int timeBeforeDash = 79; //79
            int timeOfDash = 50;
            int timeAfterDash = 5; //30

            if (timer == 0)
            {
                NPC.velocity = Vector2.Zero;
                offscreenDashDir = Main.rand.NextFloat(6.28f);
            }

            if (timer <= timeBeforeDash)
            {
                Vector2 dashStartPos = offscreenDashDir.ToRotationVector2() * offscreenDashDistance;
                NPC.Center = player.Center + dashStartPos;

                if (timer == timeBeforeDash)
                {
                    storedOffscreenDashStartPos = NPC.Center;
                    storedOffscreenDashEndPos = player.Center - dashStartPos;
                }

                float beforeDashProgress = (float)timer / (float)timeBeforeDash;


                windOverlayOpacityGoal = MathHelper.Lerp(0f, 0.75f, Easings.easeInCirc(beforeDashProgress));
                windOverlayRotation = (-dashStartPos).ToRotation();

                doPassiveWindParticles = true;

                centerPassiveWindParticlesOnPlayer = true;
                passiveWindParticleDirection = (-dashStartPos).ToRotation();
                passiveWindParticlesCount = 1;
            }
            else if (timer <= timeOfDash + timeBeforeDash)
            {
                if (timer == timeBeforeDash + 1)
                {
                    int pulse = Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<WindPulse>(), 0, 0, player.whoAmI);
                    (Main.projectile[pulse].ModProjectile as WindPulse).timeForPulse = 40;
                    (Main.projectile[pulse].ModProjectile as WindPulse).intensity = 0.75f;
                    Main.projectile[pulse].scale = 10f;

                    SoundStyle styleD = new SoundStyle("VFXPlus/Sounds/Effects/Cries/astrolotl") with { Volume = 0.11f, Pitch = 0.7f, PitchVariance = 0.05f, MaxInstances = 1 };
                    SoundEngine.PlaySound(styleD, NPC.Center);

                    SoundStyle styleC = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/flame_thrower_airblast_rocket_redirect") with { Volume = 0.09f, Pitch = .5f, PitchVariance = .1f, MaxInstances = -1 };
                    SoundEngine.PlaySound(styleC, NPC.Center);

                    player.GetModPlayer<ScreenShakePlayer>().ScreenShakePower = 25f;

                    drawDrill = true;
                    drillRotation = offscreenDashDir + MathHelper.PiOver2;
                }

                float dashProgress = (float)(timer - timeBeforeDash) / (float)timeOfDash;
                float easedProgress = Easings.easeInOutHarsh(dashProgress);

                NPC.Center = Vector2.Lerp(storedOffscreenDashStartPos, storedOffscreenDashEndPos, easedProgress);

                //Spawn feathers
                if (easedProgress > 0.20f && easedProgress < 0.7f && timer % 3 == 0)
                {
                    Vector2 vel = offscreenDashDir.ToRotationVector2() * -12f;

                    Projectile spinShot = Projectile.NewProjectileDirect(null, NPC.Center, vel, ModContent.ProjectileType<SpinShotFeather>(), 10, 5);

                    (spinShot.ModProjectile as SpinShotFeather).targetPlayer = player.whoAmI;
                }

                //Spawn Dust
                if (easedProgress > 0f && easedProgress < 0.85f)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Main.rand.NextBool())
                        {
                            Vector2 yOffset = new Vector2(0f, 1f).RotatedBy(offscreenDashDir) * Main.rand.NextFloat(-65f, 65f);

                            Vector2 fakeNPCVel = offscreenDashDir.ToRotationVector2() * -20f * Main.rand.NextFloat(0.75f, 1.25f);

                            Color dustCol = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, Main.rand.NextFloat(0.25f, 0.75f));

                            Dust p = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(20f, 20f) + yOffset, ModContent.DustType<WindLine>(), fakeNPCVel,
                                newColor: dustCol, Scale: Main.rand.NextFloat(0.75f, 1f));

                            WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.75f, 1f, 0.5f, true);
                            wlb.drawWhiteCore = true;
                            p.customData = wlb;
                        }
                    }
                }

                //Dash Trail
                #region DashTrail
                int trailCount = 30;
                dashTrailPositions.Add(NPC.Center);
                dashTrailRotations.Add(offscreenDashDir);
                if (dashTrailPositions.Count > trailCount)
                {
                    dashTrailPositions.RemoveAt(0);
                    dashTrailRotations.RemoveAt(0);
                }

                //Calculate next position
                float dashProgressNext = (float)(timer + 1 - timeBeforeDash) / (float)timeOfDash;
                float easedProgressNext = Easings.easeInOutHarsh(dashProgressNext);

                Vector2 nextPos = Vector2.Lerp(storedOffscreenDashStartPos, storedOffscreenDashEndPos, easedProgressNext);

                dashTrailPositions.Add(Vector2.Lerp(NPC.Center, nextPos, 0.5f));
                dashTrailRotations.Add(offscreenDashDir);
                if (dashTrailPositions.Count > trailCount)
                {
                    dashTrailPositions.RemoveAt(0);
                    dashTrailRotations.RemoveAt(0);
                }
                #endregion



                if (timer >= (0.5f * timeOfDash) + timeBeforeDash)
                    windOverlayOpacityGoal = 0f;

                passiveWindParticlesCount = 2;
            }
            else if (timer <= timeAfterDash + timeOfDash + timeBeforeDash)
            {
                drawDrill = false;
                doPassiveWindParticles = false;
                passiveWindParticlesCount = 1;
                
                windOverlayOpacityGoal = 0f;
                windOverlayOpacity *= 0.8f;

                dashTrailPositions.Clear();
                dashTrailRotations.Clear();

                //Reset
                if (timer == timeAfterDash + timeOfDash + timeBeforeDash)
                {
                    timer = -1;
                }
            }

            //Main.NewText(timer + "|" + (timeAfterDash + timeOfDash + timeBeforeDash));
        }

        int diveStartSide = 0; //Which side FF is on when it starts to move above player
        Vector2 diveStartPoint;
        public void Dive()
        {
            //Move to point above player, move up then dive down
            if (substate == 0)
            {
                if (timer == 0)
                {
                    diveStartSide = NPC.Center.X > player.Center.X ? 1 : -1;
                    diveStartPoint = new Vector2(Main.rand.NextFloat(-25f, 25f), Main.rand.Next(-50, 50) + -100f); //-300
                }

                Vector2 goalPos = player.Center + diveStartPoint;

                int timeToStartDive = 95; //85
                float timeForMovement = 25; //30

                //Pre Dive
                if (timer < timeToStartDive)
                {
                    float timeForOffsetShrink = 80;
                    float offsetProgress = Math.Clamp((float)timer / timeForOffsetShrink, 0f, 1f);
                    Vector2 goalOffset = Vector2.Lerp(new Vector2(1050f * diveStartSide, 0f), new Vector2(0f + player.velocity.X * 14f, -200f), Easings.easeOutQuad(offsetProgress)); //vel * 6f

                    BasicMovementVariant3(goalPos + goalOffset, moveSpeed: 4f); //3.5

                }
                //Dive
                else if (timer >= timeToStartDive)
                {
                    Vector2 initialVel = new Vector2(NPC.velocity.X, -20f); //20
                    Vector2 goalVel = new Vector2(NPC.velocity.X * 0.7f, 30f); //23
                    
                    float timeProgress = Math.Clamp((float)((timer - timeToStartDive) / timeForMovement), 0f, 1f);
                    NPC.velocity = Vector2.Lerp(initialVel, goalVel, Easings.easeInOutQuad(timeProgress));// inoutquad

                    bool shouldShootFeahters = NPC.velocity.Y > 7; 

                    if (shouldShootFeahters && timer % 5 == 0)
                    {
                        int a = Projectile.NewProjectile(null, NPC.Center, new Vector2(5.5f, 0.75f) * 2f, ModContent.ProjectileType<StopAndStartFeather>(), 1, 1);
                        int b = Projectile.NewProjectile(null, NPC.Center, new Vector2(-5.5f, 0.75f) * 2f, ModContent.ProjectileType<StopAndStartFeather>(), 1, 1);

                        (Main.projectile[a].ModProjectile as StopAndStartFeather).velShrinkTime = 31;// 35;
                        (Main.projectile[a].ModProjectile as StopAndStartFeather).velGrowTime = 54;// 60;

                        (Main.projectile[b].ModProjectile as StopAndStartFeather).velShrinkTime = 31;// 35;
                        (Main.projectile[b].ModProjectile as StopAndStartFeather).velGrowTime = 54;// 60;


                        for (int i = 0; i < 6; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                            int side = i % 2 == 0 ? 1 : -1;

                            Dust p = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<WindLine>(), vel + new Vector2(side * 5.5f, 0.75f) * 1.15f,
                                newColor: Color.DeepSkyBlue, Scale: 0.5f);

                            WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, 1f, 0.5f, true);
                            wlb.randomVelRotatePower = 0.2f;

                            p.customData = wlb;
                        }
                    }

                    if (shouldShootFeahters && Main.rand.NextBool(2))
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                        Dust p = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), ModContent.DustType<WindLine>(), vel + NPC.velocity * 0.75f,
                            newColor: Color.DeepSkyBlue, Scale: 0.65f);

                        WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.96f, TimeToStartShrink: 15, ShrinkYScalePower: 0.8f, 1f, 0.4f, true);
                        p.customData = wlb;
                    }

                    if (shouldShootFeahters)
                    {
                        windOverlayOpacityGoal = 0.5f;
                        windOverlayRotation = MathHelper.PiOver2;
                    }

                    bool shouldStartDrill = NPC.velocity.Y > 14;
                    if (shouldStartDrill && !drawDrill)
                    {
                        drawDrill = true;
                        drillRotation = 0f;

                        int pulse = Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<WindPulse>(), 0, 0, player.whoAmI);
                        (Main.projectile[pulse].ModProjectile as WindPulse).timeForPulse = 40;
                        (Main.projectile[pulse].ModProjectile as WindPulse).intensity = 0.4f;
                        Main.projectile[pulse].scale = 9f;

                        SoundStyle styleD = new SoundStyle("VFXPlus/Sounds/Effects/Cries/astrolotl") with { Volume = 0.08f, Pitch = 0.7f, PitchVariance = 0.05f, MaxInstances = 1 };
                        SoundEngine.PlaySound(styleD, NPC.Center);

                        SoundStyle styleC = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/flame_thrower_airblast_rocket_redirect") with { Volume = 0.07f, Pitch = .5f, PitchVariance = .1f, MaxInstances = -1 };
                        SoundEngine.PlaySound(styleC, NPC.Center);

                        player.GetModPlayer<ScreenShakePlayer>().ScreenShakePower = 15f;
                    }
                    
                    //DashTrail
                    if (shouldStartDrill)
                    {
                        int trailCount = 20;
                        dashTrailPositions.Add(NPC.Center);
                        dashTrailRotations.Add(NPC.velocity.ToRotation());

                        if (dashTrailPositions.Count > trailCount)
                        {
                            dashTrailPositions.RemoveAt(0);
                            dashTrailRotations.RemoveAt(0);
                        }
                    }

                    doPassiveWindParticles = true;
                    passiveWindParticleDirection = MathHelper.PiOver2;
                }

                //Reseting
                if (timer == timeToStartDive + timeForMovement + 30) //35
                {
                    drawDrill = false;

                    doPassiveWindParticles = false;
                    windOverlayOpacityGoal = 0f;
                    timer = -1;

                    dashTrailPositions.Clear();
                    dashTrailRotations.Clear();
                }

            }


        }

        Tuple<int, int> pastDirections = new Tuple<int, int>(0, 0); //Holds the past two directions of the attack (-1 or 1)
        Vector2 umbrellaCenterPoint = Vector2.Zero;
        Projectile umbrellaStormwallInstance = null; 
        Projectile umbrellaTelegraphProj = null;
        public void UmbrellaRain()
        {
            //TODO: Cull feathers and dust that will spawn very far offscreen
            //---------------------------------------------------------------

            float dashAwayDistance = 400f;
            float safeZoneWidth = 95f; //Dont change unless you also change Telegraph proj

            //Choose point and spawn setup projectiles
            if (timer == 0)
            {
                int randomDirection = (Main.rand.NextBool() ? 1 : -1);

                //If we have gone the same direction twice in a row, go the other direction
                if (pastDirections.Item1 == pastDirections.Item2 && pastDirections.Item1 == -1)
                    randomDirection = 1;
                else if (pastDirections.Item1 == pastDirections.Item2 && pastDirections.Item1 == 1)
                    randomDirection = -1;

                //Update the past directions list
                pastDirections = new Tuple<int, int>(randomDirection, pastDirections.Item1);

                //Choose the new point
                Vector2 newPoint = new Vector2(dashAwayDistance * randomDirection, -230f);

                //If this is the first repetition, have the point be right above player instead, and spawn in the stormwall projectile (clouds at top of screen)
                if (attackReps == 0)
                {
                    newPoint.X *= 0;

                    int stormwallID = Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<Stormwall>(), 0, 0);
                    (Main.projectile[stormwallID].ModProjectile as Stormwall).targetPlayer = player.whoAmI;
                    umbrellaStormwallInstance = Main.projectile[stormwallID];
                }

                umbrellaCenterPoint = new Vector2(NPC.Center.X, player.Center.Y) + newPoint;

                //Spawn the telegraph projectile
                int telegraphID = Projectile.NewProjectile(null, umbrellaCenterPoint, Vector2.Zero, ModContent.ProjectileType<UmbrellaTelegraph>(), 0, 0);
                (Main.projectile[telegraphID].ModProjectile as UmbrellaTelegraph).targetPlayer = player.whoAmI;
                umbrellaTelegraphProj = Main.projectile[telegraphID];

            }


            int timeBeforeRain = 80; //70
            int timeOfRain = 50; //50
            int timeAfterRain = 25; //100

            //Have NPC move to the center point
            if (timer < timeBeforeRain)
            {
                float prog = Math.Clamp((float)timer / (float)(timeBeforeRain / 1.35f), 0f, 1f);

                float xOvershoot = 100f * Easings.easeInSine(1f - prog) * pastDirections.Item1;
                float yOvershoot = -100f * Easings.easeInSine(1f - prog);

                float xLerp = MathHelper.Lerp(NPC.Center.X, umbrellaCenterPoint.X + xOvershoot, Easings.easeInQuad(prog));
                float yLerp = MathHelper.Lerp(NPC.Center.Y, player.Center.Y - 250f + yOvershoot, 0.06f + (-0.04f * prog));

                NPC.Center = new Vector2(xLerp, yLerp);
            }
            else if (timer >= timeBeforeRain)
            {
                //Sound and VFX shit
                if (timer == timeBeforeRain)
                {
                    bgPulsePower = 2.5f;

                    windOverlayOpacityGoal = 1f;
                    windOverlayRotation = MathHelper.PiOver2;

                    float overallVolume = 0.33f;

                    //Sound
                    SoundStyle styleA = new SoundStyle("VFXPlus/Sounds/Effects/water_blast_projectile_spell_03") with { Volume = 0.5f * overallVolume, Pitch = .7f, PitchVariance = 0.05f, MaxInstances = -1 };
                    SoundEngine.PlaySound(styleA, NPC.Center);

                    SoundStyle styleC = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/flame_thrower_airblast_rocket_redirect") with { Volume = 0.15f * overallVolume, Pitch = .4f, PitchVariance = .1f, MaxInstances = -1 };
                    SoundEngine.PlaySound(styleC, NPC.Center);

                    SoundStyle styleD = new SoundStyle("VFXPlus/Sounds/Effects/Cries/astrolotl") with { Volume = 1f * overallVolume, Pitch = .6f, PitchVariance = 0.05f, MaxInstances = 1 };
                    SoundEngine.PlaySound(styleD, NPC.Center);

                    int pulse = Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<WindPulse>(), 0, 0, player.whoAmI);
                    (Main.projectile[pulse].ModProjectile as WindPulse).timeForPulse = 60;
                    (Main.projectile[pulse].ModProjectile as WindPulse).intensity = 0.8f;
                    Main.projectile[pulse].scale = 9f;

                    randomShakePower = 2f;

                    //NPC.Center = umbrellaCenterPoint;
                }

                float yLerp = MathHelper.Lerp(NPC.Center.Y, player.Center.Y - 250, 0.005f);

                NPC.Center = new Vector2(umbrellaCenterPoint.X, yLerp);

                //Continually shake screen for a while
                if (timer < timeBeforeRain + (timeOfRain / 1.5f))
                    player.GetModPlayer<ScreenShakePlayer>().ScreenShakePower = 15f;


                //Spawning feathers and dust
                if (timer < timeBeforeRain + timeOfRain)
                {
                    //Push the player downward if they are above FF
                    if (player.Center.Y < NPC.Center.Y)
                    {
                        if (player.velocity.Y < 0)
                            player.velocity.Y *= -1;

                        player.velocity.Y += 0.75f;
                    }

                    int smokePerFrame = 10 * 2;
                    int windDustPerFrame = 5 * 2;
                    int windLinePerFrame = 1 * 2;
                    int feathersPerBurst = 5 * 2;

                    float burstWidth = 1000f * 2f;

                    //Feathers
                    if (timer % 6 == 0)
                    {
                        for (int i = 0; i < feathersPerBurst; i++)
                        {
                            float prog = (float)i / (float)feathersPerBurst;

                            float x = MathHelper.Lerp(-burstWidth, -safeZoneWidth, prog);

                            Vector2 spawnPosL = new Vector2(x + Main.rand.NextFloat(-135f, 135f), -400f + Main.rand.NextFloat(-50f, 50f));
                            Vector2 spawnPosR = new Vector2((-1f * x) + Main.rand.NextFloat(-135f, 135f), -400f + Main.rand.NextFloat(-50f, 50f));

                            //Left Side
                            int pl = Projectile.NewProjectile(null, NPC.Center + spawnPosL, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                            (Main.projectile[pl].ModProjectile as StraightFeather).accelTime = 0;
                            (Main.projectile[pl].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                            Main.projectile[pl].extraUpdates = 3;

                            //Right Side
                            int pr = Projectile.NewProjectile(null, NPC.Center + spawnPosR, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                            (Main.projectile[pr].ModProjectile as StraightFeather).accelTime = 0;
                            (Main.projectile[pr].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                            Main.projectile[pr].extraUpdates = 3;
                        }
                    }

                    //Spawn 2 feathers on the edge of the border every 14 frames
                    if (timer % 14 == 0)
                    {
                        //Dont spawn them exactly on the border size (95f) because the feathers have a wide hitbox
                        Vector2 spawnPosL = NPC.Center + new Vector2(safeZoneWidth + 15f, -400f + Main.rand.NextFloat(-25f, 25f));
                        Vector2 spawnPosR = NPC.Center + new Vector2(-safeZoneWidth - 15f, -400f + Main.rand.NextFloat(-25f, 25f));

                        int p1 = Projectile.NewProjectile(null, spawnPosL, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                        (Main.projectile[p1].ModProjectile as StraightFeather).accelTime = 0;
                        (Main.projectile[p1].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                        Main.projectile[p1].extraUpdates = 3;

                        int p2 = Projectile.NewProjectile(null, spawnPosR, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                        (Main.projectile[p2].ModProjectile as StraightFeather).accelTime = 0;
                        (Main.projectile[p2].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                        Main.projectile[p2].extraUpdates = 3;
                    }


                    #region Rain Particles
                    //Smoke Dust
                    for (int i = 0; i < smokePerFrame; i++)
                    {
                        float rot = MathHelper.PiOver2;
                        int side = Main.rand.NextBool() ? 1 : -1;

                        Vector2 smokeDustSpawnPosition = NPC.Center + (new Vector2(1f * -200f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloat(safeZoneWidth, burstWidth) * side)).RotatedBy(rot);
                        Vector2 smokeDustVelocity = new Vector2(1f, 0f).RotatedBy(rot) * Main.rand.NextFloat(8f, 12f) * 1.5f;

                        Dust smoke = Dust.NewDustPerfect(smokeDustSpawnPosition, ModContent.DustType<SmallSmoke>(), smokeDustVelocity,
                            newColor: Color.LightSkyBlue * 1f, Scale: Main.rand.NextFloat(7f, 9f));

                        SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 0.45f, ScaleFadePower: 0.97f, false);
                        smoke.customData = ssb;
                    }

                    //Wind Dust
                    for (int i2 = 0; i2 < windDustPerFrame; i2++)
                    {
                        float rot = MathHelper.PiOver2;
                        int side = Main.rand.NextBool() ? 1 : -1;

                        Vector2 windDustSpawnPosition = NPC.Center + (new Vector2(1f * -200f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloat(safeZoneWidth, burstWidth) * side)).RotatedBy(rot);
                        Vector2 windDustVelocity = new Vector2(1f, 0f).RotatedBy(rot) * Main.rand.NextFloat(0.95f, 2.5f) * 45f;

                        float dustScale = Main.rand.NextFloat(1.25f, 2f);
                        Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.SkyBlue with { A = 0 } * 1f, Scale: dustScale); //dust176
                        wind.noGravity = true;
                        wind.rotation = Main.rand.NextFloat(6.28f);
                    }

                    //Wind Line
                    for (int i3 = 0; i3 < windLinePerFrame; i3++)
                    {
                        float rot = MathHelper.PiOver2;
                        int side = Main.rand.NextBool() ? 1 : -1;


                        Vector2 windDustSpawnPosition = NPC.Center + (new Vector2(1f * -200f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloat(safeZoneWidth, burstWidth) * side)).RotatedBy(rot);
                        Vector2 windDustVelocity = new Vector2(1f, 0f).RotatedBy(rot) * Main.rand.NextFloat(0.95f, 3f) * 45f;

                        Dust p = Dust.NewDustPerfect(windDustSpawnPosition, ModContent.DustType<WindLine>(), windDustVelocity * 1f, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.4f, 0.55f));

                        p.customData = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, 4f, 2f, false);
                    }
                    #endregion
                }
                else
                {
                    if (timer == timeBeforeRain + timeOfRain)
                    {
                        if (umbrellaTelegraphProj.active)
                            (umbrellaTelegraphProj.ModProjectile as UmbrellaTelegraph).shouldFadeOut = true;
                    }

                    windOverlayOpacityGoal = 0f;
                    windOverlayOpacity *= 0.9f;
                }

                if (timer == timeBeforeRain + timeOfRain + timeAfterRain)
                {
                    //umbrellaStormwallInstance.active = false;

                    attackReps++;
                    timer = -1;
                }
            }


            /*
            if (timer >= 70)
            {
                //Sound effect and pulse shtuff
                if (timer == 70)
                {
                    bgPulsePower = 2.5f;

                    windOverlayOpacityGoal = 1f;
                    windOverlayRotation = MathHelper.PiOver2;

                    float overallVolume = 0.4f;

                    //Sound
                    SoundStyle styleA = new SoundStyle("VFXPlus/Sounds/Effects/water_blast_projectile_spell_03") with { Volume = 0.5f * overallVolume, Pitch = .7f, PitchVariance = 0.05f, MaxInstances = -1 };
                    SoundEngine.PlaySound(styleA, NPC.Center);

                    SoundStyle styleC = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/flame_thrower_airblast_rocket_redirect") with { Volume = 0.15f * overallVolume, Pitch = .4f, PitchVariance = .1f, MaxInstances = -1 };
                    SoundEngine.PlaySound(styleC, NPC.Center);

                    SoundStyle styleD = new SoundStyle("VFXPlus/Sounds/Effects/Cries/astrolotl") with { Volume = 1f * overallVolume, Pitch = .6f, PitchVariance = 0.05f, MaxInstances = 1 }; 
                    SoundEngine.PlaySound(styleD, NPC.Center);

                    int pulse = Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<WindPulse>(), 0, 0, player.whoAmI);
                    (Main.projectile[pulse].ModProjectile as WindPulse).timeForPulse = 60;
                    (Main.projectile[pulse].ModProjectile as WindPulse).intensity = 0.8f;
                    Main.projectile[pulse].scale = 9f;


                    storedShakeCenter = NPC.Center;
                }

                //Continually shake screen for a while
                if (timer < 105)
                    player.GetModPlayer<ScreenShakePlayer>().ScreenShakePower = 15f;

                NPC.Center = storedShakeCenter;

                //Spawning feathers and dust
                if (timer < 120)
                {
                    //Push the player downward if they are above FF
                    if (player.Center.Y < NPC.Center.Y)
                    {
                        if (player.velocity.Y < 0)
                            player.velocity.Y *= -1;

                        player.velocity.Y += 0.75f; 
                    }

                    int smokePerFrame = 10 * 2;
                    int windDustPerFrame = 5 * 2;
                    int windLinePerFrame = 1 * 2;
                    int feathersPerBurst = 5 * 2;

                    float burstWidth = 1000f * 2f;

                    //Feathers
                    if (timer % 6 == 0)
                    {
                        for (int i = 0; i < feathersPerBurst; i++)
                        {
                            float prog = (float)i / (float)feathersPerBurst;

                            float x = MathHelper.Lerp(-burstWidth, -safeZoneWidth, prog);

                            Vector2 spawnPosL = new Vector2(x + Main.rand.NextFloat(-135f, 135f), -400f + Main.rand.NextFloat(-50f, 50f));
                            Vector2 spawnPosR = new Vector2((-1f * x) + Main.rand.NextFloat(-135f, 135f), -400f + Main.rand.NextFloat(-50f, 50f));

                            //Left Side
                            int pl = Projectile.NewProjectile(null, NPC.Center + spawnPosL, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                            (Main.projectile[pl].ModProjectile as StraightFeather).accelTime = 0;
                            (Main.projectile[pl].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                            Main.projectile[pl].extraUpdates = 3;

                            //Right Side
                            int pr = Projectile.NewProjectile(null, NPC.Center + spawnPosR, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                            (Main.projectile[pr].ModProjectile as StraightFeather).accelTime = 0;
                            (Main.projectile[pr].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                            Main.projectile[pr].extraUpdates = 3;
                        }
                    }


                    //Spawn 2 feathers on the edge of the border every 14 frames
                    if (timer % 14 == 0)
                    {
                        //Dont spawn them exactly on the border size (95f) because the feathers have a wide hitbox
                        Vector2 spawnPosL = NPC.Center + new Vector2(safeZoneWidth + 15f, -400f + Main.rand.NextFloat(-25f, 25f));
                        Vector2 spawnPosR = NPC.Center + new Vector2(-safeZoneWidth - 15f, -400f + Main.rand.NextFloat(-25f, 25f));

                        int p1 = Projectile.NewProjectile(null, spawnPosL, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                        (Main.projectile[p1].ModProjectile as StraightFeather).accelTime = 0;
                        (Main.projectile[p1].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                        Main.projectile[p1].extraUpdates = 3;

                        int p2 = Projectile.NewProjectile(null, spawnPosR, new Vector2(0f, 15f), ModContent.ProjectileType<StraightFeather>(), 1, 1);
                        (Main.projectile[p2].ModProjectile as StraightFeather).accelTime = 0;
                        (Main.projectile[p2].ModProjectile as StraightFeather).isFromUmbrellaRain = true;
                        Main.projectile[p2].extraUpdates = 3;
                    }



                    #region Rain Particles
                    //Smoke Dust
                    for (int i = 0; i < smokePerFrame; i++)
                    {
                        float rot = MathHelper.PiOver2;
                        int side = Main.rand.NextBool() ? 1 : -1;

                        Vector2 smokeDustSpawnPosition = NPC.Center + (new Vector2(1f * -200f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloat(safeZoneWidth, burstWidth) * side)).RotatedBy(rot);
                        Vector2 smokeDustVelocity = new Vector2(1f, 0f).RotatedBy(rot) * Main.rand.NextFloat(8f, 12f) * 1.5f;

                        Dust smoke = Dust.NewDustPerfect(smokeDustSpawnPosition, ModContent.DustType<SmallSmoke>(), smokeDustVelocity,
                            newColor: Color.LightSkyBlue * 1f, Scale: Main.rand.NextFloat(7f, 9f));

                        SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 0.45f, ScaleFadePower: 0.97f, false);
                        smoke.customData = ssb;
                    }

                    //Wind Dust
                    for (int i2 = 0; i2 < windDustPerFrame; i2++)
                    {
                        float rot = MathHelper.PiOver2;
                        int side = Main.rand.NextBool() ? 1 : -1;

                        Vector2 windDustSpawnPosition = NPC.Center + (new Vector2(1f * -200f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloat(safeZoneWidth, burstWidth) * side)).RotatedBy(rot);
                        Vector2 windDustVelocity = new Vector2(1f, 0f).RotatedBy(rot) * Main.rand.NextFloat(0.95f, 2.5f) * 45f;

                        float dustScale = Main.rand.NextFloat(1.25f, 2f);
                        Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.SkyBlue with { A = 0 } * 1f, Scale: dustScale); //dust176
                        wind.noGravity = true;
                        wind.rotation = Main.rand.NextFloat(6.28f);
                    }

                    //Wind Line
                    for (int i3 = 0; i3 < windLinePerFrame; i3++)
                    {
                        float rot = MathHelper.PiOver2;
                        int side = Main.rand.NextBool() ? 1 : -1;


                        Vector2 windDustSpawnPosition = NPC.Center + (new Vector2(1f * -200f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloat(safeZoneWidth, burstWidth) * side)).RotatedBy(rot);
                        Vector2 windDustVelocity = new Vector2(1f, 0f).RotatedBy(rot) * Main.rand.NextFloat(0.95f, 3f) * 45f;

                        Dust p = Dust.NewDustPerfect(windDustSpawnPosition, ModContent.DustType<WindLine>(), windDustVelocity * 1f, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.4f, 0.55f));

                        p.customData = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, 4f, 2f, false);
                    }
                    #endregion
                }
                else
                {
                    if (timer == 120)
                    {
                        if (umbrellaTelegraphProj.active)
                            (umbrellaTelegraphProj.ModProjectile as UmbrellaTelegraph).shouldFadeOut = true;
                    }


                    windOverlayOpacityGoal = 0f;
                    windOverlayOpacity *= 0.9f;
                }

                NPC.Center = NPC.Center + (Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * player.GetModPlayer<ScreenShakePlayer>().ScreenShakePower);

                if (timer == 210)
                {
                    substate = 0;

                    umbrellaStormwallInstance.active = false;

                    windOverlayOpacityGoal = 0f;
                    timer = -1;
                }
            }
            */

        }


        bool spawnUp = false;
        int nadoSpawnCount = 0;
        Vector2 storedTornadoSpawnPos = Vector2.Zero;
        public void MadisonTornado()
        {
            int timeBeforeSpawnNados = 100;
            int timeSpawnNados = 1000000;

            //How many nados above and below player to spawn
            int nadoDoubleCount = 4;

            //Hover above player
            float hoverSpeed = (NPC.Distance(player.Center) > 500 ? 5f : 3f);

            Vector2 goalPos = player.Center + new Vector2(0f, -250);
            BasicMovementVariant3(goalPos, moveSpeed: hoverSpeed);

            if (timer <= timeBeforeSpawnNados)
            {
                //Store position to spawn tornados from
                if (timer == timeBeforeSpawnNados)
                    storedTornadoSpawnPos = player.Center;

            }
            else if (timer <= timeBeforeSpawnNados + timeSpawnNados)
            {
                //Spawn a tornado every 15 frames in a line moving up or down

                float distanceBetweenNados = 90;

                if (timer % 2 == 0 && nadoSpawnCount != 1 + (nadoDoubleCount * 2))
                {
                    Vector2 startPos = storedTornadoSpawnPos + new Vector2(0f, -distanceBetweenNados * (spawnUp ? 1f : -1f)) * nadoDoubleCount;

                    float yVal = nadoSpawnCount * distanceBetweenNados * (spawnUp ? 1f : -1f);

                    int nado = Projectile.NewProjectile(NPC.GetSource_FromThis(), startPos + new Vector2(0f, yVal), Vector2.Zero, ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
                    (Main.projectile[nado].ModProjectile as MadisonTornado).startDir = Main.rand.NextBool() ? 1 : -1;

                    nadoSpawnCount++;
                }
                
                if (timer == 150)
                {
                    timer = -1;
                    nadoSpawnCount = 0;
                }

                //Spawn tornado
                if (timer % 120 == 0)
                {

                    for (int i = -5; i < 5; i++)
                    {

                        //Vector2 tornadoSpawnPos = player.Center;
                        //tornadoSpawnPos.Y += i * 90f;

                        //int nado = Projectile.NewProjectile(NPC.GetSource_FromThis(), tornadoSpawnPos, Vector2.Zero, ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
                        //(Main.projectile[nado].ModProjectile as MadisonTornado).startDir = Main.rand.NextBool() ? 1 : -1;
                    }
                }
            }

        }


        #region MovementCode
        //Based off Emode Cryogen
        void BasicMovementVariant1(Vector2 goalPos, float accel = 0.03f, float maxSpeed = 15, float minSpeed = 5, float decel = 0.06f, float slowdown = 30)
        {
            if (NPC.Distance(goalPos) > slowdown)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * maxSpeed, accel);
            }
            else
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * minSpeed, decel);
            }
        }

        void BasicMovementVariant2(Vector2 goalPos)
        {
            Vector2 trueTarget = goalPos;
            float moveSpeed = 8f; //4f

            if (NPC.Distance(trueTarget) > moveSpeed * 2f)
            {
                NPC.velocity += NPC.DirectionTo(trueTarget) * moveSpeed;
            }

            float clampedDistance = Math.Clamp(NPC.Distance(trueTarget), 5f, 800f);

            float velocityMult = MathHelper.Lerp(0.5f, 0.9f, clampedDistance / 800f);
                
                //Utils.GetLerpValue(0.9f, 0.9f, (clampedDistance / 800f), true);
            //Main.NewText((clampedDistance / 800f));

            NPC.velocity *= velocityMult;
            
        }

        void BasicMovementVariant3(Vector2 goalPos, float moveSpeed = 6f, float clampDistance = 240f)
        {
            NPC.velocity *= 0.875f;

            Vector2 trueGoal = goalPos - NPC.Center;
            Vector2 lerpGoal = Vector2.Lerp(NPC.Center, goalPos, 0.8f);

            Vector2 goTo = trueGoal;
            float speed = moveSpeed * MathHelper.Clamp(goTo.Length() / clampDistance, 0, 1);
            if (goTo.Length() < speed)
            {
                speed = goTo.Length();
            }
            NPC.velocity += goTo.SafeNormalize(Vector2.Zero) * speed;

        }
    }
    #endregion
}
