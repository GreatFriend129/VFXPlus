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


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Staves
{
    
    public class AquaScepter : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.AquaScepter) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.AquaScepter;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item4 with { Volume = 0f };
            base.SetDefaults(entity); 
        }


        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ENV_water_splash_01") with { Volume = .17f, Pitch = .80f, PitchVariance = .2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/CommonWaterFallLight00") with { Volume = .1f, Pitch = .2f, PitchVariance = .2f, MaxInstances = 1 }; 
            SoundEngine.PlaySound(style2, player.Center);

            //return true;

            Color col = Color.Lerp(Color.Blue, Color.DodgerBlue, 0.75f);
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<GlowFlare>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(5, 16),
                    newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.5f);
            }

            return true;
        }

    }
    public class AquaScepterShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WaterStream) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.AquaScepter;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                projectile.ai[2] = projectile.scale;

            int trailCount = 60;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

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
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DodgerBlue, Scale: 0.5f);
                de.customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);


                Dust dust57 = de;
                Dust dust212 = dust57;
                dust212.velocity *= 0.45f;
                dust57 = de;
                dust212 = dust57;
                dust212.velocity += projectile.velocity * 0.5f;
            }

            timer++;

            #region vanilla Code minus dust
            float num107 = 0.01f;
            float num109 = 0.15f;

            projectile.scale -= num107;
            if (projectile.scale <= 0f)
            {
                projectile.Kill();
            }
            if (projectile.ai[0] > 3f)
            {
                projectile.velocity.Y += num109;
            }
            else
            {
                projectile.ai[0] += 1f;
            }

            #endregion

            Lighting.AddLight(projectile.Center, Color.DodgerBlue.ToVector3() * 0.75f);

            return false;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Draw(projectile);
            });

            return false;
        }

        public void Draw(Projectile projectile)
        {
            Texture2D line = CommonTextures.Flare.Value;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(6f, 6f); //3f

                    float startScale = projectile.ai[2] + sineScale;

                    Color between = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.15f);
                    Color col = Color.Lerp(between, Color.MediumBlue, 1f - progress);

                    float easedFadeValue = progress * progress;


                    Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress); //
                    Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress); //0.1f 0.2f

                    //Black
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.2f * easedFadeValue,
                        projectile.velocity.ToRotation(), line.Size() / 2f, lineScale * startScale, SpriteEffects.None);

                    //Main
                    Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.75f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);

                    //White
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.75f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

                }

            }



        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int i = 0;
            foreach (Vector2 pos in previousPostions)
            {
                i++;
                if (i % 3 == 0 && i > previousPostions.Count * 0.4f)
                {
                    int a = Dust.NewDust(pos, 0, 0, ModContent.DustType<GlowFlare>(), 0, 0, newColor: Color.DodgerBlue, Scale: 0.4f);
                    Main.dust[a].customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);
                    Main.dust[a].velocity *= ((i * 0.06f));
                    Main.dust[a].velocity += projectile.velocity * 0.5f;
                }
            }

            //CHANGE BACK TO 0.2 Vol????
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ENV_water_splash_01") with { Volume = 0.08f, Pitch = .5f, PitchVariance = 0.5f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            return base.PreKill(projectile, timeLeft);
        }
    }

}
