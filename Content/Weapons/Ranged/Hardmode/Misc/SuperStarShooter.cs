using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest;
using VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    public class SuperStarShooterOverride : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SuperStarCannon);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.SuperStarCannon,
                    AnimTime: 20,
                    NormalXOffset: 16f,
                    DestXOffset: 0f,
                    YRecoilAmount: 0.21f,
                    HoldOffset: new Vector2(0f, 1f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_67") with { Volume = 0.2f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style5 = new SoundStyle("Terraria/Sounds/Research_2") with { Volume = 0.15f, Pitch = 1f, };
            SoundEngine.PlaySound(style5, player.Center);

            SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/starUIToss") with { Volume = .07f, Pitch = 0.7f, PitchVariance = 0.15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Volume = .2f, Pitch = .55f, PitchVariance = .1f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, player.Center);

            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.25f;

            //Dust d2 = Dust.NewDustPerfect(position + velNormalized * 3f, ModContent.DustType<CirclePulse>(), velNormalized * 3f, newColor: Color.OrangeRed);
            //CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            //b2.drawLayer = "Dusts";
            //d2.customData = b2;
            //d2.scale = circlePulseSize * 0.05f;

            return true;
        }
    }
    public class SuperStarShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SuperStar);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && false)
            {
                Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<SuperStarShooterConstellationTest>(), 0, 0, projectile.owner, projectile.whoAmI);
            }

            if (timer == 0 && false)
            {
                Vector2 pulsePos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 30f;

                int pulse = Projectile.NewProjectile(null, pulsePos, projectile.velocity.SafeNormalize(Vector2.UnitX) * 1f, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
                (Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = Color.Lerp(Color.Gold, Color.Orange, 0.75f);
                Main.projectile[pulse].rotation = projectile.velocity.ToRotation();
            }

            currentRot = projectile.velocity.ToRotation();

            int trailCount = 10; 
            previousRotations.Add(currentRot);
            previousPositions.Add(projectile.Center); //40

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            overallAlpha = 1f;// Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.06f), 0f, 1f);

            float fadeInTime = Math.Clamp((timer + 3f) / 20f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            projectile.soundDelay = 10;

            if (timer % 4 == 0)
            {
                fireballFrame = (fireballFrame + 1) % 4;
            }

            timer++;

            return true;
        }

        int fireballFrame = 0;

        float overallScale = 0f;
        float overallAlpha = 0f;
        float currentRot = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D StarGlow = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarGlow").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float drawScale = projectile.scale * overallScale;

            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;
            Texture2D FireBall2 = Mod.Assets.Request<Texture2D>("Assets/Pixel/BigFireballWhite").Value;

            Color fireBallColor = Color.Lerp(Color.Orange, Color.Gold, 0.75f);

            Vector2 fireball2Pos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * 24f;
            Rectangle fireball2sourceRectangle = new Rectangle(0, (FireBall2.Height / 4) * fireballFrame, FireBall2.Width, (FireBall2.Height / 4));
            Vector2 fireball2origin = fireball2sourceRectangle.Size() / 2f;
            Vector2 fireball2Scale = new Vector2(0.55f, 1.25f) * drawScale * 1.05f;

            //Main.EntitySpriteDraw(FireBall2, fireball2Pos, fireball2sourceRectangle, Color.Orange with { A = 50 } * 0.5f, projectile.velocity.ToRotation() - MathHelper.PiOver2, fireball2origin, fireball2Scale, SpriteEffects.None);

            float time = (float)Main.timeForVisualEffects / 60f;
            for (float i = 220f; i < 1f; i += 0.5f)
            {
                float drawS = time % 0.5f / 0.5f;
                drawS = (drawS + i) % 1f;

                float drawA = drawS * 2f;
                if (drawA > 1f)
                    drawA = 2f - drawA;

                float fireballRot = projectile.velocity.ToRotation() - MathHelper.PiOver2;

                float dist = 4f;

                //Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                //Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Vector2 scalea = new Vector2(fireball2Scale.X, fireball2Scale.Y * (0.6f + drawS * 1f));

                Main.EntitySpriteDraw(FireBall2, fireball2Pos + new Vector2(0f, 0f), fireball2sourceRectangle, fireBallColor with { A = 100 } * drawA, fireballRot, new Vector2(FireBall2.Width / 2f, 100f), scalea, SpriteEffects.None);
            }


            Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -25f;
            for (int i = 0; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, fireBallColor with { A = 175 } * 0.35f, fireballRot, FireBall.Size() / 2f, drawScale * 1.05f, SpriteEffects.None);
            }

            Vector2 fireballPos2 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * 0f;
            //float time = (float)Main.timeForVisualEffects / 60f;
            for (float i = 0f; i < 1f; i += 0.5f)
            {
                float drawS = time % 0.5f / 0.5f;
                drawS = (drawS + i) % 1f;

                float drawA = drawS * 2f;
                if (drawA > 1f)
                    drawA = 2f - drawA;

                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Main.EntitySpriteDraw(FireBall, fireballPos2 + new Vector2(0f, 0f), null, Color.White with { A = 100 } * drawA, fireballRot, new Vector2((float)FireBall.Width / 2f, 10f), 0.3f + drawS * 0.5f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(StarGlow, drawPos, null, Color.Orange with { A = 200 } * overallAlpha, projectile.rotation, StarGlow.Size() / 2f, drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 50 } * overallAlpha, projectile.rotation, Star.Size() / 2f, drawScale, SpriteEffects.None);


            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D Trail = CommonTextures.Flare.Value;

            float drawScale = projectile.scale * 1f * overallScale;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float colorProg = (progress * 4f) % 1f;
                Color col = Color.Lerp(Color.Orange, Color.Gold, 0.35f);

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);// + Main.rand.NextVector2Circular(0f, 10f).RotatedBy(previousRotations[i]);

                Vector2 trailScale = new Vector2(1.5f, 0.7f * drawScale * Easings.easeInOutSine(progress));

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, col with { A = 200 } * 1f * progress,
                       previousRotations[i], Trail.Size() / 2f, trailScale, SpriteEffects.None);

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, Color.White with { A = 150 } * 0.85f * progress,
                    previousRotations[i], Trail.Size() / 2f, new Vector2(trailScale.X, trailScale.Y * 0.5f), SpriteEffects.None);
            }

            for (int i = 220; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float colorProg = (progress * 4f) % 1f;
                Color col = Color.Lerp(Color.Orange, Color.Gold, 0.75f);

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + new Vector2(0f, -100f);

                Vector2 trailScale = new Vector2(1.5f, 0.7f * drawScale * Easings.easeInOutSine(progress));

                //Main.EntitySpriteDraw(Trail, AfterImagePos, null, col with { A = 150 } * 1f * progress,
                //       previousRotations[i], Trail.Size() / 2f, trailScale, SpriteEffects.None);

                //Main.EntitySpriteDraw(FireBall, AfterImagePos, null, Color.HotPink with { A = 20 } * 1f * progress,
                //       previousRotations[i] + MathHelper.PiOver2, FireBall.Size() / 2f, new Vector2(trailScale.Y, trailScale.X), SpriteEffects.None);

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, Color.White with { A = 100 } * 0.85f * progress,
                    previousRotations[i], Trail.Size() / 2f, new Vector2(trailScale.X, trailScale.Y * 0.5f), SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.75f, Pitch = 0.15f, PitchVariance = 0.05f, MaxInstances = -1 }, projectile.position);

            Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Gold, Scale: 0.25f);

            da.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 20,
                overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 9 + Main.rand.Next(3); i++)
            {
                Color col = Color.Gold;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 7f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }

            Color newColor7 = Color.CornflowerBlue;
            if (Main.tenthAnniversaryWorld && (projectile.type == 12 || projectile.type == 955))
            {
                newColor7 = Color.HotPink;
                newColor7.A /= 2;
            }
            for (int num635 = 0; num635 < 7; num635++)
            {
                ////Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            }
            for (float num636 = 0f; num636 < 1f; num636 += 0.225f)  //0.125
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num636 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
            }
            for (float num637 = 0f; num637 < 1f; num637 += 0.35f) //0.25f
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num637 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
            }
            Vector2 vector54 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector54 / 2f, vector54 + new Vector2(400f))))
            {
                for (int num638 = 0; num638 < 4; num638++)
                {
                    Gore.NewGore(projectile.GetSource_FromThis(), projectile.position, Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * projectile.velocity.Length(), Utils.SelectRandom<int>(Main.rand, 16, 17, 17, 17, 17, 17, 17, 17));
                }
            }

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5 + Main.rand.Next(3); i++)
            {
                Color col = Color.Gold;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }
        }
    }


    public class SuperStarShooterConstellationTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private class Constellation
        {
            public Vector2 position;
            public Vector2 velocity;
            public Color color;
            public float alpha = 1f;
            public float scale = 1f;

            public List<int> connectingIndices = new List<int>();

            public int lifeTime = 0;
            public int timer = 0;
            public void Update()
            {
                //alpha *= 0.9f;

                float fadeInTime = Math.Clamp((timer + 3f) / 25f, 0f, 1f); //3 20
                scale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

                if (timer > 6)
                    alpha -= 0.06f;

                velocity *= 0.95f; //92
                position += velocity;

                timer++;
            }

            public Constellation(Vector2 position, Vector2 velocity, Color color)
            {
                this.position = position;
                this.velocity = velocity;
                this.color = color;
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.hide = true;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 250; //180
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public Color col = Color.DodgerBlue;


        int timer = 0;
        float overallAlpha = 1f;
        float overallScale = 1f;

        List<Constellation> constellations = new List<Constellation>();
        public override void AI()
        {
            if (timer == 0)
                Projectile.ai[1] = Main.rand.NextBool() ? 1f : -1f;

            Projectile parent = Main.projectile[(int)Projectile.ai[0]];

            if (parent.active == false)
            {
                Projectile.timeLeft--;
                //Projectile.timeLeft = 100;
                Projectile.active = false;
                return;
            }

            Projectile.Center = parent.Center;

            if (timer % 4 == 0 && parent.active)
            {
                Vector2 dir = parent.velocity.RotatedBy(MathHelper.PiOver2).RotatedByRandom(1f).SafeNormalize(Vector2.UnitX);

                Vector2 randomDir = dir * Main.rand.NextFloat(4f, 8f) * Projectile.ai[1];
                constellations.Add(new Constellation(Projectile.Center + randomDir * Main.rand.NextFloat(1f, 3f), (parent.velocity * 0.1f) + (randomDir * 0.1f), Color.Gold)); //* Main.rand.NextFloat(1f, 3f)

                Projectile.ai[1] *= -1;
            }

            foreach (Constellation c in constellations)
            {
                c.Update();
                if (c.alpha <= 0.1f)
                {
                    //constellations.Remove(c);
                }
            }


            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile parent = Main.projectile[(int)Projectile.ai[0]];

            Texture2D Pixel = CommonTextures.CrispStarPMA.Value;// Mod.Assets.Request<Texture2D>("Assets/Pixel/ConstellationPixel").Value;
            Texture2D Line = CommonTextures.SoulSpikePMA.Value; //Mod.Assets.Request<Texture2D>("Assets/Trails/Clear/BasicGlowSliver").Value; //CommonTextures.SoulSpikePMA.Value;

            Color betweenBlue = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0f); //bp/white
            Color betweenPink = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f); //bp/white
            Color golden = Color.Lerp(Color.Gold, Color.Orange, 0.75f); //bp/white

            //Draw connections
            for (int i = 1; i < constellations.Count; i++)
            {
                Constellation currentConst = constellations[i];
                Constellation previousConst = constellations[i - 1];

                if (previousConst.alpha > 0.1f)
                {
                    Vector2 between = (currentConst.position - previousConst.position);
                    Vector2 scale = new Vector2(between.Length() / Line.Width, 0.65f * currentConst.scale);

                    Main.spriteBatch.Draw(Line, previousConst.position - Main.screenPosition, null, golden with { A = 175 } * previousConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(Line, previousConst.position - Main.screenPosition, null, Color.White with { A = 175 } * previousConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), new Vector2(scale.X, scale.Y * 0.25f), SpriteEffects.None, 0f);
                }
            }

            //Draw connect from last point to parent
            Constellation finalConst = constellations.Last();
            if (finalConst.alpha > 0.1f)
            {
                Vector2 between = (parent.Center - finalConst.position);
                Vector2 scale = new Vector2(between.Length() / Line.Width, 0.65f * finalConst.scale);

                Main.spriteBatch.Draw(Line, finalConst.position - Main.screenPosition, null, golden with { A = 175 } * finalConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Line, finalConst.position - Main.screenPosition, null, Color.White with { A = 175 } * finalConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), new Vector2(scale.X, scale.Y * 0.25f), SpriteEffects.None, 0f);
            }

            //Draw stars
            for (int i = 0; i < constellations.Count; i++)
            {
                Constellation currentConst = constellations[i];

                float starAlpha = Easings.easeOutQuad(currentConst.alpha);

                Main.spriteBatch.Draw(Pixel, currentConst.position - Main.screenPosition, null, golden with { A = 160 } * starAlpha, 0f, Pixel.Size() / 2f, 0.5f * currentConst.scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Pixel, currentConst.position - Main.screenPosition, null, Color.White with { A = 160 } * starAlpha, 0f, Pixel.Size() / 2f, 0.25f * currentConst.scale, SpriteEffects.None, 0f);
            }

            return false;
        }

    }

    public class SuperStarShotOverrideOld : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SuperStar) && false;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            currentRot = projectile.velocity.ToRotation();

            #region trail (add 3 positions each frame)
            int trailCount = 36; //28
            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.33f);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.66f);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);
            #endregion

            #region dust
            if (timer % 2 == 0 && Main.rand.NextBool(2) && timer > 3)
            {
                Color col = Color.Lerp(Color.Orange, Color.Gold, 0.5f);

                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: 0.38f);

                de.velocity *= 0.45f;
                de.velocity += currentRot.ToRotationVector2() * 3f;

            }

            #endregion

            timer++;


            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.06f), 0f, 1f);

            float fadeInTime = Math.Clamp((timer + 3f) / 20f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            projectile.soundDelay = 10;

            return true;
        }

        float overallScale = 0f;
        float overallAlpha = 0f;
        float currentRot = 0f;
        public List<float> previousVelRots = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            CerobaStyleDraw(projectile);

            if (previousPostions.Count > 0)
                DrawProjWithStarryTrail(projectile, lightColor, 0f);
            return false;
        }

        public void CerobaStyleDraw(Projectile projectile)
        {
            float adjustedScale = projectile.scale * 1f * overallScale;

            Texture2D FireBall = CommonTextures.FireBallBlur.Value;
            Texture2D FireBallPixel = CommonTextures.Extra_91.Value;

            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;
            Texture2D CrispStar = CommonTextures.CrispStarPMA.Value;

            Texture2D VStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;

            Vector2 off = (currentRot).ToRotationVector2() * -25f * adjustedScale;

            Vector2 pos = projectile.Center - Main.screenPosition;
            float rot = currentRot + MathHelper.PiOver2;

            float thisAlpha = overallAlpha * 0.15f;

            //Glorb
            Color orbCol1 = Color.Yellow * 0.75f;
            Color orbCol2 = Color.Gold * 0.525f;
            Color orbCol3 = Color.Goldenrod * 0.375f;

            float scale1 = 0.75f * 1.25f;
            float scale2 = 1.6f * 1.25f;
            float scale3 = 2.5f * 1.25f;

            Main.EntitySpriteDraw(Glow, pos, null, orbCol1 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale1 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol2 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale2 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol3 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale3 * 0.55f, SpriteEffects.None);

            //Star
            Vector2 starScale = new Vector2(0.85f, 1.45f) * (1f - overallScale);
            Main.EntitySpriteDraw(CrispStar, pos, null, Color.Goldenrod with { A = 0 }, rot, CrispStar.Size() / 2f, 1.4f * starScale, SpriteEffects.None);
            Main.EntitySpriteDraw(CrispStar, pos, null, Color.White with { A = 0 }, rot, CrispStar.Size() / 2f, 0.55f * starScale, SpriteEffects.None); //0.6

            #region after image
            if (previousVelRots != null && previousPostions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i++)
                {
                    float progress = (float)i / previousVelRots.Count;

                    float size = (1f - (progress * 0.5f)) * adjustedScale;

                    float colVal = progress;

                    Color col = Color.Lerp(Color.Orange, Color.Gold, progress) * progress * 0.75f;

                    float size2 = (1f - (progress * 0.15f)) * adjustedScale;

                    Main.EntitySpriteDraw(FireBallPixel, previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(4f, 4f), null, col with { A = 0 } * 1.15f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                    Vector2 vec2Scale = new Vector2(0.25f * progress, 1.15f) * size;

                    Main.EntitySpriteDraw(FireBall, previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(8f, 8f), null, col with { A = 0 } * 1.25f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
                }

            }
            #endregion

            //Fireball
            Vector2 v2scale = new Vector2(1.25f, 1f);
            Main.EntitySpriteDraw(FireBall, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink with { A = 0 } * thisAlpha, rot, FireBall.Size() / 2f, adjustedScale * v2scale, SpriteEffects.None);

            //Glow Star
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            for (int i = 0; i < 4; i++)
            {
                Color col = Color.Orange;
                Main.EntitySpriteDraw(VStar, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), null, col with { A = 0 } * 0.8f * thisAlpha, projectile.rotation, VStar.Size() / 2f, adjustedScale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(VStar, drawPos, null, Color.Orange, projectile.rotation, VStar.Size() / 2f, adjustedScale, SpriteEffects.None);

            Main.EntitySpriteDraw(VStar, drawPos, null, Color.White with { A = 0 }, projectile.rotation, VStar.Size() / 2f, adjustedScale, SpriteEffects.None);
        }

        private void DrawProjWithStarryTrail(Projectile proj, Color projectileColor, SpriteEffects dir)
        {
            Color color = new Color(255, 255, 255, projectileColor.A - 255);
            Vector2 vector = proj.velocity;
            Color color2 = Color.Blue * 0.1f;
            Vector2 spinningpoint = new Vector2(0f, -4f);
            float num = 0f;
            float t = vector.Length();
            float num2 = Utils.GetLerpValue(3f, 5f, t, clamped: true);
            bool flag = true;
            if (true)
            {
                vector = proj.position - previousPostions[0];
                float num3 = vector.Length();
                if (num3 == 0f)
                {
                    vector = Vector2.UnitY;
                }
                else
                {
                    vector *= 5f / num3;
                }
                Vector2 origin = Main.MouseWorld;// new Vector2(proj.ai[0], proj.ai[1]);
                Vector2 center = Main.player[proj.owner].Center;
                float lerpValue = Utils.GetLerpValue(0f, 120f, origin.Distance(center), clamped: true);
                float num4 = 90f;

                float lerpValue2 = Utils.GetLerpValue(num4, num4 * (5f / 6f), proj.localAI[0], clamped: true);
                float lerpValue3 = Utils.GetLerpValue(0f, 120f, proj.Center.Distance(center), clamped: true);
                lerpValue *= lerpValue3;
                lerpValue2 *= Utils.GetLerpValue(0f, 15f, proj.localAI[0], clamped: true);
                
                color2 = Color.Gold * 0.15f;// * (lerpValue2 * lerpValue);

                spinningpoint = new Vector2(0f, -2f);
                float lerpValue4 = Utils.GetLerpValue(num4, num4 * (2f / 3f), proj.localAI[0], clamped: true);
                lerpValue4 *= Utils.GetLerpValue(0f, 20f, proj.localAI[0], clamped: true);
                num = -0.3f * (1f - lerpValue4);
                num += -1f * Utils.GetLerpValue(15f, 0f, proj.localAI[0], clamped: true);
                num *= lerpValue;
                num2 = lerpValue2 * lerpValue;

                num = 0f; //.15
                num2 = 1f;
            }
            Vector2 vector5 = proj.Center + vector;
            Texture2D value = TextureAssets.Projectile[856].Value;
            _ = new Rectangle(0, 0, value.Width, value.Height).Size() / 2f;
            Texture2D value2 = TextureAssets.Extra[91].Value;
            Rectangle value3 = value2.Frame();
            Vector2 origin2 = new Vector2((float)value3.Width / 2f, 10f);
            _ = Color.Cyan * 0.5f * num2;
            Vector2 vector2 = new Vector2(0f, proj.gfxOffY);
            float num5 = (float)(Main.timeForVisualEffects * 1.5f) / 60f;
            Vector2 vector3 = vector5 + vector * 0.5f;
            Color color3 = Color.White * 0.5f * num2;
            color3.A = 0;
            Color color4 = color2 * num2;
            color4.A = 0;
            Color color5 = color2 * num2;
            color5.A = 0;
            Color color6 = color2 * num2;
            color6.A = 0;
            float num6 = vector.ToRotation();

            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5), value3, color4, num6 + (float)Math.PI / 2f, origin2, overallScale * (1.5f + num), SpriteEffects.None);
            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + (float)Math.PI * 2f / 3f), value3, color5, num6 + (float)Math.PI / 2f, origin2, overallScale * (1.1f + num), SpriteEffects.None);
            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + 4.18879032f), value3, color6, num6 + (float)Math.PI / 2f, origin2, overallScale * (1.3f + num), SpriteEffects.None);
            Vector2 vector4 = vector5 - vector * 0.5f;
            for (float num7 = 0f; num7 < 1f; num7 += 0.5f)
            {
                float num8 = num5 % 0.5f / 0.5f;
                num8 = (num8 + num7) % 1f;
                float num9 = num8 * 2f;
                if (num9 > 1f)
                {
                    num9 = 2f - num9;
                }
                Main.EntitySpriteDraw(value2, vector4 - Main.screenPosition + vector2, value3, color3 * num9, num6 + (float)Math.PI / 2f, origin2, overallScale * (0.3f + num8 * 0.5f), SpriteEffects.None);
            }
            if (flag)
            {
                float rotation = proj.rotation + proj.localAI[1];
                _ = (float)Main.timeForVisualEffects / 240f;
                _ = Main.GlobalTimeWrappedHourly;
                float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
                globalTimeWrappedHourly %= 5f;
                globalTimeWrappedHourly /= 2.5f;
                if (globalTimeWrappedHourly >= 1f)
                {
                    globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
                }
                globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
                Vector2 position = proj.Center - Main.screenPosition;
                Main.instance.LoadItem(75);
                Texture2D value4 = TextureAssets.Item[75].Value;
                Rectangle rectangle = value4.Frame(1, 8);
                Main.EntitySpriteDraw(origin: rectangle.Size() / 2f, texture: value4, position: position, sourceRectangle: rectangle, color: color, rotation: rotation, scale: overallScale * proj.scale * 1f, effects: SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.75f, Pitch = 0.15f, PitchVariance = 0.05f, MaxInstances = -1 }, projectile.position);

            Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Gold, Scale: 0.25f);

            da.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 20,
                overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 9 + Main.rand.Next(3); i++)
            {
                Color col = Color.Gold;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 7f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }

            Color newColor7 = Color.CornflowerBlue;
            if (Main.tenthAnniversaryWorld && (projectile.type == 12 || projectile.type == 955))
            {
                newColor7 = Color.HotPink;
                newColor7.A /= 2;
            }
            for (int num635 = 0; num635 < 7; num635++)
            {
                ////Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            }
            for (float num636 = 0f; num636 < 1f; num636 += 0.225f)  //0.125
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num636 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
            }
            for (float num637 = 0f; num637 < 1f; num637 += 0.35f) //0.25f
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num637 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
            }
            Vector2 vector54 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector54 / 2f, vector54 + new Vector2(400f))))
            {
                for (int num638 = 0; num638 < 4; num638++)
                {
                    Gore.NewGore(projectile.GetSource_FromThis(), projectile.position, Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * projectile.velocity.Length(), Utils.SelectRandom<int>(Main.rand, 16, 17, 17, 17, 17, 17, 17, 17));
                }
            }

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5 + Main.rand.Next(3); i++)
            {
                Color col = Color.Gold;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }
        }
    }


}
