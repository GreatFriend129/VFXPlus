using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VFXPlus.Common.Drawing;
using Terraria.Audio;
using Terraria.Utilities;

namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class LifeDrain : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SoulDrain);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            for (int i = 220; i < 22; i++) //16
            {
                float progress = (float)i / 21;
                Color col = Color.Lerp(Color.Black, Color.Red, progress);

                //Color.Lerp(Color.Black, Color.Orange, Main.rand.NextFloat())
                Dust d = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 3.4f) * 1.65f,
                    newColor: col with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 1.5f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.3f); //12 28
            }

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_43") with { Volume = 0.2f, Pitch = -.25f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style4, player.Center);

            return true;
        }

    }
    public class LifeDrainShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SoulDrain);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<LifeDrainVFX>(), 0, 0, Main.player[projectile.owner].whoAmI);
            }

            timer++;
            return true;
        }

        bool firstHit = true;
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (firstHit)
            {
                SoundStyle style23 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/cleaver_hit_06") with { Pitch = 0.1f, PitchVariance = .2f, Volume = 0.03f, MaxInstances = -1 };
                SoundEngine.PlaySound(style23, projectile.Center);

                firstHit = false;
            }


            base.OnHitNPC(projectile, target, hit, damageDone);
        }

    }

    public class LifeDrainVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 700;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;
        
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;

            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 24400; 
        }

        float drawScale = 0f;
        float drawAlpha = 1f;
        int timer = 0;
        public override void AI()
        {
            if (timer == 0)
                Projectile.spriteDirection = Projectile.Center.X > Main.player[Projectile.owner].Center.X ? 1 : -1;

            if (timer == 0)
            {
                //Smoke Explosion
                for (int i = 0; i < 15; i++)
                {
                    float progress = (float)i / 15f;

                    Vector2 spawnPos = Projectile.Center + new Vector2(0f, -1f) * Main.rand.NextFloat(0, 280f * progress);
                    Vector2 smvel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 16f * (1f - progress));

                    Dust sm = Dust.NewDustPerfect(Projectile.Center + smvel, ModContent.DustType<GlowPixelAlts>(), smvel, newColor: Color.Crimson * 1f, Scale: Main.rand.NextFloat(0.65f, 0.85f));
                    sm.alpha = 10;

                    GlowPixelAltBehavior bev = new GlowPixelAltBehavior();
                    bev.base_fadeOutPower = 0.9f;
                    sm.customData = bev;
                }
            }

            /*
            if (timer == 0)
            {
                //Smoke Explosion
                for (int i = 0; i < 30; i++)
                {
                    float progress = (float)i / 30f;
                    Vector2 smvel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 18f * (1f - progress));

                    Dust sm = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedLineSpark>(), smvel, newColor: Color.DeepSkyBlue * 1f, Scale: Main.rand.NextFloat(0.65f, 1f));
                    sm.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.91f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 8 + Main.rand.Next(-5, 5), killEarlyTime: 40,
    1f, 0.5f, shouldFadeColor: false);
                }
            }
            */
            if (drawScale < 1f)
                drawScale = Math.Clamp(MathHelper.Lerp(drawScale, 3f, 0.04f), 0.15f, 2f);

            //float timeForPopInAnim = 30;
            //float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            //drawScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0, 2.5f)); //2.5

            if (drawScale >= 1f) //0.8f
                drawAlpha = Math.Clamp(MathHelper.Lerp(drawAlpha, -0.5f, 0.09f), 0f, 1f);

            if (drawAlpha == 0f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D orb = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;
            Vector2 originPoint = Projectile.Center - Main.screenPosition;

            Color col1 = Color.Crimson * 0.75f; //Gold
            Color col2 = Color.Crimson * 0.525f;
            Color col3 = Color.Red * 0.375f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            float scale = drawScale * Projectile.scale * 2.6f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, originPoint, null, col1 with { A = 0 } * drawAlpha * 0.25f, 0f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col2 with { A = 0 } * drawAlpha * 0.25f, 0f, orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col3 with { A = 0 } * drawAlpha * 0.3f, 0f, orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                GoddamnMonsoon(34); //50
            });


            return false;
        }

        public void GoddamnMonsoon(int count = 50)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/anotheranotherorb").Value;

            FastRandom r = new("Penis".GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 0.75f * Projectile.spriteDirection; //2f

            float minRange = 25f * drawScale; //25
            float maxRange = 105f * drawScale;// Easings.easeInOutBack(drawScale, 0f, 3f);
            for (int i = 0; i < count; i++)
            {
                float iprog = (float)i / (float)count;

                Texture2D texture = Tex;
                Rectangle frame = texture.Bounds;
                Vector2 scale = new Vector2(0.33f, 0.6f) * 0.4f;
                float rotation = 3.14f / 2f;
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f);
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = MathHelper.Lerp(minRange, maxRange, iprog);
                    //NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance * scaleWave;
                drawPosition += Main.rand.NextVector2Circular(2f, 2f);


                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.Red with { A = 0 } * drawAlpha * 0.55f, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 6.5f * drawScale * 1f, SpriteEffects.None);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White with { A = 0 } * 0.5f * drawAlpha * 0.65f, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave, scale.Y * scaleWave) * 3.25f * drawScale * 1f, SpriteEffects.None);
            }
        }


        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }
    }
}
