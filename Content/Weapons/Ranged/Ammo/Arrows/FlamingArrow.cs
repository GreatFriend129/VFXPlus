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


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class FlamingArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FireArrow);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 14; //12
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            
            if (timer % 6 == 0 && Main.rand.NextBool(2) && false)
            {
                Dust grass = Dust.NewDustPerfect(projectile.Center, DustID.WoodFurniture, Main.rand.NextVector2Circular(2, 2), 0, Scale: 0.85f);
                grass.velocity += projectile.velocity;
                grass.noGravity = true;
                grass.alpha = 50;
            }

            float EU = 1f + projectile.extraUpdates;

            float fadeInTime = Math.Clamp((timer + 4f * EU) / 15f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;

            //So apparently flaming arrow dust is HARD CODED into the Update() function for some reason

            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;
            Texture2D orb = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, true);
            });
            DrawTrail(projectile, false);

            Color betweenOrange = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.White with { A = 0 } * 0.4f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            Vector2 flareScale = new Vector2(0.85f, 0.3f) * projectile.scale * 1.5f * (1f - overallScale);
            Main.EntitySpriteDraw(flare, drawPos, null, Color.OrangeRed with { A = 0 } * 0.35f, projectile.rotation + MathHelper.PiOver2, flare.Size() / 2, flareScale, SpriteEffects.None);
            Main.EntitySpriteDraw(flare, drawPos, null, Color.OrangeRed with { A = 0 } * 0.85f, projectile.rotation + MathHelper.PiOver2, flare.Size() / 2, flareScale * 0.75f, SpriteEffects.None);


            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, betweenOrange with { A = 0 } * 0.5f, projectile.rotation, orb.Size() / 2, new Vector2(0.55f, 1f) * overallScale, SpriteEffects.None);


            //MainTex
            //TODO DO SPRITEFX DIPSHIT
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.Orange with { A = 0 } * 0.65f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            Color betweenOrange = Color.Lerp(Color.Orange, Color.OrangeRed, 0.8f);

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    //Start End
                    Color col = Color.Lerp(betweenOrange, Color.Orange, 1f - Easings.easeInCubic(progress)) * progress;

                    float size1 = progress * projectile.scale;
                    float size2 = (0.5f + (0.5f * progress)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.25f,
                        previousRotations[i], TexOrigin, size2 * overallScale, SpriteEffects.None);

                    if (i > 1)
                    {
                        float middleProg = (float)(i - 1) / previousPostions.Count;

                        float size3 = (0.5f + (0.5f * progress));
                        Vector2 vec2Scale = new Vector2(3f, 0.75f * size3) * overallScale * projectile.scale * 0.5f;
                        Main.EntitySpriteDraw(flare, AfterImagePos, null, betweenOrange with { A = 0 } * 0.5f * middleProg,
                            previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);


                        Vector2 randPos = Main.rand.NextVector2Circular(10f, 10f);// * middleProg;

                        Main.EntitySpriteDraw(flare, AfterImagePos + randPos, null, betweenOrange with { A = 0 } * 0.2f * middleProg,
                            previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                    }
                }
            }
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
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }
    public class ArrowTest : ModProjectile
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

            Projectile.timeLeft = 25000;
        }

        int timer = 0;
        public override void AI()
        {
            // Apply gravity after a quarter of a second
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 15f)
            {
                Projectile.ai[0] = 15f;
                Projectile.velocity.Y += 0.1f;
            }

            // The projectile is rotated to face the direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Cap downward velocity
            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }
            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow1 = CommonTextures.Flare.Value;

            Main.spriteBatch.Draw(glow1, Projectile.Center - Main.screenPosition, null, Color.White * 1f, Projectile.rotation, glow1.Size() / 2f, 1f, SpriteEffects.None, 0f);
            return false;
        }

    }

}
