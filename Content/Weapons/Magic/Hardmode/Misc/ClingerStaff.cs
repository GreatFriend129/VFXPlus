using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using rail;
using VFXPlus.Common.Drawing;
using Terraria.Audio;

namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{

    //Todo explain why I use a seperate proj for this wep
    public class ClingerStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ClingerStaff) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ClingerStaffToggle;
        }

        int vfx_child_index = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            #region VanillaCodeMinusDust
            projectile.position.Y = projectile.ai[0];
            projectile.height = (int)projectile.ai[1];
            if (projectile.Center.X > Main.player[projectile.owner].Center.X)
            {
                projectile.direction = 1;
            }
            else
            {
                projectile.direction = -1;
            }
            projectile.velocity.X = (float)projectile.direction * 1E-06f;
            if (projectile.owner == Main.myPlayer)
            {
                for (int num804 = 0; num804 < 1000; num804++)
                {
                    if (Main.projectile[num804].active && num804 != projectile.whoAmI && Main.projectile[num804].type == projectile.type && Main.projectile[num804].owner == projectile.owner && Main.projectile[num804].timeLeft > projectile.timeLeft)
                    {
                        projectile.Kill();
                        return false;
                    }
                }
            }
            #endregion

            //Spawn VFX projectile
            if (timer == 0)
            {
                Vector2 vfxPos = projectile.Center + new Vector2(0f, projectile.height / 2);

                int p = Projectile.NewProjectile(projectile.GetSource_FromThis(), vfxPos, Vector2.Zero, ModContent.ProjectileType<ClingerStaffVFX>(), 0, 0, projectile.owner);
                vfx_child_index = p;
                Main.projectile[p].rotation = -MathHelper.PiOver2;

                //Sink projectile a little further into ground
                Main.projectile[p].Center += new Vector2(0f, 9f);

                //Play extra sound
                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/hero_fury_charm_burst") with { Volume = 0.75f, Pitch = 0.05f, PitchVariance = 0.15f, MaxInstances = 1 }; 
                SoundEngine.PlaySound(style, vfxPos);
            }

            timer++;
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //Let child know we have detatched
            if (Main.projectile[vfx_child_index] != null)
                (Main.projectile[vfx_child_index].ModProjectile as ClingerStaffVFX).isAttached = false;

            return base.PreKill(projectile, timeLeft);
        }
    }

    public class ClingerStaffVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        #region Loading
        public static Asset<Texture2D> circle_053 = null;
        public static Asset<Texture2D> muzzle_flash_12 = null;
        public static Asset<Texture2D> star_07 = null;
        public static Asset<Texture2D> circle_053Black = null;

        public override void Load()
        {
            circle_053 = ModContent.Request<Texture2D>("VFXPlus/Assets/MuzzleFlashes/circle_053");
            muzzle_flash_12 = ModContent.Request<Texture2D>("VFXPlus/Assets/MuzzleFlashes/muzzle_flash_12");
            star_07 = ModContent.Request<Texture2D>("VFXPlus/Assets/Flare/star_07");
            circle_053Black = ModContent.Request<Texture2D>("VFXPlus/Assets/MuzzleFlashes/circle_053Black");
        }

        public override void Unload()
        {
            circle_053 = null;
            muzzle_flash_12 = null;
            star_07 = null;
            circle_053Black = null;
        }
        #endregion

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 700;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;
        
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;

            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 25000; 
        }

        int timer = 0;
        float true_width = 0f;
        public bool isAttached = true;
        public override void AI()
        {
            if (timer == 2)
            {
                //Smoke Explosion
                for (int i = 0; i < 30; i++)
                {
                    float progress = (float)i / 30f;

                    Vector2 spawnPos = Projectile.Center + new Vector2(0f, -1f) * Main.rand.NextFloat(0, 280f * progress);
                    Vector2 smvel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 18f * (1f - progress));

                    Dust sm = Dust.NewDustPerfect(spawnPos, ModContent.DustType<GlowPixelAlts>(), smvel, newColor: Color.GreenYellow * 1f, Scale: Main.rand.NextFloat(0.65f, 1f));
                    sm.alpha = 10;

                    sm.velocity.X *= 0.75f;
                    if (smvel.Y > 0)
                        sm.velocity.Y *= -1;

                    GlowPixelAltBehavior bev = new GlowPixelAltBehavior();
                    bev.base_fadeOutPower = 0.9f;
                    sm.customData = bev;
                }
            }
            
            if (isAttached)
            {
                float progress = Math.Clamp((timer + 5) / 20f, 0f, 1f); //timer / 50
                true_width = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(progress, 0f, 2.5f));
            }
            else
            {
                true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.06f), 0f, 1f);
            }

            if (true_width <= 0)
                Projectile.active = false;


            //Dust
            float rot = -MathHelper.PiOver2;
            Color newGreen = new Color(100, 255, 34);
            bool shouldStopDust = (true_width <= 0.99);
            if (timer > 5 && timer % 2 == 0 && !shouldStopDust)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 pos = Projectile.Center + new Vector2(0f, -1f) * Main.rand.NextFloat(0, 160);

                    if (Main.rand.NextBool())
                    {
                        Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10, 10);
                        Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.75f) * Main.rand.NextFloat(2, 7);

                        Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowPixelAlts>(), vel, newColor: newGreen * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.35f);
                        d.alpha = 10;
                    }


                    if (i % 2 == 0 && Main.rand.NextBool())
                    {
                        Vector2 offset2 = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10, 10);
                        Vector2 vel2 = rot.ToRotationVector2().RotatedByRandom(0.45f) * Main.rand.NextFloat(2, 7);

                        Dust d2 = Dust.NewDustPerfect(pos + offset2, ModContent.DustType<GlowPixelCross>(), vel2, newColor: newGreen * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.25f);
                        d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 3, postSlowPower: 0.92f, velToBeginShrink: 1.5f, fadePower: 0.93f, shouldFadeColor: false);

                    }
                }

            }

            if (timer % 1 == 0 && !shouldStopDust)
            {
                Vector2 vel = new Vector2(0f, Main.rand.NextFloat(-5f, -26f));

                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<MediumSmoke>(), vel,
                    newColor: Color.GreenYellow with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.35f) * 1.35f);

                d.rotation = Main.rand.NextFloat(6.28f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(8, 14) + 5, 0.94f, 0.01f, 0.075f); //12 28
            }


            //Lighting
            DelegateMethods.v3_1 = Color.GreenYellow.ToVector3() * 1.25f * Projectile.scale * true_width;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + new Vector2(0f, -210f * Projectile.scale), 10f * true_width, DelegateMethods.CastLight);

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow1 = circle_053.Value;
            Texture2D glow2 = muzzle_flash_12.Value;
            Texture2D glow3 = star_07.Value;
            Texture2D glow4 = circle_053Black.Value;

            Vector2 newScale = new Vector2(1.5f, 1f * true_width) * 0.5f; //sword
            newScale *= 0.55f;
            Vector2 newScale2 = new Vector2(1f, 1.5f * true_width) * 0.5f; //sword
            newScale2 *= 0.55f;
            Vector2 newScale3 = new Vector2(1.5f, 0.35f * true_width) * 0.5f; //sword
            newScale3 *= 0.55f;

            Vector2 origin1 = new Vector2(0f, glow1.Height / 2f);

            //Black Base
            Main.spriteBatch.Draw(glow1, Projectile.Center - Main.screenPosition + new Vector2(-50f, 0f).RotatedBy(Projectile.rotation), null, Color.Black * 0.25f * true_width, Projectile.rotation, origin1, newScale3, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow1, Projectile.Center - Main.screenPosition + new Vector2(-50f, 0f).RotatedBy(Projectile.rotation), null, Color.Black * 0.25f * true_width, Projectile.rotation, origin1, newScale2, SpriteEffects.None, 0f);

            //Bloom
            Color newGreen = new Color(100, 255, 34) * true_width;
            Main.spriteBatch.Draw(glow4, Projectile.Center - Main.screenPosition + new Vector2(-50f, 0f).RotatedBy(Projectile.rotation), null, newGreen with { A = 0 } * 0.25f, Projectile.rotation, origin1, newScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow4, Projectile.Center - Main.screenPosition + new Vector2(-50f, 0f).RotatedBy(Projectile.rotation), null, newGreen with { A = 0 } * 0.25f, Projectile.rotation, origin1, newScale2, SpriteEffects.None, 0f);


            //Use Dusts layer so we can draw on top of black underglow
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("Dusts", () =>
            {
                DrawFire(false);
            });
            //Using a seperate method so I can easily change between pixelating and not pixelating     
            //DrawFire(false);
            return false;
        }

        public void DrawFire(bool quitInstantly = false)
        {
            if (quitInstantly)
                return;

            Texture2D glow = circle_053.Value;
            Texture2D glow2 = muzzle_flash_12.Value;
            Texture2D glow3 = star_07.Value;

            float ySinVal = (float)Math.Sin(Main.timeForVisualEffects * 0.22f) * 0.15f;
            float xSinVal = (float)Math.Sin(Main.timeForVisualEffects * 0.22f) * 0.05f;

            //re-name these 
            Vector2 newScale = new Vector2(1.5f, 1f * true_width) * 0.5f; //sword
            Vector2 newScale2 = new Vector2(0.75f, (1.3f + ySinVal) * true_width) * (0.5f + xSinVal); //spiky
            Vector2 newScale3 = new Vector2(0.25f * true_width, 0.25f); //Hilt

            newScale *= 0.6f;
            newScale2 *= 0.6f;
            newScale3 *= 0.65f;

            Vector2 origin1 = new Vector2(0f, glow.Height / 2f);
            Vector2 origin2 = new Vector2(0f, glow2.Height / 2f);

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaser", AssetRequestMode.ImmediateLoad).Value;

            ShaderParams();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect);
            myEffect.CurrentTechnique.Passes["Aura"].Apply();

            //MainBlade
            Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition + new Vector2(-60f, 0f).RotatedBy(Projectile.rotation), null, Color.White, Projectile.rotation, origin1, newScale, SpriteEffects.None, 0f);

            //Spiky part near guard
            Main.spriteBatch.Draw(glow2, Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(1f, 1f), null, Color.White, Projectile.rotation, origin2, newScale2, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow2, Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(1f, 1f), null, Color.White, Projectile.rotation, origin2, newScale2 * 0.5f, SpriteEffects.FlipVertically, 0f);

            //"Hilt"
            Vector2 off = Projectile.rotation.ToRotationVector2() * 8f;
            Main.spriteBatch.Draw(glow3, Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(1f, 1f) + off, null, Color.White, Projectile.rotation + MathHelper.PiOver2, glow3.Size() / 2, newScale3, SpriteEffects.FlipVertically, 0f);
            Main.spriteBatch.Draw(glow3, Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(1f, 1f) + off, null, Color.White, Projectile.rotation - MathHelper.PiOver2, glow3.Size() / 2, newScale3, SpriteEffects.FlipVertically, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public void ShaderParams()
        {
            myEffect.Parameters["sampleTexture1"].SetValue(CommonTextures.Extra_196_Black.Value);
            myEffect.Parameters["sampleTexture2"].SetValue(CommonTextures.FlamesTextureButBlack.Value);
            myEffect.Parameters["sampleTexture3"].SetValue(CommonTextures.FlameTrail.Value);
            myEffect.Parameters["sampleTexture4"].SetValue(CommonTextures.ThinGlowLine.Value);
            
            Color newGreen = new Color(100, 255, 34);

            Color c1 = newGreen;
            Color c2 = newGreen;
            Color c3 = Color.Green;
            Color c4 = newGreen;

            myEffect.Parameters["Color1"].SetValue(c1.ToVector4());
            myEffect.Parameters["Color2"].SetValue(c2.ToVector4());
            myEffect.Parameters["Color3"].SetValue(c3.ToVector4());
            myEffect.Parameters["Color4"].SetValue(c4.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(1.25f * 1f);
            myEffect.Parameters["Color2Mult"].SetValue(1.25f * 0f);
            myEffect.Parameters["Color3Mult"].SetValue(1.25f * 1.5f); //1.5
            myEffect.Parameters["Color4Mult"].SetValue(1.1f * 1f);
            myEffect.Parameters["totalMult"].SetValue(1f);

            myEffect.Parameters["tex1reps"].SetValue(1f);
            myEffect.Parameters["tex2reps"].SetValue(1f);
            myEffect.Parameters["tex3reps"].SetValue(1f);
            myEffect.Parameters["tex4reps"].SetValue(1f);

            myEffect.Parameters["satPower"].SetValue(1f * true_width);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.03f);
        }
    }
}
