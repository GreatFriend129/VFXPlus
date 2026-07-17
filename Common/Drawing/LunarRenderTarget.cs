#region Using directives

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using System;
using Terraria.Graphics.Effects;
using System.Linq;
using System.Threading;
using ReLogic.Content;
using VFXPlus.Common.Interfaces;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Weapons.Ranged.Ammo.Bullets;

#endregion

namespace VFXPlus.Common.Drawing
{
    public class LunarRenderTarget : ModSystem
    {
        public RenderTarget2D renderTarget;
        public RenderTarget2D outlineTarget;
        public RenderTarget2D finalTarget;


        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() => renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
            Main.QueueMainThreadAction(() => outlineTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
            Main.QueueMainThreadAction(() => finalTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));

            On_Main.CheckMonoliths += PrepareTarget;
            On_Main.DrawDust += DrawTarget;
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            //renderTarget.Dispose();

            On_Main.CheckMonoliths -= PrepareTarget;
            On_Main.DrawDust -= DrawTarget;
        }


        private void PrepareTarget(On_Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.gameMenu || Main.dedServ)
                return;


            RenderTargetBinding[] bindings = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            foreach (Dust d in Main.dust.Where(d => ModContent.GetModDust(d.type) is LunarDust && d.active))
            {
                (ModContent.GetModDust(d.type) as LunarDust).DrawForTarget(Main.spriteBatch, d);
            }
            foreach (Projectile p in Main.projectile.Where(p => p.type == ModContent.ProjectileType<LuminiteBulletTrail>() && p.active))
            {
                (p.ModProjectile as LuminiteBulletTrail).DrawVertexTrail(false);
            }

            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);




            RenderTargetBinding[] bindings2 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(outlineTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/SimpleGalaxy", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f); //.02

            myEffect.Parameters["ScrollTexture1"].SetValue(Mod.Assets.Request<Texture2D>("Assets/StarBG").Value);
            myEffect.Parameters["ScrollTexture2"].SetValue(Mod.Assets.Request<Texture2D>("Assets/LunarStarsMiddle").Value);
            myEffect.Parameters["ScrollTexture3"].SetValue(Mod.Assets.Request<Texture2D>("Assets/LunarStarsTop").Value);
            myEffect.Parameters["zoom1"].SetValue(1.0f);
            myEffect.Parameters["offset"].SetValue(Main.LocalPlayer.position * 0.11f);
            myEffect.Parameters["exceptionColor"].SetValue(Color.Red.ToVector4());
            
            /*
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/SimpleGalaxy2", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["Texture1"].SetValue(Mod.Assets.Request<Texture2D>("Assets/SpaceDustDark").Value);
            myEffect.Parameters["Texture2"].SetValue(Mod.Assets.Request<Texture2D>("Assets/SpaceDustDark").Value);
            myEffect.Parameters["Texture3"].SetValue(Mod.Assets.Request<Texture2D>("Assets/LunarStarsLayer").Value);
            myEffect.Parameters["Texture4"].SetValue(Mod.Assets.Request<Texture2D>("Assets/LunarStarsLayer").Value);

            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.01f); //.02
            myEffect.Parameters["zoom1"].SetValue(1f);
            myEffect.Parameters["zoom2"].SetValue(1f);
            myEffect.Parameters["zoom3"].SetValue(1f);
            myEffect.Parameters["colorIntensity1"].SetValue(1f);
            myEffect.Parameters["colorIntensity2"].SetValue(1f);
            myEffect.Parameters["colorIntensity3"].SetValue(1f);

            myEffect.Parameters["reverse"].SetValue(false);
            myEffect.Parameters["curvePower"].SetValue(1f); //1.15f | 2f reverse
            myEffect.Parameters["scanlineIntensity"].SetValue(0f); //0.25 | 0.15 reverse
            myEffect.Parameters["scanLineCount"].SetValue(Main.screenHeight / 4f);
            */
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, 0, 0); //0.5f

            Main.spriteBatch.End();


            Main.graphics.GraphicsDevice.SetRenderTargets(bindings2);

            //OutlineShader

            RenderTargetBinding[] bindings3 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(finalTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);


            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/BasicOutline", AssetRequestMode.ImmediateLoad).Value;


            Color outlineCol = new Color(35, 255, 206);
            Color outlineColB = new Color(35, 255, 220);

            myEffect2.Parameters["outlineColor"].SetValue(outlineCol.ToVector4() * 1f); //DeepPink
            myEffect2.Parameters["outlineThickness"].SetValue(1f * 1f); //2f
            myEffect2.Parameters["blendBorder"].SetValue(true);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect2, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(outlineTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, 0, 0);

            Main.spriteBatch.End();

            


            Main.graphics.GraphicsDevice.SetRenderTargets(bindings2);

        }

        private void DrawTarget(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(finalTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, 0, 0); //2f

            Main.spriteBatch.End();
        }
    }

}
