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
    
    public class WoodenBoomerangProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WoodenBoomerang);
        }

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

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

            for (int i = 220; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Main.EntitySpriteDraw(vanillaTex, previousPositions[i] - Main.screenPosition, null, lightColor * 0.35f * overallAlpha * Easings.easeOutQuad(progress),
                        previousRotations[i], vanillaTex.Size() / 2f, projectile.scale * overallScale * Easings.easeInSine(progress), SpriteEffects.None);
            }
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);

            return false;

        }

        public void DrawSpinningUnder(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D trailTex = CommonTextures.SoulSpike.Value;
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 spikeScale = new Vector2(1f, 1f) * progress;

                Main.EntitySpriteDraw(trailTex, previousPositions[i] - Main.screenPosition, null, Color.SaddleBrown with { A = 0 } * 0.07f * overallAlpha * progress,
                        previousRotations[i], trailTex.Size() / 2f, projectile.scale * overallScale * spikeScale, SpriteEffects.None);
            }


            //Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            //Main.EntitySpriteDraw(Orb, drawPos, null, Color.Brown * 0.2f, 0f, Orb.Size() / 2f, 1f * projectile.scale * overallScale, SpriteEffects.None);


            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/FullSlashTinyBlack").Value; //A |0.75f time|

            float ringRot = (float)Main.timeForVisualEffects * 0.6f * projectile.direction;

            float ringScale = 0.3f * projectile.scale * overallScale; //125
            float ringAlpha = overallAlpha * 0.25f;

            //Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Brown with { A = 0 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
