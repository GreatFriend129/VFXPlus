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
using Terraria.Graphics;
using VFXPlus.Common.Drawing;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.VFXTest;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class FrostStaff : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.FrostStaff);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_43") with { Volume = 0.45f, Pitch = .35f, PitchVariance = 0.1f, MaxInstances = 1 };
            SoundEngine.PlaySound(style4, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_20") with { Volume = 0.35f, Pitch = .85f, PitchVariance = 0.15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_28") with { Volume = .1f, Pitch = -0.05f, PitchVariance = 0.1f, MaxInstances = 1  };
            SoundEngine.PlaySound(style, player.Center);

            //SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_etherian_portal_dryad_touch") with { Volume = .15f, Pitch = 1f, PitchVariance = .15f, MaxInstances = -1, };
            //SoundEngine.PlaySound(style3, player.Center);

            //Both look really Cool
            //Dust d = Dust.NewDustPerfect(player.Center, ModContent.DustType<CirclePulse>(), vel * 5f, newColor: Color.LightSkyBlue);
            //d.customData = new CirclePulseBehavior(Size: 0.15f, Pixelize: false, TimesToDraw: 2, XScale: 1.1f);

            //Dust d = Dust.NewDustPerfect(player.Center, ModContent.DustType<CirclePulse>(), vel * 5f, newColor: Color.LightSkyBlue);
            //d.customData = new CirclePulseBehavior(Size: 0.15f, Pixelize: false, TimesToDraw: 2, XScale: 0.1f);


            return true;
        }

    }
    public class FrostStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.FrostBoltStaff;
        }


        float drawAlpha = 0f;
        float drawScale = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer % 1 == 0)
            {
                int trailCount = 15; //30
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }

            bool vanillaDust = true;


            if (timer % 2 == 0 && timer > 5) //4
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<HighResSmoke>(), newColor: Color.LightSkyBlue, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 0.9f);
                Main.dust[d].velocity += projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;

                HighResSmokeBehavior hrsb = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.35f); //0.5
                hrsb.isPixelated = true;
                Main.dust[d].customData = hrsb;
            }

            if (timer % 1 == 0 && timer > 5 && false)
            {
                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.3f);

                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.4f) * 1.9f);
                Main.dust[d].rotation = Main.rand.NextFloat(6.28f);
                Main.dust[d].velocity += projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.55f;
                Main.dust[d].fadeIn = Main.rand.NextFloat(0.2f, 0.45f);
            }


            Lighting.AddLight(projectile.Center, Color.LightSkyBlue.ToVector3() * 0.85f);


            drawAlpha = Math.Clamp(MathHelper.Lerp(drawAlpha, 1.35f, 0.09f), 0f, 1f);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);

            drawScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2f)) * 1f;

            if (timer < 700 & timer % 2 == 0 && false)
            {
                Vector2 vel2 = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SmallSmoke>(), vel2, newColor: col2 with { A = 0 } * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 3f);
                d2.velocity += projectile.velocity.RotatedByRandom(0.75f) * 0.4f;
                d2.customData = new SmallSmokeBehavior(5f, 0.97f);

                //HighResSmokeBehavior hrsb = new HighResSmokeBehavior();
                //hrsb.velSlowAmount = 0.89f;
                //hrsb.overallAlpha = 0.7f;
                //d2.customData = hrsb;
            }

            timer++;

            #region vanilla
            if (vanillaDust)
            {
                for (int num234 = 0; num234 < 1; num234++)
                {
                    int num235 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 92, projectile.velocity.X, projectile.velocity.Y, 15, default(Color), 1.2f);
                    Main.dust[num235].noGravity = true;
                    Dust dust65 = Main.dust[num235];
                    Dust dust3 = dust65;
                    dust3.velocity *= 0.3f;
                }
            }
            #endregion

            return false;
            return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                TrailDraw(projectile);
            });

            Texture2D line2 = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            Color between = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);

            Main.EntitySpriteDraw(line2, projectile.Center - Main.screenPosition, null, between with { A = 0 } * drawAlpha,
                projectile.velocity.ToRotation(), line2.Size() / 2f, new Vector2(1.2f, 1f * drawScale) * 0.75f, SpriteEffects.None);

            Main.EntitySpriteDraw(line2, projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * drawAlpha,
                projectile.velocity.ToRotation(), line2.Size() / 2f, new Vector2(1.25f, 0.7f * drawScale) * 0.35f, SpriteEffects.None);

            return false;

        }

        public void TrailDraw(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            Texture2D line2 = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            if (previousRotations != null && previousPositions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;


                    float offsetIntensity = (1.5f * (1f - progress)) + 4.5f; 

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(offsetIntensity * 1f, offsetIntensity * 1f); 

                    float startScale = 1f + sineScale;

                    Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 1f - progress);

                    float easedFadeValue = progress * progress * drawAlpha;


                    Vector2 lineScale = new Vector2(1.25f, (0.25f + 0.4f * progress) * drawScale); //
                    Vector2 lineScale2 = new Vector2(1.25f, (0.05f + 0.05f * progress) * drawScale); //0.1f 0.2f

                    //Black
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.2f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * projectile.scale, SpriteEffects.None);

                    //Main
                    Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.7f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);

                    //White
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.3f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

                }

                //Color between = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);

                //Main.EntitySpriteDraw(line2, projectile.Center - Main.screenPosition, null, between with { A = 0 } * 1f,
                //    projectile.velocity.ToRotation(), line2.Size() / 2f, new Vector2(1.2f, 0.95f * drawScale) * 0.75f, SpriteEffects.None);
                
                //Main.EntitySpriteDraw(line2, projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 1f,
                //    projectile.velocity.ToRotation(), line2.Size() / 2f, new Vector2(1.25f, 0.7f * drawScale) * 0.35f, SpriteEffects.None);
            }

            return;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_1") with { Volume = .05f, Pitch = .9f, PitchVariance = 0.3f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_107") with { Volume = .3f, Pitch = .9f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);


            for (int i = 0; i < 11 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale * 1.5f);

                //p.velocity += projectile.velocity * 0.15f;
            }

            for (int i = 0; i < 4; i++)
            {
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);

                Vector2 vel = Main.rand.NextVector2Circular(1.75f, 1.75f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel, newColor: col2, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1.5f);
                d.customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.65f);
            }

            return false;

            return base.PreKill(projectile, timeLeft);
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
