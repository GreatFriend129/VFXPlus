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
using System.Timers;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    
    public class RazorbladeTyphoon : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.RazorbladeTyphoon);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle stylea = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Volume= 0.2f, Pitch = 1f, PitchVariance = .35f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(stylea, player.Center);


            return true;
        }

    }
    public class RazorbladeTyphoonShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Typhoon);
        }

        float scale = 0;
        float alpha = 0;
        int timer = 0;
        BaseTrailInfo trail1 = new BaseTrailInfo();
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 35; //35
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f);


            scale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1f));



            if (timer % 2 == 0 && timer > 3 && Main.rand.NextBool(2))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), dustVel, newColor: Color.DeepSkyBlue * 0.75f, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 2f);
                da.velocity += projectile.velocity.RotatedByRandom(0.2f) * 0.65f;
                //da.alpha = 12;
            }

            timer++;

            return base.PreAI(projectile);
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Orb glow

            //Aquamarine > Aqua 

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.Lerp(Color.Aquamarine, Color.DeepSkyBlue * 1f, progress) * Easings.easeInQuad(progress);

                    float size2 = 1f * projectile.scale * scale * progress;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    //Black
                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.Black * 0.1f, 
                        projectile.rotation, TexOrigin, size2, SpriteEffects.None);

                    //Main
                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.45f,
                            projectile.rotation, TexOrigin, size2, SpriteEffects.None);
                }

            }

            for (int i = 0; i < 4; i++)
            {
                float dist = 5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                float opacitySquared = 1f;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.DeepSkyBlue with { A = 0 } * 0.35f * scale, projectile.rotation, TexOrigin, projectile.scale * 1f, SpriteEffects.None);
            }

            //Border
            for (int i = 0; i < 12; i++)
            {
                //float opacitySquared = 1f;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                   Color.Aquamarine with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1f * scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * scale, SpriteEffects.None);

            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor with { A = 0 } * 0.1f, projectile.rotation, TexOrigin, projectile.scale * scale, SpriteEffects.None);

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
