using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Dusts;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    //Literally just so that the proj can alternate color
    public class NettleBurstPlayer : ModPlayer
    {
        public bool makeRed = false;
    }

    public class NettleBurstBaseOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NettleBurstLeft || entity.type == ProjectileID.NettleBurstRight) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NettleBurstToggle;
        }

        bool isRed = false;

        public float overallScale = 0f;
        public float overallAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && projectile.ai[1] == 0)
            {
                projectile.ai[2] = Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed ? 1 : -1;
                Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed = projectile.ai[2] != 1;
            }
            isRed = projectile.ai[2] == 1;

            if (timer == 0 && projectile.ai[1] % 2 == 0)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.07f); //14
                float pitch2 = -0.4f + (projectile.ai[1] * 0.07f); //14
                float pitch3 = 0f + (projectile.ai[1] * 0.11f); //14

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .025f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; 
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.07f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);

                SoundStyle style3 = new SoundStyle("VFXPlus/Sounds/Effects/Earth/PlantGrowth") with { Volume = 0.15f, Pitch = pitch3, PitchVariance = 0.05f, MaxInstances = -1 };
                SoundEngine.PlaySound(style3, projectile.Center);
            }


            float timeForPopInAnim = 25;
            float animProgress = Math.Clamp((timer + 7) / timeForPopInAnim, 0f, 1f); //15 60

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2f));

            if (overallScale == 1f && timer > 42)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.25f, 0.04f), 0f, 1f);

            if (timer >= 4 && timer <= 11 && timer % 2 == 0) //7
            {
                for (int i = 220; i < 1; i++) //5 + Main.rand.Next(1, 3)
                {
                    Color col = isRed ? Color.Red : Color.ForestGreen;

                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(7f, 7f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: col * 1f, Scale: Main.rand.NextFloat(0.2f, 0.25f) * projectile.scale * 1.5f); //3

                    p.velocity += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * -1f;

                    //int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<GlowPixelCross>(), newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f) * projectile.scale);
                }

            }

            if (timer >= 2 && timer <= 9 && timer % 3 == 0)
            {
                Color col = isRed ? Color.Red : Color.ForestGreen;

                Vector2 posOffset = Main.rand.NextVector2Circular(7f, 7f) + new Vector2(0f, 0f);
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f);

                Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<PulseInOutDust>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: col with { A = 20 }, Scale: Main.rand.NextFloat(0.4f, 0.5f) * projectile.scale * 2f);
                p.velocity += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * -1f;
                p.velocity *= 0.85f;
                p.rotation = MathHelper.PiOver4;
                p.noLight = false;

                p.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 18, 0.5f, 0.5f, true);
            }

            if (timer == 7)
            {

                for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++)
                {
                    Color col = Color.Brown * 2f;

                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(5f, 5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, DustID.WoodFurniture, vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: Color.LightPink, Scale: Main.rand.NextFloat(1f, 1.3f)); //3

                    p.alpha = 200;

                    p.noGravity = true;
                    Dust dust23 = p;
                    Dust dust3 = dust23;
                    dust3.velocity *= 0.5f;
                }
            }

            timer++;

            #region vanillaAI
            if (Main.netMode != 2 && projectile.ai[1] == 0f && projectile.localAI[0] == 0f)
            {
                projectile.localAI[0] = 1f;
                SoundStyle legacySoundStyle = SoundID.Item8;
                if (projectile.type == 494)
                {
                    legacySoundStyle = SoundID.Item101;
                }
                SoundEngine.PlaySound(in legacySoundStyle, projectile.Center);
            }
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            if (projectile.ai[0] == 0f)
            {
                if (projectile.type >= 150 && projectile.type <= 152 && projectile.ai[1] == 0f && projectile.alpha == 255 && Main.rand.Next(2) == 0)
                {
                    projectile.type++;
                    projectile.netUpdate = true;
                }
                projectile.alpha -= 50;
                if (projectile.type >= 150 && projectile.type <= 152)
                {
                    projectile.alpha -= 25;
                }
                else if (projectile.type == 493 || projectile.type == 494)
                {
                    projectile.alpha -= 50;
                }
                if (projectile.alpha > 0)
                {
                    return false;
                }
                projectile.alpha = 0;
                projectile.ai[0] = 1f;
                if (projectile.ai[1] == 0f)
                {
                    projectile.ai[1] += 1f;
                    projectile.position += projectile.velocity * 1f;
                }
                if (projectile.type == 7 && Main.myPlayer == projectile.owner)
                {
                    int num755 = projectile.type;
                    if (projectile.ai[1] >= 6f)
                    {
                        num755++;
                    }
                    int num766 = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + projectile.velocity.X + (float)(projectile.width / 2), projectile.position.Y + projectile.velocity.Y + (float)(projectile.height / 2), projectile.velocity.X, projectile.velocity.Y, num755, projectile.damage, projectile.knockBack, projectile.owner);
                    Main.projectile[num766].damage = projectile.damage;
                    Main.projectile[num766].ai[1] = projectile.ai[1] + 1f;
                    NetMessage.SendData(27, -1, -1, null, num766);
                }
                else if (projectile.type == 494 && Main.myPlayer == projectile.owner)
                {
                    int num777 = projectile.type;
                    if (projectile.ai[1] >= (float)(7 + Main.rand.Next(2)))
                    {
                        num777--;
                    }
                    int num788 = projectile.damage;
                    float num799 = projectile.knockBack;
                    if (num777 == 493)
                    {
                        num788 = (int)((double)projectile.damage * 1.25);
                        num799 = projectile.knockBack * 1.25f;
                    }
                    int number = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + projectile.velocity.X + (float)(projectile.width / 2),
                        projectile.position.Y + projectile.velocity.Y + (float)(projectile.height / 2), projectile.velocity.X, projectile.velocity.Y, num777, num788, num799, projectile.owner, 0f, projectile.ai[1] + 1f);
                    NetMessage.SendData(27, -1, -1, null, number);
                }
                else if ((projectile.type == 150 || projectile.type == 151) && Main.myPlayer == projectile.owner)
                {
                    int num810 = projectile.type;
                    if (projectile.type == 150)
                    {
                        num810 = 151;
                    }
                    else if (projectile.type == 151)
                    {
                        num810 = 150;
                    }
                    if (projectile.ai[1] >= 10f && projectile.type == 151)
                    {
                        num810 = 152;
                    }
                    int num821 = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + projectile.velocity.X + (float)(projectile.width / 2), 
                        projectile.position.Y + projectile.velocity.Y + (float)(projectile.height / 2), projectile.velocity.X, projectile.velocity.Y, num810, projectile.damage, projectile.knockBack, projectile.owner);
                    Main.projectile[num821].damage = projectile.damage;
                    Main.projectile[num821].ai[1] = projectile.ai[1] + 1f;

                    //Added
                    Main.projectile[num821].ai[2] = projectile.ai[2];

                    NetMessage.SendData(27, -1, -1, null, num821);
                }
                return false;
            }
            if (projectile.alpha < 170 && projectile.alpha + 5 >= 170)
            {
                if (projectile.type >= 150 && projectile.type <= 152)
                {
                    for (int num832 = 220; num832 < 8; num832++)
                    {
                        int num843 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 7, projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 200, default(Color), 1.3f);
                        Main.dust[num843].noGravity = true;
                        Dust dust41 = Main.dust[num843];
                        Dust dust212 = dust41;
                        dust212.velocity *= 0.5f;
                    }
                }
                else if (projectile.type == 493 || projectile.type == 494)
                {
                    for (int num855 = 220; num855 < 8; num855++)
                    {
                        int num866 = Dust.NewDust(projectile.position, projectile.width, projectile.height, Main.rand.Next(68, 71), projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 200, default(Color), 1.3f);
                        Main.dust[num866].noGravity = true;
                        Dust dust42 = Main.dust[num866];
                        Dust dust212 = dust42;
                        dust212.velocity *= 0.5f;
                    }
                }
                else
                {
                    for (int num877 = 220; num877 < 3; num877++)
                    {
                        Dust.NewDust(projectile.position, projectile.width, projectile.height, 18, projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 170, default(Color), 1.2f);
                    }
                    Dust.NewDust(projectile.position, projectile.width, projectile.height, 14, 0f, 0f, 170, default(Color), 1.1f);
                }
            }
            if (projectile.type >= 150 && projectile.type <= 152)
            {
                projectile.alpha += 3;
            }
            else if (projectile.type == 493 || projectile.type == 494)
            {
                projectile.alpha += 4;
            }
            else
            {
                projectile.alpha += 5;
            }
            if (projectile.alpha >= 255)
            {
                projectile.Kill();
            }
            #endregion

            return false;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Vector2 vec2Scale = new Vector2(overallScale * projectile.scale, projectile.scale);

            Color col = isRed ? Color.Red : new Color(0, 170, 0);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                for (int i = 0; i < 4; i++)
                {
                    float borderAlpha = projectile.Opacity * Easings.easeInCirc(overallAlpha);
                    Vector2 offset = (2f * (i * MathHelper.PiOver2).ToRotationVector2());

                    Main.spriteBatch.Draw(vanillaTex, drawPos + offset, null,
                        col with { A = 50 } * borderAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
                }
            });

            Effect myEffect = ModContent.Request<Effect>("Playground/Effects/Filter/Dissolve", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["progress"].SetValue(1f - overallAlpha);


            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value;
            myEffect.Parameters["maskTexture"].SetValue(Mask);
            myEffect.Parameters["zoom"].SetValue(1f);

            //new Color(20, 14, 14)
            Color dissolveCol = isRed ? new Color(30, 0, 0) : new Color(0, 30, 0);
            myEffect.Parameters["innerCol"].SetValue(dissolveCol.ToVector3());
            myEffect.Parameters["outerCol"].SetValue(dissolveCol.ToVector3());
            myEffect.Parameters["dissolveColMult"].SetValue(1f);

            myEffect.Parameters["mainTexWidth"].SetValue(vanillaTex.Width / 2f);
            myEffect.Parameters["mainTexHeight"].SetValue(vanillaTex.Height / 2f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.pixelShader.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            return false;            
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return false;
            return base.PreKill(projectile, timeLeft);
        }
    }

    public class NettleBurstTipShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NettleBurstEnd) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NettleBurstToggle;
        }

        bool isRed = false;

        float overallScale = 0;
        float overallAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && projectile.ai[1] == 0)
            {
                projectile.ai[2] = Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed ? 1 : -1;
                Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed = projectile.ai[2] != 1;
            }
            isRed = projectile.ai[2] == 1;


            if (timer == 4)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);
                float pitch3 = 0f + (projectile.ai[1] * 0.11f); //14

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .025f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.07f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);

                SoundStyle style3 = new SoundStyle("VFXPlus/Sounds/Effects/Earth/PlantGrowth") with { Volume = 0.15f, Pitch = pitch3, PitchVariance = 0.05f, MaxInstances = -1 };
                SoundEngine.PlaySound(style3, projectile.Center);
            }


            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2f));

            if (overallScale == 1f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.05f), 0f, 1f);

            if (timer >= 4 && timer <= 11 && timer % 2 == 0) //7
            {
                for (int i = 0; i < 1; i++) //5 + Main.rand.Next(1, 3)
                {
                    Color col = isRed ? Color.Red : Color.ForestGreen;

                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(7f, 7f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: col * 1f, Scale: Main.rand.NextFloat(0.2f, 0.25f) * projectile.scale * 1.5f); //3

                    p.velocity += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * -1f;
                }
            }

            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Vector2 vec2Scale = new Vector2(overallScale * projectile.scale, projectile.scale);

            Color col = isRed ? Color.Red : Color.Green;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                for (int i = 0; i < 4; i++)
                {
                    float borderAlpha = projectile.Opacity * Easings.easeInCirc(overallAlpha);
                    Vector2 offset = (2f * (i * MathHelper.PiOver2).ToRotationVector2());

                    Main.spriteBatch.Draw(vanillaTex, drawPos + offset, null,
                        col with { A = 50 } * borderAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
                }
            });

            Effect myEffect = ModContent.Request<Effect>("Playground/Effects/Filter/Dissolve", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["progress"].SetValue(1f - overallAlpha);


            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value;
            myEffect.Parameters["maskTexture"].SetValue(Mask);
            myEffect.Parameters["zoom"].SetValue(1f);

            myEffect.Parameters["innerCol"].SetValue(col.ToVector3());
            myEffect.Parameters["outerCol"].SetValue(col.ToVector3());
            myEffect.Parameters["dissolveColMult"].SetValue(1f);

            myEffect.Parameters["mainTexWidth"].SetValue(vanillaTex.Width / 2f);
            myEffect.Parameters["mainTexHeight"].SetValue(vanillaTex.Height / 2f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.pixelShader.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }
    }
}
