using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using Steamworks;
using Terraria.Map;
using System.Threading;

namespace VFXPlus.Content.Dusts
{
    public class PaintSplotch : DrawOverTilesDust
    {
        public override string Texture => "VFXPlus/Content/Dusts/Textures/MediumWhiteSmoke";

        public override void OnSpawn(Dust dust)
        {
            dust.customData = new PaintSplotchDustBehavior();
        }

        public override bool Update(Dust dust)
        {
            PaintSplotchDustBehavior behavoir = (PaintSplotchDustBehavior)dust.customData;

            if (behavoir.timer == 0)
                dust.rotation = Main.rand.NextFloat(6.28f);

            dust.velocity = Vector2.Zero;

            if (behavoir.timer > behavoir.timeBeforeFadeOut)
            {
                behavoir.opacity *= 0.95f;

                if (behavoir.opacity < 0.05f || behavoir.timer >= 600)
                {
                    dust.active = false;
                }
            }

            dust.position += dust.velocity;
            behavoir.timer++;
            return false;
        }


        public override bool PreDraw(Dust dust)
        {
            return false;
        }

        public override void DrawOverTiles(SpriteBatch spriteBatch, Dust dust)
        {
            Texture2D TexMain = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/MediumWhiteSmoke").Value;

            if (dust.customData is PaintSplotchDustBehavior psdb)
            {
                Vector2 drawPos = dust.position - Main.screenPosition;

                int frameHeight = TexMain.Height / 3;
                int startY = frameHeight * (int)psdb.frame;
                Rectangle sourceRectangle = new Rectangle(0, startY, TexMain.Width, frameHeight);
                Vector2 origin = sourceRectangle.Size() / 2f;

                Color lightColor = Lighting.GetColor((int)dust.position.X / 16, (int)dust.position.Y / 16);

                Color col = lightColor.MultiplyRGBA(dust.color);

                Main.EntitySpriteDraw(TexMain, drawPos, sourceRectangle, col * psdb.opacity, dust.rotation, origin, dust.scale * 1f, SpriteEffects.None);
            }
        }
    }

    public class PaintSplotchDustBehavior
    {
        public int timer = 0;
        public float opacity = 1f;

        public int frame = 0;

        public int timeBeforeFadeOut = 30;

        public PaintSplotchDustBehavior()
        {
            frame = Main.rand.Next(0, 3);

            timeBeforeFadeOut = Main.rand.Next(45, 60);
        }
    }

}