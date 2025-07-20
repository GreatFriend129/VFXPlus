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
using VFXPLus.Common;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Misc;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class NightglowOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.FairyQueenMagicItem) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NightglowToggle;
        }

        public override void SetDefaults(Item entity)
        {
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Item use behavior is in Player.cs
            //Shoutouts to pointPoisition being misspelled in that lmao
            #region vanillaBehavior
            Vector2 vector14 = Main.rand.NextVector2Circular(1f, 1f) + Main.rand.NextVector2CircularEdge(3f, 3f);
            if (vector14.Y > 0f)
            {
                vector14.Y *= -1f;
            }

            //float num144 = (float)player.itemAnimation / (float)player.itemAnimationMax * 0.66f + player.miscCounterNormalized;
            float num144 = (float)player.itemAnimation / (float)player.itemAnimationMax * 0.1f + player.miscCounterNormalized;
            
            Vector2 pointPoisition = player.MountedCenter + new Vector2(player.direction * 15, player.gravDir * 3f);
            Point point2 = pointPoisition.ToTileCoordinates();
            Tile tile = Main.tile[point2.X, point2.Y];
            if (tile != null && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !TileID.Sets.Platforms[tile.TileType])
            {
                pointPoisition = player.MountedCenter;
            }
            int a = Projectile.NewProjectile(source, pointPoisition.X, pointPoisition.Y, vector14.X, vector14.Y, type, damage, knockback, player.whoAmI, -1f, num144 % 1f);
            #endregion
            Main.projectile[a].spriteDirection = velocity.X > 0 ? 1 : -1;

            if (player.itemAnimation == item.useAnimation)
            {
                int vfx = Projectile.NewProjectile(source, pointPoisition, Vector2.Zero, ModContent.ProjectileType<NightglowPulseVFX2>(), 0, 0, player.whoAmI);
                Main.projectile[vfx].spriteDirection = velocity.X > 0 ? 1 : -1;
            }

            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);

                Color dustCol = Main.hslToRgb(num144 % 1f, 1f, 0.45f, 0);
                float dustScale = Main.rand.NextFloat(1f, 1.2f);

                float fadePower = Main.rand.NextFloat(0.88f, 0.91f);

                Dust d = Dust.NewDustPerfect(pointPoisition, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCol, Scale: dustScale);
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 0, postSlowPower: 0.91f, velToBeginShrink: 10f, fadePower: fadePower, colorFadePower: 0.95f);
            }

            return false;
        }

    }

    public class NightGlowProjOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FairyQueenMagicItemShot) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NightglowToggle;
        }
        public override bool InstancePerEntity => true;

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            int trailCount = 40;
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

            if (timer % 3 == 0 && Main.rand.NextBool(5))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                dustVel += projectile.velocity * 0.55f;

                Color dustCol = Main.hslToRgb((timer * 0.01f + projectile.ai[1] + 0.1f) % 1f, 1f, 0.45f, 0);
                float dustScale = Main.rand.NextFloat(0.6f, 0.7f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCol, Scale: dustScale);
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, postSlowPower: 0.89f, velToBeginShrink: 10f, fadePower: 0.89f, shouldFadeColor: false);
            }

            //The nightglow proj spawns a burst of dust when timeLeft is 238, so this is a little hack to prevent that from happening while not affecting anything
            if (projectile.timeLeft == 238)
                projectile.timeLeft--;
            if (!hasDoneTimeLeftBoost && projectile.timeLeft == 230)
            {
                projectile.timeLeft++;
                hasDoneTimeLeftBoost = true;
            }

            timer++;
            return base.PreAI(projectile);
        }

        bool hasDoneTimeLeftBoost = false;

        float overallScale = 1f;
        float overallAlpha = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawPixelTrail(projectile);
            });
            return false;
        }

        public void DrawPixelTrail(Projectile projectile)
        {
            Texture2D AfterImage = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            float xScale = 0.15f + (0.45f * Utils.GetLerpValue(2f, 5f, projectile.velocity.Length(), true));

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color color = Main.hslToRgb((timer * 0.01f + projectile.ai[1] + (progress * 0.05f)) % 1f, 1f, 0.4f, 0);

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Vector2 vec2Scale = new Vector2(xScale, 0.8f * progress) * projectile.scale * 0.75f;
                Vector2 vec2Scale2 = new Vector2(xScale, 0.4f * progress) * projectile.scale * 0.75f;

                Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, color with { A = 0 },
                       previousRotations[i], AfterImage.Size() / 2f, vec2Scale, SpriteEffects.None);

                Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.White with { A = 0 } * 1f * progress,
                       previousRotations[i], AfterImage.Size() / 2f, vec2Scale2, SpriteEffects.None);

            }

            //Star
            Texture2D spike = CommonTextures.CrispStarPMA.Value; //Spike and Fire Spike look cool

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);

            float rainbowAlpha = 1f - Utils.GetLerpValue(2f, 5f, projectile.velocity.Length(), true);

            if (rainbowAlpha == 0f)
                return;

            float starRot = (float)Main.timeForVisualEffects * (projectile.velocity.X > 0 ? -1f : 1f);

            Color rainbow = Main.hslToRgb((timer * 0.01f + projectile.ai[1]) % 1f, 1f, 0.65f, 0) * overallAlpha * rainbowAlpha;

            float scale1 = 1f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;

            float scale = 0.25f * overallScale * projectile.scale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(spike, drawPos, null, Color.White with { A = 0 } * rainbowAlpha, starRot * 0.45f, spike.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, drawPos, null, rainbow with { A = 0 } * 1f, starRot * 0.18f, spike.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, drawPos, null, rainbow with { A = 0 } * 0.75f, starRot * -0.09f, spike.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);

            Main.EntitySpriteDraw(spike, drawPos, null, Color.White with { A = 0 } * rainbowAlpha, MathHelper.PiOver2 + starRot * 0.45f, spike.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, drawPos, null, rainbow with { A = 0 } * 1f, MathHelper.PiOver2 + starRot * 0.18f, spike.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, drawPos, null, rainbow with { A = 0 } * 0.75f, MathHelper.PiOver2 + starRot * -0.09f, spike.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int i1 = 0;
            foreach (Vector2 pos in previousPositions)
            {
                i1++;
                if (i1 % 5 == 0 && Main.rand.NextBool(4) )
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                    dustVel += projectile.velocity * 0.35f;

                    Color dustCol = Main.hslToRgb((timer * 0.01f + projectile.ai[1]) % 1f, 1f, 0.45f, 0);
                    float dustScale = Main.rand.NextFloat(0.8f, 0.9f);

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<PixelGlowOrb>(), dustVel, newColor: dustCol, Scale: dustScale);
                    d.customData = DustBehaviorUtil.AssignBehavior_PGOBase(timeBeforeSlow: 0, postSlowPower: 0.89f, velToBeginShrink: 10f, fadePower: 0.89f, colorFadePower: 1f);
                }
            }

            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);

                Color dustCol = Main.hslToRgb((timer * 0.01f + projectile.ai[1]) % 1f, 1f, 0.45f, 0);
                float dustScale = Main.rand.NextFloat(1f, 1.2f);

                float fadePower = Main.rand.NextFloat(0.88f, 0.91f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), dustVel, newColor: dustCol, Scale: dustScale);
                d.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.1f, timeBeforeSlow: 0, postSlowPower: 0.92f, velToBeginShrink: 10f, fadePower: fadePower, colorFadePower: 1f);
            }

            //SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.25f, Pitch = 0.15f, PitchVariance = 0.2f, MaxInstances = -1}, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/star_impact_01") with { Volume = 0.03f, Pitch = 0.5f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            return false;
        }

    }
    public class NightglowPulseVFX2 : ModProjectile
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

        Vector2 relativePos = Vector2.Zero;
        public override void AI()
        {
            if (timer == 0)
                relativePos = (Projectile.Center - Main.player[Projectile.owner].Center);

            Projectile.Center = Main.player[Projectile.owner].Center + relativePos;

            float fadeInTime = Math.Clamp((float)(timer + 8) / 20f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 3f);

            if (fadeInTime == 1f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.15f), 0f, 1f);

            if (overallAlpha == 0f)
                Projectile.active = false;

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

            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value; //Spike and Fire Spike look cool

            //Texture2D orb = CommonTextures.flare_12.Value;
            Vector2 originPoint = Projectile.Center - Main.screenPosition;

            int dir = Projectile.spriteDirection;
            float rot = (float)Main.timeForVisualEffects * dir * 1.35f;

            Color rainbow = Main.hslToRgb(((float)Main.timeForVisualEffects * 0.02f) % 1f, 1f, 0.65f, 0) * overallAlpha * 1f;

            float scale1 = 0.85f;
            float scale2 = 1.35f;
            float scale3 = 2f;

            float scale = 0.4f * overallScale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            //0.75
            //0.525
            //0.375


            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 1f, rot * -0.45f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.75f, rot * -0.18f, orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.525f, rot * 0.09f, orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);

            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 1f, MathHelper.PiOver2 + rot * -0.45f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.75f, MathHelper.PiOver2 + rot * -0.18f, orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, rainbow with { A = 0 } * 0.525f, MathHelper.PiOver2 +rot * 0.09f, orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);
        }
    }
}
