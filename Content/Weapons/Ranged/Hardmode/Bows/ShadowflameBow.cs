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


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class ShadowflameBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ShadowFlameBow);
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
    public class ShadowflameBowShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ShadowFlameArrow);
        }
        public override bool InstancePerEntity => true;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.rotation);

            int trailCount = 14;
            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            if (projectile.ai[2] % 3 == 0 && Main.rand.NextBool(2) && projectile.ai[2] > 2)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2 + Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2, 4),
                    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.15f, 0.2f));
            }

            if (timer % 1 == 0)
            {
                Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -100f);
                Vector2 initVel = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelAlts>(), Velocity: initVel, newColor: new Color(42, 2, 82) * 2f, Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.6f);

                a.velocity *= 0.25f;
                a.velocity += projectile.velocity * 0.15f;


                Vector2 posOffset2 = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel2 = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a2 = Dust.NewDustPerfect(projectile.Center + posOffset2 + (projectile.velocity * 0.5f), ModContent.DustType<GlowPixelAlts>(), Velocity: initVel2, newColor: new Color(42, 2, 82), Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.6f);

                a2.velocity *= 0.25f;
                a2.velocity += projectile.velocity * 0.15f;
            }

            timer++;
            return base.PreAI(projectile);
        }

        //TODO SpriteDirection
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D arrowTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 arrowPos = projectile.Center - Main.screenPosition;

            for (int i = 1; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 vec2Scale = new Vector2(1f * progress, 1f) * projectile.scale;
                Vector2 vec2Scale2 = new Vector2(0.5f * progress, 1f) * projectile.scale;

                //Main.EntitySpriteDraw(arrowTex, previousPositions[i] - Main.screenPosition, null, Color.MediumPurple with { A = 0 } * progress * 0.75f, projectile.rotation, arrowTex.Size() / 2, vec2Scale, SpriteEffects.None);

                //Main.EntitySpriteDraw(arrowTex, previousPositions[i] - Main.screenPosition, null, Color.MediumPurple with { A = 0 } * progress * 1f, projectile.rotation, arrowTex.Size() / 2, vec2Scale2, SpriteEffects.None);

            }


            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/DiamondGlowPMA").Value;
            Vector2 spikeScale = new Vector2(0.25f, 0.55f) * 0.6f;

            Main.EntitySpriteDraw(spike, arrowPos, null, Color.Purple with { A = 0 } * 2f, projectile.rotation, spike.Size() / 2, projectile.scale * spikeScale, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, arrowPos, null, Color.White with { A = 0 }, projectile.rotation, spike.Size() / 2, projectile.scale * 0.5f * spikeScale, SpriteEffects.None);


            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(3, 3), null, Color.MediumPurple with { A = 0 } * 0.5f, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);

            }

            Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);


            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
           
            //Dust
            for (int i = 0; i < 5 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, 
                    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.28f, 0.32f));
            }

            base.OnKill(projectile, timeLeft);
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
