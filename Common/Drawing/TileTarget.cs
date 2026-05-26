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
    public class Tiletarget : ModSystem
    {
        public RenderTarget2D renderTarget;
        public RenderTarget2D tileTarget;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() => renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
            Main.QueueMainThreadAction(() => tileTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));

            On_Main.CheckMonoliths += PrepareTargets;
            On_Main.DrawProjectiles += DrawTarget;
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            //renderTarget.Dispose();

            On_Main.CheckMonoliths -= PrepareTargets;
            On_Main.DrawProjectiles -= DrawTarget;
        }


        private void PrepareTargets(On_Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.gameMenu || Main.dedServ)
                return;


            //Dust target
            RenderTargetBinding[] bindings = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            Texture2D tex = CommonTextures.feather_circle128PMA.Value;

            foreach (Dust d in Main.dust.Where(d => ModContent.GetModDust(d.type) is DrawOverTilesDust && d.active))
            {
                (ModContent.GetModDust(d.type) as DrawOverTilesDust).DrawOverTiles(Main.spriteBatch, d);
            }
            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);


            //Tile target
            RenderTargetBinding[] bindings2 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(tileTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(Main.instance.tileTarget, Main.sceneTilePos - Main.screenPosition, Color.White);
            
            //Trees and stuff
            //Main.spriteBatch.Draw(Main.instance.tile2Target, Main.sceneTile2Pos - Main.screenPosition, Color.White);

            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);
        }

        private void DrawTarget(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/BasicMask", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["Mask"].SetValue(tileTarget);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
        }
    }
}
