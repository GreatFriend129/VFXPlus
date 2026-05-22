using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using VFXPlus.Common.Drawing;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;
using static Terraria.ModLoader.ModContent;

namespace VFXPlus.Content.Dusts
{
	public class WindLine : ModDust
	{
        public override string Texture => "VFXPlus/Assets/Pixel/Flare";


        public override void OnSpawn(Dust dust)
		{
			//Alpha is used as a timer in this dust
			dust.alpha = 0;

			//FadeIn is used as the opacity
			dust.fadeIn = 1f;

			dust.customData = null;

			dust.noGravity = true;
			dust.noLight = true;
		}


		public override bool Update(Dust dust)
		{
            if (dust.customData == null)
            {
                dust.customData = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, XScale: 1f, YScale: 1f, Pixelize: true);
            }

            if (dust.alpha == 0)
                (dust.customData as WindLineBehavior).initialVelLength = dust.velocity.Length();

            dust.rotation = dust.velocity.ToRotation();

            if (dust.customData is WindLineBehavior wlb)
            {
                if (dust.alpha >= wlb.timeToStartShrink)
                {
                    wlb.vec2Scale.X = wlb.vec2Scale.X * wlb.shrinkXScalePower;
                    wlb.vec2Scale.Y = wlb.vec2Scale.Y * wlb.shrinkYScalePower;

                }

                dust.velocity *= wlb.velFadePower;

                if (wlb.vec2Scale.X <= 0.07f || wlb.vec2Scale.Y <= 0.07f || dust.fadeIn <= 0.02f)
                    dust.active = false;


                dust.alpha++;

                if (dust.alpha == wlb.killEarlyTime)
                    dust.active = false;


                if (wlb.randomVelRotatePower > 0)
                {
                    //Ratio of current velocity over starting velocity
                    float dustVelPower = dust.velocity.Length() / wlb.initialVelLength;
                    dust.velocity = dust.velocity.RotateRandom(wlb.randomVelRotatePower * dustVelPower);
                }

            }

            dust.position += dust.velocity;

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.5f * dust.scale);


            return false;
		}


        public override bool PreDraw(Dust dust)
        {
            if (dust.customData == null)
                return false;

            if ((dust.customData as WindLineBehavior).pixelize)
            {
                ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
                {
                    Draw(dust);
                });
            }
            else
            {
                Draw(dust);
            }

            return false;
        }


        //Doing this in a separate method for the sake of convenience (allows easier testing of pixelization) 
        public void Draw(Dust dust)
        {
            Texture2D Tex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Color col = dust.color * dust.fadeIn;
            Vector2 origin = Tex.Size() / 2f;


            if (dust.customData is WindLineBehavior wlb)
            {
                Vector2 scale = dust.scale * new Vector2(wlb.vec2Scale.X, wlb.vec2Scale.Y * 0.5f);

                Main.spriteBatch.Draw(Tex, drawPos, null, col with { A = (byte)wlb.colorAlpha } * 0.9f, dust.rotation, origin, scale, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex, drawPos, null, Color.White with { A = (byte)wlb.colorAlpha } * wlb.whiteCoreIntensity, dust.rotation, origin, scale * 0.5f, SpriteEffects.None, 0f);
            }
        }
    }

    public class WindLineBehavior
	{
        public float initialVelLength = 0f;


        //Behavior - - - - - - - -

        //How much should the xScale/yScale shirnk (when timer > timeToStartShrink)
        public float shrinkXScalePower = 0.9f;
        public float shrinkYScalePower = 0.9f;

        //How much should the velocity fade
        public float velFadePower = 0.98f;

        //
        public float randomVelRotatePower = 0;
        
        //Defaults to 300 as a safety measure in case user doesn't give it scale or alpha fade
        public int killEarlyTime = 300;

        //How many frames before the yScale will start to shrink
        public int timeToStartShrink = 25;


        //Drawing - - - - - - - -
        public bool pixelize = false;

        public float whiteCoreIntensity = 1f;

		public Vector2 vec2Scale = new Vector2(1f, 1f);

        public RenderLayer renderLayer = RenderLayer.Dusts;

        public int colorAlpha = 0;

        //Basic constructor
        /// <summary>
        /// Custom behavior for a WindLine dust. Assign dust.customData to one of these (only works for a WindLine dust).
        /// </summary>
        /// <param name="VelFadePower"> How fast the velocity of the dust fades. 1f = no fade </param>
        /// <param name="TimeToStartShrink"> Number of frames before the dust's YScale starts shrinking. </param>
        /// <param name="ShrinkYScalePower"> How fast the dust's YScale shrinks once 'TimeToStartShrink' frames have passed. </param>
        /// <param name="XScale"> The dust's base XScale.  </param>
        /// <param name="YScale"> The dust's base YScale. </param>
        /// <param name="Pixelize"> Whether to Pixelize this dust or not. </param>
        public WindLineBehavior(float VelFadePower = 0.95f, int TimeToStartShrink = 15, float ShrinkYScalePower = 0.5f, float XScale = 1f, float YScale = 1f, bool Pixelize = true) 
        {
            velFadePower = VelFadePower;
            timeToStartShrink = TimeToStartShrink;
            shrinkYScalePower = ShrinkYScalePower;
            vec2Scale = new Vector2(XScale, YScale);
            pixelize = Pixelize;
        }


        //Kitchen Sink Contrusctor
        /// <summary>
        /// Custom behavior for a WindLine dust. Assign dust.customData to one of these (only works for a WindLine dust).
        /// </summary>
        /// <param name="VelFadePower"> How fast the velocity of the dust fades. 1f = no fade </param>
        /// <param name="TimeToStartShrink"> Number of frames before the dust's YScale starts shrinking. </param>
        /// <param name="ShrinkYScalePower"> How fast the dust's YScale shrinks once 'TimeToStartShrink' frames have passed. </param>
        /// <param name="XScale"> The dust's base XScale.  </param>
        /// <param name="YScale"> The dust's base YScale. </param>
        /// <param name="Pixelize"> Whether to Pixelize this dust or not. </param>
        /// <param name="WhiteCoreIntensity"> How intense the white core of the dust should draw. </param>
        /// <param name="RandomVelRotatePower"> Causes the dust's velocity to rotate by a random amount each frame. </param>
        /// <param name="KillEarlyTime"> Automatically kills the dust after this many frames. </param>
        public WindLineBehavior(float VelFadePower = 0.95f, int TimeToStartShrink = 15, float ShrinkYScalePower = 0.5f, float XScale = 1f, float YScale = 1f, bool Pixelize = true,
            float WhiteCoreIntensity = 1f, float RandomVelRotatePower = 0f, int KillEarlyTime = 300)
        {
            velFadePower = VelFadePower;
            timeToStartShrink = TimeToStartShrink;
            shrinkYScalePower = ShrinkYScalePower;
            vec2Scale = new Vector2(XScale, YScale);
            pixelize = Pixelize;

            whiteCoreIntensity = WhiteCoreIntensity;
            randomVelRotatePower = RandomVelRotatePower;
            killEarlyTime = KillEarlyTime;
        }
    }
}