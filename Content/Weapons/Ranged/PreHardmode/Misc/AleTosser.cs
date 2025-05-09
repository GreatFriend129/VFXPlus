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
            return lateInstantiation && (entity.type == ProjectileID.MolotovCocktail);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 16;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);

            overallScale = 0.34f + MathHelper.Lerp(0f, 0.66f, Easings.easeInOutBack(animProgress, 1f, 3f));

            timer++;
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

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.25f,
                        previousRotations[i], TexOrigin, size, SE);

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 1.5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.White with { A = 0 }, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;
        }

    }
}
