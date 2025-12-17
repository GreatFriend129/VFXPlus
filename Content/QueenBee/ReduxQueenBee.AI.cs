using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;

namespace VFXPlus.Content.QueenBee
{
    public partial class ReduxQueenBee : ModNPC
    {
        Vector2 dummyGoalPos = Vector2.Zero;
        public void Dummy() 
        {
            if (timer == 0)
            {
                NPC.velocity = Vector2.Zero;

                dummyGoalPos = Main.rand.NextVector2CircularEdge(300f, 300f);

            }

            BasicMovement(player.Center + dummyGoalPos, 5f, 1240f);

            if (timer == 200)
            {
                timer = -1;
            }

        }

        Vector2 vanillaDashVec = Vector2.Zero;
        int vanillaDashSide = 1;
        public void VanillaDashes()
        {
            //Move to player side
            //Dash
            //After a time, turn around and start rising or falling to player y val
            //Repeat

            //Max Speed = 25tbd | 30 tad | 35 dash speed //|(MathF.Abs(distToPlayerY) < 5f && timer > (float)timeBeforeDash / 2f) | basic movement 10f

            
            //1f attackSpeed maybe too much but lol

            int timeBeforeDash = 50 - (int)(25 * attackSpeed);
            int timeAfterDash = 40 - (int)(10 * attackSpeed);
            float dashSpeed = 25 + (int)(10 * attackSpeed);
            float moveSpeed = 5f + (int)(5f * attackSpeed);

            //If queen bee is near the same Y value as the player and this percentage of timeBeforeDash has passed, she can dash early
            float earlyDashTime = timeBeforeDash / (float)(1f + attackSpeed);
            

            //Move to player side
            if (substate == 0)
            {
                FacePlayer();

                isDashing = false;
                if (timer == 0)
                {
                    vanillaDashVec = new Vector2(500f * vanillaDashSide, 0f);
                }

                BasicMovement(player.Center + vanillaDashVec, moveSpeed, 540);

                float distToPlayerY = (player.Center.Y - NPC.Center.Y);

                if (timer == timeBeforeDash|| (MathF.Abs(distToPlayerY) < 7f && timer > earlyDashTime)) //5
                {
                    Dust d2 = Dust.NewDustPerfect(NPC.Center + new Vector2(0f, 20f), ModContent.DustType<CirclePulse>(), new Vector2(-2f * vanillaDashSide, 0f), newColor: Color.Goldenrod * 0.3f);
                    CirclePulseBehavior b2 = new CirclePulseBehavior(1.75f, true, 2, 0.2f, 0.4f);
                    b2.drawLayer = "UnderProjectiles";
                    d2.customData = b2;

                    Dust d3 = Dust.NewDustPerfect(NPC.Center + new Vector2(0f, 20f), ModContent.DustType<CirclePulse>(), new Vector2(-4f * vanillaDashSide, 0f), newColor: Color.Goldenrod * 0.15f);
                    CirclePulseBehavior b3 = new CirclePulseBehavior(1f, true, 1, 0.2f, 0.4f);
                    d3.scale = 0.05f;
                    b3.drawLayer = "UnderProjectiles";
                    d3.customData = b3;

                    //Vanilla QB charge sound
                    SoundEngine.PlaySound(SoundID.Zombie125 with { Volume = 0.75f, Pitch = 0f, MaxInstances = -1 }, NPC.Center);

                    timer = -1;
                    substate++;
                }
            }
            //Dash
            else if (substate == 1)
            {
                isDashing = true;
                NPC.velocity = new Vector2(-dashSpeed * vanillaDashSide, 0f);

                int trailCount = 8;
                dashTrailPositions.Add(NPC.Center);
                dashTrailRotations.Add(NPC.velocity.ToRotation());

                if (dashTrailPositions.Count > trailCount)
                {
                    dashTrailPositions.RemoveAt(0);
                    dashTrailRotations.RemoveAt(0);
                }

                if (timer == timeAfterDash)
                {
                    dashTrailPositions.Clear();
                    dashTrailRotations.Clear();
                    substate = 0;
                    timer = -1;
                    vanillaDashSide *= -1;

                    attackReps++;
                    if (attackReps == 5)
                    {
                        ChooseNextAttack();
                    }

                }
            }
        }

