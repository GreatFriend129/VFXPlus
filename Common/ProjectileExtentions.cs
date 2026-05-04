#region Using directives

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPLus.Common;

#endregion

namespace VFXPlus.Common
{
	internal static class ProjectileExtensions
	{
		#region Projectile Drawing

		public static bool DrawProjectileCentered(this ModProjectile p, SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = (Texture2D)TextureAssets.Projectile[p.Projectile.type];

			return (p.DrawProjectileCenteredWithTexture(texture, spriteBatch, lightColor));
		}

		public static bool DrawProjectileCenteredWithTexture(this ModProjectile p, Texture2D texture, SpriteBatch spriteBatch, Color lightColor)
		{
			Rectangle frame = texture.Frame(1, Main.projFrames[p.Projectile.type], 0, p.Projectile.frame);
			Vector2 origin = frame.Size() / 2 + new Vector2(p.DrawOriginOffsetX, p.DrawOriginOffsetY);
			SpriteEffects effects = p.Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Vector2 drawPosition = p.Projectile.Center - Main.screenPosition + new Vector2(p.DrawOffsetX, 0);

			Main.EntitySpriteDraw(texture, drawPosition, frame, lightColor, p.Projectile.rotation, origin, p.Projectile.scale, effects, 0);

			return (false);
		}

		public static void DrawProjectileTrailCentered(this ModProjectile p, SpriteBatch spriteBatch, Color drawColor, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
		{
			Texture2D texture = (Texture2D)TextureAssets.Projectile[p.Projectile.type];

			p.DrawProjectileTrailCenteredWithTexture(texture, spriteBatch, drawColor, initialOpacity, opacityDegrade, stepSize);
		}

		public static void DrawProjectileTrailCenteredWithTexture(this ModProjectile p, Texture2D texture, SpriteBatch spriteBatch, Color drawColor, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
		{
			Rectangle frame = texture.Frame(1, Main.projFrames[p.Projectile.type], 0, p.Projectile.frame);
			Vector2 origin = frame.Size() / 2;
			SpriteEffects effects = p.Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[p.Projectile.type]; i += stepSize)
			{
				float opacity = initialOpacity - opacityDegrade * i;
				Main.EntitySpriteDraw(texture, p.Projectile.oldPos[i] + p.Projectile.Hitbox.Size() / 2 - Main.screenPosition, frame, drawColor * opacity, p.Projectile.oldRot[i], origin, p.Projectile.scale, effects, 0);
			}
		}

		#endregion

		public static void KillHeldProjIfPlayerDeadOrStunned(Projectile p)
		{
            Player Player = Main.player[p.owner];

            // Kill the projectile if the player dies or gets crowd controlled
            if (!Player.active || Player.dead || Player.noItems || Player.CCed)
            {
                p.active = false;
            }
        }

		public static void DoBaseRocketExplosionFX(Projectile projectile, Vector2 pos)
		{
            DoNewRocketExplosionFX(projectile, pos);
            return;
            
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
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

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 125, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.85f);

                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public static void DoNewRocketExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 20; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);

                float progress = (float)i / 20;
                Color col = Color.Lerp(Color.Brown * 0.35f, col1 with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.55f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 125, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.85f);

                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            //CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            //Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            //d1.scale = 0.04f;
            //d1.customData = cpb2;
            //d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            //Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            //d2.customData = cpb2;
            //d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            Projectile.NewProjectile(null, pos, Vector2.Zero, ModContent.ProjectileType<ExplosionPulse>(), 0, 0, Main.myPlayer);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public static void DoBaseRocketIIIExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 4 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 7f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.5f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 21; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);

                float progress = (float)i / 21;
                Color col = Color.Lerp(Color.Brown * 0.35f, col1 with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.25f, 4f) * 2.5f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(1.1f, 1.5f) * 1.25f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed * 1.35f, Scale: 0.3f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 14; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(4.5f, 4.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 125, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.85f);

