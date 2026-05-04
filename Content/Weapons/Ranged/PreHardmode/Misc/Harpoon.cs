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
    public class HarpoonProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Harpoon);
        }

        bool firstPullBackFrame = true;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 14; 
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 11f) / 31f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            justHitPower = Math.Clamp(MathHelper.Lerp(justHitPower, -1.5f, 0.08f), 0f, 1f);

            if (projectile.ai[0] == 1f && firstPullBackFrame)
            {
                previousPositions.Clear();
                previousRotations.Clear();
                firstPullBackFrame = false;
            }


            timer++;
            return base.PreAI(projectile);
        }

        float justHitPower = 0f;

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 offset = new Vector2(5f, 0f) + new Vector2(0f, -1f).RotatedBy(projectile.rotation);


            Vector2 drawPos = projectile.Center - Main.screenPosition + offset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width / 2f, projectile.height / 2f);// vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            float totalScale = overallScale * projectile.scale;

            //Border
            Color borderCol = Color.Lerp(new Color(150, 150, 120), Color.Red * 2f, justHitPower);
            float scaleBoost = 1f + (justHitPower * 0.07f);
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), null,
                    borderCol with { A = 0 } * 0.75f * projectile.Opacity, projectile.rotation, TexOrigin, totalScale * scaleBoost, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, totalScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = new Vector2(vanillaTex.Width / 2f, projectile.height / 2f);

            Color thisGray = new Color(150, 150, 120);

            Vector2 offset = new Vector2(5f, 0f) + new Vector2(0f, -1f).RotatedBy(projectile.rotation);


            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(thisGray, Color.Gray, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + offset;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            int hitAnim = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<BloodHitAnim>(), 0, 0, projectile.owner);
            Main.projectile[hitAnim].scale = 0.4f;
            Main.projectile[hitAnim].rotation = projectile.velocity.ToRotation() + MathHelper.Pi;


            for (int i = 0; i < 5; i++)
            {
                float arrowVel = 4f;
                Vector2 randomStart = Main.rand.NextVector2Circular(2f, 2f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: new Color(255, 15, 15), Scale: Main.rand.NextFloat(0.45f, 0.55f));
                dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * -0.35f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
            }

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/cleaver_hit_06") with { Volume = 0.05f, Pitch = 0.35f, PitchVariance = 0.35f };
            SoundEngine.PlaySound(style2, projectile.Center);

            justHitPower = 1f;
        }
    }

}
