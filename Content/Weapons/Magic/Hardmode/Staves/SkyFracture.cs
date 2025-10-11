using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class SkyFracture : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SkyFracture) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SkyFractureToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }
    public class SkyFractureShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SkyFracture) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SkyFractureToggle;
        }

        float randomSineOffset = Main.rand.NextFloat(0f, 10f);
        int randomTrailLengthOffset = Main.rand.Next(-1, 2);


        Vector2 spawnPos = Vector2.Zero;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 4f;

            if (timer == 0)
            {
                projectile.alpha = 0;
                projectile.frame = Main.rand.Next(0, 14);

                spawnPos = projectile.Center;
                projectile.ai[0] = projectile.scale;

                //Sounds really cool use for something else
                //SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_hurt_1") with { Volume = 0.8f, Pitch = .9f, PitchVariance = 0.2f, MaxInstances = 1 };
                //SoundEngine.PlaySound(style3, projectile.Center);

                SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/hero_butterfly_blade") with { Volume = 0.2f, Pitch = .8f, PitchVariance = .3f, };
                SoundEngine.PlaySound(style2, projectile.Center);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/star_impact_01") with { Volume = 0.5f, Pitch = .5f, PitchVariance = .3f, }; 
                SoundEngine.PlaySound(style, projectile.Center);

                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * 1f;
                Dust d = Dust.NewDustPerfect(spawnPos, ModContent.DustType<CirclePulse>(), vel, newColor: Color.DodgerBlue);

                d.scale = 0.025f;
                d.customData = new CirclePulseBehavior(0.05f, true, 12, 0.35f, 0.7f);
            }

            int trailCount = 10 + randomTrailLengthOffset;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer % 2 == 0 && timer > 0 && Main.rand.NextBool())
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.3f, 0.4f));
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
            }

            float timeForPopInAnim = 20f;
            float animProgress = Math.Clamp((float)(timer / timeForPopInAnim), 0f, 1f);
            projectile.scale = projectile.ai[0] + (1f - Easings.easeOutCirc(animProgress)) * 0.5f;

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.8f, 0.08f), 0f, 1f);

            Lighting.AddLight(projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.75f * overallAlpha);

            timer++;
            return false;
        }

        public float overallAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);



            //So apparently sky fracture is like the only thing in all of terraria that uses a horizontal spritesheet
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(14, 1, frameX: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //Sword is slightly off-center on spritesheet, so this is to fix that
            Vector2 swordOffset = new Vector2(0f, 2f).RotatedBy(projectile.velocity.ToRotation());

            //Orb
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Color[] cols = { Color.SkyBlue * 0.75f, Color.SkyBlue * 0.525f, Color.DeepSkyBlue * 0.375f };
            float[] scales = { 0.85f, 1.6f, 2.5f };

            float orbRot = projectile.rotation - MathHelper.PiOver4;
            float orbAlpha = 0.35f;
            Vector2 orbScale = new Vector2(0.5f, 0.25f) * 0.75f * overallAlpha;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[2] * sineScale2, SpriteEffects.None);



            float sinVal = (float)Math.Sin(Main.timeForVisualEffects * 0.12f);

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 4f + (sinVal * 1f);

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction) + swordOffset;


                float opacity = projectile.Opacity * (0.85f + sinVal * 0.15f);
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle, 
                    Color.DeepSkyBlue with { A = 0 } * 0.15f * opacity, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallAlpha, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos + swordOffset, sourceRectangle, Color.White * overallAlpha * 0.15f, projectile.rotation, TexOrigin, projectile.scale * overallAlpha, SpriteEffects.None);

            float overglowScale = projectile.scale * overallAlpha + (MathF.Sin((float)Main.timeForVisualEffects * 0.15f) * 0.08f);
            Main.EntitySpriteDraw(vanillaTex, drawPos + swordOffset, sourceRectangle, Color.SkyBlue with { A = 0 } * overallAlpha, projectile.rotation, TexOrigin, overglowScale, SpriteEffects.None);
            
            
            return false;

        }

        Effect myEffect = null;
        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            myEffect ??= ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpikeHalf").Value; //

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * Easings.easeInQuad(progress) * overallAlpha;


            float sineStripWidth = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.15f;
            float StripWidth(float progress) => 8f * sineStripWidth * overallAlpha;

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(0f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.Lerp(Color.DodgerBlue, Color.SkyBlue, 0.25f).ToVector3() * 4f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            
            vertexStrip.DrawTrail();
            
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_10") with { Volume = 0.75f, Pitch = 1f, PitchVariance = 0.25f };
            SoundEngine.PlaySound(style, projectile.Center);


            for (int i = 0; i < Main.rand.Next(4, 9); i++)
            {
                float velMult = Main.rand.NextFloat(1.5f, 4f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.55f, 1f));

                if (dust.scale > 0.9f)
                    dust.velocity *= 0.5f;

                dust.scale *= 1.3f;

                dust.fadeIn = Main.rand.NextFloat(0.25f, 1f);
                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }
       
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepSkyBlue, Scale: 0.12f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
