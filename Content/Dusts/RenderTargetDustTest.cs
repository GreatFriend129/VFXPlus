using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace VFXPlus.Content.Dusts
{
	public class RenderTargetDustTest : ModDust
	{
        public override string Texture => "VFXPlus/Assets/Pixel/Flare";

        public override void OnSpawn(Dust dust)
		{
            dust.customData = new RenderTargetDustBehavoir();
		}

		public override bool Update(Dust dust)
		{
            RenderTargetDustBehavoir behavoir = (RenderTargetDustBehavoir)dust.customData;

            if (behavoir.timer == 0)
                dust.rotation = Main.rand.NextFloat(6.28f);

            //Frame
            behavoir.animFrameTimer++;
            if (behavoir.animFrameTimer++ >= 0)
            {
                behavoir.animFrameTimer = 0;
                behavoir.animFrame = (behavoir.animFrame + 1) % 64;
                behavoir.animFrame = (behavoir.animFrame + 1) % 64;
                //behavoir.animFrame = (behavoir.animFrame + 1) % 64;

                if (Main.rand.NextBool())
                {
                    behavoir.animFrame = (behavoir.animFrame + 1) % 64;
                }

                if (Main.rand.NextBool(3))
                {
                    behavoir.animFrame = (behavoir.animFrame + 1) % 64;
                }
            }

            dust.position += dust.velocity;
            dust.position += dust.velocity;

            dust.velocity *= 0.95f; //0.95f
            dust.scale *= 0.95f;//95

            dust.scale *= 0.98f;//95

            if (behavoir.timer > 12)
                dust.scale *= 0.9f;

            if (dust.scale < 0.05f || behavoir.timer >= 100 || behavoir.animFrame >= 60)
            {
                dust.active = false;
            }

            behavoir.timer++;
            return false;

		}

        public override bool PreDraw(Dust dust)
        {
            return false;
        }
	}

    public class RenderTargetDustBehavoir
    {
        public int timer = 0;
        public int animFrame = 0;
        public int animFrameTimer = 0;
    }
}