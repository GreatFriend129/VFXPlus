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

#endregion

namespace VFXPlus.Common
{
    
    /*
    public class VilethornLayer : ModSystem
    {
        public List<VilethornTarget> activeVileTargets = new();

        public override void Load()
        {
            if (Main.dedServ)
                return;
            On_Main.DrawCachedProjs += DrawVileLayers;
        }

        public override void PostSetupContent()
        {
            Main.QueueMainThreadAction(() => activeVileTargets.Add(new VilethornTarget("Vilethorn")));
        }


        public override void Unload()
        {
            if (Main.dedServ)
                return;
            On_Main.DrawCachedProjs -= DrawVileLayers;
        }

        private void DrawVileLayers(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            SpriteBatch sb = Main.spriteBatch;

            orig(self, projCache, startSpriteBatch);

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindProjectiles))
            {
                foreach (VilethornTarget target in activeVileTargets.Where(t => t.isActive))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

        }

        private void DrawTarget(VilethornTarget target, SpriteBatch sb, bool endSpriteBatch = true)
        {
            if (endSpriteBatch)
                sb.End();

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(target.pixelationTarget.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

            sb.End();

            if (endSpriteBatch)
                sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }
        

        public void QueueRenderAction(string id, Action renderAction, int order = 0)
        {
            VilethornTarget target = activeVileTargets.Find(t => t.id == id);

            target.drawActions.Add(new Tuple<Action, int>(renderAction, order));
            target.renderTimer = 2;
        }

    }
    */

}
