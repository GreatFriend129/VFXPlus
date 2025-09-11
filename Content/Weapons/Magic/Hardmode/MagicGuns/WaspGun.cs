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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class WaspGunOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.WaspGun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.WaspGunToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Dust
            for (int i = 0; i < 8 + Main.rand.Next(0, 4); i++)
            {
                if (Main.rand.NextBool(2))
                {
                    Color col = Color.Orange;

                    Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 38, ModContent.DustType<GlowPixelAlts>(),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.Next(2, 6),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                    dp.noGravity = true;
                }
                else
                {
                    Color col = Color.Black;

                    Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 38, DustID.Bee,
                        velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.35f, 0.25f)) * Main.rand.Next(2, 6),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.25f);

                    dp.noGravity = true;
                }

            }


            //SoundEngine.PlaySound(SoundID.Item11 with { Pitch = -0.15f }, player.Center);

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_97") with { Volume = 0.2f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }

    public class WaspGunBeeOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.Wasp && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.WaspGunToggle;
        }

        int timer = 0;
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI(Projectile projectile)
        {
            int trailCount = 9;
            previousPositions.Add(projectile.Center);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.1f), 0f, 1f);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f);

            overallScale = 0.5f + MathHelper.Lerp(0f, 0.5f, Easings.easeInOutBack(animProgress, 0f, 1f));



            timer++;
            base.AI(projectile);
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float rot = projectile.velocity.ToRotation();
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/GlowCircleFlare").Value;
            Color orbCol1 = Color.DarkGoldenrod with { A = 0 };

            float scale1 = 1.15f * overallScale;
            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol1 with { A = 0 } * 0.6f, rot, Glow.Size() / 2f, projectile.scale * scale1 * 0.65f, SpriteEffects.None);


            //Bee
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            float drawRot = projectile.rotation;
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                float size = (projectile.scale * overallScale) - (0.33f - (progress * 0.33f));

                Color col = Color.Gold with { A = 0 } * progress;


                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col * 0.25f,
                        projectile.rotation, TexOrigin, size, SpriteEffects.None);

            }


            //Border
            for (int i = 0; i < 6; i++)
            {
                Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle, Color.White with { A = 0 } * overallAlpha, drawRot, TexOrigin, projectile.scale * overallScale, SE, 0f); //0.3
            }

            Main.spriteBatch.Draw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, drawRot, TexOrigin, projectile.scale * overallScale, SE, 0f); //0.3

            return false;

        }

    }

}
