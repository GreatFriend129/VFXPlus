using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Boomerangs
{
    public class FlamarangProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Flamarang) && false;
        }

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 10;
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            //float fadeInAlphaTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            //overallAlpha = Easings.easeInOutHarsh(fadeInTime);

            //visualRotation = projectile.rotation;
            visualRotation += 0.45f * projectile.direction * overallScale;

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

            for (int i = 220; i < 5; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.Orange with { A = 0 }, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);
            }


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);

            return false;

        }

        public void DrawSpinningUnder(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.OrangeRed with { A = 0 } * 0.18f, 0f, Orb.Size() / 2f, 1f * projectile.scale * overallScale, SpriteEffects.None);


            Texture2D trailTex = CommonTextures.SoulSpike.Value;
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float spikeScale = 1f * progress;
                Color spikeCol = Color.Lerp(Color.Orange, Color.OrangeRed, progress);

                Main.EntitySpriteDraw(trailTex, previousPositions[i] - Main.screenPosition, null, spikeCol with { A = 0 } * 1f * overallAlpha,
                        previousRotations[i], trailTex.Size() / 2f, projectile.scale * overallScale * spikeScale, SpriteEffects.None);
            }

            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/twirl_03").Value; //A |0.75f time|

            float ringRot = (float)Main.timeForVisualEffects * 0.6f * projectile.direction;

            float ringScale = 0.08f * projectile.scale * overallScale; //125
            float ringAlpha = overallAlpha * 1f;



            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Orange with { A = 0 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.OrangeRed with { A = 0 } * ringAlpha, ringRot + MathHelper.PiOver4, RingTex.Size() / 2f, ringScale * 1.1f, SpriteEffects.None);
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
