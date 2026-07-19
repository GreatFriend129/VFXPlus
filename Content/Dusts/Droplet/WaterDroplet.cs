using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace VFXPlus.Content.Dusts
{
    public class WaterDroplet : ModDust
    {
        public override string Texture => "VFXPlus/Content/Dusts/Droplet/WaterDroplet";

        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.noGravity = false;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
                dust.customData = new WaterDropletBehavior();

            WaterDropletBehavior behavior = (dust.customData as WaterDropletBehavior);

            dust.velocity.Y += 0.2f;

            if (dust.velocity.Y > 12f)
            {
                dust.velocity.Y = 12f;
            }

            Vector2 storedVel = dust.velocity;
            dust.velocity = Collision.TileCollision(dust.position, dust.velocity, 4, 4);

            //if (dust.velocity != storedVel)
            //if (Collision.SolidCollision(dust.position, 4, 4, false))
            if (dust.velocity != storedVel)
            {
                //Spawn splash anim
                //Dust.NewDustPerfect(dust.position, ModContent.DustType<WaterSplash>());

                Vector2 gorePos = dust.position + new Vector2(-10f, 6f);

                gorePos.Y = gorePos.ToTileCoordinates().ToWorldCoordinates().Y;

                //Gore.NewGorePerfect(null, gorePos, Vector2.Zero, ModContent.GoreType<WaterSplashGore>(), Scale: 1f);

                Gore.NewGorePerfect(null, dust.position + new Vector2(-12f, 0f), Vector2.Zero, ModContent.GoreType<WaterSplashGore>(), Scale: 1f);

                //Dust.NewDustPerfect(dust.position, ModContent.DustType<GlowPixel>(), Scale: 0.15f);

                dust.velocity *= 0f;

                dust.active = false;
            }

            dust.position += dust.velocity;
            dust.rotation = dust.velocity.ToRotation();

            //Gore.NewGorePerfect(null, dust.position, Vector2.Zero, DustID.Smoke, Scale: 0.25f);

            //Dust d = Dust.NewDustPerfect(dust.position + new Vector2(1f, 0f), DustID.Adamantite, Scale: 2f);
            //d.velocity = Vector2.Zero;
            //d.noGravity = true;



            if (behavior.timer > 200)
                dust.active = false;

            if (behavior.timer % 6 == 0)
                behavior.currentFrame = (behavior.currentFrame + 1) % 3;

            dust.color = Lighting.GetColor(dust.position.ToTileCoordinates());


            behavior.timer++;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            if (dust.customData == null)
                dust.customData = new WaterDropletBehavior();

            WaterDropletBehavior behavior = (dust.customData as WaterDropletBehavior);

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/Dusts/Droplet/WaterDroplet").Value;

            int frameHeight = Tex.Height / 3;
            int startY = frameHeight * behavior.currentFrame;
            Rectangle sourceRectangle = new Rectangle(0, startY, Tex.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Main.spriteBatch.Draw(Tex, dust.position - Main.screenPosition, sourceRectangle, dust.color, dust.rotation, origin, dust.scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    //Not needed here but makes things a bit cleaner imo
    public class WaterDropletBehavior
    {
        public int timer;

        public int currentFrame = 0;
    }
}