        Vector2 wallDashVec = Vector2.Zero;
        int wallDashSide = 1;
        int wallDashVerticleSide = 1;
        public void WalledDashes()
        {
            //Move to player side
            //Spawn Wall of bees and wait
            //Dash


            //Max Speed = 25tbd | 30 tad | 35 dash speed //|(MathF.Abs(distToPlayerY) < 5f && timer > (float)timeBeforeDash / 2f) | basic movement 10f


            int timeBeforeSpawnWall = 10 - (int)(4 * attackSpeed);
            int timeBeforeDash = 85 - (int)(25 * attackSpeed); //75 //Dash starts after timeBeforeSpawnWall + timeBeforeDash
            int timeAfterDash = 50 - (int)(15 * attackSpeed);
            float dashSpeed = 20f + (int)(7 * attackSpeed);

            //Move to player side
            if (substate == 0)
            {
                FacePlayer();

                isDashing = false;
                if (timer == 0)
                {
                    wallDashVerticleSide = Main.rand.NextBool() ? 1 : -1;
                    wallDashVec = new Vector2(500f * wallDashSide, 0f);
                }

                NPC.velocity.X *= 0.95f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + wallDashVec) * (NPC.Distance(player.Center + wallDashVec) / 15), 0.3f); //high lerpval gives less overshoot

