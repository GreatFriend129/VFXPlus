using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils.Cil;
using rail;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;

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
            
            if (substate == 1)
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
            else if (substate == 2)
            {
                //NPC.velocity = Vector2.Zero;

                //BasicMovementVariant1(player.Center + basicAttackPoint); // A bit jittery, follows point not 100%, tends to overshoot then rebound (sometimes too much)

                //BasicMovementVariant2(player.Center + basicAttackPoint); A bit static to reach dest, but very quick


                BasicMovementVariant3(player.Center + basicAttackPoint, 3f, 540f);


                if (timer == 30 || timer == 85)
                {
                    Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    for (int iaa = -3; iaa < 4; iaa++)
                    {
                        Vector2 vel = toPlayer.RotatedBy(iaa * MathHelper.PiOver4 * 1.25f) * 10f;

                        float curvePower = iaa * 0.009f;

                        int curveFeather = Projectile.NewProjectile(null, NPC.Center, vel, ModContent.ProjectileType<CurvingFeather>(), 2, 0, Main.myPlayer);

                        if (Main.projectile[curveFeather].ModProjectile is CurvingFeather cf)
                        {
                            cf.curveValue = curvePower * 1.25f; //1.5
                        }

                        Main.projectile[curveFeather].hostile = true;
                        Main.projectile[curveFeather].friendly = false;
                        Main.projectile[curveFeather].extraUpdates = 0; //1

                    }
                }

                if (timer > 20 && timer < 65 && timer % 8 == 0 && false)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(8f, 8f) * Main.rand.NextFloat();

                    int feather = Projectile.NewProjectile(null, NPC.Center, vel, ModContent.ProjectileType<OrbitingFeather>(), 2, 0, Main.myPlayer);

                    if (Main.projectile[feather].ModProjectile is OrbitingFeather of)
                    {
                        of.timeToOrbit = 0;
                        of.orbitVector = vel;
                        of.orbitVal = 300;
                        of.rotSpeed = 0;
                    }
                }

                if (timer == 100) //100
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

        Vector2 fiveSpreadGoalVec = Vector2.Zero;
        Vector2 fiveSpreadStartVec = Vector2.Zero;
        float fiveSpreadRotAmount = 0f;
        Vector2 resultingVec = Vector2.Zero;
        public void FiveSpread()
        {
            //To hopefully fix a weird NaN case
            
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

            Vector2 dir = new Vector2((float)Math.Sign(NPC.Center.X - player.Center.X) * 300, -100);


            float timeBeforeShot = 30; //30
            float timeForShot = 120; //60

            if (timer >= timeBeforeShot + timeForShot)
            {
                Dust.NewDustPerfect(player.Center + dir, DustID.PortalBolt, newColor: Color.OrangeRed);

                NPC.velocity.X *= 0.95f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + dir) * (NPC.Distance(player.Center + dir) / 15), 0.2f); //high lerpval gives less overshoot
            }
            else if (timer >= timeBeforeShot)
            {
                Dust.NewDustPerfect(player.Center + dir, DustID.PortalBolt, newColor: Color.DeepPink);


                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + dir) * (NPC.Distance(player.Center + dir) / 15), 0.2f);
                NPC.velocity *= 0.92f;
                //NPC.velocity.X *= 0.92f;


                float angleRange = MathHelper.TwoPi * 0.4f; 


                if (timer % 5 == 0)
                {
                    float progress = (float)(timer - timeBeforeShot) / timeForShot; 

                    float shotAngle = MathHelper.Lerp(-angleRange / 2f, angleRange / 2f, progress);

                    Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

                    Projectile proj = Projectile.NewProjectileDirect(null, NPC.Center, toPlayer.RotatedBy(shotAngle) * 10, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);
                    Projectile proj2 = Projectile.NewProjectileDirect(null, NPC.Center, toPlayer.RotatedBy(shotAngle + MathHelper.TwoPi * 0.33f) * 10, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);
                    Projectile proj3 = Projectile.NewProjectileDirect(null, NPC.Center, toPlayer.RotatedBy(shotAngle + MathHelper.TwoPi * 0.66f) * 10, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);

                    //Projectile proj = Projectile.NewProjectileDirect(null, NPC.Center, toPlayer.RotatedBy(shotAngle) * 10, ModContent.ProjectileType<SpinShotFeather>(), 10, 5);
                    //(proj.ModProjectile as SpinShotFeather).targetPlayer = player.whoAmI;
                }

                /*
                float count = timer % 30;
                if (count == 0)
                {
                }
                else
                {
                    if (count % 5 == 0) //5
                    {
                        float vectorRotation = -count + 15; //15 | 3 | 10
                        Projectile proj = Projectile.NewProjectileDirect(null, NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(MathHelper.ToRadians(vectorRotation * 3)) * 10, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);

                    }
                }
                */
            }
            else
            {
                Dust.NewDustPerfect(player.Center + dir, DustID.PortalBolt, newColor: Color.DeepSkyBlue);

                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + dir) * (NPC.Distance(player.Center + dir) / 20), 0.6f);
            }


            //if (timer % 220 == 0 && timer > 0)
                //NPC.Center = player.Center + Main.rand.NextVector2Circular(1600f, 1600f);

            if (timer == 180)
            {
                NPC.Center = player.Center + Main.rand.NextVector2CircularEdge(1100f, 1100f);
                timer = -1;
            }
            /*
            
            float timeBeforeShot = 30; //30
            float timeForShot = 120; //60
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + dir) * (NPC.Distance(player.Center + dir) / 15), 0.2f);
                ///NPC.velocity.X *= 0.92f;

                float angleRange = 40; //40 is good


                if (timer % 2 == 0)
                {
                    float progress = (float)(timer - timeBeforeShot) / timeForShot; 

                    float shotAngle = MathHelper.Lerp(-angleRange / 2f, angleRange / 2f, progress);

                    Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

                    Projectile proj = Projectile.NewProjectileDirect(null, NPC.Center, toPlayer.RotatedBy(shotAngle) * 10, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);
                    Projectile proj2 = Projectile.NewProjectileDirect(null, NPC.Center, toPlayer.RotatedBy(shotAngle) * -10, ModContent.ProjectileType<StopAndStartFeather>(), 10, 5);

                    //Projectile proj = Projectile.NewProjectileDirect(null, NPC.Center, toPlayer.RotatedBy(shotAngle) * 10, ModContent.ProjectileType<SpinShotFeather>(), 10, 5);
                    //(proj.ModProjectile as SpinShotFeather).targetPlayer = player.whoAmI;
                }
            */
        }

        public void MartletOrbitFeather()
        {

        }

        public void CircleBurstFeather()
        {

        }

        public void SwirlFeather()
        {

        }

        public void CornerTravelShot()
        {

        }

        public void CircleDash() { }

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
                    diveStartPoint = new Vector2(Main.rand.NextFloat(-25f, 25f), Main.rand.Next(-50, 50) + -150f); //-300
                }

                Vector2 goalPos = player.Center + diveStartPoint;

                int timeToStartDive = 85; //90
                float timeForMovement = 35; //35

                if (timer < timeToStartDive)
                {
                    float timeForOffsetShrink = 80;
                    float offsetProgress = Math.Clamp((float)timer / timeForOffsetShrink, 0f, 1f);
                    Vector2 goalOffset = Vector2.Lerp(new Vector2(850f * diveStartSide, 0f), new Vector2(0f + player.velocity.X * 6f, -200f), Easings.easeOutQuad(offsetProgress));

                    Dust.NewDustPerfect(goalPos + goalOffset, DustID.GemSapphire);

                    //BasicMovementVariant1(goalPos + goalOffset, accel: 0.07f, maxSpeed: 25f, minSpeed: 3f);
                    BasicMovementVariant3(goalPos + goalOffset, moveSpeed: 4f); //3.5

                }
                else if (timer >= timeToStartDive)
                {
                    Vector2 initialVel = new Vector2(NPC.velocity.X, -20f); //20
                    Vector2 goalVel = new Vector2(NPC.velocity.X * 0.7f, 30f); //23
                    
                    float timeProgress = Math.Clamp((float)((timer - timeToStartDive) / timeForMovement), 0f, 1f);
                    NPC.velocity = Vector2.Lerp(initialVel, goalVel, timeProgress);


                    if (timer >= timeToStartDive + timeForMovement - 8 && timer % 5 == 0) //-12 %6 //
                    {
                        int a = Projectile.NewProjectile(null, NPC.Center, new Vector2(5.5f, 0.75f) * 2f, ModContent.ProjectileType<StopAndStartFeather>(), 1, 1);
                        int b = Projectile.NewProjectile(null, NPC.Center, new Vector2(-5.5f, 0.75f) * 2f, ModContent.ProjectileType<StopAndStartFeather>(), 1, 1);

                        (Main.projectile[a].ModProjectile as StopAndStartFeather).velShrinkTime = 31;// 35;
                        (Main.projectile[a].ModProjectile as StopAndStartFeather).velGrowTime = 54;// 60;

                        (Main.projectile[b].ModProjectile as StopAndStartFeather).velShrinkTime = 31;// 35;
                        (Main.projectile[b].ModProjectile as StopAndStartFeather).velGrowTime = 54;// 60;
                    }
                }

                if (timer == timeToStartDive + timeForMovement + 40)
                {
                    timer = -1;
                }

            }


        }

        public void UmbrellaRain()
        {
            
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
