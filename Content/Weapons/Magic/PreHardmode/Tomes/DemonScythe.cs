using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using System.Linq;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using Terraria.GameContent;
using System.Threading;
using VFXPlus.Common.Drawing;
using Terraria.Graphics;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Tomes
{
    
    public class DemonScythe : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.DemonScythe) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.DemonScytheToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle stylees = new SoundStyle("Terraria/Sounds/Item_117") with { Pitch = .45f, PitchVariance = .25f, Volume = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(stylees, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Custom/dd2_book_staff_cast_0") with { Volume = 0.3f, Pitch = -0.25f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_8") with { Volume = 0.85f, PitchVariance = 0.1f, Pitch = .05f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class DemonScytheShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.DemonScythe) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.DemonScytheToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Color newPurple = new Color(61, 2, 92) * 1f;
            Color darkPurple = new Color(42, 2, 82) * 1f;

            int trailCount = 30;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 1 == 0 && Main.rand.NextBool())
            {
                Vector2 offset2 = projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-20, 20);
                Vector2 vel2 = projectile.rotation.ToRotationVector2().RotatedByRandom(0.15f) * Main.rand.NextFloat(2, 7);

                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center + offset2, ModContent.DustType<GlowPixelCross>(), vel2, newColor: new Color(42, 2, 82) * 2f, Scale: Main.rand.NextFloat(0.25f, 1.5f) * 0.25f);
                d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 3, postSlowPower: 0.9f, velToBeginShrink: 1.75f, fadePower: 0.93f, shouldFadeColor: false);

                d2.velocity += projectile.velocity * 0.25f;
                d2.velocity *= 0.25f;
            }

            float progress = Math.Clamp(timer / 50f, 0f, 1f);
            fadeInAlpha = MathHelper.Lerp(0f, 1f, progress);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f); // + 5

            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 2f));

            timer++;


            #region vanillaAI
            projectile.rotation += (float)projectile.direction * 0.8f;
            projectile.ai[0] += 1f;
            if (!(projectile.ai[0] < 30f))
            {
                if (projectile.ai[0] < 100f)
                {
                    projectile.velocity *= 1.06f;
                }
                else
                {
                    projectile.ai[0] = 200f;
                }
            }
            for (int num262 = 0; num262 < 1; num262++) //Vanilla is 2
            {
                if (timer % 2 == 0) //every frame in vanilla
                {
                    int num263 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Shadowflame, 0f, 0f, 100);
                    Main.dust[num263].noGravity = true;
                    Main.dust[num263].velocity += projectile.velocity * 0.25f;
                } 


            }
            #endregion

            return false;

        }

        float fadeInAlpha = 0f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawOrb(projectile, true);
                DrawTrail();
            });

            Texture2D pixelSwirl = CommonTextures.PixelSwirl.Value;
            Texture2D star = CommonTextures.RainbowRod.Value;


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            Color newPurple = new Color(61, 2, 92) * 1f;
            Color darkPurple = new Color(42, 2, 82) * 1f;

            //Glow
            //Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/Projectile_540").Value;
            //Main.EntitySpriteDraw(Orb, drawPos, null, newPurple * 0.15f, projectile.rotation, Orb.Size() / 2, projectile.scale * 0.75f * overallScale, 0);
            //Main.EntitySpriteDraw(Orb, drawPos, null, darkPurple * 0.15f, projectile.rotation, Orb.Size() / 2, projectile.scale * 1.25f * overallScale, 0);

            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/SoftGlow64").Value;
            //Main.EntitySpriteDraw(Orb, drawPos, null, darkPurple with { A = 0 } * 0.25f, projectile.rotation, Orb.Size() / 2, projectile.scale * 1.15f * overallScale, 0);


            //Star
            Vector2 starPos = drawPos;
            float starScale = MathHelper.Lerp(1.5f, 0f, Easings.easeOutCirc(fadeInAlpha)) * projectile.scale * 0.75f;

            Main.EntitySpriteDraw(star, starPos, null, newPurple with { A = 0 } * 2f * (1f - fadeInAlpha), projectile.rotation, star.Size() / 2f, starScale * 1.4f, SpriteEffects.None);
            Main.EntitySpriteDraw(star, starPos, null, Color.White with { A = 0 } * 0.85f * (1f - fadeInAlpha), projectile.rotation, star.Size() / 2f, starScale * 0.65f, SpriteEffects.None);

            //Border
            for (int num163 = 0; num163 < 4; num163++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)num163) * 3f, null, newPurple with { A = 0 } * 2f, projectile.rotation, TexOrigin,
                    projectile.scale * overallScale, 0f);
            }

            float overGlowProg = Math.Clamp((float)(timer / 30f), 0f, 1f);

            //Main tex
            float easedOverGlowProg = Easings.easeOutQuad(overGlowProg);

            float minAlpha = Math.Clamp(255f * easedOverGlowProg, 50f, 255f);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor with { A = (byte)(minAlpha) } * 1f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 } * (1f - overGlowProg) * 0.5f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * 1f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);


            return false;
        }

        Effect myEffect = null;
        public void DrawTrail()
        {
            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);

            Color purple3 = new Color(121, 7, 179);

            Color purple4 = Color.Lerp(purple, darkPurple, 0.75f);

            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/SmokeTrail").Value; //|spark_06 | Extra_196_Black
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/EvenThinnerGlowLine").Value; //Trail7 30

            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress * progress) * 0.25f * (progress > 0.99f ? 0f : 1f);
            float StripWidth(float progress) => 60f * Easings.easeInSine(progress) * fadeInAlpha;

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.03f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);

            myEffect.Parameters["glowThreshold"].SetValue(0.4f);
            myEffect.Parameters["glowIntensity"].SetValue(2.5f);
            myEffect.Parameters["reps"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(purple4.ToVector3() * 3f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }

        public void DrawOrb(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;


            Color newPurple = new Color(61, 2, 92) * 1f;
            Color darkPurple = new Color(42, 2, 82) * 1f;
            Color purple3 = new Color(135, 15, 209);

            //Glow
            Color[] cols = { Color.White, purple3, purple3 * 1.25f };
            float[] scales = { 0.85f, 1.35f, 2.5f };

            float orbRot = projectile.rotation - MathHelper.PiOver4;
            float orbAlpha = 0.5f;
            float orbScale = 0.35f * overallScale * projectile.scale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 255 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 255 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 255 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[2] * sineScale2, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_43") with { Volume = .25f, Pitch = -.32f, PitchVariance = .15f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_60") with { Volume = .15f, Pitch = .15f, PitchVariance = .12f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style2, projectile.Center);

            SoundStyle styleNormal = new SoundStyle("Terraria/Sounds/Item_10") with { Volume = 0.8f, Pitch = .15f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(styleNormal, projectile.Center);


            for (int i = 0; i < 30 + Main.rand.Next(1, 7); i++)
            {
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Shadowflame, Alpha: 100, Scale: projectile.scale);

                Main.dust[a].velocity += projectile.velocity * 0.25f;
                Main.dust[a].noGravity = true;
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            for (int i = 0; i < 7 + Main.rand.Next(1, 7); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Vector2 spawnOffset = Main.rand.NextVector2Circular(15f,15f);

                Dust p = Dust.NewDustPerfect(projectile.Center + spawnOffset, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.75f, 1.05f),
                    newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.15f, 0.45f) * projectile.scale);
                p.velocity += oldVelocity * Main.rand.NextFloat(0.25f, 0.51f);
            }

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
