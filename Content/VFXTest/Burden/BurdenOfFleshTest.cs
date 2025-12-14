using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ReLogic.Content;
using VFXPlus.Common;

namespace VFXPlus.Content.VFXTest.Burden
{
	public class BurdenOfFleshTest : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";

		public override void SetDefaults()
		{
			Projectile.damage = 0;
			Projectile.friendly = false;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.tileCollide = false;

			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.timeLeft = 800;
			Projectile.scale = 1f;

		}

		int timer = 0;
		public float xProgress = 0f;
        public override void AI()
        {
			Projectile.velocity = Vector2.Zero;
			Player player = Main.player[Main.myPlayer];

			Projectile.rotation = (player.Center - Projectile.Center).ToRotation();

			Projectile.timeLeft = 2;

			if (Projectile.ai[0] < 70)
			{
				float progress = Utils.GetLerpValue(0f, 1f, Projectile.ai[0] / 50, true);

				xProgress = Easings.easeOutQuad(progress);
                //xProgress = Math.Clamp(MathHelper.Lerp(xProgress, 1.5f, 0.06f), 0f, 1.25f);
            }
            else
			{
                xProgress = Math.Clamp(MathHelper.Lerp(xProgress, -0.25f, 0.04f), 0f, 2f);

				if (Projectile.ai[0] == 180) Projectile.ai[0] = 0;
            }

            //xProgress = (float)Math.Clamp(Math.Sin(Projectile.ai[0] * 0.05f), 0f, 1f);
            //xProgress = 1;
            //float sinVal = MathF.Cos((timer * 0.04f) - MathF.PI); //0-1
            //xProgress = MathHelper.Lerp(0f, 1f, Easings.easeInOutQuad(sinVal));

            timer++;
			Projectile.ai[0]++;
		}

		Vector2 drawScale = Vector2.Zero;
        public override bool PreDraw(ref Color lightColor)
		{
            #region textures
            Texture2D Outer = Mod.Assets.Request<Texture2D>("Content/VFXTest/Burden/BoFOuterTest").Value;
			Texture2D Inner = Mod.Assets.Request<Texture2D>("Content/VFXTest/Burden/BoFInnerBlack").Value;
 
			Texture2D Pupil = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle").Value;
			Texture2D Pupil2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;

			Texture2D Pupil3 = Mod.Assets.Request<Texture2D>("Assets/Orbs/circle_02").Value;

			Texture2D star = Mod.Assets.Request<Texture2D>("Content/VFXTest/Burden/EyeSpike").Value;
            #endregion

            float totalScale = 1.5f;
			drawScale = new Vector2(1 - (0.5f * xProgress), 1f) * totalScale;
			float squashFloat = 1f - (0.5f * xProgress);

            #region option3

			//Draw Pixel Parts
            Main.spriteBatch.Draw(Outer, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, Outer.Size() / 2, totalScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(Inner, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Inner.Size() / 2, totalScale, SpriteEffects.None, 0f);

            //Iris Effect
            #region effect1
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/BoFIrisAlt", AssetRequestMode.ImmediateLoad).Value;

			myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/mgma").Value);
			myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/BrighterPinkGrad").Value);
			myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value); //rgbnoise

			myEffect.Parameters["flowSpeed"].SetValue(-0.3f); //-0.3
			myEffect.Parameters["vignetteSize"].SetValue(0.1f); //0.1
			myEffect.Parameters["vignetteBlend"].SetValue(0.52f - (xProgress * 0.07f)); //0.52
			myEffect.Parameters["distortStrength"].SetValue(0.1f); //0.1
			myEffect.Parameters["squashValue"].SetValue(0.42f * xProgress); //0.22f * xprog
			myEffect.Parameters["colorIntensity"].SetValue(0.85f - (xProgress * 0.15f)); //0.85f

			float offsetValue = 0.22f * xProgress; //.22
			myEffect.Parameters["xOffset"].SetValue(offsetValue + 0.1f); //offsetValue + 0.1f
			myEffect.Parameters["uTime"].SetValue(timer * 0.007f);
            #endregion

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            //myEffect.CurrentTechnique.Passes[0].Apply(); //!

            //Main.spriteBatch.Draw(Pupil, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Pupil.Size() / 2, drawScale * 0.35f, 0, 0f);

            Main.spriteBatch.Draw(Inner, Projectile.Center - Main.screenPosition - new Vector2(0f, 0f), null, Color.Black, Projectile.rotation, Inner.Size() / 2, totalScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            #region pupil
            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/Radial/BoFIrisAlt", AssetRequestMode.ImmediateLoad).Value;

			myEffect2.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/VoroNoise").Value);
			myEffect2.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/BrighterPinkGrad").Value);
			myEffect2.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value);

			myEffect2.Parameters["flowSpeed"].SetValue(0.3f);
			myEffect2.Parameters["vignetteSize"].SetValue(0.4f);
			myEffect2.Parameters["vignetteBlend"].SetValue(1f);
			myEffect2.Parameters["distortStrength"].SetValue(0.06f);
			myEffect2.Parameters["xOffset"].SetValue(0f);
			myEffect2.Parameters["uTime"].SetValue(timer * 0.01f);
			myEffect2.Parameters["colorIntensity"].SetValue(1f);
			myEffect2.Parameters["squashValue"].SetValue(0.22f * xProgress);

			Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(50f * xProgress, 0).RotatedBy(Projectile.rotation) + new Vector2(20f, 0).RotatedBy(Projectile.rotation);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect2, Main.GameViewMatrix.TransformationMatrix);
			//myEffect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(Pupil, pos, null, Color.White, Projectile.rotation, Pupil.Size() / 2, drawScale * 0.35f, 0, 0f);
			Main.spriteBatch.Draw(Pupil, pos, null, Color.White, Projectile.rotation, Pupil.Size() / 2, drawScale * 0.35f, 0, 0f);
			Main.spriteBatch.Draw(Pupil, pos, null, Color.White, Projectile.rotation, Pupil.Size() / 2, drawScale * 0.35f, 0, 0f);
			Main.spriteBatch.Draw(Pupil, pos, null, Color.White, Projectile.rotation, Pupil.Size() / 2, drawScale * 0.35f, 0, 0f);

			Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(Pupil, pos, null, Color.Orange, Projectile.rotation, Pupil.Size() / 2, drawScale * 0.25f, 0, 0f);
			Main.spriteBatch.Draw(Pupil, pos, null, Color.White, Projectile.rotation, Pupil.Size() / 2, drawScale * 0.15f, 0, 0f);

