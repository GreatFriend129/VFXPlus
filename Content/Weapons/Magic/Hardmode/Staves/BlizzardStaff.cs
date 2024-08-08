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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class BlizzardStaff : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BlizzardStaff);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f }; //Turn item sound off
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = .12f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Research_1") with { Pitch = .85f, PitchVariance = .2f, Volume = 0.25f };
            SoundEngine.PlaySound(style2, player.Center);

            //Dust
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<LineSpark>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 19),
                    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80

            }
            */
            return true;
        }

    }

    public class BlizzardStaffShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Blizzard);
        }

        public override void SetDefaults(Projectile entity)
        {
            base.SetDefaults(entity);
        }

        public static int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                previousRotations = new List<float>();
                previousPostions = new List<Vector2>();
            }



            //Remove last trail index if long enough
            int trailCount = 10;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            timer++;

            return base.PreAI(projectile);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            //OPTION A:
            
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_0") with { Volume = .035f, Pitch = .6f, PitchVariance = 0.5f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style, projectile.Center);

            //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_107") with { Volume = .1f, Pitch = .8f, PitchVariance = 0.3f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style2, projectile.Center);
            
            //return false;
            

            //Option B:
            
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ice_hit") with { Volume = 0.1f, Pitch = 0.15f, PitchVariance = .5f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_107") with { Volume = .1f, Pitch = .55f, PitchVariance = 0.8f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_death_1") with { Volume = 0.05f, Pitch = .85f, PitchVariance = 0.45f, MaxInstances = -1 };
            SoundEngine.PlaySound(style4, projectile.Center);

            //Main.NewText(projectile.ai[1]); <- Current proj spite (blizz staff chooses between 1 of 5 sprites for each shot)

            return false;

        }


        public static List<float> previousRotations; //Why am I doing after-images this way?
        public static List<Vector2> previousPostions; //Makes it easier to tweak the length of the trail without restarting everything (also skill issue prolly)
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPosOffset = (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 84f * projectile.scale;
            Vector2 drawPos = projectile.Center - Main.screenPosition + drawPosOffset;
            
            
            float drawRot = projectile.rotation;
            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);


            #region After-image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    Vector2 AE_drawPos = previousPostions[i] - Main.screenPosition + drawPosOffset;

                    //Main.EntitySpriteDraw(vanillaTex, AE_drawPos, sourceRectangle, Color.White with { A = 0 } * 0.9f,
                        //previousRotations[i], TexOrigin, 1f, SpriteEffects.None);

                }

            }
            #endregion

            for (int i = 0; i < 5; i++)
            {
                Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle, Color.White with { A = 0 } * 0.5f, drawRot, TexOrigin, projectile.scale, SpriteEffects.None, 0f); //0.3
            }

            Main.spriteBatch.Draw(vanillaTex, drawPos, sourceRectangle, Color.White * 0.95f, drawRot, TexOrigin, projectile.scale, SpriteEffects.None, 0f); //0.3


            /*
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Flare/CyverLaserPMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -30);

            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 Tex2Origin = Tex2.Size() / 2f;

            Color pinkToUse = Color.Lerp(Color.Purple, Color.DeepPink, 0.1f);

            Main.spriteBatch.Draw(Tex2, drawPos, null, pinkToUse with { A = 0 } * 0.5f, drawRot, Tex2Origin, projectile.scale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, pinkToUse with { A = 0 }, drawRot, Tex2Origin, projectile.scale * 0.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, Color.White with { A = 0 } * 0.7f, drawRot, Tex2Origin, projectile.scale * 0.1f, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.HotPink with { A = 0 } * 0.7f, drawRot, TexOrigin, new Vector2(2f, 0.35f * projectile.Opacity) * projectile.scale, SpriteEffects.None, 0f); //0.3
            */

            return false;

        }



    }

}
