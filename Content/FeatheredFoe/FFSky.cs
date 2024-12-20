using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;

namespace VFXPlus.Content.FeatheredFoe
{
    public class FFSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private int timer = 0;
        private float bonusIntensity;

        private Vector2[] bgLines = new Vector2[100]; //50
        private int[] xPos = new int[50];
        private int[] yPos = new int[50];


        bool runOnce = true;

        private Texture2D _bgTexture;
        private Texture2D _lineTexture;
        public override void OnLoad()
        {
            _bgTexture = ModContent.Request<Texture2D>("VFXPlus/Content/FeatheredFoe/Assets/FFSky1080", AssetRequestMode.ImmediateLoad).Value;
            _lineTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/GlowingFlare", AssetRequestMode.ImmediateLoad).Value;
            //_bgTexture = ModContent.Request<Texture2D>("Terraria/Images/Misc/StarDustSky/Background", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
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

            intensity = Math.Clamp(intensity, 0, 1);

            timer++;
        }

        public bool CheckActive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<FeatheredFoe>())
                {
                    return true;
                }
            }
            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {

            if (maxDepth >= 0 && minDepth < 0)
            {

                //Sky
                ///spriteBatch.Draw(_bgTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.LightSkyBlue with { A = 0 } * 0.05f * intensity);

                //Lines

                if (runOnce)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        bgLines[i].X = Main.rand.Next(Main.screenWidth);
                        bgLines[i].Y = Main.rand.NextBool() ? Main.rand.Next(0, (int)(Main.screenHeight / 4.5f)) : Main.rand.Next((int)((Main.screenHeight / 4.5f) * 3.5), Main.screenHeight);
                    }
                    runOnce = false;
                }

                for (int i = 0; i < 0; i++)
                {

                    if (i % 2 == 0)
                        bgLines[i] += new Vector2(10f + (i / 15), 0) * 1f;
                    else
                        bgLines[i] -= new Vector2(10f + (i / 15), 0) * 1f;

                    if (bgLines[i].X > (Main.screenWidth + 300))
                    {
                        bgLines[i].X = -200 + Main.rand.Next(-1000, 180);
                        bgLines[i].Y = Main.rand.NextBool() ? Main.rand.Next(0, (int)(Main.screenHeight / 4.5f)) : Main.rand.Next((int)((Main.screenHeight / 4.5f) * 3.5), Main.screenHeight);
                    }
                    else if (bgLines[i].X < -300)
                    {
                        bgLines[i].X = Main.screenWidth + 200 + Main.rand.Next(-180, 1000);
                        bgLines[i].Y = Main.rand.NextBool() ? Main.rand.Next(0, (int)(Main.screenHeight / 4.5f)) : Main.rand.Next((int)((Main.screenHeight / 4.5f) * 3.5), Main.screenHeight);
                    }

                    float width2 = Main.rand.NextFloat(0.25f, 1.75f);

                    Color colToUse = Color.Lerp(Color.DeepSkyBlue, Color.DeepSkyBlue, bgLines[i].Y / Main.screenHeight);

                    spriteBatch.Draw(_lineTexture, new Vector2(bgLines[i].X, bgLines[i].Y), null, colToUse with { A = 0 } * intensity * 1f, 0, 
                        _lineTexture.Size() / 2f, new Vector2(width2, 0.10f), SpriteEffects.None, 0f);

                    //spriteBatch.Draw(AerovelenceMod.Instance.Assets.Request<Texture2D>("Assets/TrailImages/Starlight", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                        //new Vector2(bgLines[i].X, bgLines[i].Y), null, Color.White with { A = 0 } * bonusIntensity * intensity * lineAlpha, 0, new Vector2(36, 36), new Vector2(width2, 0.10f + (2f * bgLineBoost)) * 0.5f, SpriteEffects.None, 0f);
                }

            }

        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            return isActive;
        }

    }
}