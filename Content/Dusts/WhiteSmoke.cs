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

namespace VFXPlus.Content.Dusts
{
	public class SmallSmoke : ModDust
	{
		public override string Texture => "VFXPlus/Content/Dusts/Textures/WhiteSmokeSmall";

		public override void OnSpawn(Dust dust)
		{
            dust.alpha = Main.rand.Next(15);

            //FadeIn is used as a timer
            dust.fadeIn = 1f;

            dust.customData = null;

            dust.noGravity = true;
            dust.noLight = true;
        }

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return lightColor * (0.01f + dust.alpha / 250f) * (float)Math.Sin(dust.fadeIn / 120f * 3.14f);
		}

		public override bool Update(Dust dust)
		{
            if (dust.customData == null)
            {
                dust.customData = new SmallSmokeBehavior(1f, 0.99f, true, 6);
            }
            
            if (dust.customData is SmallSmokeBehavior ssb)
            {
                if (ssb.animate)
                {
                    if (dust.fadeIn % ssb.timeBetweenFrames == 0)
                        ssb.currentFrame = ((int)ssb.currentFrame + 1) % 5;
                }

                dust.scale *= ssb.scaleFadePower;// 0.999f;
            }

            dust.position += dust.velocity;

            float rotVel = dust.velocity.Y / 40f * (dust.alpha > 7 ? -1 : 1);
            dust.rotation += rotVel;


            dust.fadeIn++;

            if (dust.fadeIn > 120)
                dust.active = false;
            return false;

		}


		public override bool PreDraw(Dust dust)
		{
            Texture2D TexMain = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/WhiteSmokeSmall").Value;

            if (dust.customData is SmallSmokeBehavior ssb)
            {
                Vector2 drawPos = dust.position - Main.screenPosition;

                int frameHeight = TexMain.Height / 5;
                int startY = frameHeight * (int)ssb.currentFrame;
                Rectangle sourceRectangle = new Rectangle(0, startY, TexMain.Width, frameHeight);
                Vector2 origin = sourceRectangle.Size() / 2f;

                Color col = dust.color * (0.01f + dust.alpha / 250f) * (float)Math.Sin(dust.fadeIn / 120f * 3.14f);

                Main.EntitySpriteDraw(TexMain, drawPos, sourceRectangle, col * ssb.colorIntensity, dust.rotation, origin, dust.scale * 2f, SpriteEffects.None);
            }
            
            return false;
        }

	}

	public class SmallSmokeBehavior
	{
        public int currentFrame = 0;


		public bool animate = true;
        public int timeBetweenFrames = 6;

		public float colorIntensity = 1f;
        public float scaleFadePower = 0.999f;

        public SmallSmokeBehavior(float ColorIntensity = 1f, float ScaleFadePower = 0.999f)
        {
            colorIntensity = ColorIntensity;
            scaleFadePower = ScaleFadePower;
        }

        public SmallSmokeBehavior(float ColorIntensity = 1f, float ScaleFadePower = 0.999f, bool Animate = true, int TimeBetweenFrames = 6)
        {
            colorIntensity = ColorIntensity;
            scaleFadePower = ScaleFadePower;
        }
    }

    public class MediumSmoke : ModDust
    {
        public override string Texture => "VFXPlus/Content/Dusts/Textures/MediumWhiteSmoke";

        public override void OnSpawn(Dust dust)
        {
            //alpha is used as a timer 
            dust.alpha = 0;

            dust.customData = null;
        }
        
        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
            {
                dust.customData = new MediumSmokeBehavior(22, 0.98f, 0.01f, 1f);
            }

            if (dust.customData is MediumSmokeBehavior msb)
            {
                dust.velocity *= msb.velShrinkAmount;
                dust.scale += msb.scaleGrowthAmount;

                float halftime = (msb.maxTime / 2f);

                msb.totalOpacity = (float)Math.Pow((halftime - Math.Abs(halftime - dust.alpha)) / halftime, 0.75);

                if (dust.alpha >= msb.maxTime)
                    dust.active = false;
            }

            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.05f;


            dust.alpha++;
            return false;
        }


        public override bool PreDraw(Dust dust)
        {
            Texture2D TexMain = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/MediumWhiteSmoke").Value;

            if (dust.customData is MediumSmokeBehavior msb)
            {
                if (msb.currentFrame == -1)
                    Main.NewText("How did this happen");
                
                Vector2 drawPos = dust.position - Main.screenPosition;

                int frameHeight = TexMain.Height / 3;
                int startY = frameHeight * (int)msb.currentFrame;
                Rectangle sourceRectangle = new Rectangle(0, startY, TexMain.Width, frameHeight);
                Vector2 origin = sourceRectangle.Size() / 2f;

                Color lightColor = Lighting.GetColor((int)dust.position.X / 16, (int)dust.position.Y / 16);

                Color col = (lightColor.MultiplyRGBA(dust.color) * msb.totalOpacity);

                Main.EntitySpriteDraw(TexMain, drawPos, sourceRectangle, col * msb.colorIntensity, dust.rotation, origin, dust.scale * 1f, SpriteEffects.None);
            }

            return false;
        }

    }

    public class MediumSmokeBehavior
    {
        public int currentFrame = -1;
        public float totalOpacity = 1f;

        public float scaleGrowthAmount = 0.01f;
        public float velShrinkAmount = 0.98f;
        public float colorIntensity = 1f;

        public int maxTime = 22;

        public MediumSmokeBehavior(int MaxTime = 22, float VelShrinkAmount = 0.98f, float ScaleGrowthAmount = 0.01f, float ColorIntensity = 1f)
        {
            maxTime = MaxTime;
            velShrinkAmount = VelShrinkAmount;
            scaleGrowthAmount = ScaleGrowthAmount;
            colorIntensity = ColorIntensity;

            currentFrame = Main.rand.Next(0, 3);
        }

        public MediumSmokeBehavior(int MaxTime = 22, float ColorIntensity = 1f)
        {
            maxTime = MaxTime;
            colorIntensity = ColorIntensity;

            currentFrame = Main.rand.Next(0, 3);
        }
    }

}