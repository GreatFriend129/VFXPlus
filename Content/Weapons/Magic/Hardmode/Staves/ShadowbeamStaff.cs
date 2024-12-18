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
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class ShadowbeamStaff : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ShadowbeamStaff);
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
    public class ShadowbeamStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ShadowBeamFriendly);
        }

        int vfxIndex = -1;
        Vector2 previousHead;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //Create shadowbeam vfx proj
            if (timer == 0)
            {
                previousHead = projectile.Center;

                int p = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<ShadowbeamStaffVFX>(), 0, 0, projectile.owner);
                vfxIndex = p;
            }

            /*
            if (this.type == 294)
            {
                this.localAI[0] += 1f;
                if (this.localAI[0] > 9f)
                {
                    for (int num379 = 0; num379 < 4; num379++)
                    {
                        Vector2 vector105 = base.position;
                        vector105 -= base.velocity * ((float)num379 * 0.25f);
                        this.alpha = 255;
                        int num380 = Dust.NewDust(vector105, 1, 1, 173);
                        Main.dust[num380].position = vector105;
                        Main.dust[num380].scale = (float)Main.rand.Next(70, 110) * 0.013f;
                        Dust dust157 = Main.dust[num380];
                        Dust dust212 = dust157;
                        dust212.velocity *= 0.2f;
                    }
                }
                return;
            }
            */

            timer++;
            return false;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //Add node and disconnect proj
            AddNewNode(projectile);

            (Main.projectile[vfxIndex].ModProjectile as ShadowbeamStaffVFX).isAttached = false;

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            //Add node
            AddNewNode(projectile);

            return base.OnTileCollide(projectile, oldVelocity);
        }

        public void AddNewNode(Projectile projectile)
        {
            if (vfxIndex == -1)
            {
                Main.NewText("vfxIndex is -1");
                return;
            }

            ShadowbeamNode sbn = new ShadowbeamNode(previousHead, projectile.Center);
            (Main.projectile[vfxIndex].ModProjectile as ShadowbeamStaffVFX).nodes.Add(sbn);
            previousHead = projectile.Center;
        }

    }

    public class ShadowbeamStaffVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            //Make sure to draw projectile even if its position is off screen
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

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

        int timer = 0;
        float true_width = 1f;
        float true_alpha = 1f;

        public bool isAttached = true;

        public override void AI()
        {

            if (!isAttached && timer > 60)
                Projectile.active = false;


            if (timer % 10 == 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    //Dust.NewDustPerfect(nodes[i].head, ModContent.DustType<GlowStrong>(), Velocity: Vector2.Zero, newColor: Color.White);
                }
            }

            timer++;
        }

        Effect myEffect = null;
        public List<ShadowbeamNode> nodes = new List<ShadowbeamNode>();
        public override bool PreDraw(ref Color lightColor)
        {
            //return false;

            //ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            //{
            //    for (int i = 0; i < nodes.Count; i++)
            //    {
            //        ShadowbeamNode node = nodes[i];
            //        DrawNodeTrail(node);
            //    }
            //});

            for (int i = 0; i < nodes.Count; i++)
            {
                ShadowbeamNode node = nodes[i];
                DrawNodeTrail(node);

                //Utils.DrawLine(Main.spriteBatch, node.head, node.tail, Color.Purple with { A = 0 }, Color.Purple with { A = 0 }, 5f);

                Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;
                Texture2D bloom = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

                Main.EntitySpriteDraw(portal, node.head - Main.screenPosition, null, Color.Purple with { A = 0 } * 2f, node.rot, portal.Size() / 2f, 1f, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, node.head - Main.screenPosition, null, Color.White with { A = 0 } * 2f, node.rot, portal.Size() / 2f, 0.5f, SpriteEffects.None);
            }

            return false;
        }

        public void DrawNodeTrail(ShadowbeamNode node)
        {
            Color purple = new Color(61, 2, 92); 
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);

            Color purple3 = new Color(121, 7, 179);
            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_06").Value; //|spark_06
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value;

            Vector2[] pos_arr = { node.tail, node.head };
            float[] rot_arr = { node.rot, node.rot };

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.0f;

            Color StripColor(float progress) => Color.White;
            float StripWidth(float progress) => 60f * true_width * sineWidthMult; //25
            float StripWidth2(float progress) => 90f * true_width * sineWidthMult;
            //^ Doing Easings.easeOutQuad(progress) * Easings.easeInQuad(progress) gives a really nice zigzag patter (or do 1f - EaseIn)

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;
           
            float dist = (node.head - node.tail).Length();
            float repValue = dist / 100f;
            myEffect.Parameters["reps"].SetValue(repValue);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.02f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(purple.ToVector3() * 4f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Color purp = new Color(215, 18, 215);

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(purp.ToVector3() * 10f);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f); //0.6
            myEffect.Parameters["glowIntensity"].SetValue(2.2f); //2.25
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }

        public void DrawNodeStar(ShadowbeamNode node, bool isFirst = false)
        {

        }
    }


    public class ShadowbeamNode
    {
        public Vector2 head;
        public Vector2 tail;
        public float rot;

        public ShadowbeamNode(Vector2 start, Vector2 end)
        {
            head = start;
            tail = end;

            rot = (end - start).ToRotation();
        }
    }

}
