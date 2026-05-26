using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace VFXPlus.Content.Dusts
{
	public abstract class DrawOverTilesDust : ModDust
	{
        public virtual void DrawOverTiles(SpriteBatch spriteBatch, Dust dust) { }
    }
}