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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class BatScepter : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BatScepter);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            Vector2 pos = position + velocity.SafeNormalize(Vector2.UnitX) * 30;

            for (int i = 0; i < 2 + Main.rand.Next(0, 3); i++) //2 //0,3
            {

                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = pos,
                    MovementVector = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-1.5f, 1.5f)) * Main.rand.NextFloat(0f, 4f)
                };
                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.BlackLightningSmall, particleSettings);

            }

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wyvern_dive_down_1") with { Volume = 0.5f, Pitch = 1f, PitchVariance = .35f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class BatScepterShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Bat);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 10;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

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

            timer++;
            return base.PreAI(projectile);
        }


        public float overallAlpha = 0f;
        public List<float> previousVelrots = new List<float>();
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //147, 112, 219 
            Color purp = new Color(105, 63, 191);

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float size = Easings.easeOutSine(1f * progress) * projectile.scale;
                    //float size = (0.75f + (progress * 0.25f)) * projectile.scale;
                    float size2 = progress * projectile.scale;


                    Color col = Color.Black * progress * overallAlpha;


                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col * 0.2f,
                            previousRotations[i], TexOrigin, size * overallAlpha, SpriteEffects.None);

                    //Inner Line
                    Vector2 vec2Scale = new Vector2(0.5f * size2, 1.5f) * overallAlpha;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.Black * 0.15f * progress * overallAlpha,
                        previousVelrots[i] + MathHelper.PiOver2, TexOrigin, vec2Scale, SpriteEffects.None);

                }

            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.Black * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallAlpha, SpriteEffects.None);

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 3f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.2f * projectile.direction);

                float opacitySquared = projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle, 
                    purp with { A = 0 } * 1.25f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallAlpha, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallAlpha, SpriteEffects.None);
            return false;

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

}