            //Main.spriteBatch.Draw(star, pos, null, Color.White, Projectile.rotation, star.Size() / 2, drawScale * 0.3f, 0, 0f);


            #endregion

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            //Main.spriteBatch.Draw(FireEyeClear, Projectile.Center - Main.screenPosition + new Vector2(50f * xProgress, 0).RotatedBy(Projectile.rotation) + new Vector2(20f, 0).RotatedBy(Projectile.rotation), null, Color.Black, Projectile.rotation, FireEyeClear.Size() / 2, drawScale * 1f, SpriteEffects.None, 0f);

			Color yel = Color.Lerp(Color.Yellow, Color.White, 0.35f);

			Main.spriteBatch.Draw(Pupil2, pos, null, Color.Black, Projectile.rotation, Pupil2.Size() / 2, drawScale * 0.15f, 0, 0f);
			Main.spriteBatch.Draw(Pupil2, pos, null, Color.Black, Projectile.rotation, Pupil2.Size() / 2, drawScale * 0.15f, 0, 0f);
			Main.spriteBatch.Draw(Pupil3, pos, null, yel with { A = 0 }, Projectile.rotation, Pupil3.Size() / 2, drawScale * 0.15f, 0, 0f);

			Vector2 starScale1 = new Vector2(1f, 1f) * 0.45f;
			Vector2 starScale2 = new Vector2(1f, 1f) * 0.45f;

            //Spike
            Main.spriteBatch.Draw(star, pos + Main.rand.NextVector2Circular(1f, 1f), null, Color.Black, Projectile.rotation, star.Size() / 2, new Vector2(squashFloat, 1f) * starScale1, 0, 0f);
            Main.spriteBatch.Draw(star, pos + Main.rand.NextVector2Circular(1f, 1f), null, Color.Black, Projectile.rotation + MathHelper.PiOver2, star.Size() / 2, new Vector2(1f, squashFloat) * starScale2, 0, 0f);


            //Star
            //Main.spriteBatch.Draw(star, pos + Main.rand.NextVector2Circular(1f, 1f), null, Color.Black, Projectile.rotation, star.Size() / 2, new Vector2(squashFloat, 1f) * starScale1, 0, 0f);

            return false;
			#endregion

		}

	}
}