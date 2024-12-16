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
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    
    public class LunarFlare : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.LunarFlareBook);
        }

        public override void SetDefaults(Item entity)
        {
            entity.scale = 1f;
            
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
    public class LunarFlareShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LunarFlare);
        }

        float scale = 0;
        float alpha = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 55;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 2 == 0 && Main.rand.NextBool() && false)
            {

                Dust p = Dust.NewDustPerfect(projectile.Center, DustID.Torch,
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.White, Scale: Main.rand.NextFloat(0.9f, 1.1f) * 1f);

                p.noGravity = true;
                p.velocity += projectile.velocity * 0.2f;
            }

            timer++;
            return true;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float rot = projectile.velocity.ToRotation();
            
            //Orb
            //Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D orb = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;
            Vector2 originPoint = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);

            Color col1 = Color.White * 0.75f;
            Color col2 = Color.SkyBlue * 0.525f;
            Color col3 = Color.DeepSkyBlue * 0.375f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            float scale = 1f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, originPoint, null, col1 with { A = 0 }, rot, orb.Size() / 2f, scale1 * scale * new Vector2(1f, 0.75f), SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col2 with { A = 0 }, rot, orb.Size() / 2f, scale2 * scale * sineScale1 * new Vector2(1f, 0.75f), SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col3 with { A = 0 }, rot, orb.Size() / 2f, scale3 * scale * sineScale2 * new Vector2(1f, 0.75f), SpriteEffects.None);

            
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.Lerp(Color.DeepSkyBlue, Color.DeepSkyBlue * 1f, progress) * 1f;// * Easings.easeInQuad(progress);

                    //DodgerBlue or Blue

                    Vector2 lineScale = new Vector2(1f, 0.3f * progress) * progress;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Vector2 offset = Main.rand.NextVector2Circular(5f + (10f * (1f - progress)), 20f);

                    Vector2 innerScale = new Vector2(1f, 0.15f * progress) * progress;

                    Main.EntitySpriteDraw(line, AfterImagePos + offset, null, col with { A = 0 } * 0.85f * progress * 1f,
                        previousRotations[i], line.Size() / 2f, lineScale * 1.25f, SpriteEffects.None);


                    Main.EntitySpriteDraw(line, AfterImagePos + offset, null, Color.White with { A = 0 } * 0.26f * progress * 1f,
                        previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);


                }
            }

            return false;
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {


            return base.PreKill(projectile, timeLeft);
        }



    }

}
