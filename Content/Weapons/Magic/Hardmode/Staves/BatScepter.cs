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
using Terraria.GameContent.Drawing;
using VFXPlus.Common.Drawing;


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
            int trailCount = 24;

            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = projectile.Center + (i == 0 ? Vector2.Zero : projectile.velocity * 0.5f);

                previousRotations.Add(projectile.rotation);
                previousPositions.Add(pos);
                previousVelrots.Add(projectile.velocity.ToRotation());

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);

                if (previousVelrots.Count > trailCount)
                    previousVelrots.RemoveAt(0);
            }


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
        List<float> previousVelrots = new List<float>();
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

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Line = CommonTextures.SoulSpike.Value;

            //147, 112, 219 
            Color purp = new Color(105, 63, 191);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;
            float orbScale = overallScale * 1.25f;
            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;

            Main.EntitySpriteDraw(orb, drawPos, null, purp with { A = 0 } * 0.1f, projectile.velocity.ToRotation(), orb.Size() / 2f, orbScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(orb, drawPos, null, Color.White with { A = 0 } * 0.1f, projectile.velocity.ToRotation(), orb.Size() / 2f, orbScale * 0.5f, SpriteEffects.None);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float size = Easings.easeOutSine(1f * progress) * projectile.scale;
                //float size = (0.75f + (progress * 0.25f)) * projectile.scale;
                float size2 = progress * projectile.scale;


                Color col = purp * progress * overallAlpha;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col * 0.25f,
                        previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

                //Inner Line
                Vector2 vec2Scale = new Vector2(0.5f * size2, 1.5f) * overallScale;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.Black * 0.15f * progress * overallAlpha,
                    previousVelrots[i] + MathHelper.PiOver2, TexOrigin, vec2Scale, SpriteEffects.None);
            }


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
