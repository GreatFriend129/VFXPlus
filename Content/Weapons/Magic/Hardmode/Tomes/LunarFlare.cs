using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using System.Linq;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using Terraria.GameContent;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    public class LunarFlareShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LunarFlare) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LunarFlareToggle;
        }

        float drawScale = 0;
        float drawAlpha = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 35; //55

            if (timer % 2 == 0)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }

            float timeForPopInAnim = 180;
            float animProgress = Math.Clamp((timer + 30) / timeForPopInAnim, 0f, 1f);

            drawScale = 0.25f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1f)) * 0.75f;

            if (projectile.ai[1] != -1)
                Lighting.AddLight(projectile.Center, Color.Aqua.ToVector3() * projectile.scale * 1f); //0.030

            if (timer % 5 == 0 && Main.rand.NextBool(2) && projectile.ai[1] != -1)
            {
                Color between = Color.Lerp(Color.Aquamarine, Color.Aqua, Main.rand.NextFloat());

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 7f),
                    newColor: between, Scale: Main.rand.NextFloat(0.6f, 1.2f) * 0.35f);
                //p.alpha = 2;
                p.velocity += projectile.velocity * 0.45f;

                ///p.customData = DustBehaviorUtil.AssignBehavior_GSSBase(rotPower: 0.55f, postSlowPower: 0.89f, velToBeginShrink: 3.75f, fadePower: 0.92f, shouldFadeColor: false);
                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.55f, postSlowPower: 0.89f, velToBeginShrink: 4f, fadePower: 0.92f, shouldFadeColor: false); 
            }

            #region VanillaAI
            if (projectile.ai[1] != -1f && projectile.position.Y > projectile.ai[1])
            {
                projectile.tileCollide = true;
            }
            if (projectile.position.HasNaNs())
            {
                projectile.Kill();
                return false;
            }
            //bool num205 = WorldGen.SolidTile(Framing.GetTileSafely((int)projectile.position.X / 16, (int)projectile.position.Y / 16));
            //Dust dust22 = Main.dust[Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 229)];
            //dust22.position = projectile.Center;
            //dust22.velocity = Vector2.Zero;
            //dust22.noGravity = true;
            //if (num205)
            //{
            //    dust22.noLight = true;
            //}
            if (projectile.ai[1] == -1f)
            {
                if (projectile.ai[0] == 0f)
                {
                    Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<LunarExplosionAnim>(), 0, 0f);
                }

                projectile.ai[0] += 1f;
                projectile.velocity = Vector2.Zero;
                projectile.tileCollide = false;
                projectile.penetrate = -1;
                projectile.position = projectile.Center;
                projectile.width = (projectile.height = 140);
                projectile.Center = projectile.position;
                projectile.alpha -= 10;
                if (projectile.alpha < 0)
                {
                    projectile.alpha = 0;
                }
                if (++projectile.frameCounter >= projectile.MaxUpdates * 3)
                {
                    projectile.frameCounter = 0;
                    projectile.frame++;
                }
                if (projectile.ai[0] >= (float)(Main.projFrames[projectile.type] * projectile.MaxUpdates * 3))
                {
                    projectile.Kill();
                }
                return false;
            }
            projectile.alpha = 255;
            if (projectile.numUpdates == 0)
            {
                int num206 = -1;
                float num207 = 60f;
                for (int num208 = 0; num208 < 200; num208++)
                {
                    NPC nPC2 = Main.npc[num208];
                    if (nPC2.CanBeChasedBy(this))
                    {
                        float num209 = projectile.Distance(nPC2.Center);
                        if (num209 < num207 && Collision.CanHitLine(projectile.Center, 0, 0, nPC2.Center, 0, 0))
                        {
                            num207 = num209;
                            num206 = num208;
                        }
                    }
                }
                if (num206 != -1)
                {
                    projectile.ai[0] = 0f;
                    projectile.ai[1] = -1f;
                    projectile.netUpdate = true;
                    return false;
                }
            }
            #endregion

            timer++;
            return false;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawShit(projectile);
            });

            if (projectile.ai[1] == -1)
                return false;

            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value; //GF
            float rot = projectile.velocity.ToRotation();

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);

            Main.EntitySpriteDraw(line, drawPos, null, Color.White with { A = 0 }, rot, line.Size() / 2f, 1f * new Vector2(1f, 0.55f * drawScale), SpriteEffects.None);
            Main.EntitySpriteDraw(line, drawPos, null, Color.Aquamarine with { A = 0 }, rot, line.Size() / 2f, 2f * new Vector2(1f, 0.55f * drawScale), SpriteEffects.None);

            return false;
        }

        public void DrawShit(Projectile projectile)
        {
            if (projectile.ai[1] == -1)
                return;

            float rot = projectile.velocity.ToRotation();

            //Orb
            //Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D orb = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;
            Vector2 originPoint = projectile.Center - Main.screenPosition;

            Color col1 = Color.White * 0.75f;
            Color col2 = Color.SkyBlue * 0.525f * 0.75f;
            Color col3 = Color.Aqua * 0.375f * 0.75f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            float scale = 1.25f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, originPoint, null, col1 with { A = 0 }, rot, orb.Size() / 2f, scale1 * scale * new Vector2(1f, 0.5f * drawScale), SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col2 with { A = 0 }, rot, orb.Size() / 2f, scale2 * scale * sineScale1 * new Vector2(1f, 0.5f * drawScale), SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col3 with { A = 0 }, rot, orb.Size() / 2f, scale3 * scale * sineScale2 * new Vector2(1f, 0.5f * drawScale), SpriteEffects.None);


            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value; //Flare

            //After-Image
            for (int i = 0; i < previousRotations.Count - 2; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.Lerp(Color.Aqua, Color.DeepSkyBlue * 1f, progress) * 1f;// * Easings.easeInQuad(progress);

                //DodgerBlue or Blue

                Vector2 lineScale = new Vector2(1f, 0.3f * progress * drawScale) * progress;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Vector2 offset = Main.rand.NextVector2Circular(5f, 8f + (10f * (1f - progress))).RotatedBy(projectile.velocity.ToRotation());

                Vector2 innerScale = new Vector2(1f, 0.15f * progress * drawScale) * progress;

                Main.EntitySpriteDraw(line, AfterImagePos + offset, null, col with { A = 0 } * 0.85f * progress * 1f,
                    previousRotations[i], line.Size() / 2f, lineScale * 1.25f * 1.5f, SpriteEffects.None);


                Main.EntitySpriteDraw(line, AfterImagePos + offset, null, Color.White with { A = 0 } * 0.85f * progress * 1f,
                    previousRotations[i], line.Size() / 2f, innerScale * 1.5f, SpriteEffects.None);


            }

        }
    }

    public class LunarExplosionAnim : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public int timer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.timeLeft = 20000;
            Projectile.penetrate = -1;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (timer == 0)
            {
                Projectile.rotation = Main.rand.NextFloat(6.28f);
                pulseVal = 1f;

                //Dust
                for (int fg = 0; fg < 8; fg++)
                {
                    Vector2 randomStart = Main.rand.NextVector2CircularEdge(7f, 7f);
                    Dust gd = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.35f) * 1.5f, newColor: Color.Aquamarine, 
                        Scale: Main.rand.NextFloat(1f, 1.6f) * 0.65f);
                    gd.alpha = 2;
                }

                for (int i = 0; i < 2; i++)
                {
                    var v = Main.rand.NextVector2Unit();
                    Dust a = Dust.NewDustPerfect(Projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(1f, 6f), 0,
                        Color.Aqua, Main.rand.NextFloat(0.4f, 0.9f) * 1.5f);

                    if (a.velocity.Y > 0)
                        a.velocity.Y *= -1;
                }

                for (int i = 0; i < 12; i++) //16
                {
                    Color col1 = Color.Lerp(Color.Aquamarine, Color.Aqua, 0.65f);

                    float progress = (float)i / 21;
                    Color col = Color.Lerp(Color.Black, col1, progress);

                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 4f) * 2.5f,
                        newColor: col with { A = 0 } * 0.5f, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 2.15f);
                    d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.25f); //12 28
                }
            }

            overallScale = Math.Clamp(MathHelper.Lerp(overallScale, 2.25f, 0.1f), 0f, 2.25f);
            pulseVal *= 0.8f;


            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                if (Projectile.frame == 4)
                {
                    Projectile.active = false;
                }

                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            timer++;
        }

        float pulseVal = 0f;

        float overallScale = 0f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D ExploA = Mod.Assets.Request<Texture2D>("Assets/Anim/NewLunarExplodeMain").Value;
            Texture2D ExploB = Mod.Assets.Request<Texture2D>("Assets/Anim/GrayscaleVanillaExplode").Value;
            Texture2D ExploC = Mod.Assets.Request<Texture2D>("Assets/Anim/NewLunarExplodeGlowmask").Value;
            //Texture2D ExploD = Mod.Assets.Request<Texture2D>("Assets/Anim/LunarExplosion").Value;

            int frameHeight = ExploA.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, ExploA.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;


            float scale12 = Projectile.scale * overallScale;

            Main.spriteBatch.Draw(ExploA, Projectile.Center - Main.screenPosition, sourceRectangle, Color.Aquamarine with { A = 0 } * (1f * pulseVal) * 1f, Projectile.rotation, origin, 1.25f * (1f - pulseVal) * overallScale * Projectile.scale, 0, 0f);


            Main.spriteBatch.Draw(ExploB, Projectile.Center - Main.screenPosition, sourceRectangle, Color.DodgerBlue * 0.65f, Projectile.rotation, origin, scale12, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(ExploA, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White with { A = 0 } * 0.65f, Projectile.rotation, origin, scale12, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(ExploC, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White with { A = 0 } , Projectile.rotation, origin, scale12, SpriteEffects.None, 0f);

            return false;
        }
    }

}
