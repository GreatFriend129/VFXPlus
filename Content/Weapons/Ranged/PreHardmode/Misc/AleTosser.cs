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
using rail;
using VFXPlus.Content.Projectiles;
using VFXPLus.Common;
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class AleTosserItemOverride : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.AleThrowingGlove);
        }

        public override void SetDefaults(Item entity)
        {
            entity.useStyle = ItemUseStyleID.Swing;
            entity.noUseGraphic = true;

            entity.UseSound = SoundID.Item1 with { Volume = 0f, MaxInstances = -1 };

            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow with { Volume = 0.8f, Pitch = 0.35f }, player.Center);

            SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Volume = 0.2f, Pitch = 0.2f }, player.Center);


            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_106") with { Volume = .4f, Pitch = -.15f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }

    public class AleTosserProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Ale);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 13;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);

            overallScale = 0.34f + MathHelper.Lerp(0f, 0.66f, Easings.easeInOutBack(animProgress, 1f, 2f));

            timer++;

            #region vanillaAI
            projectile.rotation += 0.25f * (float)projectile.direction;
            bool flag25 = projectile.type == 399;
            bool flag26 = projectile.type == 669;
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 3f)
            {
                projectile.alpha -= 40;
                if (projectile.alpha < 0)
                {
                    projectile.alpha = 0;
                }
            }
            if (projectile.ai[0] >= 15f)
            {
                projectile.velocity.Y += 0.2f;
                if (projectile.velocity.Y > 16f)
                {
                    projectile.velocity.Y = 16f;
                }
                projectile.velocity.X *= 0.99f;
            }
            if (projectile.alpha == 0)
            {
                if (flag25)
                {
                    Vector2 spinningpoint10 = new Vector2(4f, -8f);
                    float num622 = projectile.rotation;
                    if (projectile.direction == -1)
                    {
                        spinningpoint10.X = -4f;
                    }
                    spinningpoint10 = spinningpoint10.RotatedBy(num622);
                    for (int num623 = 0; num623 < 1; num623++)
                    {
                        int num624 = Dust.NewDust(projectile.Center + spinningpoint10 - Vector2.One * 5f, 4, 4, 6);
                        Main.dust[num624].scale = 1.5f;
                        Main.dust[num624].noGravity = true;
                        Main.dust[num624].velocity = Main.dust[num624].velocity * 0.25f + Vector2.Normalize(spinningpoint10) * 1f;
                        Main.dust[num624].velocity = Main.dust[num624].velocity.RotatedBy(-(float)Math.PI / 2f * (float)projectile.direction);
                    }
                }
                if (flag26 && timer % 2 == 0)
                {
                    for (int num625 = 0; num625 < 1; num625++)
                    {
                        Vector2 spinningpoint11 = new Vector2(MathHelper.Lerp(-8f, 8f, Main.rand.NextFloat()), -4f);
                        float num626 = projectile.rotation;
                        spinningpoint11 = spinningpoint11.RotatedBy(num626);
                        int num627 = Dust.NewDust(projectile.Center + spinningpoint11 - Vector2.One * 5f, 4, 4, 4);
                        Main.dust[num627].scale = 0.8f - Main.rand.NextFloat() * 0.2f;
                        Main.dust[num627].velocity = Main.dust[num627].velocity * 0.25f + Vector2.Normalize(spinningpoint11) * 1f;
                        Main.dust[num627].velocity = Main.dust[num627].velocity.RotatedBy(-(float)Math.PI / 2f * (float)projectile.direction);
                        Main.dust[num627].color = Utils.SelectRandom<Color>(Main.rand, new Color(255, 255, 255, 110), new Color(245, 200, 30, 110));
                    }
                }
            }
            projectile.spriteDirection = projectile.direction;
            if (projectile.timeLeft <= 3)
            {
                projectile.tileCollide = false;
                projectile.alpha = 255;
                projectile.position.X += projectile.width / 2;
                projectile.position.Y += projectile.height / 2;
                projectile.width = 80;
                projectile.height = 80;
                projectile.position.X -= projectile.width / 2;
                projectile.position.Y -= projectile.height / 2;
                projectile.knockBack = 8f;
            }
            if (projectile.wet && projectile.timeLeft > 3)
            {
                projectile.timeLeft = 3;
            }
            #endregion

            return false;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;
                float size = projectile.scale * overallScale;

                Color col = Color.White * progress * projectile.Opacity;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.15f,
                        previousRotations[i], TexOrigin, size, SE);

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 1.5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.White with { A = 0 } * 0.5f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Shatter, projectile.position);
            Vector2 vector17 = new Vector2(20f, 20f);
            for (int num52 = 0; num52 < 5; num52++)
            {
                Dust dust283 = Dust.NewDustDirect(projectile.Center - vector17 / 2f, (int)vector17.X, (int)vector17.Y, 4, 0f, 0f, 100, new Color(255, 255, 255, 110), 1.1f * 0.75f);
                Dust dust117 = dust283;
                Dust dust334 = dust117;
                dust334.velocity *= 1.4f;
            }
            for (int num54 = 0; num54 < 20; num54++)
            {
                Dust dust285 = Dust.NewDustDirect(projectile.Center - vector17 / 2f, (int)vector17.X, (int)vector17.Y, 4, 0f, 0f, 50, new Color(245, 200, 30, 155), 1.2f * 0.75f);
                dust285.noGravity = true;
                Dust dust114 = dust285;
                Dust dust334 = dust114;
                dust334.velocity *= 3f;
                dust285 = Dust.NewDustDirect(projectile.Center - vector17 / 2f, (int)vector17.X, (int)vector17.Y, 4, 0f, 0f, 50, new Color(245, 200, 30, 155), 0.8f * 0.75f);
                dust114 = dust285;
                dust334 = dust114;
                dust334.velocity *= 1.5f;
            }

            return false;
        }

    }
}
