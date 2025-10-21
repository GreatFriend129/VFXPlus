using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace VFXPlus.Content.FeatheredFoe
{
    public class FFSky : CustomSky
    {
        private float bgPulsePower = 0f;
        private float timeSpeed = 1f;
        
        
        private bool isActive = false;
        private float intensity = 0f;
        private int timer = 0;


        private Texture2D _bgTexture;
        private Texture2D _mainTexture;
        public override void OnLoad()
        {
            _bgTexture = ModContent.Request<Texture2D>("VFXPlus/Content/FeatheredFoe/Assets/FFSkyBorder", AssetRequestMode.ImmediateLoad).Value;
            _mainTexture = ModContent.Request<Texture2D>("VFXPlus/Content/FeatheredFoe/Assets/FFSkyMain", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Update(GameTime gameTime)
        {
            const float increment = 0.01f;
            if (CheckActive())
            {
                intensity += increment;
                if (intensity > 1f)
                {
                    intensity = MathHelper.Lerp(intensity, 1, 0.2f); //1f;
                }

                //We cant do this here because it would never go down as we reset the bgPulsePower in CheckActive()
                //bgPulsePower = Math.Clamp(MathHelper.Lerp(bgPulsePower, -1f, 0.07f), 0f, 100f);
            }
            else
            {
                intensity -= increment;
                if (intensity <= 0f)
                {
                    intensity = 0f;
                    Deactivate();
                }
            }

            windRotation = MathHelper.Lerp(windRotation, windRotationGoal, 0.1f);

            intensity = Math.Clamp(intensity, 0, 1);

            //Simple trig to get the direction components | xComponent of right triangle is cos(theta) and y is sin(theta)
            float xDir = MathF.Cos(windRotation);
            float yDir = MathF.Sin(windRotation);

            float mult = 0.002f * (1f + bgPulsePower * 7f);

            xOffset -= xDir * mult;
            yOffset -= yDir * mult;

            timer++;
        }

        private float xOffset = 0f;
        private float yOffset = 0f;
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {

            if (maxDepth >= 0 && minDepth < 0)
            {
                Color between = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 1f);

                float sineAlpha =  0.1f + MathF.Sin((float)Main.timeForVisualEffects * 0.02f) * 0.05f;

                //Sky
                spriteBatch.Draw(_bgTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), between with { A = 0 } * (sineAlpha + bgPulsePower * 0.2f));
            }

            if (minDepth < 9f && maxDepth > 9) //min < 9 | max > 9 = under tiles player npcs
            {
                Effect smokeEffect = VFXPlus.SmokeNoiseShader;

                Color smokeCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.25f);
                Color smokeB = Color.Lerp(Color.Silver, smokeCol, Easings.easeOutSine(bgPulsePower));

                float smokeAlpha = 0.15f + (bgPulsePower * 0.25f);

                smokeEffect.Parameters["zoom"].SetValue(3.0f);
                smokeEffect.Parameters["color"].SetValue(smokeB.ToVector3() * smokeAlpha); //0f
                smokeEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.07f);

                smokeEffect.Parameters["xOffset"].SetValue(xOffset);
                smokeEffect.Parameters["yOffset"].SetValue(yOffset);
                //smokeEffect.Parameters["yOffset"].SetValue(-(float)Main.timeForVisualEffects * 0.001f);


                //DeepSkyBlue -> SkyBlue | 5.0 | 0.25f | 0.05f

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, default, default, smokeEffect);


                spriteBatch.Draw(_mainTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, default, default, null, Main.GameViewMatrix.TransformationMatrix);

            }

            return;
        }

        
        private float windRotation = 0f;
        private float windRotationGoal = 0f;
        public bool CheckActive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<FeatheredFoeBoss>())
                {
                    if (Main.npc[i].ModNPC is FeatheredFoeBoss ff)
                    {
                        bgPulsePower = ff.windOverlayOpacity;// ff.bgPulsePower;
                        windRotationGoal = ff.windOverlayRotation;
                    }

                    return true;
                }
            }
            return false;
        }


        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        } 

        public override void Deactivate(params object[] args) => isActive = false;
        
        public override void Reset() => isActive = false;

        public override bool IsActive() => isActive;

    }

}