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
using static tModPorter.ProgressUpdate;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Swords
{
    
    public class Starfury : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Starfury) && false;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //return false;
            return true;
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
