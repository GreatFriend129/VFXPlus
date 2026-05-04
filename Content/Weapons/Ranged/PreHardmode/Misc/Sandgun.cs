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
using rail;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class Sandgun : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Sandgun);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.Sandgun,
                    AnimTime: 15,
                    NormalXOffset: 16f,
                    DestXOffset: 3f,
                    YRecoilAmount: 0.13f,
                    HoldOffset: new Vector2(0f, 1f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.25f;

            //Dust d2 = Dust.NewDustPerfect(position + velNormalized * 3f, ModContent.DustType<CirclePulse>(), velNormalized * 3f, newColor: Color.OrangeRed);
            //CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            //b2.drawLayer = "Dusts";
            //d2.customData = b2;
            //d2.scale = circlePulseSize * 0.05f;

            return true;
        }
    }
    public class Sandball : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SandBallGun);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 20; 
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            //Dust
            if (timer % 1 == 0 && timer > 5)
            {
                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<HighResSmoke>(), Velocity: Vector2.Zero,
                    newColor: Color.Goldenrod with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.25f);
                HighResSmokeBehavior hrsb =  DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 0, fadeDuration: 20);
                hrsb.isPixelated = true;
                d2.customData = hrsb;

                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }


            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousVelrots = new List<float>();
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.Silver with { A = 0 } * 0.35f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.Yellow, Color.White, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 1f * size2) * overallScale * projectile.scale * 0.25f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, Color.White with { A = 0 } * 0.2f * middleProg,
                        previousVelrots[i], flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

    }

    public class Ebonsandball : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.EbonsandBallGun);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 20;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);


            Color purp = new Color(44, 9, 79) * 3f;

            //Dust
            if (timer % 1 == 0 && timer > 5)
            {
                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<HighResSmoke>(), Velocity: Vector2.Zero,
                    newColor: purp with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.25f);
                HighResSmokeBehavior hrsb = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 0, fadeDuration: 20);
                hrsb.isPixelated = true;
                d2.customData = hrsb;

                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousVelrots = new List<float>();
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.Purple with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Color purp = new Color(44, 9, 79) * 3f;
            Color purp2 = new Color(24, 5, 34) * 3f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(purp, Color.White, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 1f * size2) * overallScale * projectile.scale * 0.25f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, Color.White with { A = 0 } * 0.1f * middleProg,
                        previousVelrots[i], flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

    }

    public class Crimsandball : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrimsandBallGun);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 20;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            //Dust
            if (timer % 1 == 0 && timer > 5)
            {
                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<HighResSmoke>(), Velocity: Vector2.Zero,
                    newColor: Color.DarkRed with { A = 0 } * 1.15f, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.25f);
                HighResSmokeBehavior hrsb = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 0, fadeDuration: 20);
                hrsb.isPixelated = true;
                d2.customData = hrsb;

                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousVelrots = new List<float>();
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.Red with { A = 0 } * 0.35f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.Red, Color.Red, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 1f * size2) * overallScale * projectile.scale * 0.25f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, new Color(255, 195, 195) with { A = 0 } * 0.2f * middleProg,
                        previousVelrots[i], flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

    }

    public class Pearlsandball : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PearlSandBallGun);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 20;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            //Dust
            if (timer % 1 == 0 && timer > 5)
            {
                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<HighResSmoke>(), Velocity: Vector2.Zero,
                    newColor: new Color(255, 210, 210) with { A = 0 } * 0.85f, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.25f);
                HighResSmokeBehavior hrsb = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 0, fadeDuration: 20);
                hrsb.isPixelated = true;
                d2.customData = hrsb;

                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousVelrots = new List<float>();
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.White with { A = 0 } * 0.35f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.LightPink, Color.White, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.15f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 1f * size2) * overallScale * projectile.scale * 0.25f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, Color.White with { A = 0 } * 0.1f * middleProg,
                        previousVelrots[i], flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

    }

}
