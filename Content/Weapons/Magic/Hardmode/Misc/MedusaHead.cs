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
using System.Runtime.CompilerServices;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class MedusaHeadHeldProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MedusaHead) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.MedusaHeadToggle;
        }

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


            float timeForPopInAnim = 25;
            float animProgress = Math.Clamp((timer + 12) / timeForPopInAnim, 0f, 1f);

            projectile.scale = 0.5f + MathHelper.Lerp(0f, 0.5f, Easings.easeInOutBack(animProgress, 1f, 3f));
            drawAlpha = 1f;

            timer++;

            return base.PreAI(projectile);
        }

        float drawAlpha = 0f;
        float drawScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects se = Main.player[projectile.owner].direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = projectile.scale * progress;

                    Color col = Color.Gold * progress * projectile.Opacity * drawAlpha;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.15f,
                            previousRotations[i], TexOrigin, size, se);

                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = projectile.ai[0] > 0 ? 1.25f : 2.15f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.Gold with { A = 0 } * drawAlpha * 1.25f, projectile.rotation, TexOrigin, projectile.scale * 1f, se);
            }


            //We MUST return true here because otherwise the rays dont ever run PreDraw for some reason
            return true;
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
            {
                DrawOrb(projectile, false);
            });
            DrawOrb(projectile, true); 
        }

        public void DrawOrb(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            //(Actively shooting rays)
            if (projectile.ai[0] > 0)
            {
                //ai[0] becomes 60 once rays are shot and goes to 0 (ai[0]--)
                //Utils.GetLerpValue give the progress between to and from (ie t = 50, from = 0, to = 100 gives 0.5 because 50 is halfway to 100)
                
                float progress = Utils.GetLerpValue(20f, 40f, projectile.ai[0], clamped: true);
                float progress2 = 1f - Utils.GetLerpValue(40f, 60f, projectile.ai[0], clamped: true) * 0.5f;


                Texture2D orb = CommonTextures.flare_12.Value;
                Vector2 originPoint = projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[projectile.owner].gfxOffY);

                Color col1 = Color.LightGoldenrodYellow * 0.75f;
                Color col2 = Color.Gold * 0.525f;
                Color col3 = Color.Orange * 0.375f;

                float scale1 = 0.85f;
                float scale2 = 1.6f;
                float scale3 = 2.5f;

                float scale = 0.15f * progress * progress2;

                float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
                float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

                Main.EntitySpriteDraw(orb, originPoint, null, col1 with { A = 0 } * progress, (float)Main.timeForVisualEffects * 0.05f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
                Main.EntitySpriteDraw(orb, originPoint, null, col2 with { A = 0 } * progress, (float)Main.timeForVisualEffects * 0.02f, orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
                Main.EntitySpriteDraw(orb, originPoint, null, col3 with { A = 0 } * progress, (float)Main.timeForVisualEffects * -0.01f, orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);
            }
        }


    }

    public class MedusaRayShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.MedusaHeadRay && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.MedusaHeadToggle;
        }

        float overallWidth = 0.05f;

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                storedVelocity = projectile.velocity;

            projectile.velocity = Vector2.Zero;
            

            overallWidth = Math.Clamp(MathHelper.Lerp(overallWidth, 1.15f, 0.05f), 0f, 1f);

            if (timer > 5 && timer % 4 == 0)
            {
                Vector2 startPos = projectile.Center + (storedVelocity * Main.rand.NextFloat(0f, 0.25f));
                Vector2 posOffset = new Vector2(0f, Main.rand.NextFloat(-10f, 10)).RotatedBy(storedVelocity.ToRotation());

                Vector2 vel = storedVelocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4, 7);

                Dust d = Dust.NewDustPerfect(startPos + posOffset, ModContent.DustType<LineSpark>(), vel * 2.5f, newColor: Color.Goldenrod * 0.5f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.25f);
                d.noLight = false;

                d.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.83f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 3 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                    1f, 0.5f, shouldFadeColor: false);
            }

            timer++;
            return base.PreAI(projectile);
        }

        Vector2 storedVelocity = Vector2.Zero;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (overallWidth == 0)
                return false;
            
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
            {
                DrawRay(projectile, false);
            });

            return false;
        }

        public void DrawRay(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            float easedWidth = Easings.easeInOutBack(overallWidth, 0f, 2f);

            Texture2D beam = CommonTextures.Medusa_Gray.Value;

            Vector2 startPosition = projectile.Center + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Vector2 goalPosition = startPosition + storedVelocity;
            Vector2 drawPosition = startPosition - Main.screenPosition;

            float rot = storedVelocity.ToRotation();
            Vector2 origin = new Vector2(0f, beam.Height / 2f);

            float distance = storedVelocity.Length();
            float XScale = (distance / (float)beam.Width) * projectile.scale * 1.15f; //1.5 for half-flare
            float YScale = easedWidth * projectile.scale;

            Vector2 scale = new Vector2(XScale, YScale);
            Vector2 scale2 = new Vector2(XScale, YScale * 0.65f);
            Vector2 scale3 = new Vector2(XScale, YScale * 0.25f);
            Vector2 scale4 = new Vector2(XScale, YScale * 3f);


            Main.EntitySpriteDraw(beam, drawPosition, null, Color.Gold with { A = 0 } * 0.15f, rot, origin, scale4, SpriteEffects.None);

            Main.EntitySpriteDraw(beam, drawPosition + Main.rand.NextVector2Circular(5f, 5f), null, Color.Orange with { A = 0 } * 0.5f, rot, origin, scale, SpriteEffects.None);
            Main.EntitySpriteDraw(beam, drawPosition + Main.rand.NextVector2Circular(3f, 3f), null, Color.LightGoldenrodYellow with { A = 0 } * 0.75f, rot, origin, scale2, SpriteEffects.None);
            Main.EntitySpriteDraw(beam, drawPosition + Main.rand.NextVector2Circular(1.5f, 1.5f), null, Color.White with { A = 0 }, rot, origin, scale3, SpriteEffects.None);

        }
    }

}
