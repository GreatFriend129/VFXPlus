using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace VFXPlus.Content.Dusts
{
	public class RenderTargetDustTest : ModDust
	{
        public override string Texture => "VFXPlus/Assets/Pixel/Flare";

        public override void OnSpawn(Dust dust)
		{
		}

		public override bool Update(Dust dust)
		{
            dust.noGravity = true;

            dust.position += dust.velocity;
            dust.velocity *= 0.92f;
            dust.scale *= 0.98f;
            dust.alpha += 15;

            if (dust.scale < 0.25f)
            {
                dust.active = false;
            }
            return false;

		}

        public override bool PreDraw(Dust dust)
        {
            return false;
        }
	}
}