                if (timer == timeBeforeSpawnWall)
                {
                    int beeCount = 10;
                    for (int j = 0; j < 10; j++)
                    {
                        float prog = (float)j / (float)beeCount;

                        Vector2 spawnPos = NPC.Center + new Vector2(130f, 0f).RotatedBy(MathHelper.TwoPi * prog); //430

                        int bee = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<WalledDashBee>());
                        (Main.npc[bee].ModNPC as WalledDashBee).isHittable = false;
                        (Main.npc[bee].ModNPC as WalledDashBee).ownerIndex = NPC.whoAmI;
                        (Main.npc[bee].ModNPC as WalledDashBee).goalPos = new Vector2(0f, (-50f - (35f * j)) * wallDashVerticleSide);
                        (Main.npc[bee].ModNPC as WalledDashBee).timeToReachDest = 60 + Main.rand.Next(-14, 4);
                        (Main.npc[bee].ModNPC as WalledDashBee).circleRadius = 450f + (100f * prog);
                    }
                }
                if (timer == timeBeforeSpawnWall + timeBeforeDash)
                {
                    timer = -1;
                    substate++;
                }
            }
            //Dash
            else if (substate == 1)
            {
                if (timer == 0)
                {
                    Dust d2 = Dust.NewDustPerfect(NPC.Center + new Vector2(0f, 20f), ModContent.DustType<CirclePulse>(), new Vector2(-2f * wallDashSide, 0f), newColor: Color.Goldenrod * 0.3f);
                    CirclePulseBehavior b2 = new CirclePulseBehavior(1.75f, true, 2, 0.2f, 0.4f);
                    b2.drawLayer = "UnderProjectiles";
                    d2.customData = b2;

                    Dust d3 = Dust.NewDustPerfect(NPC.Center + new Vector2(0f, 20f), ModContent.DustType<CirclePulse>(), new Vector2(-4f * wallDashSide, 0f), newColor: Color.Goldenrod * 0.15f);
                    CirclePulseBehavior b3 = new CirclePulseBehavior(1f, true, 1, 0.2f, 0.4f);
                    d3.scale = 0.05f;
                    b3.drawLayer = "UnderProjectiles";
                    d3.customData = b3;

                    //Vanilla QB charge sound
                    SoundEngine.PlaySound(SoundID.Zombie125 with { Volume = 0.75f, Pitch = 0f, MaxInstances = -1 }, NPC.Center);

                }

                isDashing = true;
                NPC.velocity = new Vector2(-dashSpeed * wallDashSide, 0f);

                int trailCount = 10;
                dashTrailPositions.Add(NPC.Center);
                dashTrailRotations.Add(NPC.velocity.ToRotation());

                if (dashTrailPositions.Count > trailCount)
                {
                    dashTrailPositions.RemoveAt(0);
                    dashTrailRotations.RemoveAt(0);
                }

                if (timer == timeAfterDash)
                {
                    dashTrailPositions.Clear();
                    dashTrailRotations.Clear();
                    substate = 0;
                    timer = -1;
                    wallDashSide *= -1;

                    attackReps++;
                    if (attackReps == 4)
                    {
                        ChooseNextAttack();
                    }

                }
            }
        }


        Vector2 sweepGoalPos = Vector2.Zero;
        int sweepGoalSide = 1;
        float sweepAngle = 0f;
        int shotCounter = 0;
        public void StingerSweep()
        {
            //attackSpeed = 0.5f;

            int timeBeforeShots = 80 - (int)(attackSpeed * 15); //60 base
            int timeBetweenShots = 5 - (int)(attackSpeed * 2); //5 base
            int shotCount = 16; //16
            float angleBetweenShots = MathHelper.PiOver4 * 0.25f;

            if (timer == 0)
            {
                NPC.velocity = Vector2.Zero;
                sweepGoalPos = new Vector2(210f * sweepGoalSide, -120f);
            }

            //Hover near player
            if (timer < timeBeforeShots)
            {
                FacePlayer();

                NPC.velocity.X *= 0.95f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + sweepGoalPos) * (NPC.Distance(player.Center + sweepGoalPos) / 15), 0.2f); //high lerpval gives less overshoot
            }
            //Move slower while firing
            else
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(player.Center + sweepGoalPos) * (NPC.Distance(player.Center + sweepGoalPos) / 150), 0.05f);
                NPC.velocity *= 0.92f;
            }

            //Start shooting
            if (timer >= timeBeforeShots)
            {
                if (shotCounter % timeBetweenShots == 0)
                {
                    float rot = (MathHelper.Pi + MathHelper.PiOver4) + sweepAngle;

                    Vector2 vel = (rot.ToRotationVector2() * 8f).RotatedBy(sweepGoalSide == 1 ? 0f : MathHelper.PiOver2);
                    Vector2 spawnPos = NPC.Center + new Vector2(-20f * sweepGoalSide, 45f);

                    int stinger = Projectile.NewProjectile(null, spawnPos, vel, ModContent.ProjectileType<StopAndStartStinger>(), 1, 0, Main.myPlayer);
                    Main.projectile[stinger].scale = 1.15f;

                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velShrinkTime = 35; 
                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velGrowTime = 60;
                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velShrinkAmount = 0.93f; 
                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velGrowAmount = 1.15f;



                    for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
                    {
                        Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                        Dust dust = Dust.NewDustPerfect(spawnPos + randomStart, DustID.t_Honey, vel * 0.65f + randomStart, Scale: Main.rand.NextFloat(1f, 1.75f));

                        dust.noGravity = true;
                    }

                    Color GPCCol = Color.Lerp(Color.OrangeRed, Color.DarkGoldenrod, 0.95f);
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 randomStart = Main.rand.NextVector2Circular(2f, 2f) * 1f;
                        Dust dust = Dust.NewDustPerfect(spawnPos + randomStart, ModContent.DustType<GlowPixelCross>(), vel * 0.65f + randomStart, newColor: GPCCol, Scale: Main.rand.NextFloat(0.35f, 0.45f));

                        dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                            rotPower: 0.15f, preSlowPower: 0.95f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.88f, shouldFadeColor: false);
                    }

                    sweepAngle -= angleBetweenShots * sweepGoalSide;
                }
                shotCounter++;
            }

            if (timer == (timeBeforeShots + (timeBetweenShots * shotCount)))
            {
                sweepGoalSide *= -1;
                sweepAngle = 0;
                shotCounter = 0;
                timer = -1;

                attackReps++;
                if (attackReps == 2)
                {
                    //sweepGoalSide *= -1;
                    ChooseNextAttack();
                }
            }

        }

        Vector2 radialBurstPos = Vector2.Zero;
        int radialBurstSide = 1;
        public void RadialBurst()
        {
            //attackSpeed = 1f; //1.5 valid attackSpeed

            int timeBeforeShots = 80 - (int)(attackSpeed * 40); //80 base
            int timeBetweenShots = 30 - (int)(attackSpeed * 20); //30 base
            int timeAfterShots = 80 - (int)(attackSpeed * 40); //80 base

            //Make sure that timeAfterShots isnt less than 2 * timeBetweenShots unless you want less than 3 burts

            float moveSpeedMult = 1f + (attackSpeed);

            if (timer == 0)
            {
                if (attackReps == 0)
                    radialBurstSide = (NPC.Center.X > player.Center.X) ? 1 : -1;

                NPC.velocity = Vector2.Zero;

                radialBurstPos = new Vector2(225f * radialBurstSide, -130f); //225 -120

            }

            //Hover above player
            float hoverSpeed = (NPC.Distance(player.Center) > 500 ? 5f : 3f);

            BasicMovement(player.Center + radialBurstPos, hoverSpeed * moveSpeedMult, 480f);

            FacePlayer();


            if (timer == timeBeforeShots || timer == timeBeforeShots + timeBetweenShots || timer == timeBeforeShots + timeBetweenShots + timeBetweenShots)
            {
                Vector2 spawnPos = NPC.Center + new Vector2(-20f * radialBurstSide, 40f);

                CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.35f, true, 1, 0.8f, 0.8f);

                Dust d1 = Dust.NewDustPerfect(spawnPos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.5f);
                d1.scale = 0.1f;
                d1.customData = cpb2;
                d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

                Dust d2 = Dust.NewDustPerfect(spawnPos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.5f);
                d2.scale = 0.1f;
                d2.customData = cpb2;
                d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

                float shotCount = 10; //10
                Vector2 randStart = Main.rand.NextVector2Unit();
                for (float i = 0; i < shotCount; i++)
                {
                    float prog = (float)i / shotCount;

                    float rot = MathHelper.TwoPi * prog;

                    Vector2 vel = randStart.RotatedBy(rot) * 6f;

                    int stinger = Projectile.NewProjectile(null, spawnPos, vel, ModContent.ProjectileType<StopAndStartStinger>(), 1, 0, Main.myPlayer);
                    Main.projectile[stinger].scale = 1.15f;

                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velShrinkTime = 40; 
                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velGrowTime = 90; 
                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velShrinkAmount = 0.9f; 
                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).velGrowAmount = 1.14f; 
                    (Main.projectile[stinger].ModProjectile as StopAndStartStinger).maxVel = 12f; 

                    for (int m = 0; m < 1 + Main.rand.Next(0, 2); m++)
                    {
                        Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                        Dust dust = Dust.NewDustPerfect(spawnPos + randomStart, DustID.t_Honey, vel * 0.65f + randomStart, Scale: Main.rand.NextFloat(1f, 1.75f));

                        dust.noGravity = true;

                        //dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        //    rotPower: 0.15f, preSlowPower: 0.95f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.88f, shouldFadeColor: true);
                    }

                    Color GPCCol = Color.Lerp(Color.OrangeRed, Color.DarkGoldenrod, 0.95f);
                    for (int m = 0; m < 1; m++)
                    {
                        Vector2 randomStart = Main.rand.NextVector2Circular(2f, 2f) * 1f;
                        Dust dust = Dust.NewDustPerfect(spawnPos + randomStart, ModContent.DustType<GlowPixelCross>(), vel * 0.65f + randomStart, newColor: GPCCol, Scale: Main.rand.NextFloat(0.35f, 0.45f));

                        dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                            rotPower: 0.15f, preSlowPower: 0.95f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.88f, shouldFadeColor: false);
                    }
                }
                NPC.velocity *= 0.5f;
                NPC.velocity += new Vector2(6f * radialBurstSide, -6f);
            }

            if (timer == timeBeforeShots + timeAfterShots)
            {
                radialBurstSide *= -1;
                timer = -1;

                attackReps++;

                if (attackReps == 2)
                    ChooseNextAttack();
            }

        }


        int wallSide = 1;
        public void NamaahWall()
        {            
            int timeBeforeSpawn = 60 - (int)(15 * attackSpeed);
            int timeAfterSpawn = 200 - (int)(75 * attackSpeed);
            float beeVelocity = 4f + (int)(2.75f * attackSpeed);

            float hoverSpeed = (NPC.Distance(player.Center) > 500 ? 9f : 3f);

            BasicMovement(player.Center + new Vector2(0f, -200f), hoverSpeed, 1240f); //150
            FacePlayer();

            if (timer == timeBeforeSpawn)
            {
                //Spawn Wall

                for (int j = 0; j < 2; j++)
                {
                    for (int i = -10; i < 10; i++)
                    {
                        Vector2 basePos = player.Center + new Vector2((-650f - (350 * j)) * wallSide, 0f);
                        Vector2 adjustedPos = basePos + new Vector2(0f, 50f * i); //60

                        bool shouldBeHittable = (i > -7 && i < -2) || (i < 7 && i > 2);

                        if (j == 1)
                        {
                            shouldBeHittable = !shouldBeHittable;
                        }

                        int bee = NPC.NewNPC(null, (int)adjustedPos.X, (int)adjustedPos.Y, ModContent.NPCType<HittableBee>());
                        (Main.npc[bee].ModNPC as HittableBee).isHittable = shouldBeHittable;

                        Main.npc[bee].velocity = new Vector2(beeVelocity * wallSide, 0f * (j % 2 == 0 ? -1f : 1f));//4 | 0.65
                        Main.npc[bee].scale = 1.15f;
                    }
                }
            }
            if (timer == timeBeforeSpawn + timeAfterSpawn)
            {
                timer = -1;
                wallSide *= -1;

                attackReps++;
                if (attackReps == 2)
                {
                    ChooseNextAttack();
                    //timer = -200;
                }
            }

        }

        Vector2 ophaPos = Vector2.Zero;
        float ophaOffset = 0f;
        public void OphanaimBees()
        {
            //Move to a random point on circle in certain angle range above player
            //Shoot out bees in 6 directions

            int timeToMove = 60 - (int)(15 * attackSpeed);
            int timeBeforeRelease = 40 - (int)(10 * attackSpeed);
            int timeAfterRelease = 140 - (int)(40 * attackSpeed);

            //How long until the shot bees move perpendicular to the way they were shot out
            int timeBeforeSecondDir = 60 - (int)(25 * attackSpeed); 

            if (timer == 0)
                ophaPos = new Vector2(0f, -200f).RotatedByRandom(MathHelper.PiOver4);

            if (timer < timeToMove)
            {
                drawOphaLines = true;

                BasicMovement(player.Center + ophaPos, 3f, 240f);

                FacePlayer();
            }
            else if (timer < timeToMove + timeBeforeRelease)
            {
                if (NPC.velocity.Length() > 10)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitX) * 10f;

                FacePlayer();

                NPC.velocity *= 0.87f;
            }
            else if (timer < timeToMove + timeBeforeRelease + timeAfterRelease)
            {
                drawOphaLines = false;

                NPC.velocity = Vector2.Zero;
                if (timer == timeToMove + timeBeforeRelease)
                {
                    float ophaniamCount = 9f;
                    float spineCount = 6;
                    float ophSide = Main.rand.NextBool() ? 1f : -1f;
                    for (int j = 0; j < spineCount; j++)
                    {
                        float jProg = (float)j / spineCount;

                        float rot = (MathHelper.TwoPi * jProg) + ophaOffset;

                        for (int i = 1; i < ophaniamCount; i++)
                        {
                            float prog = (float)(i + 1f) / ophaniamCount;

                            int bee = NPC.NewNPC(null, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<OphanaimBee>());
                            (Main.npc[bee].ModNPC as OphanaimBee).isHittable = i % 2 == 0;

                            (Main.npc[bee].ModNPC as OphanaimBee).secondDirection = new Vector2(0f, ophSide).RotatedBy(rot);
                            (Main.npc[bee].ModNPC as OphanaimBee).secondSpeed = 6f;
                            (Main.npc[bee].ModNPC as OphanaimBee).timeBeforeSecondDirection = timeBeforeSecondDir;
                            (Main.npc[bee].ModNPC as OphanaimBee).velFadeAmount = 0.85f;

                            Main.npc[bee].velocity = new Vector2(5 + (12f * i), 0f).RotatedBy(rot);
                        }
                    }
                }
            }
            else
            {
                attackReps++;
                timer = -1;

                ophaOffset = (ophaOffset == 0 ? MathHelper.PiOver2 : 0f);

                if (attackReps == 2)
                {
                    ChooseNextAttack();
                }
            }

        }

        #region MovementCode
        void BasicMovement(Vector2 goalPos, float moveSpeed = 6f, float clampDistance = 240f)
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
