using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common.Drawing;

namespace VFXPlus.Common
{
    public class ShaderParticleHandler : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawInfernoRings += DrawOver;

            particles = new List<ShaderParticle>();
            particlesToKill = new List<ShaderParticle>();
            particlesToSpawn = new List<ShaderParticle>();

            //dustsLayerParticles = new List<ShaderParticle>();
            //underprojLayerParticles = new List<ShaderParticle>();
        }

        public override void Unload()
        {
            On_Main.DrawInfernoRings -= DrawOver;

            particles = null;
            particlesToKill = null;
            particlesToSpawn = null;

            //dustsLayerParticles = null;
            //underprojLayerParticles = null;
        }
        public override void PostUpdateEverything()
        {
            if (!Main.dedServ)
                UpdateParticles();
        }

        private static void DrawOver(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            DrawAllParticles(Main.spriteBatch);
            orig(self);
        }

        internal static List<ShaderParticle> particles;
        private static List<ShaderParticle> particlesToSpawn;
        private static List<ShaderParticle> particlesToKill;
        private static bool updatingParticles = false;

        public static void SpawnParticle(ShaderParticle particle)
        {
            //Don't spawn particles if the game is paused, on the server side, or if the list is null
            if (Main.gamePaused || Main.dedServ || particles == null)
                return;

            //We don't want to update the particle list while we are foreaching through it
            if (updatingParticles)
            {
                particlesToSpawn.Add(particle);
                return;
            }

            particles.Add(particle);
        }


        public static void UpdateParticles()
        {
            updatingParticles = true;
            foreach (ShaderParticle particle in particles)
            {
                if (particle == null)
                    continue;

                particle.Timer++;
                particle.Center += particle.Velocity;

                particle.Update();

                if (particle.shouldKillEarly && particle.Timer >= particle.timeToKillEarly)
                    particle.active = false;// RemoveParticle(particle);
            }
            updatingParticles = false;

            //Clear out particles whose time is up
            particles.RemoveAll(particle => (particle.Timer >= particle.timeToKillEarly && particle.shouldKillEarly) || particlesToKill.Contains(particle));
            foreach (ShaderParticle particle in particlesToKill)
                particle.active = false;
            particlesToKill.Clear();

            particles.AddRange(particlesToSpawn);
            particlesToSpawn.Clear();

            //Need to clear here instead of in draw method otherwise it won't work for reasons
            //underprojLayerParticles.Clear();
            //dustsLayerParticles.Clear();
        }

        public static void RemoveParticle(ShaderParticle particle)
        {
            particlesToKill.Add(particle);
            //if (Main.dedServ)
            //    return;

            //particles.Remove(particle);
        }


        //TODO optimize
        //private static List<ShaderParticle> underprojLayerParticles;
        //private static List<ShaderParticle> dustsLayerParticles;
        public static void DrawAllParticles(SpriteBatch sb)
        {
            if (Main.dedServ || particles.Count == 0)
                return;

            foreach (ShaderParticle particle in particles)
            {
                if (particle == null)
                    continue;

                //Draw normal layer
                particle.Draw(sb);
            }

            //Draw shader layer
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, VFXPlus.SmokeColShader, Main.GameViewMatrix.EffectMatrix);

                foreach (ShaderParticle particle in particles)
                {
                    if (particle.renderLayer == RenderLayer.Dusts)
                        particle.DrawWithShader(sb, particle.myShader);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            });

            //UnderProjLayer
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, VFXPlus.SmokeColShader, Main.GameViewMatrix.EffectMatrix);

                foreach (ShaderParticle particle in particles)
                {
                    if (particle.renderLayer == RenderLayer.UnderProjectiles)
                        particle.DrawWithShader(sb, particle.myShader);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            });
        }

        //Todo make a version of this that actually works
        public static void DrawAllParticlesFailedAttempt(SpriteBatch sb)
        {
            /*
            if (Main.dedServ || particles.Count == 0)
                return;

            foreach (ShaderParticle particle in particles)
            {
                if (particle == null)
                    continue;

                //Add particle to designated RenderLayer list (unless it is already in it, which can happen when game is paused)
                if (particle.renderLayer == RenderLayer.UnderProjectiles && !underprojLayerParticles.Contains(particle))
                    underprojLayerParticles.Add(particle);
                else if (particle.renderLayer == RenderLayer.Dusts && !dustsLayerParticles.Contains(particle))
                    dustsLayerParticles.Add(particle);
            }

            //--Draw normal Layer
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                foreach (ShaderParticle particle in underprojLayerParticles)
                    particle.Draw(sb);
            });

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                foreach (ShaderParticle particle in dustsLayerParticles)
                    particle.Draw(sb);
            });


            //--Draw shader layer
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, VFXPlus.SmokeColShader, Main.GameViewMatrix.EffectMatrix);

                foreach (ShaderParticle particle in underprojLayerParticles)
                    particle.DrawWithShader(sb, particle.myShader);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            });

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, VFXPlus.SmokeColShader, Main.GameViewMatrix.EffectMatrix);

                foreach (ShaderParticle particle in dustsLayerParticles)
                    particle.DrawWithShader(sb, particle.myShader);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            });

            //dustsLayerParticles.Clear();
            //underprojLayerParticles.Clear();

            //sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            //Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            */
        }

    }

}