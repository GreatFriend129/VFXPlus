using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace VFXPlus.Content.Dusts
{
    public class WaterSplashGore : ModGore
    {
        public override string Texture => "VFXPlus/Content/Dusts/Droplet/WaterSplash";

        public override void SetStaticDefaults()
        {
            ChildSafety.SafeGore[Type] = true;
            GoreID.Sets.LiquidDroplet[Type] = true;
        }

        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            gore.numFrames = 5;
            gore.behindTiles = true;
            gore.timeLeft = 200;
        }

        public override bool Update(Gore gore)
        {
            gore.velocity = Vector2.Zero;

            gore.behindTiles = true;

            if (gore.frameCounter >= 4)
            {
                gore.frameCounter = 0;
                gore.frame++;
                
                if (gore.frame >= 5)
                {
                    gore.active = false;
                }
            }

            Vector2 goreToTile = (gore.position / 16f).Floor();

            //gore.position.Y = goreToTile.Y;

            gore.frameCounter++;

            return false;
        }
    }

    public class WaterSplash : ModDust
    {
        public override string Texture => "VFXPlus/Content/Dusts/Droplet/WaterSplash";

        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.noGravity = false;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
                dust.customData = new WaterSplashBehavior();

            WaterSplashBehavior behavior = (dust.customData as WaterSplashBehavior);

            dust.velocity = Vector2.Zero;

            if (behavior.timer > 200)
                dust.active = false;

            if (behavior.timer % 3 == 0)
                behavior.currentFrame += 1;

            if (behavior.currentFrame == 5)
                dust.active = false;

            dust.color = Lighting.GetColor(dust.position.ToTileCoordinates());

            behavior.timer++;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            if (dust.customData == null)
                dust.customData = new WaterSplashBehavior();
            
            WaterSplashBehavior behavior = (dust.customData as WaterSplashBehavior);

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/Dusts/Droplet/WaterSplash").Value;

            Vector2 position = dust.position.ToTileCoordinates().ToWorldCoordinates();

            int frameHeight = Tex.Height / 5;
            int startY = frameHeight * behavior.currentFrame;
            Rectangle sourceRectangle = new Rectangle(0, startY, Tex.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Main.spriteBatch.Draw(Tex, dust.position - Main.screenPosition + new Vector2(0f, 4f), sourceRectangle, dust.color, dust.rotation, origin, dust.scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    //Not needed here but makes things a bit cleaner imo
    public class WaterSplashBehavior
    {
        public int timer;

        public int currentFrame = 0;
    }
}