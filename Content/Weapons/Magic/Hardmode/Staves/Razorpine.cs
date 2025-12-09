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
using Terraria.Utilities.Terraria.Utilities;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    public class RazorpineShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PineNeedleFriendly) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.RazorpineToggle;
        }

        int vfxProjIndex = -1;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && Main.myPlayer == projectile.owner)
            {
                int a = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<RazorpineVFX>(), 0, 0, projectile.owner);
                vfxProjIndex = a;
            }

            if (timer % 10 == 0 && Main.rand.NextBool())
            {

                Dust p = Dust.NewDustPerfect(projectile.Center, DustID.Everscream,
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.White, Scale: Main.rand.NextFloat(0.9f, 1.1f) * 1f);

                p.noGravity = true;
                p.velocity += projectile.velocity * 0.2f;
            }

            timer++;
            return base.PreAI(projectile);
        }

        public override void PostAI(Projectile projectile)
        {
            if (vfxProjIndex == -1)
                return;

            Projectile myVFX = Main.projectile[vfxProjIndex];

            myVFX.Center = projectile.Center;
            myVFX.rotation = projectile.rotation;
            myVFX.scale = projectile.scale;
            myVFX.alpha = projectile.alpha;

            if (myVFX.ModProjectile is RazorpineVFX rvfx)
            {
                int trailCount = 12;
                rvfx.previousRotations.Add(projectile.rotation);
                rvfx.previousPositions.Add(projectile.Center);

                if (rvfx.previousRotations.Count > trailCount)
                    rvfx.previousRotations.RemoveAt(0);

                if (rvfx.previousPositions.Count > trailCount)
                    rvfx.previousPositions.RemoveAt(0);
            }

        }


        public override bool PreDraw(Projectile projectile, ref Color lightColor) => false;
        

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (vfxProjIndex != -1)
            {
                (Main.projectile[vfxProjIndex].ModProjectile as RazorpineVFX).isAttached = false;
                (Main.projectile[vfxProjIndex].ModProjectile as RazorpineVFX).stuckInPower = 1f;
                (Main.projectile[vfxProjIndex].ModProjectile as RazorpineVFX).justHitPower = 1.65f;

                Main.projectile[vfxProjIndex].Center += projectile.velocity;
            }

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (vfxProjIndex != -1)
            {
                (Main.projectile[vfxProjIndex].ModProjectile as RazorpineVFX).stuckInNPC = true;
                (Main.projectile[vfxProjIndex].ModProjectile as RazorpineVFX).npcWeAreStuckIn = target.whoAmI;
                (Main.projectile[vfxProjIndex].ModProjectile as RazorpineVFX).relativePosition = projectile.Center - target.Center;
            }


            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

    //We want the vfx to extend past the lifetime of the projectile, so we duct tape a new projectile to the razorpine shot that handles vfx
    public class RazorpineVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        //Safety Checks
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2400;
        }

        public bool isAttached = true;

        public bool stuckInNPC = false;
        public int npcWeAreStuckIn = -1;
        public Vector2 relativePosition = Vector2.Zero;



        public float stuckInPower = 0f;
        int stuckInTimer = 0;

        float fadeInScale = 0f;
        int timer = 0;
        public override void AI()
        {
            float fadeInProg = Math.Clamp(timer / 15f, 0f, 1f);
            fadeInScale = Easings.easeOutCubic(fadeInProg);


            stuckInPower = MathHelper.Clamp(stuckInPower - 0.06f, 0f, 1f); //0.06
            justHitPower = Math.Clamp(MathHelper.Lerp(justHitPower, 0.15f, 0.13f), 1f, 1.15f);

            if (!isAttached && stuckInTimer > 12)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.06f), 0f, 1f);

            if (overallAlpha == 0f)
                Projectile.active = false;

            if (!isAttached)
            {
                if (previousPositions.Count > 0)
                    previousPositions.RemoveAt(0);
                if (previousRotations.Count > 0)
                    previousRotations.RemoveAt(0);
            }

            if (stuckInNPC)
            {
                if (Main.npc[npcWeAreStuckIn].active == true)
                    Projectile.Center = Main.npc[npcWeAreStuckIn].Center + relativePosition;
                else
                    Projectile.active = false;
            }


            if (!isAttached)
                stuckInTimer++;
            timer++;
        }

        public float justHitPower = 0f;

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[ProjectileID.PineNeedleFriendly].Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;// + Main.rand.NextVector2Circular(3f, 3f) * stuckInPower;
            Vector2 TexOrigin = new Vector2(vanillaTex.Width / 2f, 0f); //vanillaTex.Size() / 2f; //

            Vector2 originOffset = new Vector2(vanillaTex.Height / 2f, 0f).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
            drawPos += originOffset;

            Vector2 vec2Scale = new Vector2(0.25f + (fadeInScale * 0.75f * overallAlpha), 1f) * Projectile.scale * overallScale * justHitPower;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 vec2Scale2 = new Vector2(progress, 1f) * Projectile.scale;

                Color colw = Color.DarkGreen * Easings.easeOutSine(progress) * overallAlpha;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, colw with { A = 0 } * 0.75f,
                        previousRotations[i], TexOrigin, vec2Scale2 * overallScale, SpriteEffects.None);
            }

            float rotBonus = MathF.Sin(stuckInPower * MathHelper.TwoPi * 2f) * 0.5f * Easings.easeInCirc(stuckInPower); //0.4

            int layers = isAttached ? 6 : 5;
            for (int i = 0; i < layers; i++)
            {
                Color toUse = Color.Lerp(Color.Green, Color.Green, stuckInPower);

                float opacitySquared = Projectile.Opacity * Projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), null,
                    toUse with { A = 0 } * 1f * opacitySquared * overallAlpha, Projectile.rotation + rotBonus, TexOrigin, vec2Scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * Projectile.Opacity * overallAlpha * 1f, Projectile.rotation + rotBonus, TexOrigin, vec2Scale, SpriteEffects.None);

            return false;
        }

    }
}
