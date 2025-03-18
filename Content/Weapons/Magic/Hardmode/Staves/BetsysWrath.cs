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
    
    public class BetsysWrathOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ApprenticeStaffT3);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }
    public class BetsysWrathShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.ApprenticeStaffT3Shot;
        }


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14;
            
            if (timer % 2 == 0)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }


            if (timer % 2 == 0 && Main.rand.NextBool(2) && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -6f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) - projectile.velocity * 0.4f;

                Color dustCol = Color.Lerp(Color.OrangeRed, Color.Orange, 0.8f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 1f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: Color.Purple * 0.5f, Scale: dustScale);
                smoke.alpha = 2;
            }

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.1f + MathHelper.Lerp(0f, 0.9f, Easings.easeInOutBack(animProgress, 0f, 1.35f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);

            //Color lightColor = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);
            //Lighting.AddLight(projectile.position, lightColor.ToVector3() * 1.25f * overallScale);

            timer++;
            return true;
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawFireball(projectile, false);
            });

            DrawFireball(projectile, true);

            return false;
        }

        public void DrawFireball(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D FireBallPixel = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float rot = projectile.velocity.ToRotation();

            Vector2 totalScale = new Vector2(overallScale, 1f) * projectile.scale * 0.85f * 1.43f;

            Color betweenGold = Color.Lerp(Color.Gold, Color.OrangeRed, 0.5f);// Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f);

            Vector2 off = rot.ToRotationVector2() * -10f * totalScale;
            Main.EntitySpriteDraw(Glow, drawPos, null, Color.Orange with { A = 0 } * overallAlpha * 0.15f, rot + MathHelper.PiOver2, Glow.Size() / 2f, totalScale, SpriteEffects.None);


            Color outerCol = Color.Orange * 0.4f;
            for (int i = 0; i < 1; i++)
            {
                Main.EntitySpriteDraw(FireBall, drawPos + off, null, outerCol with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale, SpriteEffects.None);
            }

            #region after image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                Vector2 pos = previousPositions[i] - Main.screenPosition + off;

                float progress = (float)i / previousRotations.Count;

                Vector2 size = (1f - (progress * 0.5f)) * totalScale;

                float colVal = progress;

                //End - Start
                Color col = Color.Lerp(Color.Purple * 3f, betweenGold, Easings.easeOutSine(progress)) * progress * 0.7f;

                Vector2 size2 = (1f - (progress * 0.15f)) * totalScale;
                Main.EntitySpriteDraw(FireBallPixel, pos + Main.rand.NextVector2Circular(10f, 10f) * (1f - progress), null, col with { A = 0 } * 0.85f * overallAlpha * colVal,
                        previousRotations[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                Main.EntitySpriteDraw(FireBall, pos + Main.rand.NextVector2Circular(0f, 0f) * (1f - progress), null, col with { A = 0 } * 1.25f * overallAlpha * colVal,
                        previousRotations[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
            }
            #endregion

            Vector2 v2scale = new Vector2(1f, 1f);

            Main.EntitySpriteDraw(FireBall, drawPos + off + off + Main.rand.NextVector2Circular(2f, 2f), null, betweenGold with { A = 0 } * overallAlpha * 0.75f, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale * v2scale, SpriteEffects.None);

            Main.EntitySpriteDraw(FireBall, drawPos + off, null, Color.White with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, v2scale * totalScale * 0.5f, SpriteEffects.None);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }
    }

}
