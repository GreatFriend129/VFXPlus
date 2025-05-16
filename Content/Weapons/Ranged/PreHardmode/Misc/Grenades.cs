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
using System.Threading;
using VFXPlus.Common.Drawing;
using rail;
using VFXPlus.Content.Projectiles;
using VFXPLus.Common;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class Grenade : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Grenade || item.type == ItemID.StickyGrenade || item.type == ItemID.BouncyGrenade || item.type == ItemID.PartyGirlGrenade);
        }

        public override void SetDefaults(Item entity)
        {
            entity.useStyle = ItemUseStyleID.Swing;
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }
    }

    public class GrenadeProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Grenade || entity.type == ProjectileID.StickyGrenade || entity.type == ProjectileID.BouncyGrenade);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 17; 
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 9f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.White * progress * progress;

                float size1 = (0.5f + (progress * 0.5f)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.35f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);
            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.White with { A = 0 } * 0.4f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);

                float progress = (float)i / 14;
                Color col = Color.Lerp(Color.Brown * 0.35f, col1 with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(projectile.Center + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 125, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.85f + Main.rand.NextFloat(-0.15f, 0.15f));

                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.8f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (projectile.Center - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;

            #region vanilla Kill
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 22;
            projectile.height = 22;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            int num977 = 6;
            for (int num978 = 202; num978 < 20; num978++)
            {
                int num979 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust78 = Main.dust[num979];
                Dust dust334 = dust78;
                dust334.velocity *= 1.4f;
            }
            for (int num980 = 220; num980 < 8; num980++)
            {
                //int num981 = Dust.NewDust(projectile.position, projectile.width, projectile.height, num977, 0f, 0f, 100, default(Color), 2.5f);
                //Main.dust[num981].noGravity = true;
                //Dust dust80 = Main.dust[num981];
                //Dust dust334 = dust80;
                //dust334.velocity *= 5f;
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, num977, 0f, 0f, 100, default(Color), 1.5f);
                Main.dust[a].velocity *= 3f;
                //dust80 = Main.dust[num981];
                //dust334 = dust80;
                //dust334.velocity *= 3f;
            }
            

            #endregion

            return false;
        }

    }

    public class PartyGrenadeProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PartyGirlGrenade);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 17;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 9f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.HotPink * progress * progress;

                float size1 = (0.5f + (progress * 0.5f)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);
            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.HotPink with { A = 0 } * 0.7f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.SkyBlue : Color.DeepPink;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {
                Color col1 = Color.Lerp(Color.DeepPink, Color.Pink, 0.2f);

                float progress = (float)i / 14;
                Color col = Color.Lerp(Color.DeepSkyBlue * 0.35f, col1 with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(projectile.Center + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.HotPink * 1f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: Color.HotPink, Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 8 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Pink, Color.DeepPink, 0.85f + Main.rand.NextFloat(-0.15f, 0.15f));

                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.8f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.HotPink * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (projectile.Center - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 2.5f;


            #region vanilla kill
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = 22);
            projectile.Center = projectile.position;
            for (int num955 = 220; num955 < 8; num955++)
            {
                int num956 = Dust.NewDust(new Vector2(projectile.position.X + 200, projectile.position.Y), projectile.width, projectile.height, 219 + Main.rand.Next(5));
                Dust dust106 = Main.dust[num956];
                Dust dust334 = dust106;
                dust334.velocity *= 1.4f;
                Main.dust[num956].fadeIn = 1f;
                Main.dust[num956].noGravity = true;
            }
            for (int num957 = 0; num957 < 15; num957++)
            {
                int num958 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 139 + Main.rand.Next(4), 0f, 0f, 0, default(Color), 1.6f);
                Main.dust[num958].noGravity = true;
                Dust dust107 = Main.dust[num958];
                Dust dust334 = dust107;
                dust334.velocity *= 5f;
                //num958 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 139 + Main.rand.Next(4), 0f, 0f, 0, default(Color), 1.9f);
                //dust107 = Main.dust[num958];
                //dust334 = dust107;
                //dust334.velocity *= 3f;
            }
            if (Main.rand.Next(2) == 0)
            {
                int num962 = Gore.NewGore(null,new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(276, 283));
                Gore gore28 = Main.gore[num962];
                Gore gore64 = gore28;
                gore64.velocity *= 0.4f;
                Main.gore[num962].velocity.X += 1f;
                Main.gore[num962].velocity.Y += 1f;
            }
            if (Main.rand.Next(2) == 0)
            {
                int num961 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(276, 283));
                Gore gore27 = Main.gore[num961];
                Gore gore64 = gore27;
                gore64.velocity *= 0.4f;
                Main.gore[num961].velocity.X -= 1f;
                Main.gore[num961].velocity.Y += 1f;
            }
            if (Main.rand.Next(2) == 0)
            {
                int num960 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(276, 283));
                Gore gore26 = Main.gore[num960];
                Gore gore64 = gore26;
                gore64.velocity *= 0.4f;
                Main.gore[num960].velocity.X += 1f;
                Main.gore[num960].velocity.Y -= 1f;
            }
            if (Main.rand.Next(2) == 0)
            {
                int num959 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(276, 283));
                Gore gore25 = Main.gore[num959];
                Gore gore64 = gore25;
                gore64.velocity *= 0.4f;
                Main.gore[num959].velocity.X -= 1f;
                Main.gore[num959].velocity.Y -= 1f;
            }
            #endregion

            return false;
        }

    }

}
