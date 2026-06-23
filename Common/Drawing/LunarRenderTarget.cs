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




            ////Draw shader to RT

            RenderTargetBinding[] bindings2 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(outlineTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);



            
            Texture2D scroll1 = Mod.Assets.Request<Texture2D>("Assets/Starbasesnow").Value;//Starbasesnow
            Texture2D scroll2 = Mod.Assets.Request<Texture2D>("Assets/Starbasesnow").Value; //LunarNebula3

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy2", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.05f);//.02
            myEffect.Parameters["offset"].SetValue(Main.LocalPlayer.position * 0.11f);

            myEffect.Parameters["ScrollTexture1"].SetValue(scroll1);
            myEffect.Parameters["ScrollTexture2"].SetValue(scroll2);


            myEffect.Parameters["NUM_LAYERS"].SetValue(8f);
            myEffect.Parameters["Velocity"].SetValue(0.015f);
            myEffect.Parameters["StarGlow"].SetValue(0.03f); //0.015 |3
            myEffect.Parameters["Zoom"].SetValue(50.0f);

            Color starCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.35f);

            myEffect.Parameters["color1"].SetValue(starCol.ToVector4() * 0.25f);
            myEffect.Parameters["color2"].SetValue(starCol.ToVector4() * 1f);
            myEffect.Parameters["scrollColor1"].SetValue(Color.White.ToVector4() * 1f);
            myEffect.Parameters["scrollColor2"].SetValue(Color.White.ToVector4() * 0f); //0.66
            
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, 0, 0);

            Main.spriteBatch.End();


            Main.graphics.GraphicsDevice.SetRenderTargets(bindings2);

            //OutlineShader
            
            RenderTargetBinding[] bindings3 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(finalTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/BasicOutline", AssetRequestMode.ImmediateLoad).Value;

            myEffect2.Parameters["outlineColor"].SetValue(Color.White.ToVector4() * 0.75f); //75
            myEffect2.Parameters["outlineThickness"].SetValue(2f * 1f); //2f

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect2, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(outlineTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, 0, 0); //0.5f

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
