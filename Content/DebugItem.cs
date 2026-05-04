using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.FeatheredFoe;
using VFXPlus.Content.Particles;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest;
using VFXPlus.Content.VFXTest.Aero;


namespace VFXPlus.Content
{
    public class DebugItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 22;

            Item.width = 46;
            Item.height = 28;

            Item.useTime = Item.useAnimation = 7; 
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.knockBack = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.shootSpeed = 10f;

            Item.shoot = ProjectileID.TopazBolt;

            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
        }
        public override bool AltFunctionUse(Player player) => true;
        
        
        bool tick = false;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                //ShaderParticleHandler.RemoveParticle();
                //Vector2 pos = Main.MouseWorld;
                //NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<FeatheredFoeBoss>());
                return false;
            }
            else 
            {
                //Vector2 pos = Main.MouseWorld;
                //NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<ReduxQueenBee>());
                //return false;
            }

            //Vector2 thisVel = new Vector2(0f, -4f).RotateRandom(0.25f);
            //Dust dad = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<MediumSmoke>(), Velocity: thisVel,
            //    newColor: Color.Tan with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f));
            //dad.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.1f, 0.1f); //12 28
            //dad.rotation = Main.rand.NextFloat(6.28f);

            int windFX3 = Projectile.NewProjectile(null, position, velocity.SafeNormalize(Vector2.UnitX) * 8f, ProjectileID.PainterPaintball, 1, 0, Main.myPlayer);

            //int windFX3 = Projectile.NewProjectile(null, Main.MouseWorld, velocity.SafeNormalize(Vector2.UnitX) * 5f, ModContent.ProjectileType<AngleGlowTest2>(), 1, 0, Main.myPlayer);
            ///Main.projectile[windFX3].scale = 0.85f;

            return false;

            for (int i = 220; i < 2; i++)
            {
                //Good one
                Vector2 myvel = new Vector2(0f, -1.75f).RotatedByRandom(6.28f) * Main.rand.NextFloat(6f, 12f) * 1f;

                int smoke = Projectile.NewProjectile(null, Main.MouseWorld, myvel, ModContent.ProjectileType<WindAnimTest>(), 1, 0, Main.myPlayer);
            }

            /*
            int are5 = Projectile.NewProjectile(null, Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<H3Impact>(), 0, 0, player.whoAmI);
            Main.projectile[are5].rotation = 0f;
            Main.projectile[are5].scale = 1.5f;

            Color[] colarr = { Color.White, Color.Orange, Color.OrangeRed };
            (Main.projectile[are5].ModProjectile as H3Impact).cols = colarr;
            (Main.projectile[are5].ModProjectile as H3Impact).xScaleMult = 0.5f;
            (Main.projectile[are5].ModProjectile as H3Impact).yScaleMult = 1f;
            (Main.projectile[are5].ModProjectile as H3Impact).pixelize = false;
            (Main.projectile[are5].ModProjectile as H3Impact).additive = false;

            int are6 = Projectile.NewProjectile(null, Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<H3Impact>(), 0, 0, player.whoAmI);
            Main.projectile[are6].rotation = MathHelper.PiOver2;
            Main.projectile[are6].scale = 1.5f;

            (Main.projectile[are6].ModProjectile as H3Impact).cols = colarr;
            (Main.projectile[are6].ModProjectile as H3Impact).xScaleMult = 0.5f;
            (Main.projectile[are6].ModProjectile as H3Impact).yScaleMult = 1f;
            (Main.projectile[are6].ModProjectile as H3Impact).pixelize = false;
            (Main.projectile[are6].ModProjectile as H3Impact).additive = false;
            */

            for (int i = 220; i < 2; i++)
            {
                float fireScale = Main.rand.NextFloat(1.35f, 1.55f);
                float alphaFade = Main.rand.NextFloat(0.94f, 0.95f);
                float scaleFade = Main.rand.NextFloat(1.03f, 1.05f);

                Color thisCol = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.25f);
                // Color.Lerp(Color.Purple, Color.DeepPink, 0f);// Color.Lerp(Color.DodgerBlue, Color.Blue, 0.25f);// Color.Lerp(Color.OrangeRed, Color.Red, 0.35f);

                //Vector2 myvel = new Vector2(0f, -1.5f).RotatedByRandom(6.28f) * Main.rand.NextFloat(2f, 4.5f);
                //FireParticle fire1 = new FireParticle(Main.MouseWorld, myvel, fireScale, thisCol, colorMult: 0.5f, bloomAlpha: 2f, AlphaFade: alphaFade, VelFade: 0.9f, RotPower: 0.02f);
                //fire1.randomRotPower = 0.5f;
                //fire1.scaleFadePower = 1.05f;
                //fire1.timeBeforeStartAlphaFade = 5;
                //ShaderParticleHandler.SpawnParticle(fire1);

                //Vector2 myvel = new Vector2(0f, -1.75f).RotatedByRandom(0.2f) * Main.rand.NextFloat(8f, 14f) * 1f;
                //FireParticle fire = new FireParticle(Main.MouseWorld + Main.rand.NextVector2Circular(5f, 5f), myvel, 1f, thisCol, colorMult: 0.5f, bloomAlpha: 1f,
                //    AlphaFade: 0.95f, RotPower: 0.01f);
                //ShaderParticleHandler.SpawnParticle(fire);

                //Good one
                Vector2 myvel = new Vector2(0f, -1.75f).RotatedByRandom(0.2f) * Main.rand.NextFloat(8f, 14f) * 1.75f;
                FireParticle fire1 = new FireParticle(Main.MouseWorld, myvel, fireScale * 1.75f, thisCol, colorMult: 2f, bloomAlpha: 1.65f, AlphaFade: alphaFade, VelFade: 0.87f, RotPower: 0.02f);
                fire1.randomRotPower = 0.5f;
                fire1.scaleFadePower = 1.05f;
                ShaderParticleHandler.SpawnParticle(fire1);
            }



            for (int i = 220; i < 5; i++)
            {
                float prog = (float)i / 5;


                Vector2 veloF = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(2f, 4f);

                float fireScale = Main.rand.NextFloat(1.5f, 2f);

                FireParticle fire = new FireParticle(Main.MouseWorld, veloF, fireScale, Color.Lerp(Color.Aqua, Color.Aquamarine, 0.35f), colorMult: 0.75f, bloomAlpha: 0.75f, AlphaFade: 0.91f, VelFade: 0.92f);
                fire.scaleFadePower = 1.05f; //1.05
                fire.randomRotPower = 0.5f;
                ShaderParticleHandler.SpawnParticle(fire);


            }

            for (int i = 220; i < 6; i++)
            {
                float prog = (float)i / 5f;

                //Vector2 vel =  velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 35f);
                //float myScale = Main.rand.NextFloat(1.25f, 1.5f);
                //FireParticle fire = new FireParticle(Main.MouseWorld, vel * 1.5f, myScale, Color.OrangeRed, colorMult: 1f, bloomAlpha: 1f, AlphaFade: 0.92f);
                //fire.scaleFadePower = 1.1f;
                //ShaderParticleHandler.SpawnParticle(fire);

                Vector2 vel = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 28f); //30
                float myScale = Main.rand.NextFloat(1.25f, 1.5f);
                FireParticle fire = new FireParticle(Main.MouseWorld, vel * 1.5f * 1.5f, myScale * 1.5f, Color.OrangeRed, colorMult: 1f, bloomAlpha: 1f, AlphaFade: 0.92f, VelFade: 0.8f);
                fire.scaleFadePower = 1.1f;
                
                ShaderParticleHandler.SpawnParticle(fire);


                //FireParticle fire = new FireParticle(Main.MouseWorld, Main.projectile[smoke].velocity, 1.5f, Color.DeepSkyBlue, colorMult: 1f, bloomAlpha: 1f);
                //ShaderParticleHandler.SpawnParticle(fire);
            }



            //int windFX2 = Projectile.NewProjectile(null, player.Center, velocity.SafeNormalize(Vector2.UnitX) * 0f, ModContent.ProjectileType<InfernoForkVFX>(), 0, 0, Main.myPlayer);
            //int windFX3 = Projectile.NewProjectile(null, player.Center + new Vector2(-200f, 0f), velocity.SafeNormalize(Vector2.UnitX) * 0f, ModContent.ProjectileType<InfernoForkVFXImOld>(), 0, 0, Main.myPlayer);

            //FlashSystem.SetCAFlashEffect(0.08f, 35, 1f, 0.5f, true);



            return false;

            //Color purple3 = new Color(121, 7, 179);
            Color betweenGold = Color.Lerp(Color.Gold, Color.OrangeRed, 0.2f);

            for (int i = 220; i < 13; i++)
            {
                float prog = (float)i / 13f;

                float proggg = Main.rand.NextFloat();
                Color col = Color.Lerp(betweenGold with { A = 0 }, Color.Purple * 1.25f, Easings.easeOutQuad(1f - prog));

                Dust d = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 3f) * 2.5f,
                    newColor: col * Easings.easeOutCubic(prog), Scale: Main.rand.NextFloat(1.15f, 1.5f) * 1f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(15, 25), 0.93f, 0.01f, 0.9f); //12 28

            }

            for (int i = 220; i < 7 + Main.rand.Next(0, 6); i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.Purple * 1f : Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);


                float velMult = Main.rand.NextFloat(1f, 6f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult);
                Dust dust = Dust.NewDustPerfect(Main.MouseWorld + randomStart * 5f, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0,
                    newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));

                dust.scale *= 1.75f;

                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 0.95f);
            }

            int[] orbitValues1 = { 25, 100, 175, //20 80 140 | 40 10 160 | 60 120 180
                                  50,  125, 200,
                                  75,  150, 225 };

            int[] orbitValues2 = { 30, 86, 142, //20 60 40 | 100 40 100 | 60 80 80
                                  38,  94, 150,
                                  46,  102, 158 };

            int[][] orbitValues = { orbitValues1, orbitValues2 };

            int numberOfFeahters = 9;
            for (int ab = 0; ab < 2; ab++)
            {
                for (int index = 1; index <= numberOfFeahters; index++)
                {
                    int orbfeather = Projectile.NewProjectile(null, player.Center, Vector2.Zero, ModContent.ProjectileType<OrbitingFeather>(), damage, 0, Main.myPlayer);

                    if (Main.projectile[orbfeather].ModProjectile is OrbitingFeather of)
                    {
                        of.timeToOrbit = 60 + (orbitValues[ab][index - 1] * 1) + (225 * ab);  //60 * index;
                        of.orbitVector = new Vector2(405f - (85 * ab), 0f).RotatedBy(MathHelper.TwoPi * ((index - 1f) / numberOfFeahters));
                        of.orbitVal = 300f - (100 * ab);
                        of.rotSpeed = ab == 0 ? 1.85f : 1.5f;
                    }

                }
            }

            int barrier = Projectile.NewProjectile(null, player.Center + new Vector2(0f, -255f), Vector2.Zero, ModContent.ProjectileType<WindBarrier>(), 0, 0, Main.myPlayer);
            (Main.projectile[barrier].ModProjectile as WindBarrier).center = player.Center;

            return false;
            
            //return false;
            Vector2 posAA = player.Center + new Vector2(-450f, 600f);
            Vector2 posBB = player.Center + new Vector2(-450f, -600f);
            Vector2 posCC = player.Center + new Vector2(-450f, 0f);

            Vector2 velAA = new Vector2(8f, 0f);
            int bA2 = Projectile.NewProjectile(null, posAA, velAA, ModContent.ProjectileType<FFWindOrb>(), 10, 0, player.whoAmI);
            int bB2 = Projectile.NewProjectile(null, posBB, velAA, ModContent.ProjectileType<FFWindOrb>(), 10, 0, player.whoAmI);
            int bC2 = Projectile.NewProjectile(null, posCC, velAA, ModContent.ProjectileType<FFWindOrb>(), 10, 0, player.whoAmI);

            (Main.projectile[bA2].ModProjectile as FFWindOrb).startDir = -1;

            return false;

            
            //

            for (int i = 220; i < 22 + Main.rand.Next(0, 2); i++) //4 //2,2
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) * 9f;

                Color rainbow = Main.hslToRgb((i * 0.05f + 0.5f) % 1f, 1f, 0.4f, 0) * 1f;



                Dust p = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<PixelatedLineSpark>(), vel,
                    newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1f);

                p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                    1f, 0.5f, shouldFadeColor: false);

                if (i % 1 == 0)
                {
                    Dust p2 = Dust.NewDustPerfect(Main.MouseWorld + vel * 4f, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.4f);
                    p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.05f);
                }

            }
            //return false;

            int number_of_feathers = 6;
            for (int a1 = 2220; a1 < number_of_feathers; a1++)
            {
                float prog = (float)a1 / (float)number_of_feathers;

                int feather = Projectile.NewProjectile(null, Main.MouseWorld, new Vector2(13f, 0f).RotatedBy(MathHelper.TwoPi * prog), ModContent.ProjectileType<CurvingFeather>(), 1, 1);
                (Main.projectile[feather].ModProjectile as CurvingFeather).curveValue = -0.1f;

            }

            /*
            CirclePulseBehavior cpb2 = new CirclePulseBehavior(10f, false, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(player.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(player.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);
            */

            Vector2 posA = player.Center + new Vector2(0f, 0f);
            Vector2 posB = player.Center + new Vector2(0f, 90f);
            Vector2 posC = player.Center + new Vector2(0f, -90f);

            int ar = Projectile.NewProjectile(null, posA, new Vector2(0f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
            int br = Projectile.NewProjectile(null, posB, new Vector2(0f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
            int cr = Projectile.NewProjectile(null, posC, new Vector2(0f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);

            (Main.projectile[ar].ModProjectile as MadisonTornado).startDir = -1;
            (Main.projectile[br].ModProjectile as MadisonTornado).startDir = 1;
            (Main.projectile[cr].ModProjectile as MadisonTornado).startDir = -1;


            /*
            for (int i = 0; i < 22 + Main.rand.Next(0, 2); i++) //4 //2,2
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);

                Color rainbow = Main.hslToRgb((i * 0.05f + 0.5f) % 1f, 1f, 0.6f, 0) * 1f;



                Dust p = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<PixelatedLineSpark>(), vel,
                    newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.7f);

                p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                    1f, 0.5f, shouldFadeColor: false);

                if (i % 1 == 0)
                {
                    Dust p2 = Dust.NewDustPerfect(Main.MouseWorld + vel * 4f, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.2f);
                    p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.05f);
                }

            }
            */
            //return false;



            return false;

            //int a21 = Projectile.NewProjectile(null, position, velocity * 0f, ModContent.ProjectileType<GoozmaPrismStudy>(), 2, 0, player.whoAmI);

            //Main.projectile[a21].direction = (Main.rand.Next(5) > 2 ? 1 : -1);
            //Main.projectile[a21].rotation = Main.rand.NextFloat(-2f, 2f);
            //Main.projectile[a21].ai[0] = -66;
            //Main.projectile[a21].ai[1] = 0;
            //Main.projectile[a21].ai[2] = Main.rand.NextFloat(-3f, 3f);


            //!!!!!!!int b = Projectile.NewProjectile(null, position, velocity.SafeNormalize(Vector2.UnitX) * 17f, ProjectileID.WaterBolt, 2, 0, player.whoAmI);



            //Projectile.NewProjectile(null, position, velocity, ProjectileID.ShadowFlame, 2, 0, player.whoAmI);

            //Projectile.NewProjectile(null, position, velocity.RotatedBy(2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);
            //Projectile.NewProjectile(null, position, velocity.RotatedBy(-2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);


            return false;
        }
        
    }
}