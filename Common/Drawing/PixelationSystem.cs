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

#endregion

namespace VFXPlus.Common.Drawing
{

    //This is VERY VERY heavily based on Starlight River's Pixelation system
    //https://github.com/ProjectStarlight/StarlightRiver/blob/master/Core/Systems/PixelationSystem/PixelationSystem.cs

    //Their code is under the GPL v3 License which basically means that if you use their code then you have to 
    // open source yours too.

    //VFXPlus is also under the same license.

    public class PixelationSystem : ModSystem
    {
        public List<PixelationTarget> pixelationTargets = new();

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawCachedProjs += DrawTargets;
            On_Main.DrawDust += DrawDustTargets;
        }

        public override void PostSetupContent()
        {
            RegisterScreenTarget(RenderLayer.UnderTiles);

            RegisterScreenTarget(RenderLayer.UnderNPCs);

            RegisterScreenTarget(RenderLayer.UnderProjectiles);

            RegisterScreenTarget(RenderLayer.OverPlayers);

            RegisterScreenTarget(RenderLayer.Dusts);
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawCachedProjs -= DrawTargets;
            On_Main.DrawDust -= DrawDustTargets;
        }


        //Calls DrawTarget() on all everything in pixelationTargets, according to what layer they are on
        private void DrawTargets(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            SpriteBatch sb = Main.spriteBatch;

            orig(self, projCache, startSpriteBatch);

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCsAndTiles))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderTiles))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCs))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderNPCs))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindProjectiles))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderProjectiles))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsOverPlayers))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.OverPlayers))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }
        }

        
        private void DrawDustTargets(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.Dusts))
            {
                DrawTarget(target, Main.spriteBatch, false);
            }
        }

        private void DrawTarget(PixelationTarget target, SpriteBatch sb, bool endSpriteBatch = true)
        {
            if (endSpriteBatch)
            {
                sb.End();
            }

            BlendState blendState = BlendState.AlphaBlend;

            sb.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

            sb.End();


            if (endSpriteBatch)
                sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Registers a ScreenTarget for use with a drawing action or list of drawing actions.
        /// </summary>
        /// <param name="id">ID of the rendertarget and its layer.</param>
        public void RegisterScreenTarget(RenderLayer renderType = RenderLayer.UnderProjectiles)
        {
            Main.QueueMainThreadAction(() => pixelationTargets.Add(new PixelationTarget(renderType)));
        }

        public void QueueRenderAction(string id, Action renderAction, int order = 0)
        {
            RenderLayer layer;
            if (id == "UnderTiles")
                layer = RenderLayer.UnderTiles;
            else if (id == "UnderNPCs")
                layer = RenderLayer.UnderNPCs;
            else if (id == "UnderProjectiles")
                layer = RenderLayer.UnderProjectiles;
            else if (id == "OverPlayers")
                layer = RenderLayer.OverPlayers;
            else
                layer = RenderLayer.Dusts;



            PixelationTarget target = pixelationTargets.Find(t => t.renderType == layer);

            target.pixelationDrawActions.Add(new Tuple<Action, int>(renderAction, order));
            target.renderTimer = 2;
        }

        public void QueueRenderAction(RenderLayer renderType, Action renderAction, int order = 0)
        {

            PixelationTarget target = pixelationTargets.Find(t => t.renderType == renderType);

            target.pixelationDrawActions.Add(new Tuple<Action, int>(renderAction, order));
            target.renderTimer = 2;
        }
    }

    public class PixelationTarget
    {
        public int renderTimer;

        // list of actions, and their draw order. Default order is zero, but actions with an order of 1 are drawn over 0, etc.
        public List<Tuple<Action, int>> pixelationDrawActions;

        public ScreenTarget pixelationTarget;

        public ScreenTarget pixelationTarget2;

        public RenderLayer renderType;

        public bool Active => renderTimer > 0;

        public PixelationTarget(RenderLayer renderType)
        {
            pixelationDrawActions = new List<Tuple<Action, int>>();

            pixelationTarget = new(DrawPixelTarget, () => Active, 1f);
            pixelationTarget2 = new(DrawPixelTarget2, () => Active, 1.1f);


            this.renderType = renderType;
        }

        //Draw RenderTarget at half scale
        private void DrawPixelTarget2(SpriteBatch sb)
        {
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null);

            sb.Draw(pixelationTarget.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawPixelTarget(SpriteBatch sb)
        {
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null);

            foreach (Tuple<Action, int> tuple in pixelationDrawActions.OrderBy(t => t.Item2))
            {
                tuple.Item1.Invoke();
            }

            pixelationDrawActions.Clear();
            renderTimer--;

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
    }

    public enum RenderLayer : int
    {
        UnderTiles = 1,
        UnderNPCs = 2,
        UnderProjectiles = 3,
        OverPlayers = 4,
        Dusts = 6,
    }


    //Seperate Pixelization System for when we want to additive draw
    //Is there a smarter way to have done this? Absolutely.
    //However this was the path of least resistance so I did it anyway
    //I could only get this to work when it was a separate class for some reason
    //Make sure to use Effect matrix if using a shader with this 
    public class AdditivePixelationSystem : ModSystem
    {
        public List<PixelationTarget> pixelationTargets = new();

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawCachedProjs += DrawTargets;
            On_Main.DrawDust += DrawDustTargets;
        }

        public override void PostSetupContent()
        {
            RegisterScreenTarget(RenderLayer.UnderTiles);

            RegisterScreenTarget(RenderLayer.UnderNPCs);

            RegisterScreenTarget(RenderLayer.UnderProjectiles);

            RegisterScreenTarget(RenderLayer.OverPlayers);

            RegisterScreenTarget(RenderLayer.Dusts);
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawCachedProjs -= DrawTargets;
            On_Main.DrawDust -= DrawDustTargets;
        }


        //Calls DrawTarget() on all everything in pixelationTargets, according to what layer they are on
        private void DrawTargets(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            SpriteBatch sb = Main.spriteBatch;

            orig(self, projCache, startSpriteBatch);

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCsAndTiles))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderTiles))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCs))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderNPCs))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindProjectiles))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderProjectiles))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsOverPlayers))
            {
                foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.OverPlayers))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }
        }


        private void DrawDustTargets(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.Dusts))
            {
                DrawTarget(target, Main.spriteBatch, false);
            }
        }

        private void DrawTarget(PixelationTarget target, SpriteBatch sb, bool endSpriteBatch = true)
        {
            if (endSpriteBatch)
            {
                sb.End();
            }

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

            sb.End();

            //Reset spritebatch again to stop bleedthrough for some reason
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            sb.End();


            if (endSpriteBatch)
                sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Registers a ScreenTarget for use with a drawing action or list of drawing actions.
        /// </summary>
        /// <param name="id">ID of the rendertarget and its layer.</param>
        public void RegisterScreenTarget(string id, RenderLayer renderType = RenderLayer.UnderProjectiles)
        {
            Main.QueueMainThreadAction(() => pixelationTargets.Add(new PixelationTarget(renderType)));
        }

        /// <summary>
        /// Registers a ScreenTarget for use with a drawing action or list of drawing actions. This is used so that all draw calls of a needed palette can be done with a single ScreenTarget.
        /// </summary>
        /// <param name="id">ID of the rendertarget and its layer.</param>
        /// <param name="palettePath">The given palette's texture path.</param>
        public void RegisterScreenTarget(RenderLayer renderType = RenderLayer.UnderProjectiles)
        {
            Main.QueueMainThreadAction(() => pixelationTargets.Add(new PixelationTarget(renderType)));
        }

        public void QueueRenderAction(string id, Action renderAction, int order = 0)
        {
            RenderLayer layer;
            if (id == "UnderTiles")
                layer = RenderLayer.UnderTiles;
            else if (id == "UnderNPCs")
                layer = RenderLayer.UnderNPCs;
            else if (id == "UnderProjectiles")
                layer = RenderLayer.UnderProjectiles;
            else if (id == "OverPlayers")
                layer = RenderLayer.OverPlayers;
            else
                layer = RenderLayer.Dusts;



            PixelationTarget target = pixelationTargets.Find(t => t.renderType == layer);

            target.pixelationDrawActions.Add(new Tuple<Action, int>(renderAction, order));
            target.renderTimer = 2;
        }

        public void QueueRenderAction(RenderLayer renderType, Action renderAction, int order = 0)
        {

            PixelationTarget target = pixelationTargets.Find(t => t.renderType == renderType);

            target.pixelationDrawActions.Add(new Tuple<Action, int>(renderAction, order));
            target.renderTimer = 2;
        }
    }
}