                Vector2 smvel = Main.rand.NextVector2Circular(1.75f, 1.75f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f) * 1.5f);

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.75f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            d1.scale = 0.07f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public static void DoBaseMiniNukeExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 4 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 7f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.5f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 25; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);

                float progress = (float)i / 25;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.25f, 4f) * 2.5f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(1.1f, 1.5f) * 1.35f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed * 1.35f, Scale: 0.33f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 14; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(4.75f, 4.75f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 125, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.85f);

                Vector2 smvel = Main.rand.NextVector2Circular(1.75f, 1.75f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f) * 1.5f);

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.85f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            d1.scale = 0.085f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 6f;
        }

        public static void DoBaseClusterRocketExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.Orange : Color.Yellow;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Yellow, 0.3f);

                float progress = (float)i / 14;
                Color col = Color.Lerp(Color.DarkGoldenrod * 0.35f, col1 with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Orange * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 155, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Yellow, Color.OrangeRed, 0.85f);

                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;

            for (int i = 0; i < 13 + Main.rand.Next(0, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(1f, 3f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<ElectricSparkGlow>(), vel, newColor: Color.Goldenrod, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.5f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f,
                    UnderGlowPower: 3f, WhiteLayerPower: 0.5f);

                if (i < 8)
                    esb.randomVelRotatePower = 1f; 
                dp.customData = esb;
            }
        }

        public static void DoBaseDryRocketExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Color.Silver;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {
                float progress = (float)i / 14;
                Color col = Color.Lerp(Color.DarkGray * 0.35f, Color.Silver with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Gray * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: Color.Silver, Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Gray;

                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Silver * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Gray * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public static void DoBaseWetRocketExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    Color.DeepSkyBlue, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {

                float progress = (float)i / 14;
                Color col = Color.Lerp(Color.Blue * 0.35f, Color.DeepSkyBlue with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepSkyBlue * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.DeepSkyBlue * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.DodgerBlue * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public static void DoBaseLavaRocketExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {
                float progress = (float)i / 14;
                Color col = Color.Lerp(Color.Brown * 0.35f, Color.OrangeRed with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 95, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public static void DoBaseHoneyRocketExplosionFX(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.Yellow : Color.Gold;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {

                float progress = (float)i / 14;
                Color col = Color.Lerp(Color.OrangeRed * 0.35f, Color.Gold with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DarkGoldenrod * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: Color.DarkGoldenrod, Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: Color.DarkGoldenrod, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Gold * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.DarkGoldenrod * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

    }

    //TODO: move somewhere else
    public class ExplosionPulse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }

        float overallAlpha = 1f;
        float overallScale = 1f;

        int timer = 0;

        float progress = 0f;
        public override void AI()
        {
            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);
            
            float timeForPulse = 22f;
            float myProg = Utils.GetLerpValue(0f, timeForPulse, (float)timer, true);

            //Easings.easeOutSine(myProg);

            progress = myProg;

            if (progress > 0.99f)
            {
                progress = 1f;
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawEffect(false);
            });

            DrawEffect(true);

            return false;
        }

        public void DrawEffect(bool giveUp = false)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Noise_1").Value); //Trail_2 0.45 totalAlpha
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(0.75f * progress);// * Easings.easeOutQuint(progress)); //0.5
            myEffect.Parameters["ringPower"].SetValue(0.15f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(0.5f);
            myEffect.Parameters["ringWaveLength"].SetValue(21f * 0f);

            //Caustic values

            Color atasd3 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f); //0.5
            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                atasd3.ToVector3() * 0.5f,
                Color.Goldenrod.ToVector3() * 1f,
                Color.Yellow.ToVector3(),
                Color.White.ToVector3()
            };


            myEffect.Parameters["gradColors"].SetValue(gradCols);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(2.0f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(0.0f);

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha * 0.75f);
            myEffect.Parameters["fadeStrength"].SetValue(0f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 160 * new Vector2(1f, 1f) * overallScale, SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            /* Non Post
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Noise_1").Value); //Trail_2 0.45 totalAlpha
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(0.5f * progress);// * Easings.easeOutQuint(progress)); //0.5
            myEffect.Parameters["ringPower"].SetValue(0.15f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(0.5f);
            myEffect.Parameters["ringWaveLength"].SetValue(21f * 0f);

            //Caustic values

            Color atasd3 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f); //0.5
            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                atasd3.ToVector3() * 0.5f,
                Color.Goldenrod.ToVector3() * 1f,
                Color.Yellow.ToVector3(),
                Color.White.ToVector3()
            };


            myEffect.Parameters["gradColors"].SetValue(gradCols);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(2.0f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(0.0f);

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha * 0.75f);
            myEffect.Parameters["fadeStrength"].SetValue(0f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 160 * new Vector2(1f, 1f) * overallScale, SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            */

            //Post
            /*
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Noise_1").Value); //Trail_2 0.45 totalAlpha
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(1f * progress);// * Easings.easeOutQuint(progress)); //0.5
            myEffect.Parameters["ringPower"].SetValue(0.15f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(0.5f);
            myEffect.Parameters["ringWaveLength"].SetValue(21f * 0f);

            //Caustic values

            Color atasd3 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f); //0.5
            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                atasd3.ToVector3() * 1f,
                Color.Goldenrod.ToVector3() * 1f,
                Color.Yellow.ToVector3(),
                Color.White.ToVector3()
            };


            myEffect.Parameters["gradColors"].SetValue(gradCols);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(2.0f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(5.0f); //0.0

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha * 0.75f);
            myEffect.Parameters["fadeStrength"].SetValue(0f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 160 * new Vector2(1f, 1f) * overallScale, SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            */
        }
    }

    public class ExplosionPulseAlt : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }

        float overallAlpha = 1f;
        float overallScale = 1f;

        int timer = 0;

        float progress = 0f;
        public override void AI()
        {
            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);

            float timeForPulse = 22f;
            float myProg = Utils.GetLerpValue(0f, timeForPulse, (float)timer, true);

            //Easings.easeOutSine(myProg);

            progress = myProg;

            if (progress > 0.99f)
            {
                progress = 1f;
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawEffect(false);
            });

            DrawEffect(true);

            return false;
        }

        public void DrawEffect(bool giveUp = false)
        {
            if (giveUp)
                return;


            //progress = 0.5f + MathF.Sin((float)Main.timeForVisualEffects * 0.03f) * 0.5f;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value); //Trail_2 0.45 totalAlpha
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(1f * progress);// * Easings.easeOutQuint(progress)); //0.5
            myEffect.Parameters["ringPower"].SetValue(0.25f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(0.25f);
            myEffect.Parameters["ringWaveLength"].SetValue(21f);

            //Caustic values

            Color atasd3 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f); //0.5
            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                atasd3.ToVector3() * 0.5f,
                Color.Goldenrod.ToVector3() * 1f,
                Color.Yellow.ToVector3(),
                Color.White.ToVector3()
            };

            Color atasd = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);
            Vector3[] gradCols2 = {
                Color.Black.ToVector3(),
                atasd.ToVector3() * 0.5f,
                Color.HotPink.ToVector3(),
                Color.LightPink.ToVector3(),
                Color.White.ToVector3()
            };

            myEffect.Parameters["gradColors"].SetValue(gradCols);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(2.0f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(2.0f);

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha * 0.75f);
            myEffect.Parameters["fadeStrength"].SetValue(0f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 160 * new Vector2(1f, 1f) * overallScale, SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
    }

}
