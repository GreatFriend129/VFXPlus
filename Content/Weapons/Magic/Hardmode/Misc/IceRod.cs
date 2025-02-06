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
using Terraria.ObjectData;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{ 
    public class IceRod : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.IceRod) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.IceRodToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_43") with { Volume = 0.45f, Pitch = .25f, PitchVariance = 0.1f, MaxInstances = 1 };
            SoundEngine.PlaySound(style4, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_20") with { Volume = 0.3f, Pitch = .45f, PitchVariance = 0.15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, player.Center);

            return true;
        }

    }
    public class IceRodShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.IceBlock) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.IceRodToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 7;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            int mod2 = projectile.velocity.Length() > 0 ? 2 : 7;
            if (timer % mod2 == 0 && Main.rand.NextBool())
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.45f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: col * 1f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * 1.25f);
                d.velocity += projectile.velocity * 0.2f;

                if (mod2 == 7)
                    d.scale *= 0.75f;
            }

            
            int mod = projectile.ai[0] >= 1f ? 1 : 12;
            if (timer % mod == 0)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel, newColor: col * 1f, Scale: Main.rand.NextFloat(0.25f, 0.5f));
                d.customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.2f); //0.5f
                d.velocity += projectile.velocity * 0.2f;
            }

            justBecameTilePower = Math.Clamp(MathHelper.Lerp(justBecameTilePower, -0.5f, 0.06f), 0f, 1f);
            timer++;


            //Tile.active -> HasTile
            //Tile.type -> TileType
            #region vanillaAIholyfuckingshitwhatthefuckisthisgodhelpme
            if (projectile.velocity.X == 0f && projectile.velocity.Y == 0f)
            {
                projectile.alpha = 255;
            }
            Dust dust84;
            Dust dust212;
            if (projectile.ai[1] < 0f)
            {
                if (projectile.timeLeft > 60)
                {
                    projectile.timeLeft = 60;
                }
                if (projectile.velocity.X > 0f)
                {
                    projectile.rotation += 0.3f;
                }
                else
                {
                    projectile.rotation -= 0.3f;
                }
                int num168 = (int)(projectile.position.X / 16f) - 1;
                int num169 = (int)((projectile.position.X + (float)projectile.width) / 16f) + 2;
                int num170 = (int)(projectile.position.Y / 16f) - 1;
                int num171 = (int)((projectile.position.Y + (float)projectile.height) / 16f) + 2;
                if (num168 < 0)
                {
                    num168 = 0;
                }
                if (num169 > Main.maxTilesX)
                {
                    num169 = Main.maxTilesX;
                }
                if (num170 < 0)
                {
                    num170 = 0;
                }
                if (num171 > Main.maxTilesY)
                {
                    num171 = Main.maxTilesY;
                }
                int num172 = (int)projectile.position.X + 4;
                int num173 = (int)projectile.position.Y + 4;
                Vector2 vector94 = default(Vector2);
                for (int num175 = num168; num175 < num169; num175++)
                {
                    for (int num176 = num170; num176 < num171; num176++)
                    {
                        if (Main.tile[num175, num176] != null && Main.tile[num175, num176].HasTile && Main.tile[num175, num176].TileType != 127 && Main.tileSolid[Main.tile[num175, num176].TileType] && !Main.tileSolidTop[Main.tile[num175, num176].TileType])
                        {
                            vector94.X = num175 * 16;
                            vector94.Y = num176 * 16;
                            if ((float)(num172 + 8) > vector94.X && (float)num172 < vector94.X + 16f && (float)(num173 + 8) > vector94.Y && (float)num173 < vector94.Y + 16f)
                            {
                                projectile.Kill();
                            }
                        }
                    }
                }
                int num177 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 67);
                Main.dust[num177].noGravity = true;
                dust84 = Main.dust[num177];
                dust212 = dust84;
                dust212.velocity *= 0.3f;
                return false;
            }
            if (projectile.ai[0] < 0f)
            {
                if (projectile.ai[0] == -1f)
                {
                    for (int num178 = 0; num178 < 10; num178++)
                    {
                        int num179 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 67, 0f, 0f, 0, default(Color), 1.1f);
                        Main.dust[num179].noGravity = true;
                        dust84 = Main.dust[num179];
                        dust212 = dust84;
                        dust212.velocity *= 1.3f;
                    }
                }
                else if (Main.rand.Next(30) == 0)
                {
                    int num180 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 67, 0f, 0f, 100);
                    dust84 = Main.dust[num180];
                    dust212 = dust84;
                    dust212.velocity *= 0.2f;
                }
                int num181 = (int)projectile.position.X / 16;
                int num182 = (int)projectile.position.Y / 16;
                if (Main.tile[num181, num182] == null || !Main.tile[num181, num182].HasTile)
                {
                    projectile.Kill();
                }
                projectile.ai[0] -= 1f;
                if (projectile.ai[0] <= -900f && (Main.myPlayer == projectile.owner || Main.netMode == 2) && Main.tile[num181, num182].HasTile && Main.tile[num181, num182].TileType == 127)
                {
                    WorldGen.KillTile(num181, num182);
                    if (Main.netMode == 1)
                    {
                        NetMessage.SendData(17, -1, -1, null, 0, num181, num182);
                    }
                    projectile.Kill();
                }
                return false;
            }
            int num183 = (int)(projectile.position.X / 16f) - 1;
            int num184 = (int)((projectile.position.X + (float)projectile.width) / 16f) + 2;
            int num187 = (int)(projectile.position.Y / 16f) - 1;
            int num188 = (int)((projectile.position.Y + (float)projectile.height) / 16f) + 2;
            if (num183 < 0)
            {
                num183 = 0;
            }
            if (num184 > Main.maxTilesX)
            {
                num184 = Main.maxTilesX;
            }
            if (num187 < 0)
            {
                num187 = 0;
            }
            if (num188 > Main.maxTilesY)
            {
                num188 = Main.maxTilesY;
            }
            int num189 = (int)projectile.position.X + 4;
            int num190 = (int)projectile.position.Y + 4;
            Vector2 vector95 = default(Vector2);
            for (int num191 = num183; num191 < num184; num191++)
            {
                for (int num192 = num187; num192 < num188; num192++)
                {
                    if (Main.tile[num191, num192] != null && Main.tile[num191, num192].HasUnactuatedTile /*.nactive()*/ && Main.tile[num191, num192].TileType != 127 && Main.tileSolid[Main.tile[num191, num192].TileType] && !Main.tileSolidTop[Main.tile[num191, num192].TileType])
                    {
                        vector95.X = num191 * 16;
                        vector95.Y = num192 * 16;
                        if ((float)(num189 + 8) > vector95.X && (float)num189 < vector95.X + 16f && (float)(num190 + 8) > vector95.Y && (float)num190 < vector95.Y + 16f)
                        {
                            projectile.Kill();
                        }
                    }
                }
            }
            if (projectile.lavaWet)
            {
                projectile.Kill();
            }
            int num193 = (int)(projectile.Center.X / 16f);
            int num194 = (int)(projectile.Center.Y / 16f);
            //! Point of possible Error | commented out part is vanilla code
            if (WorldGen.InWorld(num193, num194) && Main.tile[num193, num194] != null && Main.tile[num193, num194].LiquidAmount > 0 /*&& Main.tile[num193, num194].shimmer()*/)
            {
                projectile.Kill();
            }
            if (!projectile.active)
            {
                return false;
            }
            int num195 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 67);
            Main.dust[num195].noGravity = true;
            dust84 = Main.dust[num195];
            dust212 = dust84;
            dust212.velocity *= 0.3f;
            int num196 = (int)projectile.ai[0];
            int num198 = (int)projectile.ai[1];
            if (WorldGen.InWorld(num196, num198) && WorldGen.SolidTile(num196, num198))
            {
                if (Math.Abs(projectile.velocity.X) > Math.Abs(projectile.velocity.Y))
                {
                    if (projectile.Center.Y < (float)(num198 * 16 + 8) && WorldGen.InWorld(num196, num198 - 1) && !WorldGen.SolidTile(num196, num198 - 1))
                    {
                        num198--;
                    }
                    else if (WorldGen.InWorld(num196, num198 + 1) && !WorldGen.SolidTile(num196, num198 + 1))
                    {
                        num198++;
                    }
                    else if (WorldGen.InWorld(num196, num198 - 1) && !WorldGen.SolidTile(num196, num198 - 1))
                    {
                        num198--;
                    }
                    else if (projectile.Center.X < (float)(num196 * 16 + 8) && WorldGen.InWorld(num196 - 1, num198) && !WorldGen.SolidTile(num196 - 1, num198))
                    {
                        num196--;
                    }
                    else if (WorldGen.InWorld(num196 + 1, num198) && !WorldGen.SolidTile(num196 + 1, num198))
                    {
                        num196++;
                    }
                    else if (WorldGen.InWorld(num196 - 1, num198) && !WorldGen.SolidTile(num196 - 1, num198))
                    {
                        num196--;
                    }
                }
                else if (projectile.Center.X < (float)(num196 * 16 + 8) && WorldGen.InWorld(num196 - 1, num198) && !WorldGen.SolidTile(num196 - 1, num198))
                {
                    num196--;
                }
                else if (WorldGen.InWorld(num196 + 1, num198) && !WorldGen.SolidTile(num196 + 1, num198))
                {
                    num196++;
                }
                else if (WorldGen.InWorld(num196 - 1, num198) && !WorldGen.SolidTile(num196 - 1, num198))
                {
                    num196--;
                }
                else if (projectile.Center.Y < (float)(num198 * 16 + 8) && WorldGen.InWorld(num196, num198 - 1) && !WorldGen.SolidTile(num196, num198 - 1))
                {
                    num198--;
                }
                else if (WorldGen.InWorld(num196, num198 + 1) && !WorldGen.SolidTile(num196, num198 + 1))
                {
                    num198++;
                }
                else if (WorldGen.InWorld(num196, num198 - 1) && !WorldGen.SolidTile(num196, num198 - 1))
                {
                    num198--;
                }
            }
            if (projectile.velocity.X > 0f)
            {
                projectile.rotation += 0.3f;
            }
            else
            {
                projectile.rotation -= 0.3f;
            }
            if (Main.myPlayer != projectile.owner)
            {
                return false;
            }
            int num199 = (int)((projectile.position.X + (float)(projectile.width / 2)) / 16f);
            int num200 = (int)((projectile.position.Y + (float)(projectile.height / 2)) / 16f);
            bool flag70 = false;
            if (num199 == num196 && num200 == num198)
            {
                flag70 = true;
            }
            if (((projectile.velocity.X <= 0f && num199 <= num196) || (projectile.velocity.X >= 0f && num199 >= num196)) && ((projectile.velocity.Y <= 0f && num200 <= num198) || (projectile.velocity.Y >= 0f && num200 >= num198)))
            {
                flag70 = true;
            }
            if (!flag70)
            {
                return false;
            }
            if (WorldGen.PlaceTile(num196, num198, 127, mute: true /*false*/, forced: false, projectile.owner))
            {
                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ice_hit") with { Volume = 0.2f, Pitch = 0f, PitchVariance = .5f, MaxInstances = -1 };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .2f, Pitch = .5f, PitchVariance = 0.8f, MaxInstances = -1 };
                SoundEngine.PlaySound(style2, projectile.Center);

                justBecameTilePower = 1;

                if (Main.netMode == 1)
                {
                    NetMessage.SendData(17, -1, -1, null, 1, num196, num198, 127f);
                }
                projectile.damage = 0;
                projectile.ai[0] = -1f;
                projectile.velocity *= 0f;
                projectile.alpha = 255;
                projectile.position.X = num196 * 16;
                projectile.position.Y = num198 * 16;
                projectile.netUpdate = true;
            }
            else
            {
                projectile.ai[1] = -1f;
            }
            #endregion


            return false;

        }

        float justBecameTilePower = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null && projectile.velocity.Length() > 0)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.LightSkyBlue * progress * projectile.Opacity;

                    float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.25f, //0.5f
                            previousRotations[i], TexOrigin, size2, SpriteEffects.None);

                }

            }

            for (int i = 0; i < 4; i++)
            {
                float dist = 5f;

                float sinScale = 1.05f + (float)Math.Sin(Main.timeForVisualEffects * 0.05f) * 0.2f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.08f * (projectile.velocity.X > 0 ? 1f : -1f));

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * projectile.Opacity * 0.2f, projectile.rotation, TexOrigin, projectile.scale * sinScale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);


            float jbtScale = MathHelper.Lerp(1f, 1.45f, justBecameTilePower) * projectile.scale;
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 } * justBecameTilePower * 1f, 0f, TexOrigin, jbtScale, SpriteEffects.None);


            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            
            for (int i = 0; i < 9 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);

                p.velocity += projectile.velocity * 0.05f;
            }

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_1") with { Volume = .05f, Pitch = 0.9f, PitchVariance = 0.3f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .27f, Pitch = .7f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);


            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.75f, 1.75f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel, newColor: Color.LightSkyBlue * 1f, Scale: Main.rand.NextFloat(0.25f, 0.5f));
                d.customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.65f);
            }

            return false;
        }

    }


    //This is literally just to change the sound
    public class MagicalIceBlockOverride : GlobalTile
    {
        public override bool KillSound(int i, int j, int type, bool fail)
        {
            if (type == TileID.MagicalIceBlock)
            {
                return false;
            }

            return base.KillSound(i, j, type, fail);
        }

    }

}
