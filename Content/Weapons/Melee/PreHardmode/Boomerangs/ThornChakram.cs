using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Dusts;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Boomerangs
{
    public class ThornChakramProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public List<float> previousRotations = new List<float>();

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ThornChakram);
        }

        public List<Vector2> previousPositions = new List<Vector2>();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14; 
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);


            float fadeInTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            visualRotation += 0.45f * projectile.direction * overallScale;

            if (timer % 6 == 0 && false)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f);

                Color dustCol = Main.rand.NextBool(40) ? Color.Red : Color.Lerp(Color.DodgerBlue, Color.Blue, 0.5f);

                Dust d = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PulseInOutDust>(), dustVel, 0, dustCol with { A = 100 }, Main.rand.NextFloat(0.7f, 0.9f));
                d.velocity += projectile.velocity * 0.2f;
                d.scale *= 0.7f;

                int pulseTime = Main.rand.Next(18, 22);
                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.VanillaStar, pulseTime, 0.5f, 0.5f, Pixelize: false);
            }


            timer++;
            return true;
        }

        float visualRotation = 0f;
        float overallAlpha = 1f;
        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawSpinningUnder(projectile, false); 
            });
            DrawSpinningUnder(projectile, true);


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);

            return false;

        }

        public void DrawSpinningUnder(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D trailTex = CommonTextures.SoulSpikePMA.Value;
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 spikeScale = new Vector2(1f, 1.75f * progress) * progress;
                Color spikeCol = Color.ForestGreen * 1f * overallAlpha * Easings.easeInQuad(progress);

                Main.EntitySpriteDraw(trailTex, previousPositions[i] - Main.screenPosition, null, spikeCol with { A = 100 } * overallAlpha * progress,
                        previousRotations[i], trailTex.Size() / 2f, projectile.scale * overallScale * spikeScale, SpriteEffects.None);
            }

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.DarkGreen with { A = 0 } * 0.2f, 0f, Orb.Size() / 2f, 0.5f * projectile.scale * overallScale, SpriteEffects.None);

            //Ring
            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingB").Value;

            float ringRot = (float)Main.timeForVisualEffects * 0.6f * projectile.direction;

            float ringScale = 0.1f * projectile.scale * overallScale; 
            float ringAlpha = overallAlpha * 1f;

            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.ForestGreen with { A = 50 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
            Main.EntitySpriteDraw(RingTex, drawPos, null, new Color(0, 120, 0) with { A = 50 } * ringAlpha, ringRot + MathHelper.PiOver4, RingTex.Size() / 2f, ringScale * 1.1f, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
