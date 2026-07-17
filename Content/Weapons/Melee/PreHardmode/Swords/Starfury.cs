using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Mono.Cecil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Bows;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Swords
{
    
    //Not currently enabled
    public class Starfury : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Starfury);
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults(Item entity)
        {
            //entity.shootsEveryUse = true;
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity);
        }

        public override void HoldItem(Item item, Player player)
        {
            base.HoldItem(item, player);
        }

        public override void UseAnimation(Item item, Player player)
        {
            //Starfury shooting is at 45120 in Player.cs | maxmana ding is at 39040 |
            //39718 for flag setting stuff that doesn't shoot every swing (beam sword, ice blade)

            float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
            int trail = Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<BaseSwordProj>(), 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);

            Vector3[] gradCols = {
                    Color.Black.ToVector3(),
                    Color.DeepPink.ToVector3(),
                    Color.HotPink.ToVector3(),
                    Color.Pink.ToVector3(),
                };


            SwordProjInfo info = new SwordProjInfo(item.type, gradCols, 8f, 0f, 50f, 5, 3f, 1f, 1f);
            info.flowSpeed = 0f;
            (Main.projectile[trail].ModProjectile as BaseSwordProj).info = info;

            base.UseAnimation(item, player);
        }

        public override bool? UseItem(Item item, Player player)
        {
            //if (player.ItemAnimationJustStarted)
            //    Main.NewText("JustStarted |" + Main.timeForVisualEffects);

            //if (player.ItemAnimationJustStarted)
            //    Main.NewText("ItemTimeIsZero |" + Main.timeForVisualEffects + " | " + player.itemAnimation);
            //else
            //    Main.NewText("NotZero");

            if (player.itemAnimation == item.useAnimation && false)
            {

                float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
                int trail = Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<BaseSwordProj>(), 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);

                Vector3[] gradCols = {
                    Color.Black.ToVector3(),
                    Color.DeepPink.ToVector3(),
                    Color.HotPink.ToVector3(),
                    Color.Pink.ToVector3(),
                };


                SwordProjInfo info = new SwordProjInfo(item.type, gradCols, 8f, 0f, 50f, 5, 3f, 1f, 1f);
                info.flowSpeed = 0f;
                (Main.projectile[trail].ModProjectile as BaseSwordProj).info = info;
            }

            return base.UseItem(item, player);

        }

        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            //Starfury shooting is at 45120 in Player.cs | maxmana ding is at 39040 |
            //39718 for flag setting stuff that doesn't shoot every swing (beam sword, ice blade)

            /*
            float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
            int trail = Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<BaseSwordProj>(), 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);

            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                Color.DeepPink.ToVector3(),
                Color.HotPink.ToVector3(),
                Color.Pink.ToVector3(),
            };


            SwordProjInfo info = new SwordProjInfo(item.type, gradCols, 8f, 0f, 50f, 5, 3f, 1f, 1f);
            info.flowSpeed = 0f;
            (Main.projectile[trail].ModProjectile as BaseSwordProj).info = info;
            */

            base.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            /*
            int dustCount = 5 + Main.rand.Next(0, 3);
            for (int i = 220; i < dustCount; i++)
            {
                float prog = (float)(i + 1f) / dustCount;
                Color col = Main.rand.NextBool(2) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);


                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5f);

                Dust p = Dust.NewDustPerfect(target.Center + vel, ModContent.DustType<WindLine>(), vel, newColor: col * 1.5f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 2f);

                float velFadePower = Main.rand.NextFloat(0.9f, 0.93f);
                int shrinkTime = Main.rand.Next(2, 5);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: velFadePower, TimeToStartShrink: shrinkTime, ShrinkYScalePower: 0.85f, XScale: 0.5f, YScale: 0.5f, Pixelize: true);
                wlb.colorAlpha = 40;
                wlb.whiteCoreIntensity = 0f;

                p.customData = wlb;
            }
            */

            float randomRot = Main.rand.NextFloat(6.28f);

            int dustCount = 12;
            for (int i = 0; i < dustCount; i++)
            {
                float progress = (float)i / (float)dustCount;
                float theta = progress * MathHelper.TwoPi;

                float numer = MathF.Cos((2f * MathF.Asin(1) + 3f * MathHelper.Pi) / 10f);
                float denom = MathF.Cos((2f * MathF.Asin(MathF.Cos(4f * theta)) + 3f * MathHelper.Pi) / 10f);

                float r = numer / denom;

                Color dustCol = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                Dust d = Dust.NewDustPerfect(target.Center, ModContent.DustType<PulseInOutDust>(), new Vector2(r * 1.5f, 0f).RotatedBy(randomRot + theta), newColor: dustCol with { A = 50 });
                d.scale *= Main.rand.NextFloat(0.75f, 1f) * 1f;
                //d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.ShakyStar, 40, 0.05f, 0.95f, Pixelize: true);

                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 30, 0.15f, 0.85f, Pixelize: true);

            }

        }

    }
    public class StarfuryShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Starfury);
        }

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousVelRots = new List<float>();

        int timer = 0;
        public override void AI(Projectile projectile)
        {
            int trailCount = 18; //34
            previousVelRots.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            bool addInBetween = true;
            if (addInBetween)
            {
                previousVelRots.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);

                if (previousVelRots.Count > trailCount)
                    previousVelRots.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }

            timer++;

            base.AI(projectile);
        }

        float alpha = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return true;
            
            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D StarBlack = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;
            Texture2D Line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Nightglow").Value;
            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;

            Texture2D Glorb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            //Starfury uses 0.8 scale so x1.25 that is one
            float scale = projectile.scale * 1.25f;

            //Nightglow
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            if (previousVelRots != null && previousPositions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i++)
                {
                    float progress = (float)i / previousVelRots.Count;
                    float size = (1f - (progress * 0.5f)) * scale;

                    float colVal = progress * alpha;

                    Color col = Color.Lerp(Color.LightGoldenrodYellow * 0.75f, Color.HotPink, progress) * progress * 0.5f;

                    float size2 = (1f - (progress * 0.15f)) * scale;
                    Vector2 vec2Scale = new Vector2(2f, 3f) * size;

                    //Black
                    Main.EntitySpriteDraw(Line, previousPositions[i] - Main.screenPosition, null, Color.Black * 0.15f * (colVal * colVal),
                            previousVelRots[i] + MathHelper.PiOver2, Line.Size() / 2f, vec2Scale * size2, SpriteEffects.None);

                    Main.EntitySpriteDraw(StarBlack, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.85f * colVal,
                            previousVelRots[i], StarBlack.Size() / 2f, size2, SpriteEffects.None);

                    Main.EntitySpriteDraw(Line, previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(5f, 5f), null, col with { A = 0 } * 2f * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, Line.Size() / 2f, vec2Scale * size2, SpriteEffects.None);

                }

            }
            float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

            Main.EntitySpriteDraw(Glorb, drawPos, null, Color.HotPink with { A = 0 } * alpha * 0.2f, projectile.rotation, Glorb.Size() / 2f, scale * 1.5f + sineScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Glorb, drawPos, null, Color.LightPink with { A = 0 } * alpha * 0.3f, projectile.rotation, Glorb.Size() / 2f, scale * 1f + sineScale, SpriteEffects.None);


            for (int i = 0; i < 6; i++)
            {
                Color col = Color.DeepPink;
                Main.EntitySpriteDraw(Star, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), null, col with { A = 0 } * 0.8f * alpha, projectile.rotation, Star.Size() / 2f, scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(Star, drawPos, null, Color.HotPink * alpha, projectile.rotation, Star.Size() / 2f, scale * 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(StarBlack, drawPos, null, Color.White with { A = 0 } * 0.35f * alpha, projectile.rotation, StarBlack.Size() / 2f, scale * 1.1f, SpriteEffects.None);

            for (int i = 0; i < 4; i++)
            {
                Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f;
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, Color.HotPink with { A = 0 } * 0.35f, fireballRot, FireBall.Size() / 2f, projectile.scale * 1.05f * alpha, SpriteEffects.None);
            }


            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
