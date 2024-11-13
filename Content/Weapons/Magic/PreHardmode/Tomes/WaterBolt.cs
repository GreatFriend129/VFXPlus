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
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Tomes
{
    
    public class WaterBolt : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.WaterBolt);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ENV_water_splash_01") with {Volume = 0.25f, Pitch = 0.9f, PitchVariance = 0.2f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class WaterBoltShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WaterBolt);
        }


        float scale = 1f;
        float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            if (timer % 2 == 0)
            {
                int trailCount = 10; //30
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }

            if (timer % 2 == 0 && timer > 3 && Main.rand.NextBool(2))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: Color.DodgerBlue * 0.75f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.75f);
                da.velocity += projectile.velocity.RotatedByRandom(0.2f) * 0.65f;
                da.alpha = 12;
            }

            if (timer % 3 == 0 && Main.rand.NextBool(5) && timer > 3)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DodgerBlue, Scale: 0.6f);
                de.customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);


                Dust dust57 = de;
                Dust dust212 = dust57;
                dust212.velocity *= 0.45f;
                dust57 = de;
                dust212 = dust57;
                dust212.velocity += projectile.velocity * 0.5f;
            }

            /*
            if (timer % 4 == 0 && Main.rand.NextBool() && timer > 8)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.25f);

                p.velocity -= projectile.velocity * 1f;
            }

            //Make it feel more magical but less like water
            if (timer % 3 == 0 && timer > 8f && false)
            {
                Vector2 sideOffset = new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(projectile.velocity.ToRotation());
                Vector2 vel = -projectile.velocity * 0.25f;

                Dust line = Dust.NewDustPerfect(projectile.Center + sideOffset, ModContent.DustType<MuraLineBasic>(), vel, 255,
                    newColor: Color.DodgerBlue * 0.25f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.6f);

            }
            */
            timer++;

            return false;
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                TrailDraw(projectile);
            });

            return false;

        }

        public void MyDraw(Projectile projectile)
        {
            Texture2D Line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 origin = Line.Size() / 2f;
            float rot = projectile.velocity.ToRotation();

            float alpha = 1f;
            float scale = projectile.scale * 1.25f;
            Vector2 vec2Scale = new Vector2(1f, 0.7f) * scale;

            Color col = Color.DodgerBlue with { A = 0 };

            //For this, the plan is to draw the core of the VFX, and then have bigger and bigger layers on top
            //that grow in scale, shrink in alpha, and are offset more randomly for each layer

            int layers = 8;
            float startingOffset = 2f;
            float offsetBonus = 1f;
            float scaleIncreasePower = 0.15f;

            for (int i = 0; i < layers; i++)
            {
                float progress = (float)i / layers;
                
                //pos
                float randomOffDist = startingOffset + (i * offsetBonus);
                Vector2 randomOffset = Main.rand.NextVector2Circular(randomOffDist, randomOffDist);
                Vector2 newDrawPos = drawPos + randomOffset;

                //alpha
                float myAlpha = Easings.easeInCubic(progress);

                Main.NewText(progress);

                //scale
                float scaleAmount = 1f + (i * scaleIncreasePower);
                Vector2 myScale = vec2Scale * scaleAmount;

                Main.EntitySpriteDraw(Line, newDrawPos, null, col * myAlpha, rot, origin, myScale, SpriteEffects.None);
            }


            //Draw Core
            Main.EntitySpriteDraw(Line, drawPos, null, col, rot, origin, vec2Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(Line, drawPos, null, Color.White with { A = 0 } * 0.75f, rot, origin, vec2Scale * 0.5f, SpriteEffects.None);
        }


        public void TrailDraw(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            if (previousRotations != null && previousPositions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;


                    //Want 0 -> 3f
                    //Max  
                    float offsetIntensity = (1.5f * (1f - progress)) + 4.5f; //6f

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(offsetIntensity, offsetIntensity); //3f

                    float startScale = 1f + sineScale;

                    //Color col = Color.DodgerBlue;
                    Color col = Color.Lerp(Color.DodgerBlue, Color.Blue, 1f - progress);

                    float easedFadeValue = progress * progress;


                    Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress); //
                    Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress); //0.1f 0.2f

                    //Black
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.2f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * projectile.scale, SpriteEffects.None);

                    //Main
                    Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 1f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);

                    //White
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

                }

                Main.EntitySpriteDraw(line, projectile.Center - Main.screenPosition, null, Color.DodgerBlue with { A = 0 } * 1f,
                    projectile.velocity.ToRotation(), line.Size() / 2f, new Vector2(1.2f, 0.7f) * 0.75f, SpriteEffects.None);

                Main.EntitySpriteDraw(line, projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 1f,
                    projectile.velocity.ToRotation(), line.Size() / 2f, new Vector2(1.25f, 0.7f) * 0.5f, SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //return false;
            return base.PreKill(projectile, timeLeft);
        }




        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);


            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.45f, 0.75f));
                d.customData = new GlowFlareBehavior(GlowThreshold: 0.45f, GlowPower: 2.5f, TotalBoost: 0.95f);

                //d.velocity += oldVelocity * 0.14f;
            }


            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: Color.DodgerBlue, Scale: 1f);
            d1.rotation = 0f + oldVelocity.ToRotation();
            d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: Color.DodgerBlue, Scale: 1f);
            d2.rotation = MathHelper.PiOver4 + oldVelocity.ToRotation();
            d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

            //Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), oldVelocity * 3f, newColor: Color.DodgerBlue);
            //d.scale = 2f;
            //d.customData = new GlowFlareBehavior(GlowThreshold: 0.45f, GlowPower: 2.5f);



            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
