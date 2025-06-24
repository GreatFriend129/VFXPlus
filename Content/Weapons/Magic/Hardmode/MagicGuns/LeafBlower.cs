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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class LeafBlower : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.LeafBlower) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LeafBlowerToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/NPC_Killed_55") with { Volume = .05f, Pitch = .4f, PitchVariance = .2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, position);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Custom/dd2_book_staff_cast_2") with { Volume = 0.25f, Pitch = .65f, PitchVariance = 0.2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style2, position);

            SoundStyle styleVan = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_7") with { Volume = 0.9f, Pitch = .15f, PitchVariance = .15f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(styleVan, position);

            //Dust
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Vector2 dustPos = position + velocity.SafeNormalize(Vector2.UnitX) * 25;
                Dust dp = Dust.NewDustPerfect(dustPos, DustID.JungleGrass,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 9),
                    Scale: Main.rand.NextFloat(0.90f, 1.1f) * 1f);

                dp.alpha = 100;
                dp.noGravity = true;
                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80

            }
            return true;
        }

    }
    public class LeafBlowerShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Leaf) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LeafBlowerToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 10;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 4 == 0 && Main.rand.NextBool())
            {
                Dust grass = Dust.NewDustPerfect(projectile.Center, DustID.JungleGrass, Main.rand.NextVector2Circular(2, 2), 0, Scale: 0.9f);
                grass.velocity += projectile.velocity;
                grass.noGravity = true;
                grass.alpha = 50;
            }

            timer++;
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;
                float size = (0.75f + (progress * 0.25f)) * projectile.scale;

                Color col = Color.Lerp(Color.Green, Color.DarkGreen, progress) * progress * projectile.Opacity;

                float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f,
                        previousRotations[i], TexOrigin, size2, SpriteEffects.None);

                if (i > 4)
                {
                    float middleProg = (float)(i - 4) / previousPostions.Count;
                    Vector2 vec2Scale = new Vector2(1.5f, 0.25f) * size;
                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.DarkGreen with { A = 0 } * 0.85f * middleProg,
                        previousRotations[i], TexOrigin, vec2Scale, SpriteEffects.None);
                }
            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.LawnGreen with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);
            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < previousRotations.Count; i++)
            {
                if (i % 3 == 0)
                {
                    int grass = Dust.NewDust(previousPostions[i], projectile.width, projectile.height, DustID.JungleGrass, Scale: 0.75f);
                    Main.dust[grass].velocity += projectile.velocity * 0.5f;
                    Main.dust[grass].noGravity = true;
                }
            }

            for (int j = 0; j < 4 + Main.rand.Next(1,4); j++)
            {
                int grass = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.JungleGrass, Scale: 0.75f);
            }

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Grab") with { Pitch = .85f, PitchVariance = .3f, Volume = 0.4f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/NPC_Hit_11") with { Pitch = .35f, PitchVariance = .25f, Volume = 0.55f, MaxInstances = -1};
            SoundEngine.PlaySound(style2, projectile.Center);

            return false;
        }
    }

}
