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
    public class RendertargetTest : ModSystem
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
                //Main.spriteBatch.Draw(Smoke, (d.position - Main.screenPosition) * 1f, sourceRectangle, Color.White, d.rotation, origin, d.scale * 1f, 0, 0);


                //Main.spriteBatch.Draw(tex, (d.position - Main.screenPosition) * 1f, null, Color.White, d.rotation, tex.Size() / 2f, new Vector2(1.25f, d.scale * 0.5f) * 1f, 0, 0);
                Main.spriteBatch.Draw(tex, (d.position - Main.screenPosition) * 1f, null, Color.White, d.rotation, tex.Size() / 2f, d.scale * 1f, 0, 0);

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

            /*
            Texture2D scroll1 = Mod.Assets.Request<Texture2D>("Assets/Crack/DitheredLunarSpaceDust2").Value;
            Texture2D scroll2 = Mod.Assets.Request<Texture2D>("Assets/Crack/DitheredLunarSpaceDust1").Value;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy4", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.03f); //.02

            myEffect.Parameters["ScrollTexture1"].SetValue(scroll1);
            myEffect.Parameters["ScrollTexture2"].SetValue(scroll2);
            myEffect.Parameters["zoom"].SetValue(2.5f);
            myEffect.Parameters["posterizationSteps"].SetValue(0f);

            myEffect.Parameters["screenWidth"].SetValue(Main.screenWidth / 4f);
            myEffect.Parameters["screenHeight"].SetValue(Main.screenHeight / 4f);
            myEffect.Parameters["offset"].SetValue(Main.LocalPlayer.position * 0.11f);
            */
            
            Texture2D scroll1 = Mod.Assets.Request<Texture2D>("Assets/Crack/LunarSpaceDust1").Value; //Starbasesnow
            Texture2D scroll2 = Mod.Assets.Request<Texture2D>("Assets/Crack/LunarSpaceDust1").Value;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy2", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.05f);//.02
            myEffect.Parameters["offset"].SetValue(Main.LocalPlayer.position * 0.11f);

            myEffect.Parameters["ScrollTexture1"].SetValue(scroll1);
            myEffect.Parameters["ScrollTexture2"].SetValue(scroll2);


            myEffect.Parameters["NUM_LAYERS"].SetValue(8f);
            myEffect.Parameters["Velocity"].SetValue(0.015f);
            myEffect.Parameters["StarGlow"].SetValue(0.03f); //0.015 |5
            myEffect.Parameters["Zoom"].SetValue(50.0f);

            Color starCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.35f);
            //Color starCol = Color.Lerp(Color.DeepPink, Color.HotPink, 1f);


            myEffect.Parameters["color1"].SetValue(starCol.ToVector4() * 0.25f);
            myEffect.Parameters["color2"].SetValue(starCol.ToVector4() * 1f);
            myEffect.Parameters["scrollColor1"].SetValue(Color.White.ToVector4() * 0.35f * 2.5f * 0.75f);
            myEffect.Parameters["scrollColor2"].SetValue(Color.White.ToVector4() * 1f * 1f);
            
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

            /*
             * Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value
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

            /*
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/NebulaGalaxy", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["zoom"].SetValue(40.0f);
            myEffect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.02f);

            Vector4[] cols =
            {
                new Color(23, 168, 209).ToVector4() * 1.5f,
                new Color(244, 83, 251).ToVector4() * 1.5f,
            };


            Vector4[] cols2 =
            {
                Color.Aqua.ToVector4()  * 0.85f,
                Color.Aquamarine.ToVector4() * 0.65f,
                Color.DeepSkyBlue.ToVector4() * 0.35f,
                Color.DodgerBlue.ToVector4() * 0.25f
            };

            myEffect.Parameters["Colors"].SetValue(cols);
            myEffect.Parameters["layers"].SetValue(cols.Length);
            */

            /*
            Texture2D scroll1 = Mod.Assets.Request<Texture2D>("Assets/Smoke/SpaceDust1").Value;
            Texture2D scroll2 = Mod.Assets.Request<Texture2D>("Assets/Smoke/SpaceDust2").Value;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy2", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f);

            myEffect.Parameters["ScrollTexture1"].SetValue(scroll1);
            myEffect.Parameters["ScrollTexture2"].SetValue(scroll2);


            myEffect.Parameters["NUM_LAYERS"].SetValue(8f);
            myEffect.Parameters["Velocity"].SetValue(0.015f);
            myEffect.Parameters["StarGlow"].SetValue(0.025f);
            myEffect.Parameters["Zoom"].SetValue(50.0f);

            myEffect.Parameters["color1"].SetValue(new Color(51, 77, 230).ToVector4());
            myEffect.Parameters["color2"].SetValue(new Color(0, 255, 228).ToVector4());
            myEffect.Parameters["scrollColor1"].SetValue(new Color(51, 77, 230).ToVector4() * 1f);
            myEffect.Parameters["scrollColor2"].SetValue(new Color(0, 255, 228).ToVector4() * 1f);
            */

            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/BasicOutline", AssetRequestMode.ImmediateLoad).Value;

            myEffect2.Parameters["outlineColor"].SetValue(Color.Aquamarine.ToVector4());
            myEffect2.Parameters["outlineThickness"].SetValue(1f);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            //Main.spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
            Main.spriteBatch.Draw(finalTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, 0, 0); //2f

            Main.spriteBatch.End();
        }
    }

    public class LunarRenderTargetTest : ModSystem
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

            //On_Main.CheckMonoliths += PrepareTarget;
            //On_Main.DrawDust += DrawTarget;
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            //renderTarget.Dispose();

            //On_Main.CheckMonoliths -= PrepareTarget;
            //On_Main.DrawDust -= DrawTarget;
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

            Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Circle").Value;
            //Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/StarlightNoGlow").Value;

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

                //Main.spriteBatch.Draw(Smoke, (d.position - Main.screenPosition) * 1f, sourceRectangle, Color.White, d.rotation, origin, d.scale * 1f, 0, 0);


                Main.spriteBatch.Draw(tex, (d.position - Main.screenPosition) * 1f, null, Color.White, 0, tex.Size() / 2f, d.scale * 1f, 0, 0);
            }
            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);




            ////Draw shader to RT
            
            RenderTargetBinding[] bindings2 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(outlineTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Texture2D scroll1 = Mod.Assets.Request<Texture2D>("Assets/Smoke/SpaceDust1").Value;
            Texture2D scroll2 = Mod.Assets.Request<Texture2D>("Assets/Smoke/SpaceDust2").Value;


            Vector3[] cols2 =
            {
                Color.DeepSkyBlue.ToVector3() * 1f,
                Color.Lerp(Color.DeepSkyBlue, Color.Aqua, 0.5f).ToVector3(),
                Color.Aqua.ToVector3() * 1f,
                Color.Lerp(Color.Aqua, Color.Aquamarine, 0.5f).ToVector3(),
                Color.Aquamarine.ToVector3() * 2f,
                Color.MediumAquamarine.ToVector3() * 2f,
            };


            Effect trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy3", AssetRequestMode.ImmediateLoad).Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.01f);
            trailEffect.Parameters["posterizationSteps"].SetValue(6.0f);
            trailEffect.Parameters["zoom"].SetValue(10.0f);

            trailEffect.Parameters["gradColors"].SetValue(cols2);
            trailEffect.Parameters["numberOfColors"].SetValue(cols2.Length);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, trailEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, 0, 0);

            Main.spriteBatch.End();
            

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings2);
            
            //OutlineShader

            RenderTargetBinding[] bindings3 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(finalTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/BasicOutline", AssetRequestMode.ImmediateLoad).Value;

            myEffect2.Parameters["outlineColor"].SetValue(Color.White.ToVector4());
            myEffect2.Parameters["outlineThickness"].SetValue(2f);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, myEffect2, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(outlineTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, 0, 0);

            Main.spriteBatch.End();


            Main.graphics.GraphicsDevice.SetRenderTargets(bindings3);

        }

        private void DrawTarget(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            /*
             * Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value
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

            /*
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/NebulaGalaxy", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["zoom"].SetValue(40.0f);
            myEffect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.02f);

            Vector4[] cols =
            {
                new Color(23, 168, 209).ToVector4() * 1.5f,
                new Color(244, 83, 251).ToVector4() * 1.5f,
            };


            Vector4[] cols2 =
            {
                Color.Aqua.ToVector4()  * 0.85f,
                Color.Aquamarine.ToVector4() * 0.65f,
                Color.DeepSkyBlue.ToVector4() * 0.35f,
                Color.DodgerBlue.ToVector4() * 0.25f
            };

            myEffect.Parameters["Colors"].SetValue(cols);
            myEffect.Parameters["layers"].SetValue(cols.Length);
            */

            /*
            Texture2D scroll1 = Mod.Assets.Request<Texture2D>("Assets/Smoke/SpaceDust1").Value;
            Texture2D scroll2 = Mod.Assets.Request<Texture2D>("Assets/Smoke/SpaceDust2").Value;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy2", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f);

            myEffect.Parameters["ScrollTexture1"].SetValue(scroll1);
            myEffect.Parameters["ScrollTexture2"].SetValue(scroll2);


            myEffect.Parameters["NUM_LAYERS"].SetValue(8f);
            myEffect.Parameters["Velocity"].SetValue(0.015f);
            myEffect.Parameters["StarGlow"].SetValue(0.025f);
            myEffect.Parameters["Zoom"].SetValue(50.0f);

            myEffect.Parameters["color1"].SetValue(new Color(51, 77, 230).ToVector4());
            myEffect.Parameters["color2"].SetValue(new Color(0, 255, 228).ToVector4());
            myEffect.Parameters["scrollColor1"].SetValue(new Color(51, 77, 230).ToVector4() * 1f);
            myEffect.Parameters["scrollColor2"].SetValue(new Color(0, 255, 228).ToVector4() * 1f);
            */

            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/BasicOutline", AssetRequestMode.ImmediateLoad).Value;

            myEffect2.Parameters["outlineColor"].SetValue(Color.Aquamarine.ToVector4());
            myEffect2.Parameters["outlineThickness"].SetValue(1f);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            //Main.spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
            Main.spriteBatch.Draw(finalTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, 0, 0);

            Main.spriteBatch.End();
        }
    }
}
