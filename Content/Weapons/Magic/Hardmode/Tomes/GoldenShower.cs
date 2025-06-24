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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    
    public class GoldenShower : GlobalItem 
    {
        //I think we need this?
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.GoldenShower) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.GoldenShowerToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item4 with { Volume = 0f };
            base.SetDefaults(entity); 
        }


        //We only want to play sfx on first shot of weapon (Golden Shower does 3 per cast)
        int shotCount = 0;
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ENV_water_splash_01") with { Volume = 0.2f, Pitch = 0.95f, PitchVariance = .25f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/CommonWaterFallLight00") with { Volume = 0.05f, Pitch = 0.65f, PitchVariance = .2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style2, player.Center);

            return true;
        }

    }
    public class GoldenShowerShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;


        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.GoldenShowerFriendly) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.GoldenShowerToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                projectile.ai[2] = projectile.scale;

            int trailCount = 60; ///60
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            bool addInBetween = false;
            if (addInBetween)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPostions.Count > trailCount)
                    previousPostions.RemoveAt(0);
            }


            if (timer % 2 == 0 && timer > 3 && Main.rand.NextBool(2))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: Color.Gold * 0.75f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.75f);
                da.velocity += projectile.velocity.RotatedByRandom(0.2f) * 0.65f;
                da.alpha = 12;
            }

            timer++;

            #region vanilla Code minus dust
            projectile.scale -= 0.002f;
            if (projectile.scale <= 0f)
            {
                projectile.Kill();
            }
            if (projectile.ai[0] > 3f)
            {
                projectile.velocity.Y += 0.075f;
                for (int num100 = 0; num100 < 3; num100++)
                {
                    //float num101 = projectile.velocity.X / 3f * (float)num100;
                    //float num102 = projectile.velocity.Y / 3f * (float)num100;
                    //int num103 = 14;
                    //int num104 = Dust.NewDust(new Vector2(projectile.position.X + (float)num103, projectile.position.Y + (float)num103), projectile.width - num103 * 2, projectile.height - num103 * 2, 170, 0f, 0f, 100);
                    //Main.dust[num104].noGravity = true;
                    //Dust dust61 = Main.dust[num104];
                    //Dust dust212 = dust61;
                    //dust212.velocity *= 0.1f;
                    //dust61 = Main.dust[num104];
                    //dust212 = dust61;
                    //dust212.velocity += projectile.velocity * 0.5f;
                    //Main.dust[num104].position.X -= num101;
                    //Main.dust[num104].position.Y -= num102;
                }
                if (Main.rand.Next(8) == 0 && timer % 2 == 0)
                {
                    int num105 = 16;


                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust de = Dust.NewDustPerfect(projectile.Center, DustID.Ichor, vel , 100, Scale: 0.5f);

                    //int num106 = Dust.NewDust(new Vector2(projectile.position.X + (float)num105, projectile.position.Y + (float)num105), projectile.width - num105 * 2, projectile.height - num105 * 2, 170, 0f, 0f, 100, default(Color), 0.5f);
                    Dust dust57 = de;//Main.dust[num106];
                    Dust dust212 = dust57;
                    dust212.velocity *= 0.25f;
                    dust57 = de;
                    dust212 = dust57;
                    dust212.velocity += projectile.velocity * 0.5f;
                }
            }
            else
            {
                projectile.ai[0] += 1f;
            }

            #endregion

            Lighting.AddLight(projectile.Center, Color.Gold.ToVector3() * 0.65f);

            return false;
            return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Utils.DrawBorderString(Main.spriteBatch, "" + projectile.scale, projectile.Center - Main.screenPosition, Color.White);
            
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                Draw(projectile);
            });

            return false;
        }

        public void Draw(Projectile projectile)
        {
            Texture2D line = CommonTextures.Flare.Value;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f); //3f

                float startScale = projectile.ai[2] + sineScale;

                Color col = Color.Orange;

                float easedFadeValue = progress * progress;


                Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress); //
                Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress); //0.1f 0.2f

                //Black
                Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.4f * easedFadeValue,
                    projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * projectile.scale, SpriteEffects.None);

                //Main
                Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.5f * easedFadeValue,
                    previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);

                //White
                Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * easedFadeValue,
                    previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

            }

        }

        public void DrawBackupHolyShitThisLooksCool(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(6f, 6f); //3f

                    float startScale = projectile.ai[2] + sineScale;

                    Color col = Color.Aquamarine;

                    float easedFadeValue = progress * progress;


                    Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress); //
                    Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress); //0.1f 0.2f

                    //Black
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.4f * easedFadeValue,
                        projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * projectile.scale, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);


                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

                }

            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            //SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/ENV_water_splash_01") with { Volume = 0.25f, Pitch = .5f, PitchVariance = 0.2f, MaxInstances = 1 }; 
            //SoundEngine.PlaySound(style, projectile.Center);


            return base.PreKill(projectile, timeLeft);

            for (int i = 0; i < previousRotations.Count; i++)
            {
                if (i % 3 == 0 && i > 10)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 pos = previousPostions[i];
                    float velDir = previousRotations[i];

                    Dust da = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), dustVel, newColor: Color.Gold * 0.75f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.5f);
                    da.velocity += velDir.ToRotationVector2().RotatedByRandom(0.2f) * 3.25f;
                    //da.alpha = 12;

                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_" + soundVariant1) with { Volume = 0.45f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1, };
            //SoundEngine.PlaySound(style, projectile.Center);
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
           
            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
