using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using static Terraria.NPC;
using VFXPlus.Common;

namespace VFXPlus.Content.Gores
{
    public class BulletCasing : ModGore
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasing";

        public override bool Update(Gore gore)
        {
            gore.drawOffset = new Vector2(0f, -gore.AABBRectangle.Height / 2f);

            //Main.NewText(-gore.AABBRectangle.Height / 2f);

            if (gore.frameCounter == 0)
            {
                gore.rotation = Main.rand.NextFloat(6.28f);
                gore.alpha = 255;

                gore.position += new Vector2(0f, 4.5f);
            }

            float sizeProg = Utils.GetLerpValue(0, 255, gore.alpha, true);


            if (gore.frameCounter <= 60)
            {
                gore.alpha -= 20;

                gore.scale = 0.675f * Easings.easeInOutBack(1f - sizeProg, 0f, 2f);

            }
            else if (gore.frameCounter > 60)
            {
                gore.alpha += 10;

                //gore.scale = 0.675f * Easings.easeInOutQuad(1f - sizeProg);

                if (gore.alpha >= 250)
                    gore.active = false;
            }

            gore.alpha = Math.Clamp(gore.alpha, 0, 255);



            gore.velocity.X *= 0.99f;

            if (gore.velocity.Y == 0 && gore.light == 0 && gore.scale > 0.15f)
            {
                SoundStyle style = new SoundStyle("Terraria/Sounds/Coin_3") with { Volume = 0.015f, Pitch = -1f, PitchVariance = 0.1f, MaxInstances = -1 }; //.018
                SoundEngine.PlaySound(style, gore.position);

                gore.light = -0.01f;

                gore.velocity.X *= 0.5f;
            }

            gore.frameCounter++;

            return base.Update(gore);
        }

    }

    public class BulletCasingDust : ModDust
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasing";

        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.noGravity = false;

            //dust.fadeIn is used as a timer
            dust.fadeIn = 0;

            dust.firstFrame = true;
        }

        public override bool Update(Dust dust)
        {
            if (dust.fadeIn == 0)
            {
                dust.rotation = Main.rand.NextFloat(6.28f);
                dust.alpha = 255;
            }

            if (dust.firstFrame)
                dust.velocity.Y += 0.1f;

            float sizeProg = Utils.GetLerpValue(0, 255, dust.alpha, true);


            if (dust.fadeIn <= 60)
            {
                dust.alpha -= 20;

                dust.scale = 0.675f * Easings.easeInOutBack(1f - sizeProg, 0f, 2f);

            }
            else if (dust.fadeIn > 60)
            {
                dust.alpha += 10;

                dust.scale = 0.675f * Easings.easeInOutQuad(1f - sizeProg);

                if (dust.alpha >= 250)
                    dust.active = false;
            }

            dust.alpha = Math.Clamp(dust.alpha, 0, 255);

            dust.velocity.X *= 0.99f;

            Vector2 storedVel = dust.velocity;
            if (storedVel != Collision.TileCollision(dust.position - Vector2.One * 5f, dust.velocity, 10, 10) && dust.firstFrame)
            {
                dust.velocity *= 0.25f;
            }

            if (dust.velocity.Y == 0 && dust.firstFrame && dust.scale > 0.15f)
            {
                SoundStyle style = new SoundStyle("Terraria/Sounds/Coin_3") with { Volume = 0.015f, Pitch = -1f, PitchVariance = 0.1f, MaxInstances = -1 }; //.018
                SoundEngine.PlaySound(style, dust.position);

                dust.firstFrame = false;

                dust.velocity.X *= 0.5f;
            }

            dust.fadeIn++;

            dust.rotation += dust.velocity.X * 0.1f;
            dust.position += dust.velocity;
            return false;
            return base.Update(dust);
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

            float alpha = 1f - Utils.GetLerpValue(0, 255, dust.alpha, true);
            Main.spriteBatch.Draw(texture, dust.position - Main.screenPosition, null, dust.color * alpha, dust.rotation, texture.Size() / 2f, dust.scale, SpriteEffects.None, 0f);


            return false;
        }

        /*
        public override bool Update(Gore gore)
        {
            if (gore.frameCounter == 0)
            {
                gore.rotation = Main.rand.NextFloat(6.28f);
                gore.alpha = 255;
            }

            float sizeProg = Utils.GetLerpValue(0, 255, gore.alpha, true);


            if (gore.frameCounter <= 60)
            {
                gore.alpha -= 20;

                gore.scale = 0.675f * Easings.easeInOutBack(1f - sizeProg, 0f, 2f);

            }
            else if (gore.frameCounter > 60)
            {
                gore.alpha += 10;

                gore.scale = 0.675f * Easings.easeInOutQuad(1f - sizeProg);

                if (gore.alpha >= 250)
                    gore.active = false;
            }

            gore.alpha = Math.Clamp(gore.alpha, 0, 255);



            gore.velocity.X *= 0.99f;

            if (gore.velocity.Y == 0 && gore.light == 0 && gore.scale > 0.15f)
            {
                SoundStyle style = new SoundStyle("Terraria/Sounds/Coin_3") with { Volume = 0.015f, Pitch = -1f, PitchVariance = 0.1f, MaxInstances = -1 }; //.018
                SoundEngine.PlaySound(style, gore.position);

                gore.light = -0.01f;

                gore.velocity.X *= 0.5f;
            }

            gore.frameCounter++;

            return base.Update(gore);
        }
        */
    }

    public class BulletCasingSmall : BulletCasing
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasingSmall";
    }

    public class BulletCasingSmallRed : BulletCasing
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasingSmallRed";
    }

    public class AquaCasing : BulletCasing
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasingAqua";
    }
    public class GreenCasing : BulletCasing
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasingGreen";
    }
}

