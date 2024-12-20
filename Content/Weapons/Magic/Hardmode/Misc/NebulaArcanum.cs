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
using Terraria.Utilities;
using VFXPlus.Common.Drawing;
using Terraria.Graphics;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class NebulaArcanum : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.NebulaArcanum);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_1") with { Volume = 1f, Pitch = -1f, PitchVariance = .33f, };
            //SoundEngine.PlaySound(style, player.Center);


            return true;
        }

    }
    public class NebulaArcanumMainShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanum);
        }

        float storedScale = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 12;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            drawScale = Math.Clamp(MathHelper.Lerp(drawScale, 1.25f, 0.04f), 0f, 1f);

            //Main.NewText(projectile.penetrate);

            timer++;

            return base.PreAI(projectile);
        }

        float drawAlpha = 0f;
        float drawScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                UniqueCircularOrbitBehaviorThing(projectile, 40);
            });

            //if (drawScale == 1f)
            //return true;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Star
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Flare/FireSpike").Value;
            Texture2D bloom = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            float dir = projectile.velocity.X > 0 ? 1 : -1;

            float starRotation = projectile.velocity.ToRotation() + MathHelper.Lerp(0f, MathHelper.Pi * 2f * dir, Easings.easeInOutQuad(drawScale));
            float starScale = Easings.easeOutCirc(1f - drawScale) * projectile.scale * 0.75f;

            if (drawScale != 1f)
            {
                Main.EntitySpriteDraw(star, drawPos, null, Color.HotPink with { A = 0 } * drawScale * 2f, starRotation, star.Size() / 2f, starScale, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.White with { A = 0 } * drawScale * 2f, starRotation, star.Size() / 2f, starScale * 0.5f, SpriteEffects.None);

                Main.EntitySpriteDraw(star, drawPos, null, Color.HotPink with { A = 0 } * drawScale * 2f, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScale, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.White with { A = 0 } * drawScale * 2f, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScale * 0.5f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(bloom, drawPos, null, Color.DeepPink with { A = 0 } * drawScale * 0.25f, starRotation, bloom.Size() / 2f, 1.3f * drawScale * projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(bloom, drawPos, null, Color.HotPink with { A = 0 } * drawScale * 0.5f, starRotation, bloom.Size() / 2f, 0.9f * drawScale * projectile.scale, SpriteEffects.None);

            return true;
        }

        public void UniqueCircularOrbitBehaviorThing(Projectile projectile, int count)
        {
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Twinkle").Value;

            FastRandom r = new(Main.player[projectile.owner].name.GetHashCode());
            float speedTime = (Main.GlobalTimeWrappedHourly * 0.5f) + timer * 0.002f;

            float minRange = 25f * drawScale; //55
            float maxRange = 100f * drawScale; //160
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = 0f;
                if (r.NextFloat() < 0.2f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 1, frameY: r.Next(0));
                    scale = new Vector2(0.5f, 0.5f) * 0.35f;
                    //rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 1.8f, 4f); //0.8, 4f
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance * scaleWave;

                Color purp = Color.Lerp(Color.Purple, Color.DeepPink, 0.85f) * projectile.Opacity;

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, purp with { A = 0 } * 3f, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White with { A = 0 }, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 1.85f, SpriteEffects.None);

            }
        }
        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            //return false;
            return base.PreKill(projectile, timeLeft);
        }


    }

    public class NebulaArcanumSubshotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanumSubshot);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            return true;
            
            int trailCount = 20;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            timer++;
            return base.PreAI(projectile);
        }



        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVertexTrail(false);
            });

            return false;

        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress * progress);
            float StripWidth(float progress) => 30f * Easings.easeOutQuad(progress);// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.08f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            vertexStrip.DrawTrail();

            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(Color.OrangeRed.ToVector3() * 10f);
            myEffect.Parameters["glowThreshold"].SetValue(0.5f);
            myEffect.Parameters["glowIntensity"].SetValue(2.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {

            base.OnKill(projectile, timeLeft);
        }
    }

    public class NebulaArcanumExplosionShardOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanumExplosionShotShard);
        }

        float drawAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 20;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);
            
            //Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStrong>());

            timer++;

            #region vanillaCode
            int num247 = (int)projectile.ai[0];
            projectile.ai[1] += 1f;
            float num248 = (60f - projectile.ai[1]) / 60f;
            if (projectile.ai[1] > 40f)
            {
                projectile.Kill();
            }
            projectile.velocity.Y += 0.2f;
            if (projectile.velocity.Y > 18f)
            {
                projectile.velocity.Y = 18f;
            }
            projectile.velocity.X *= 0.98f;
            for (int num249 = 2220; num249 < 2; num249++)
            {
                int num250 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num247, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1.1f);
                Main.dust[num250].position = (Main.dust[num250].position + projectile.Center) / 2f;
                Main.dust[num250].noGravity = true;
                Dust dust70 = Main.dust[num250];
                Dust dust3 = dust70;
                dust3.velocity *= 0.3f;
                dust70 = Main.dust[num250];
                dust3 = dust70;
                dust3.scale *= num248;
            }
            for (int num251 = 2220; num251 < 1; num251++)
            {
                int num252 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num247, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 0.6f);
                Main.dust[num252].position = (Main.dust[num252].position + projectile.Center * 5f) / 6f;
                Dust dust71 = Main.dust[num252];
                Dust dust3 = dust71;
                dust3.velocity *= 0.1f;
                Main.dust[num252].noGravity = true;
                Main.dust[num252].fadeIn = 0.9f * num248;
                dust71 = Main.dust[num252];
                dust3 = dust71;
                dust3.scale *= num248;
            }


            #endregion

            return false;
            return base.PreAI(projectile);
        }



        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVertexTrail(false);
            });

            return false;

        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress * progress);
            float StripWidth(float progress) => 30f * Easings.easeOutQuad(progress);// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.08f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            vertexStrip.DrawTrail();

            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(Color.OrangeRed.ToVector3() * 10f);
            myEffect.Parameters["glowThreshold"].SetValue(0.5f);
            myEffect.Parameters["glowIntensity"].SetValue(2.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            base.OnKill(projectile, timeLeft);
        }
    }

    //So aparently this proj is literally a copy of Crystal Serpents main proj and is used to immediately die and spawn sub shards
    //This should never appear in "normal-play" 
    public class NebulaArcanumExplosionOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanumExplosionShot);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            return base.PreAI(projectile);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }
    }

}
