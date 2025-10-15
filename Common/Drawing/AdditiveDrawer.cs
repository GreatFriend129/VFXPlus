using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using System;
using Terraria.Graphics.Effects;
using System.Linq;
using System.Threading;
using ReLogic.Content;
using VFXPlus.Common.Interfaces;
using VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns;
using VFXPlus.Content.Weapons.Magic.PreHardmode.GemStaves;


namespace VFXPlus.Common.Drawing
{
    //Based off SLRs IDrawAdditive which I think is based off Spirit's IDrawAdditive
    class AdditiveDrawer : IOrderedLoadable
    {
        public float Priority => 1;

        public void Load()
        {
            //Never load shit on dedicated servers
            if (Main.dedServ)
                return;

            On_Main.DrawProjectiles += DrawAdditiveUnder;
            On_Main.DrawDust += DrawAdditive;
        }

        public void Unload() 
        {
            On_Main.DrawProjectiles -= DrawAdditiveUnder;
            On_Main.DrawDust -= DrawAdditive;
        }

        private void DrawAdditive(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                //Layed if statements are more optimal I think because the first layer filters out more
                //Idk if C# gives up if the first part of an AND statement is false 
                //if (p.ModProjectile is IDrawAdditive) { }
                //    if (p.active)


                if (p.active)
                {
                    if (p.ModProjectile is IDrawAdditive)
                        (p.ModProjectile as IDrawAdditive).DrawAdditive(Main.spriteBatch);

                    ///-----------
                    //Ugly and slow but I don't know a faster way to do this rn
                    //Only check class projectiles to filter most projectiles out
                    if (p.DamageType == DamageClass.Ranged)
                    {
                        switch (p.type)
                        {
                            /*
                            case ProjectileID.Bullet:
                                if (p.TryGetGlobalProjectile(out MusketBallProjOverride globalMB))
                                    globalMB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.SilverBullet:
                                if (p.TryGetGlobalProjectile(out SilverBulletProjOverride globalSB))
                                    globalSB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.MeteorShot:
                                if (p.TryGetGlobalProjectile(out MeteorBulletProjOverride globalMeB))
                                    globalMeB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.PartyBullet:
                                if (p.TryGetGlobalProjectile(out PartyBulletProjOverride globalPB))
                                    globalPB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.GoldenBullet:
                                if (p.TryGetGlobalProjectile(out GoldenBulletProjOverride globalGB))
                                    globalGB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.ExplosiveBullet:
                                if (p.TryGetGlobalProjectile(out ExplodingBulletProjOverride globalEB))
                                    globalEB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.BulletHighVelocity:
                                if (p.TryGetGlobalProjectile(out HighVelocityBulletProjOverride globalHVB))
                                    globalHVB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.CrystalBullet:
                                if (p.TryGetGlobalProjectile(out CrystalBulletProjOverride globalCB))
                                    globalCB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.CursedBullet:
                                if (p.TryGetGlobalProjectile(out CursedBulletProjOverride globalCurB))
                                    globalCurB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.IchorBullet:
                                if (p.TryGetGlobalProjectile(out IchorBulletProjOverride globalIB))
                                    globalIB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.ChlorophyteBullet:
                                if (p.TryGetGlobalProjectile(out ChlorophyteBullet globalChB))
                                    globalChB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.NanoBullet:
                                if (p.TryGetGlobalProjectile(out NanoBulletProjOverride globalNB))
                                    globalNB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.VenomBullet:
                                if (p.TryGetGlobalProjectile(out VenomBulletProjOverride globalVB))
                                    globalVB.DrawAdditive(Main.spriteBatch);
                                break;
                            case ProjectileID.MoonlordBullet:
                                if (p.TryGetGlobalProjectile(out LuminiteBulletProjOverride globalLB))
                                    globalLB.DrawAdditive(Main.spriteBatch);
                                break;
                            */
                        }

                    }
                    else if (p.DamageType == DamageClass.Magic)
                    {
                        switch (p.type)
                        {
                        }
                    }
                    //
                }
            }

            Main.spriteBatch.End();

        }

        private void DrawAdditiveUnder(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                //Layed if statements are more optimal I think because the first layer filters out more
                //Idk if C# gives up if the first part of an AND statement is false 
                //if (p.ModProjectile is IDrawAdditive) { }
                //    if (p.active)


                if (p.active)
                {
                    if (p.ModProjectile is IDrawAdditive)
                        (p.ModProjectile as IDrawAdditive).DrawAdditive(Main.spriteBatch);

                    ///-----------
                    //Ugly and slow but I don't know a faster way to do this rn
                    //Only check class projectiles to filter most projectiles out
                    if (p.DamageType == DamageClass.Magic)
                    {
                        switch (p.type)
                        {
                            //case ProjectileID.DiamondBolt:
                            //    if (p.TryGetGlobalProjectile(out DiamondStaffShotOverride globalDS))
                            //        globalDS.DrawAdditive(Main.spriteBatch);
                            //    break;
                        }
                    }
                    //
                }
            }

            Main.spriteBatch.End();
        }

    }
}
