using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace VFXPlus.Content.FeatheredFoe
{
    public class FFSky : CustomSky
    {
        private float bgPulsePower = 0f;
        
        
        
        private bool isActive = false;
        private float intensity = 0f;
        private int timer = 0;
        private UnifiedRandom myRand = new();


        private BackgroundFeather[] FallingFeathers;
        //private BackgroundFeather[] FallingFeathers;

        private Texture2D _bgTexture;
        private Texture2D _featherTexture;
        public override void OnLoad()
        {
            _bgTexture = ModContent.Request<Texture2D>("VFXPlus/Content/FeatheredFoe/Assets/FFSkyBorder", AssetRequestMode.ImmediateLoad).Value;
            _featherTexture = ModContent.Request<Texture2D>("VFXPlus/Content/FeatheredFoe/Assets/Feather", AssetRequestMode.ImmediateLoad).Value;

            //GenerateFeathers();
        }

        private int FeathersRemaining;
        private void GenerateFeathers()
        {
            FallingFeathers = new BackgroundFeather[400]; //400
            for (int i = 0; i < FallingFeathers.Length; i++)
            {
                FallingFeathers[i] = new BackgroundFeather();

                int num = (int)((Main.screenPosition.Y * 0.7) - Main.screenHeight);
                int minValue = (int)(num - (Main.worldSurface * 16.0));
                FallingFeathers[i].Center = new Vector2(myRand.Next(0, Main.maxTilesX) * 16, Main.rand.Next(minValue, num));
                FallingFeathers[i].Rotation = MathHelper.PiOver2;// Main.rand.NextFloat(6.282f);
                FallingFeathers[i].Speed = 5f + (3f * (float)myRand.NextDouble());
                FallingFeathers[i].Depth = ((float)i / FallingFeathers.Length * 1.75f) + 1.6f;
                if (myRand.NextBool(60))
                {
                    FallingFeathers[i].Speed = 6f + (3f * (float)myRand.NextDouble());
                    FallingFeathers[i].Depth += 0.5f;
                }
                else if (myRand.NextBool(30))
                {
                    FallingFeathers[i].Speed = 6f + (2f * (float)myRand.NextDouble());
                }

                FallingFeathers[i].Speed *= 2f;

                FallingFeathers[i].Active = true;
            }

            FeathersRemaining = FallingFeathers.Length;


            //Falling Feathers
            /*
            FallingFeathers = new BackgroundFeather[200];
            for (int i = 0; i < 200; i++)
            {
                FallingFeathers[i] = new BackgroundFeather();

                float startingX = Main.rand.Next(Main.screenWidth);
                float startingY = Main.rand.Next(Main.screenHeight);
                Vector2 startPos = new Vector2(startingX, startingY);

                FallingFeathers[i].Center = startPos;
                FallingFeathers[i].Speed = 5f + (3f * (float)myRand.NextDouble());
                FallingFeathers[i].Depth = ((float)i / FallingFeathers.Length * 1.75f) + 1.6f;
                FallingFeathers[i].Rotation = MathHelper.PiOver2;
            }
            */
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

            intensity = Math.Clamp(intensity, 0, 1);



            //Feathers
            #region FeatherUpdate
            for (int i = 0; i < FallingFeathers.Length; i++)
            {
                if (!FallingFeathers[i].Active)
                    continue;

                FallingFeathers[i].Center.Y += FallingFeathers[i].Speed * 2.5f; //2f
                //Feathers[i].rotation += Feathers[i].Speed / 65;
                if (!(FallingFeathers[i].Center.Y > Main.worldSurface * 16.0))
                    continue;

                FallingFeathers[i].Depth = (i / (float)FallingFeathers.Length * 1.75f) + 1.6f;
                FallingFeathers[i].Center = new Vector2(myRand.Next(0, Main.maxTilesX) * 16, -100f);
                //Feathers[i].Rotation = Main.rand.NextFloat(6.282f);
                FallingFeathers[i].Speed = 5f + (3f * (float)myRand.NextDouble());
                if (myRand.NextBool(60))
                {
                    FallingFeathers[i].Speed = 6f + (3f * (float)myRand.NextDouble());
                    FallingFeathers[i].Depth += 0.5f;
                }
                else if (myRand.NextBool(30))
                {
                    FallingFeathers[i].Speed = 6f + (2f * (float)myRand.NextDouble());
                }
            }

            #endregion

            timer++;
        }


        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {

                Color between = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 1f);

                //Sky
                spriteBatch.Draw(_bgTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), between with { A = 0 } * 0.1f * bgPulsePower);
            }

            //Feathers
            return;

            //Main.NewText(spriteBatch.ToString());
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            int num = -1;
            int num2 = 0;
            for (int i = 0; i < FallingFeathers.Length; i++)
            {
                float depth = FallingFeathers[i].Depth;
                if (num == -1 && depth < maxDepth)
                    num = i;

                if (depth <= minDepth)
                    break;

                num2 = i;
            }

            if (num == -1)
                return;

            Vector2 vector = Main.screenPosition + new Vector2(Main.screenWidth >> 1, Main.screenHeight >> 1);
            Rectangle rectangle = new(-1000, -1000, 4000, 4000);
            for (int j = num; j < num2; j++)
            {
                if (FallingFeathers[j].Active)
                {
                    Color color = new Color((Main.ColorOfTheSkies.ToVector4() * 0.9f) + new Vector4(0.1f)) * 0.8f;
                    float num3 = 1f;
                    if (FallingFeathers[j].Depth > 3f)
                        num3 = 0.6f;
                    else if (FallingFeathers[j].Depth > 2.5)
                        num3 = 0.7f;
                    else if (FallingFeathers[j].Depth > 2f)
                        num3 = 0.8f;
                    else if (FallingFeathers[j].Depth > 1.5)
                        num3 = 0.9f;

                    num3 *= 0.6f;
                    color = new Color((int)(color.R * num3), (int)(color.G * num3), (int)(color.B * num3), (int)(color.A * num3));
                    Vector2 vector2 = new(1f / FallingFeathers[j].Depth, 0.9f / FallingFeathers[j].Depth);
                    Vector2 position = FallingFeathers[j].Center;
                    position = ((position - vector) * vector2) + vector - Main.screenPosition;
                    position.X = (position.X + 500f) % 4000f;
                    if (position.X < 0f)
                        position.X += 4000f;

                    position.X -= 500f;
                    if (rectangle.Contains((int)position.X, (int)position.Y))
                    {
                        Vector2 origin = _featherTexture.Size() / 2;
                        for (int i = 1; i < 8; i += 2)
                            spriteBatch.Draw(_featherTexture, position - new Vector2(0, i * FallingFeathers[j].Speed), null, Color.SkyBlue with { A = 0 } * 1f * (0.5f - (0.5f * i / 8)), FallingFeathers[j].Rotation, origin, vector2.X * 1.25f, SpriteEffects.None, 0f);

                        spriteBatch.Draw(_featherTexture, position, null, Color.White * 1f, FallingFeathers[j].Rotation, origin, vector2.X * 1.25f, SpriteEffects.None, 0f);

                        spriteBatch.Draw(_featherTexture, position, null, Color.White with { A = 0 } * 0.4f, FallingFeathers[j].Rotation, origin, vector2.X * 1.25f, SpriteEffects.None, 0f);
                    }
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, default, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);
        }

        public bool CheckActive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<FeatheredFoeBoss>())
                {
                    if (Main.npc[i].ModNPC is FeatheredFoeBoss ff)
                    {
                        bgPulsePower = ff.bgPulsePower;
                    }

                    return true;
                }
            }
            return false;
        }


        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            GenerateFeathers();
        } 

        public override void Deactivate(params object[] args) => isActive = false;
        
        public override void Reset() => isActive = false;

        public override bool IsActive() => isActive;

    }

    public class BackgroundFeather
    {
        public Vector2 Center;
        public float Rotation;
        public float Depth;
        public float Speed;

        public bool Active;

        public Vector2 vec2Speed;
    }
}