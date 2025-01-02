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
using Terraria.Utilities;
using VFXPlus.Common.Drawing;
using Terraria.Graphics;
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class NebulaArcanum : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.NebulaArcanum);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_1") with { Volume = 1f, Pitch = -1f, PitchVariance = .33f, };
            //SoundEngine.PlaySound(style, player.Center);


            return true;
        }

    }
    public class NebulaArcanumMainShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanum);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;
            
            starPower = Math.Clamp(MathHelper.Lerp(starPower, 1.25f, 0.04f), 0f, 1f);
            outerAlpha = Math.Clamp(MathHelper.Lerp(outerAlpha, 1.25f, 0.04f), 0f, 1f);

            float timeForPopInAnim = 37; //37
            float animProgress = Math.Clamp((timer + 13) / timeForPopInAnim, 0f, 1f);

            drawScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2.25f)) * 1f;


            timer++;

            return base.PreAI(projectile);
        }

        float drawRotation = 0f;
        float drawScale = 0f;
        float outerAlpha = 0f;
        float starPower = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                UniqueCircularOrbitBehaviorThing(projectile, 30); //40
            });

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Star
            //Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Flare/flare_1black").Value;
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Flare/star_01").Value;
            Texture2D bloom = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            float dir = projectile.velocity.X > 0 ? 1 : -1;

            float starRotation = projectile.velocity.ToRotation() + MathHelper.Lerp(0.75f, MathHelper.Pi * 2f * dir, Easings.easeInOutQuad(starPower));
            float starScale = Easings.easeOutCirc(1f - starPower) * projectile.scale * 0.45f;

            if (starPower != 1f)
            {
                //Color inBetween = Color.Lerp(Color.);
                Main.EntitySpriteDraw(star, drawPos, null, Color.Purple with { A = 0 } * starPower * 2.1f, starRotation, star.Size() / 2f, starScale * drawScale, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.White with { A = 0 } * starPower * 2.1f, starRotation, star.Size() / 2f, starScale * 0.65f * drawScale, SpriteEffects.None);

                //Main.EntitySpriteDraw(star, drawPos, null, Color.DeepPink with { A = 0 } * drawScale * 2f, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScale, SpriteEffects.None);
                //Main.EntitySpriteDraw(star, drawPos, null, Color.White with { A = 0 } * drawScale * 2f, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScale * 0.5f, SpriteEffects.None);
            }

            float sineScale1 = 1f + MathF.Sin(timer * 0.09f) * 0.15f;

            Main.EntitySpriteDraw(bloom, drawPos, null, Color.Purple with { A = 0 } * starPower * 0.3f, starRotation, bloom.Size() / 2f, 1.1f * starPower * projectile.scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(bloom, drawPos, null, Color.MediumPurple with { A = 0 } * 0.35f * starPower, starRotation, bloom.Size() / 2f, 0.8f * starPower * projectile.scale * sineScale1, SpriteEffects.None);


            DrawVanillaSwirl(projectile);

            return false;
        }

        //Draws the vanilla arcanum portal thing (with minor tweaks like growing scale
        public void DrawVanillaSwirl(Projectile projectile)
        {

            float dir = projectile.velocity.X > 0 ? 1f : -1f;
            SpriteEffects ef1 = dir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects ef2 = dir == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;



            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);
            Color purple3 = new Color(121, 7, 179);
            //FFWind orb
            if (true)
            {
                Texture2D Swirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirl").Value; //FireSpot goes kinda crazy| same with PixelSwirl

                Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);

                float rot = projectile.rotation;
                Vector2 origin = Swirl.Size() / 2f;

                float sinOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.2f;
                float cosOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.077f) * 0.1f;

                float startScale = sinOff * drawScale;
                float endScale = 7f * cosOff * drawScale;

                float scale = 1f;

                Main.EntitySpriteDraw(Swirl, drawPos, null, Color.Black * 0.3f * outerAlpha, (float)Main.timeForVisualEffects * 0.11f * dir, origin, endScale * 0.3f, ef1);

                for (int i = 6; i < 12; i++) //18
                {
                    float prog = 1f - ((float)i / 16f);

                    //End Color <--> Start color
                    Color between = Color.Lerp(Color.Black, Color.Purple, 0.7f);
                    Color col = Color.Lerp(purple * 1f, purple * 1.5f, prog);

                    float alpha = Easings.easeOutQuad(prog);// prog;// prog;

                    float newRot = (float)Main.timeForVisualEffects * 0.025f * scale * dir;

                    float newScale = MathHelper.Lerp(endScale, startScale, Easings.easeOutCubic(prog));

                    Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha * 0.5f * outerAlpha, newRot, origin, newScale * 1.25f, ef1);

                    //8
                    if (i >= 8)
                        scale = scale * 1.25f;
                    else
                        scale = scale * 1.15f;
                }

            }

            Color projectileColor = Lighting.GetColor((int)((double)projectile.position.X + (double)projectile.width * 0.5) / 16, (int)(((double)projectile.position.Y + (double)projectile.height * 0.5) / 16.0));
            Projectile proj = projectile;

            Vector2 vector161 = proj.position + new Vector2(proj.width, proj.height) / 2f + Vector2.UnitY * proj.gfxOffY - Main.screenPosition;
            Texture2D value180 = TextureAssets.Projectile[617].Value; //641 = lunarportal
            Color color155 = proj.GetAlpha(projectileColor);
            Vector2 origin21 = new Vector2(value180.Width, value180.Height) / 2f;
            float num314 = proj.rotation;

            Color color158 = color155 * 0.8f;
            color158.A /= 2;
            Color color159 = Color.Lerp(color155, Color.Black, 0.5f);
            color159.A = color155.A;
            float num317 = 0.95f + (proj.rotation * 0.75f).ToRotationVector2().Y * 0.1f;
            color159 *= num317;
            float scale22 = 0.6f + proj.scale * 0.6f * num317;
            Texture2D value183 = TextureAssets.Extra[50].Value;
            Vector2 origin24 = value183.Size() / 2f;

            float scaleA = scale22 * drawScale;
            float scaleB = proj.scale * drawScale;

            //New Black
            //Main.EntitySpriteDraw(value183, vector161, null, Color.Purple * 0.4f, (0f - num314 * dir + 0.35f) * 1.4f, origin24, scaleA * 1.4f, ef1);
            //Main.EntitySpriteDraw(value183, vector161, null, Color.Purple * 0.15f, (0f - num314 * dir + 0.35f) * 1.8f, origin24, scaleA * 1.8f, ef1);

            Main.EntitySpriteDraw(value183, vector161, null, color159, 0f - num314 * dir + 0.35f, origin24, scaleA, ef1);
            Main.EntitySpriteDraw(value183, vector161, null, color155, 0f - num314 * dir, origin24, scaleB, ef1);
            Main.EntitySpriteDraw(value180, vector161, null, color158, (0f - num314) * 0.7f * dir, origin21, scaleB, ef1);
            Main.EntitySpriteDraw(value180, vector161, null, color158, (0f - num314) * -0.7f * dir, origin21, scaleB, ef2);
            
            Main.EntitySpriteDraw(value183, vector161, null, color155 with { A = 0 } * 0.8f, num314 * 0.5f * dir, origin24, scaleB * 0.9f, ef2);
            color155.A = 0;
        }

        public void UniqueCircularOrbitBehaviorThing(Projectile projectile, int count)
        {
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Twinkle").Value;

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82); 
            Color purple3 = new Color(121, 7, 179);


            float dir = projectile.spriteDirection;

            //String seed = ("I LOVE DOING CRIME" + timer);

            FastRandom r = new(Main.player[projectile.owner].name.GetHashCode());
            float speedTime = (Main.GlobalTimeWrappedHourly * 0.55f) + timer * 0.002f;

            float minRange = 25f * starPower; //55
            float maxRange = 85f * starPower; //160
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = 0f;
                if (r.NextFloat() < 0.25f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 1, frameY: r.Next(0));
                    scale = new Vector2(0.5f, 0.5f) * 0.35f;
                    //rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 1.8f, 4f); //0.8, 4f
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed * dir;

                Vector2 drawPosition = projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance * scaleWave;

                float randLerp = Main.rand.NextFloat(0.15f, 1f);

                float indexProgress = (float)(i) / (float)(count - 1);

                Color purp = Color.Lerp(purple3 * 1.75f, darkPurple * 1.25f, indexProgress) * projectile.Opacity * drawScale * drawScale * drawScale * outerAlpha;

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, purp with { A = 0 } * 2f, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White with { A = 0 } * drawScale * drawScale * drawScale, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 1.7f, SpriteEffects.None);

            }
        }
        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);
            Color purple3 = new Color(121, 7, 179);

            for (int i = 0; i < 25; i++)
            {
                float prog = (float)i / 25f;

                float proggg = Main.rand.NextFloat();
                Color col = Color.Lerp(Color.Purple * 2f, Color.Black * 0.75f, proggg);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 3f) * 2.75f,
                    newColor: col * prog, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 1f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(15, 25), 0.93f, 0.01f, 0.9f); //12 28

            }

            for (int i = 0; i < 17 + Main.rand.Next(0, 6); i++)
            {
                Color col = Main.rand.NextBool() ? Color.Purple * 2f : Color.Purple * 2f;


                float velMult = Main.rand.NextFloat(3f, 9f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0,
                    newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));

                dust.scale *= 1.75f;

                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }

            for (int i = 0; i < 7 + Main.rand.Next(0, 5); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 2f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: purple3 * 2f, Scale: Main.rand.NextFloat(0.65f, 0.75f) * 0.65f);

                //dust.velocity += projectile.velocity * 0.2f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            Dust softGlow = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Purple * 2f, Scale: 0.35f);
            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.91f, sizeChangeSpeed: 0.92f, timeToKill: 150,
                overallAlpha: 0.18f, DrawWhiteCore: true, 1f, 1f);

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.6f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: purple3 * 1.5f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: purple3 * 1.5f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            #region VanillaKill
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = 176);
            projectile.Center = projectile.position;
            projectile.maxPenetrate = -1;
            projectile.penetrate = -1;
            projectile.Damage();
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            for (int num167 = 0; num167 < 4; num167++)
            {
                //int num168 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 240, 0f, 0f, 100, default(Color), 1.5f);
                //Main.dust[num168].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
            }
            for (int num169 = 0; num169 < 30; num169++)
            {
                //int num170 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 62, 0f, 0f, 200, default(Color), 3.7f);
                //Main.dust[num170].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
                //Main.dust[num170].noGravity = true;
                //Dust dust83 = Main.dust[num170];
                //Dust dust3 = dust83;
                //dust3.velocity *= 3f;
                //num170 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 90, 0f, 0f, 100, default(Color), 1.5f);
                //Main.dust[num170].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
                //dust83 = Main.dust[num170];
                //dust3 = dust83;
                //dust3.velocity *= 2f;
                //Main.dust[num170].noGravity = true;
                //Main.dust[num170].fadeIn = 1f;
                //Main.dust[num170].color = Color.Crimson * 0.5f;
            }
            for (int num171 = 0; num171 < 10; num171++)
            {
                //int num172 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 62, 0f, 0f, 0, default(Color), 2.7f);
                //Main.dust[num172].position = projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(projectile.velocity.ToRotation()) * projectile.width / 2f;
                //Main.dust[num172].noGravity = true;
                //Dust dust84 = Main.dust[num172];
                //Dust dust3 = dust84;
                //dust3.velocity *= 3f;
            }
            for (int num173 = 0; num173 < 10; num173++)
            {
                //int num174 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 240, 0f, 0f, 0, default(Color), 1.5f);
                //Main.dust[num174].position = projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(projectile.velocity.ToRotation()) * projectile.width / 2f;
                //Main.dust[num174].noGravity = true;
                //Dust dust85 = Main.dust[num174];
                //Dust dust3 = dust85;
                //dust3.velocity *= 3f;
            }
            for (int num175 = 0; num175 < 2; num175++)
            {
                //int num176 = Gore.NewGore(null, projectile.position + new Vector2((float)(projectile.width * Main.rand.Next(100)) / 100f, (float)(projectile.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64));
                //Main.gore[num176].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
                //Gore gore8 = Main.gore[num176];
                //Gore gore3 = gore8;
                //gore3.velocity *= 0.3f;
                //Main.gore[num176].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                //Main.gore[num176].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
            }
            if (Main.myPlayer == projectile.owner)
            {
                for (int num177 = 0; num177 < 1000; num177++)
                {
                    if (Main.projectile[num177].active && Main.projectile[num177].type == 618 && Main.projectile[num177].ai[1] == (float)projectile.whoAmI)
                    {
                        Main.projectile[num177].Kill();
                    }
                }
                int num178 = Main.rand.Next(5, 9);
                int num179 = Main.rand.Next(5, 9);
                int num180 = Utils.SelectRandom<int>(Main.rand, 86, 90);
                int num181 = ((num180 == 86) ? 90 : 86);
                for (int num182 = 0; num182 < num178; num182++)
                {
                    Vector2 vector28 = projectile.Center + Utils.RandomVector2(Main.rand, -30f, 30f);
                    Vector2 vector29 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                    while (vector29.X == 0f && vector29.Y == 0f)
                    {
                        vector29 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                    }
                    vector29.Normalize();
                    if (vector29.Y > 0.2f)
                    {
                        vector29.Y *= -1f;
                    }
                    vector29 *= (float)Main.rand.Next(70, 101) * 0.1f;
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), vector28.X, vector28.Y, vector29.X, vector29.Y, 620, (int)((double)projectile.damage * 0.65), projectile.knockBack * 0.8f, projectile.owner, num180);
                }
                for (int num183 = 0; num183 < num179; num183++)
                {
                    Vector2 vector30 = projectile.Center + Utils.RandomVector2(Main.rand, -30f, 30f);
                    Vector2 vector31 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                    while (vector31.X == 0f && vector31.Y == 0f)
                    {
                        vector31 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                    }
                    vector31.Normalize();
                    if (vector31.Y > 0.4f)
                    {
                        vector31.Y *= -1f;
                    }
                    vector31 *= (float)Main.rand.Next(40, 81) * 0.1f;
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), vector30.X, vector30.Y, vector31.X, vector31.Y, 620, (int)((double)projectile.damage * 0.65), projectile.knockBack * 0.8f, projectile.owner, num181);
                }
            }
            #endregion

            return false;
            return base.PreKill(projectile, timeLeft);
        }


    }

    public class NebulaArcanumSubshotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanumSubshot);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //return true;
            
            int trailCount = 20;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            timer++;

            #region vanilla code
            int num950 = 0;
            float num951 = 0f;
            float x7 = 0f;
            float y6 = 0f;
            bool flag58 = false;
            bool flag59 = false;
            int num952 = projectile.type;
            if (num952 == 618)
            {
                num950 = 617;
                num951 = 420f;
                x7 = 0.15f;
                y6 = 0.15f;
            }
            if (flag59)
            {
                int num953 = (int)projectile.ai[1];
                if (!Main.projectile[num953].active || Main.projectile[num953].type != num950)
                {
                    projectile.Kill();
                    return false;
                }
                projectile.timeLeft = 2;
            }
            projectile.ai[0]++;
            if (!(projectile.ai[0] < num951))
            {
                return false;
            }
            bool flag60 = true;
            int num954 = (int)projectile.ai[1];
            if (Main.projectile[num954].active && Main.projectile[num954].type == num950)
            {
                if (!flag58 && Main.projectile[num954].oldPos[1] != Vector2.Zero)
                {
                    projectile.position += Main.projectile[num954].position - Main.projectile[num954].oldPos[1];
                }
                if (projectile.Center.HasNaNs())
                {
                    projectile.Kill();
                    return false;
                }
            }
            else
            {
                projectile.ai[0] = num951;
                flag60 = false;
                projectile.Kill();
            }
            if (flag60 && !flag58)
            {
                projectile.velocity += new Vector2(Math.Sign(Main.projectile[num954].Center.X - projectile.Center.X), Math.Sign(Main.projectile[num954].Center.Y - projectile.Center.Y)) * new Vector2(x7, y6);
                if (projectile.velocity.Length() > 6f)
                {
                    projectile.velocity *= 6f / projectile.velocity.Length();
                }
            }
            if (projectile.type == 618)
            {
                if (Main.rand.Next(2) == 0 && false)
                {
                    int num955 = Dust.NewDust(projectile.position, 8, 8, 86);
                    Main.dust[num955].position = projectile.Center + new Vector2(0f, 0f);
                    Main.dust[num955].velocity = projectile.velocity;
                    Main.dust[num955].noGravity = true;
                    Main.dust[num955].scale = 1.25f;
                    if (flag60)
                    {
                        Main.dust[num955].customData = Main.projectile[(int)projectile.ai[1]];
                    }
                }
                projectile.alpha = 255;
            }
            else
            {
                projectile.Kill();
            }
            #endregion

            return false;
            return base.PreAI(projectile);
        }



        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVertexTrail(true);
            });

            DrawVertexTrail(true);

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, -0f);
            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);


            float drawRot = projectile.velocity.ToRotation();
            Vector2 scale = new Vector2(1f, 1f - (0.14f * projectile.velocity.Length())) * projectile.scale * 0.5f;


            //Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, Color.Black * 0.15f, drawRot, TexOrigin, scale * 0.7f, SpriteEffects.None, 0f);

            Color col1 = Color.Purple;
            Color col2 = Color.HotPink;

            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, col2 with { A = 0 }, drawRot, TexOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, col1 with { A = 0 }, drawRot, TexOrigin, scale * 0.55f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 }, drawRot, TexOrigin, scale * 0.3f, SpriteEffects.None, 0f);

            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress * progress);
            float StripWidth(float progress) => 30f * Easings.easeOutQuad(progress);// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.08f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            vertexStrip.DrawTrail();

            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(Color.OrangeRed.ToVector3() * 10f);
            myEffect.Parameters["glowThreshold"].SetValue(0.5f);
            myEffect.Parameters["glowIntensity"].SetValue(2.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {

            base.OnKill(projectile, timeLeft);
        }
    }

    public class NebulaArcanumExplosionShardOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanumExplosionShotShard);
        }

        float drawAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            
            int trailCount = 12; //20
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            bool doubleTrail = false;
            if (doubleTrail)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPostions.Count > trailCount)
                    previousPostions.RemoveAt(0);
            }


            //Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStrong>());

            timer++;

            #region vanillaCode
            int num247 = (int)projectile.ai[0];
            projectile.ai[1] += 1f;
            float num248 = (60f - projectile.ai[1]) / 60f;
            if (projectile.ai[1] > 40f)
            {
                projectile.Kill();
            }
            projectile.velocity.Y += 0.2f;
            if (projectile.velocity.Y > 18f)
            {
                projectile.velocity.Y = 18f;
            }
            projectile.velocity.X *= 0.98f;
            for (int num249 = 2220; num249 < 2; num249++)
            {
                int num250 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num247, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1.1f);
                Main.dust[num250].position = (Main.dust[num250].position + projectile.Center) / 2f;
                Main.dust[num250].noGravity = true;
                Dust dust70 = Main.dust[num250];
                Dust dust3 = dust70;
                dust3.velocity *= 0.3f;
                dust70 = Main.dust[num250];
                dust3 = dust70;
                dust3.scale *= num248;
            }
            for (int num251 = 2220; num251 < 1; num251++)
            {
                int num252 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num247, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 0.6f);
                Main.dust[num252].position = (Main.dust[num252].position + projectile.Center * 5f) / 6f;
                Dust dust71 = Main.dust[num252];
                Dust dust3 = dust71;
                dust3.velocity *= 0.1f;
                Main.dust[num252].noGravity = true;
                Main.dust[num252].fadeIn = 0.9f * num248;
                dust71 = Main.dust[num252];
                dust3 = dust71;
                dust3.scale *= num248;
            }


            #endregion

            return false;
            return base.PreAI(projectile);
        }



        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawPixelTrail(projectile);
            });

            bool isRed = projectile.ai[0] == 90;

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);
            Color purple3 = new Color(121, 7, 179);

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);


            float drawRot = projectile.velocity.ToRotation();
            Vector2 scale = new Vector2(0.75f, 1.16f) * projectile.scale * 0.85f;


            Color col1 = isRed ? darkPurple * 2f : Color.DeepPink;
            Color col2 = isRed ? purple * 2f : Color.HotPink;

            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, col2 with { A = 0 }, drawRot, TexOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, col1 with { A = 0 }, drawRot, TexOrigin, scale * 0.55f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 }, drawRot, TexOrigin, scale * 0.4f, SpriteEffects.None, 0f);

            return false;
        }

        public void DrawPixelTrail(Projectile projectile)
        {
            Texture2D AfterImage = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            bool isRed = projectile.ai[0] == 90;


            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);
            Color purple3 = new Color(121, 7, 179);

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count - 1; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    if (progress != 1)
                    {
                        Color col;

                        if (isRed)
                            col = Color.Lerp(purple * 2f, purple3 * 2f, Easings.easeInCirc(progress));
                        else
                            col = Color.Lerp(Color.DeepPink, Color.HotPink, Easings.easeInCirc(progress));

                        float size2 = Easings.easeInSine(progress) * projectile.scale * 1f;

                        Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f * progress, 3f);

                        Vector2 newVec2 = new Vector2(1f, 0.4f) * size2;
                        Vector2 newVec22 = new Vector2(1f, 0.104f) * size2;

                        Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.Black * 0.7f * progress,
                            previousRotations[i], AfterImage.Size() / 2f, newVec2 * 0.6f, SpriteEffects.None);

                        Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, col with { A = 0 } * 1f,
                               previousRotations[i], AfterImage.Size() / 2f, newVec2 * 1f, SpriteEffects.None);

                        Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.White with { A = 0 } * 0.75f * progress,
                               previousRotations[i], AfterImage.Size() / 2f, newVec22 * 1f, SpriteEffects.None);
                    }

                }

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            bool isRed = projectile.ai[0] == 90;
            Color col = isRed ? new Color(121, 7, 179) * 2f : Color.DeepPink;

            for (int i = 0; i < Main.rand.Next(2, 5); i++)
            {

                float velMult = Main.rand.NextFloat(1.5f, 2.75f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, newColor: col, Scale: Main.rand.NextFloat(0.55f, 1f));

                if (dust.scale > 0.9f)
                    dust.velocity *= 0.5f;

                dust.scale *= 1.3f * 0.8f;

                dust.fadeIn = Main.rand.NextFloat(0.25f, 0.9f);
                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }

            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: col, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
                overallAlpha: 0.1f, DrawWhiteCore: false, 1f, 1f);

            return false;
        }
    }

    //So aparently this proj is literally a copy of Crystal Serpents main proj and is used to immediately die and spawn sub shards
    //This should never appear in "normal-play" afaik 
    public class NebulaArcanumExplosionOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaArcanumExplosionShot);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            return base.PreAI(projectile);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }
    }

}
