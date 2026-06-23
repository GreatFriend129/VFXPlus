using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace VFXPlus.Content.Dusts
{
	public abstract class LunarDust : ModDust
	{
        public virtual void DrawForTarget(SpriteBatch spriteBatch, Dust dust) { }
    }

    public class LunarDustCircle : LunarDust
    {
        public override string Texture => "VFXPlus/Assets/Pixel/Flare";

        public override void OnSpawn(Dust dust)
        {
            dust.customData = new RenderTargetDustBehavoir();
            dust.alpha = 0;
        }

        public override bool Update(Dust dust)
        {
            if (dust.alpha == 0)
                dust.rotation = Main.rand.NextFloat(6.28f);

            //Frame
            dust.position += dust.velocity;
            dust.position += dust.velocity;

            dust.velocity *= 0.95f; 
            dust.scale *= 0.95f;

            //dust.scale *= 0.98f;

            if (dust.alpha > 12)
                dust.scale *= 0.9f;

            if (dust.scale < 0.05f || dust.alpha >= 100)
            {
                dust.active = false;
            }

            dust.alpha++;
            return false;

        }

        public override bool PreDraw(Dust dust)
        {
            return false;
        }

        public override void DrawForTarget(SpriteBatch spriteBatch, Dust dust)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Circle").Value;
            Vector2 drawPos = dust.position - Main.screenPosition;

            Main.spriteBatch.Draw(Tex, dust.position - Main.screenPosition, null, Color.White, dust.rotation, Tex.Size() / 2f, dust.scale * 0.55f, 0, 0);
        }
    }
}