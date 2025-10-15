using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    
    public class BatScepter : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BatScepter) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BatScepterToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 pos = position + velocity.SafeNormalize(Vector2.UnitX) * 30;

            for (int i = 0; i < 2 + Main.rand.Next(0, 3); i++)
            {
                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = pos,
                    MovementVector = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-1.5f, 1.5f)) * Main.rand.NextFloat(0f, 4f)
                };
                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.BlackLightningSmall, particleSettings);
            }

            return true;
        }

    }
    public class BatScepterShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Bat) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BatScepterToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 13;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 3 == 0 && Main.rand.NextBool(6) && timer != 0)
            {
                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = projectile.Center,
                    MovementVector = Main.rand.NextVector2Circular(0.5f, 0.5f) + projectile.velocity * 0.3f
                };
                
                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.BlackLightningSmall, particleSettings);
            }

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.4f, 0.1f), 0f, 1f);

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 2) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.25f)) * 1f;

            timer++;
            return base.PreAI(projectile);
        }


        float overallScale = 0f;
        float overallAlpha = 0f;
        List<float> previousRotations = new List<float>();
        List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //147, 112, 219 
            Color purp = new Color(105, 63, 191);

            Texture2D orb = CommonTextures.PartiGlowPMA.Value;
            float orbScale = overallScale * 1.15f;
            Main.EntitySpriteDraw(orb, drawPos, null, Color.Black * 0.08f, projectile.velocity.ToRotation(), orb.Size() / 2f, orbScale, SpriteEffects.None);


            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.Black * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 3f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.2f * projectile.direction);

                float opacitySquared = 1f;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle, 
                    purp with { A = 0 } * 1.75f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);
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
            float StripWidth(float progress) => 16f * sineStripWidth * overallScale * Easings.easeOutQuad(progress);

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(0f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.Lerp(Color.Black, Color.MediumPurple, 0.2f).ToVector3() * 1.5f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 2 + Main.rand.Next(0,2); i++)
            {
                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = projectile.Center,
                    MovementVector = Main.rand.NextVector2Circular(2f, 2f) + projectile.velocity * 0.25f
                };

                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.BlackLightningSmall, particleSettings);
            }

            return base.PreKill(projectile, timeLeft);
        }
    }

}
