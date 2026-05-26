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

#endregion

namespace VFXPlus.Common.Drawing
{
    public class RendertargetTest : ModSystem
    {
        public RenderTarget2D renderTarget;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() => renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));

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

            Texture2D tex = CommonTextures.feather_circle128PMA.Value;

            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/Smoke/smokeFlipbook1k").Value;
            int frameHeight = Smoke.Height / 64;
            int frameWidth = Smoke.Width / 64;

            foreach (Dust d in Main.dust.Where(d => d.type == ModContent.DustType<RenderTargetDustTest>() && d.active))
            {
                RenderTargetDustBehavoir behavoir = (RenderTargetDustBehavoir)d.customData;

                int startX = (behavoir.animFrame % 8);
                int startY = (int)Math.Floor(behavoir.animFrame / 8f);
                Rectangle sourceRectangle = Smoke.Frame(8, 8, startX, startY);
                Vector2 origin = sourceRectangle.Size() / 2f;

                Main.spriteBatch.Draw(Smoke, d.position - Main.screenPosition, sourceRectangle, Color.White, d.rotation, origin, d.scale, 0, 0);


                //Main.spriteBatch.Draw(tex, d.position - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, d.scale, 0, 0);
            }
            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);


            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            //Main.spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Main.DiscoColor);

            //Main.spriteBatch.End();
            //Main.graphics.GraphicsDevice.SetRenderTargets(null);

        }

        private void DrawTarget(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            /*
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialScrollOneCol", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Starbasesnow").Value);
            myEffect.Parameters["distortTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f); //0.02


            myEffect.Parameters["inputColor"].SetValue(Color.White.ToVector3() * 1f);

            myEffect.Parameters["zoom"].SetValue(2f);
            myEffect.Parameters["flowSpeed"].SetValue(1.5f);

            myEffect.Parameters["radius"].SetValue(1f);
            myEffect.Parameters["edgeBlendDist"].SetValue(0f); //14
            myEffect.Parameters["insideBlendDist"].SetValue(0f);
            myEffect.Parameters["distortIntensity"].SetValue(0.03f);
            */

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/NebulaGalaxy", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["zoom"].SetValue(40.0f);
            myEffect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.02f);

            Vector4[] cols =
            {
                new Color(23, 168, 209).ToVector4(),
                new Color(244, 83, 251).ToVector4(),
            };


            Vector4[] cols2 =
            {
                Color.Aqua.ToVector4()  * 1f,
                Color.Aquamarine.ToVector4() * 0.75f,
                Color.DeepSkyBlue.ToVector4() * 0.5f,
                Color.DodgerBlue.ToVector4() * 0.25f
            };

            myEffect.Parameters["Colors"].SetValue(cols2);
            myEffect.Parameters["layers"].SetValue(cols2.Length);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
        }
    }
}
