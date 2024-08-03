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
            /*
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = .12f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Research_1") with { Pitch = .85f, PitchVariance = .2f, Volume = 0.25f };
            SoundEngine.PlaySound(style2, player.Center);

            //Dust
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<LineSpark>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 19),
                    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80

            }
            */
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

            //Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, projectile.rotation, arrowTex.Size() / 2, projectile.scale * 1.1f, SpriteEffects.None);
            //Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);
            //Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, Color.Orange with { A = 0 }, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);


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

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_0") with { Volume = .05f, Pitch = .7f, PitchVariance = 0.35f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_107") with { Volume = .3f, Pitch = .7f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            /*
            Color pinkToUse = Color.Lerp(Color.Purple, Color.DeepPink, 0.1f);

            for (int i = 0; i < 4 + Main.rand.Next(1, 3); i++)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-2f, 2f)) * Main.rand.Next(1, 3),
                    newColor: pinkToUse, Scale: Main.rand.NextFloat(0.3f, 0.5f));
            }
            */
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
