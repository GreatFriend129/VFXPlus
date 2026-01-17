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
    
    public class IceBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.IceBow);
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
    public class IceBowShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FrostArrow);
        }
        public override bool InstancePerEntity => true;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        public override void AI(Projectile projectile)
        {
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.rotation);

            int trailCount = 10;
            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            if (projectile.ai[2] % 3 == 0 && Main.rand.NextBool(2) && projectile.ai[2] > 2)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), 
                    projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2 + Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2, 4),
                    newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.15f, 0.2f));
            }

            projectile.ai[2]++;

            base.AI(projectile);
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

                Main.EntitySpriteDraw(arrowTex, previousPositions[i] - Main.screenPosition, null, Color.LightSkyBlue with { A = 0 } * progress * 0.75f, projectile.rotation, arrowTex.Size() / 2, vec2Scale, SpriteEffects.None);

                Main.EntitySpriteDraw(arrowTex, previousPositions[i] - Main.screenPosition, null, Color.LightSkyBlue with { A = 0 } * progress * 1f, projectile.rotation, arrowTex.Size() / 2, vec2Scale2, SpriteEffects.None);

            }


            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/DiamondGlowPMA").Value;
            Vector2 spikeScale = new Vector2(0.25f, 0.55f) * 0.6f;

            Main.EntitySpriteDraw(spike, arrowPos, null, Color.DeepSkyBlue with { A = 0 }, projectile.rotation, spike.Size() / 2, projectile.scale * spikeScale, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, arrowPos, null, Color.White with { A = 0 }, projectile.rotation, spike.Size() / 2, projectile.scale * 0.5f * spikeScale, SpriteEffects.None);


            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(3, 3), null, Color.LightSkyBlue with { A = 0 } * 0.5f, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);

            }

            Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);


            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_1") with { Volume = .05f, Pitch = .85f, PitchVariance = 0.3f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_107") with { Volume = .3f, Pitch = .8f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            //Dust
            for (int i = 0; i < 5 + Main.rand.Next(1, 3); i++)
            {
                //The less straight the dust flies, the slower it should be
                Vector2 velDir = projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2);
                float randomTilt = Main.rand.NextFloat(0f, 1f); 
                float tiltPercent = randomTilt / 1f;
                float velVal = 1.25f * ((1f - tiltPercent) * 2.75f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), 
                    velDir.RotatedBy(randomTilt * (Main.rand.NextBool() ? 1f : -1f)) * velVal, 
                    newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.28f, 0.32f));
            }

            base.OnKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
