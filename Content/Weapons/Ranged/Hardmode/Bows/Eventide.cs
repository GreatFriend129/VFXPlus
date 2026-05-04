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
using static tModPorter.ProgressUpdate;
using VFXPlus.Common.Drawing;
using Terraria.Graphics;
using VFXPlus.Content.Weapons.Ranged.PreHardmode.Bows;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class EventideOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.FairyQueenRangedItem);
        }

        public override void SetDefaults(Item entity)
        {
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);


    }
    public class EventideShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FairyQueenRangedItemShot);
        }
        public override bool InstancePerEntity => true;


        int trailRandomLengthOffset = Main.rand.Next(-3, 4);
        float rainbowRandomOffsetTime = Main.rand.NextFloat();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                Vector2 spawnPos = projectile.Center - projectile.velocity.SafeNormalize(Vector2.Zero) * 25f;
                Vector2 spawnVel = projectile.velocity.SafeNormalize(Vector2.UnitX) * 1f;

                Projectile.NewProjectile(null, spawnPos, spawnVel, ModContent.ProjectileType<EventideStarVFX>(), 0, 0, projectile.owner);
            }

            int trailCount = 22 + trailRandomLengthOffset; //22

            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            float fadeInTime = Math.Clamp((timer + 11f) / 18f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            overallAlpha = 1f;

            if (timer % 2 == 0 && Main.rand.NextBool(3) && timer > 5)
            {
                Color rainbow = Main.hslToRgb(Main.rand.NextFloat(0f, 1f), 1f, 0.65f, 0) * 1f;

                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowPixelCross>(), newColor: rainbow, Scale: Main.rand.NextFloat(0.4f, 0.55f));
                Main.dust[d].velocity -= projectile.velocity * 0.35f;
                Main.dust[d].velocity *= 0.45f;

                Main.dust[d].customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.25f, velToBeginShrink: 3.5f, fadePower: 0.88f, shouldFadeColor: false);
            }

            timer++;

            #region vanillaAI
            projectile.ai[0] += 1f;
            projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Utils.GetLerpValue(0f, 10f, projectile.ai[0], clamped: true));
            projectile.rotation = projectile.velocity.ToRotation();
            #endregion

            return false;

            return base.PreAI(projectile);
        }


        float overallScale = 1f;
        float overallAlpha = 0f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;


            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.8f, vanillaTex.Height / 2f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            float rainbowTime = (float)Main.timeForVisualEffects * 0.03f;
            Color rainbow = Main.hslToRgb(rainbowTime % 1f, 1f, 0.65f, 0);


            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawRainbowTrail(projectile, false);
            });
            DrawRainbowTrail(projectile, true);


            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;
            Vector2 glowPos = drawPos + (projectile.rotation.ToRotationVector2() * -20f) + new Vector2(0f, 0f);

            Vector2 glowScale = new Vector2(1.5f, 0.45f) * projectile.scale * overallScale;
            Main.EntitySpriteDraw(Glow, glowPos, null, rainbow with { A = 0 } * 0.3f, projectile.rotation, Glow.Size() / 2f, glowScale, SE);
            Main.EntitySpriteDraw(Glow, glowPos, null, rainbow with { A = 0 } * 0.06f, projectile.rotation, Glow.Size() / 2f, glowScale * 1.2f, SE);


            //Border
            for (int i = 0; i < 4; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(5f, 5f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, null, rainbow with { A = 0 } * 0.8f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.White with { A = 0 }, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;

        }

        Effect myEffect = null;
        public void DrawRainbowTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            //Create arrays
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            float widthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.07f) * 0.25f;

            Color StripColor(float progress) => Color.White * overallAlpha * Utils.GetLerpValue(0f, 0.2f, progress, true);
            float StripWidth(float progress) => 30f * widthMult * overallScale * Easings.easeOutQuad(progress);

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            #region ShaderParams
            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value);
            myEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/RainbowGrad1").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["satPower"].SetValue(0.4f); //higher power -> less affected by background  |95 | 3f looks very goozma

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/ThinGlowLine").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_06").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);

            myEffect.Parameters["grad1Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad2Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad3Speed"].SetValue(3.1f / 3f);

            myEffect.Parameters["tex1Mult"].SetValue(1.25f);
            myEffect.Parameters["tex2Mult"].SetValue(1.5f);
            myEffect.Parameters["tex3Mult"].SetValue(1.15f);
            myEffect.Parameters["totalMult"].SetValue(1f);

            //We want the number of repititions to be relative to the number of points
            float repValue = 0.05f * previousPositions.Count;
            myEffect.Parameters["gradientReps"].SetValue(0.25f * repValue); //1f
            myEffect.Parameters["tex1reps"].SetValue(1f * repValue); //2.5
            myEffect.Parameters["tex2reps"].SetValue(0.3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(1f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f); //-0.05
            #endregion

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int i1 = 0;
            foreach (Vector2 pos in previousPositions)
            {
                i1++;
                if (i1 % 3 == 0 && Main.rand.NextBool(3) && i1 < previousPositions.Count * 0.75f)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                    dustVel -= projectile.velocity * 0.2f;

                    Color dustCol = Main.hslToRgb(Main.rand.NextFloat(), 0.75f, 0.55f, 0);
                    float dustScale = Main.rand.NextFloat(0.6f, 0.75f);

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCol, Scale: dustScale);
                    d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, postSlowPower: 0.89f, velToBeginShrink: 10f, fadePower: 0.87f, colorFadePower: 1f);
                }
            }

            for (int i = 0; i < Main.rand.Next(2, 7); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);

                Color dustCol = Main.hslToRgb(Main.rand.NextFloat(), 0.75f, 0.55f, 0);
                float dustScale = Main.rand.NextFloat(0.8f, 1f);

                float fadePower = Main.rand.NextFloat(0.88f, 0.91f);

                Dust d = Dust.NewDustPerfect(projectile.oldPosition + projectile.velocity, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCol, Scale: dustScale);
                //d.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.1f, timeBeforeSlow: 0, postSlowPower: 0.92f, velToBeginShrink: 10f, fadePower: fadePower, colorFadePower: 1f);
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.1f, timeBeforeSlow: 0, postSlowPower: 0.92f, velToBeginShrink: 10f, fadePower: fadePower, colorFadePower: 1f);

                d.fadeIn = Main.rand.NextFloat(0.5f, 1f);
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.White, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.1f, DrawWhiteCore: false, 1f, 1f);

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 0.2f, PitchVariance = 0.15f, MaxInstances = -1}, projectile.Center);

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    public class EventideStarVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        //Safety Checks
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2400;
        }

        int timer = 0;

        public override void AI()
        {
            float fadeInTime = Math.Clamp((float)(timer + 8) / 20f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 3f);

            if (fadeInTime == 1f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.15f), 0f, 1f);

            if (overallAlpha == 0f)
                Projectile.active = false;

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;

            timer++;
        }

        float overallScale = 1f;
        float overallAlpha = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawPortal(false);
            });
            DrawPortal(true);

            return false;
        }

        public void DrawPortal(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value; //Spike and Fire Spike look cool
            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value; //Spike and Fire Spike look cool

            Vector2 originPoint = Projectile.Center - Main.screenPosition;

            int dir = Projectile.spriteDirection;
            //float rot = (float)Main.timeForVisualEffects * dir * 1.35f;

            Color rainbow = Main.hslToRgb(((float)Main.timeForVisualEffects * 0.03f) % 1f, 1f, 0.65f, 0) * overallAlpha * 1f;


            float rot = (float)Main.timeForVisualEffects * 0.15f * Projectile.spriteDirection;

            float starScale = (1f - overallScale) * 1.35f;

            Main.spriteBatch.Draw(Flare, originPoint, null, rainbow with { A = 0 }, rot * -1, Flare.Size() / 2, starScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flare, originPoint, null, Color.White with { A = 0 }, rot, Flare.Size() / 2, starScale * 0.5f, SpriteEffects.None, 0f);
            return;

            float scale1 = 0.85f;
            float scale2 = 1.35f;
            float scale3 = 1.7f;

            float scale = 0.3f * overallScale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;


            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 1f, rot * -0.45f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.75f, rot * -0.18f, orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.525f, rot * 0.09f, orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);

            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 1f, MathHelper.PiOver2 + rot * -0.45f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.75f, MathHelper.PiOver2 + rot * -0.18f, orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.525f, MathHelper.PiOver2 + rot * 0.09f, orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);
        }
    }

}
