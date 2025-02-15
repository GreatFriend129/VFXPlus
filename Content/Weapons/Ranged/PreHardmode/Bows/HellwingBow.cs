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
using static tModPorter.ProgressUpdate;
using System.Runtime.Intrinsics.Arm;
using Terraria.Graphics;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Bows
{
    
    public class HellwingBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BeesKnees);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            return true;

        }

    }

    public class HellwingBatOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Hellwing);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 20;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            //Trailing Fire Dust
            if (timer % 2 == 0 && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.3f;

                Color dustCol = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 0.7f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 1f, Scale: dustScale);
                smoke.alpha = 2;
            }


            float fadeInTime = Math.Clamp((timer + 6f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.06f), 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVertexTrail(false);
            });
            DrawVertexTrail(true);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            float rot = projectile.velocity.ToRotation();
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
                Main.EntitySpriteDraw(vanillaTex, offset + drawPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)i) * 3f, sourceRectangle, Color.Orange with { A = 0 } * 2f, rot, TexOrigin,
                    projectile.scale * overallScale, SE);

            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * overallAlpha, rot, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/smoketrailsmudge").Value;
            // CommonTextures.Extra_196_Black.Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress);
            float StripWidth(float progress) => 10f * Easings.easeOutQuad(progress) * overallAlpha;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.05f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //Black UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            //vertexStrip.DrawTrail();

            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(Color.OrangeRed.ToVector3() * 5f);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f);
            myEffect.Parameters["glowIntensity"].SetValue(2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            

            return true;
        }

    }

}
