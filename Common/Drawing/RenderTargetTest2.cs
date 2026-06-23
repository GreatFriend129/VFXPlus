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
    public class RendertargetTest2 : ModSystem
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

            if (Main.gameMenu || Main.dedServ || true)
                return;


            RenderTargetBinding[] bindings = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            //Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Circle").Value;
            Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;
            //Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/StarlightNoGlow").Value;

            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/Smoke/SmokeFull1k").Value; //SmokeFull1k |smokeFlipbook1k
            int frameHeight = Smoke.Height / 64;
            int frameWidth = Smoke.Width / 64;

            foreach (Dust d in Main.dust.Where(d => d.type == ModContent.DustType<RenderTargetDustTest>() && d.active))
            {
                RenderTargetDustBehavoir behavoir = (RenderTargetDustBehavoir)d.customData;

                int startX = (behavoir.animFrame % 8);
                int startY = (int)Math.Floor(behavoir.animFrame / 8f);
                Rectangle sourceRectangle = Smoke.Frame(8, 8, startX, startY);
                Vector2 origin = sourceRectangle.Size() / 2f;

                //Main.spriteBatch.Draw(Smoke, (d.position - Main.screenPosition) * 1f, sourceRectangle, Color.White, d.rotation, origin, d.scale * 1f, 0, 0);

                Main.spriteBatch.Draw(tex, (d.position - Main.screenPosition) * 1f, null, Color.White, d.rotation, tex.Size() / 2f, d.scale * 1f, 0, 0);
            }

            foreach (Projectile p in Main.projectile.Where(p => p.type == ModContent.ProjectileType<LunarBulletTest>() && p.active))
            {

                (p.ModProjectile as LunarBulletTest).DrawVertexTrail(false);
            }

            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);




            ////Draw shader to RT

            RenderTargetBinding[] bindings2 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(outlineTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);


            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy5", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.05f);
            
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, 0, 0);

            Main.spriteBatch.End();


            Main.graphics.GraphicsDevice.SetRenderTargets(bindings2);

            //OutlineShader
            
            RenderTargetBinding[] bindings3 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(finalTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/BasicOutline", AssetRequestMode.ImmediateLoad).Value;

            myEffect2.Parameters["outlineColor"].SetValue(Color.White.ToVector4() * 2f); //DeepPink
            myEffect2.Parameters["outlineThickness"].SetValue(2f  * 0f); //2f

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect2, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(outlineTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, 0, 0); //0.5f

            Main.spriteBatch.End();


            Main.graphics.GraphicsDevice.SetRenderTargets(bindings2);
            
        }

        private void DrawTarget(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            return;
           
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(finalTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, 0, 0); //2f

            Main.spriteBatch.End();
        }
    }

}
