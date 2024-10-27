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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class SpiritFlame : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SpiritFlame);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {



            return true;
        }

    }
    public class SpiritFlameShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SpiritFlame);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            float adjustedRot = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            projectile.rotation = projectile.velocity.Length() > 0 ? adjustedRot : 0f;
            
            int trailCount = 9;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 5 == 0 && Main.rand.NextBool())
            {

                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + projectile.velocity * 0.25f;

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: Color.Purple * 3f, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity += projectile.velocity * 0.2f;

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(shouldFadeColor: false);

            }

            timer++;

            return base.PreAI(projectile);
        }

        float inFadePower = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Utils.DrawBorderString(Main.spriteBatch, "" + projectile.aiStyle, projectile.Center - Main.screenPosition + new Vector2(0f, -50f), Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "" + projectile.type, projectile.Center - Main.screenPosition + new Vector2(0f, -75f), Color.Black);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = (0.75f + (progress * 0.25f)) * projectile.scale;

                    Color col = Color.Purple * progress * projectile.Opacity;

                    float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f, //0.5f
                            previousRotations[i], TexOrigin, size2, SpriteEffects.None);

                }

            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.Purple with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);
            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            /*
            #region vanillaKillStuff
            if (projectile.ai[0] >= 0f)
            {
                projectile.position = projectile.Center;
                projectile.width = (projectile.height = 40);
                projectile.Center = projectile.position;
                projectile.Damage();

                SoundEngine.PlaySound(SoundID.Item14, )

                Main.PlaySound(SoundID.Item14, base.position);
                for (int num130 = 0; num130 < 2; num130++)
                {
                    int num131 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                    Main.dust[num131].position = base.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * base.width / 2f;
                }
                for (int num132 = 0; num132 < 10; num132++)
                {
                    int num133 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 27, 0f, 0f, 0, default(Color), 2.5f);
                    Main.dust[num133].position = base.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * base.width / 2f;
                    Main.dust[num133].noGravity = true;
                    Dust dust52 = Main.dust[num133];
                    Dust dust2 = dust52;
                    dust2.velocity *= 2f;
                }
                for (int num134 = 0; num134 < 5; num134++)
                {
                    int num135 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 31, 0f, 0f, 0, default(Color), 1.5f);
                    Main.dust[num135].position = base.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(base.velocity.ToRotation()) * base.width / 2f;
                    Main.dust[num135].noGravity = true;
                    Dust dust53 = Main.dust[num135];
                    Dust dust2 = dust53;
                    dust2.velocity *= 2f;
                }
            }
            
            #endregion
            */
            return false;

            for (int i = 0; i < 9 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.Purple * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);
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
