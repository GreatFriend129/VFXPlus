using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.FeatheredFoe;
using VFXPlus.Content.Particles;
using VFXPlus.Content.VFXTest;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Guns;


namespace VFXPlus.Content
{
    public class DevPetItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<DevPetProjectile>(), ModContent.BuffType<DevPetBuff>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600);
            }
            return true;
        }
    }

    public class DevPetBuff : ModBuff
    {
        public override string Texture => "VFXPlus/Content/DevPetItem";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        { 
            // This method gets called every frame your buff is active on your player.
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<DevPetProjectile>());
        }
    }

    public class DevPetProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;

            // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
            // * It uses fluent API syntax, just like Recipe
            // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
            // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
            // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
            // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
            //ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Type], 6)
            //    .WithOffset(-10, -20f)
            //    .WithSpriteDirection(-1)
            //    .WithCode(DelegateMethods.CharacterPreview.Float);
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            
            Projectile.width = 20;
            Projectile.height = 20;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.penetrate = -1;

        }
        public override bool? CanDamage() => false;

        int timer = 0;
        bool spinning = false;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.dead && player.HasBuff(ModContent.BuffType<DevPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 2f;

            Projectile.rotation = 0f;// 0.02f;
        }

        float xScale, yScale = 1f;
        float overallAlpha = 1f;
        float overallScale = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Full = Mod.Assets.Request<Texture2D>("Content/DevPetItem").Value;
            Texture2D Bald = Mod.Assets.Request<Texture2D>("Content/DevPetBald").Value;
            Texture2D Left = Mod.Assets.Request<Texture2D>("Content/DevPetLeft").Value;
            Texture2D Right = Mod.Assets.Request<Texture2D>("Content/DevPetRight").Value;
            Texture2D Top = Mod.Assets.Request<Texture2D>("Content/DevPetTop").Value;

            overallScale = 1f;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(Bald, drawPos, null, lightColor, Projectile.rotation, Bald.Size() / 2f, overallScale, SpriteEffects.None, 0);


            //Left
            float leftRotAdd = Math.Clamp(GeneralUtilities.OscSine(-0.085f, 0.085f, 0.8f, 0f) + Projectile.velocity.X * -0.1f, -0.15f, 0.15f);
            Vector2 leftPos = drawPos + new Vector2(-6f, -3f).RotatedBy(Projectile.rotation) * overallScale;
            Vector2 leftOrigin = new Vector2(Left.Width, Left.Height);
            Main.spriteBatch.Draw(Left, leftPos, null, lightColor, Projectile.rotation + leftRotAdd, leftOrigin, overallScale, SpriteEffects.None, 0);


            //Right
            float rightRotAdd = Math.Clamp(GeneralUtilities.OscSine(-0.1f, 0.1f, 0.6f, 0.5f) + Projectile.velocity.X * -0.1f, -0.2f, 0.2f);
            Vector2 rightPos = drawPos + new Vector2(6f, -5f).RotatedBy(Projectile.rotation) * overallScale;
            Vector2 rightOrigin = new Vector2(0f, Right.Height);
            Main.spriteBatch.Draw(Right, rightPos, null, lightColor, Projectile.rotation + rightRotAdd, rightOrigin, overallScale, SpriteEffects.None, 0);


            //Top
            float topRotAdd = Math.Clamp(GeneralUtilities.OscSine(-0.1f, 0.1f, 0.5f, 0.25f) + Projectile.velocity.X * -0.05f, -0.1f, 0.1f);
            Vector2 topPos = drawPos + new Vector2(-1f, -7f).RotatedBy(Projectile.rotation) * overallScale;
            Vector2 topOrigin = new Vector2(Top.Width / 2f, Top.Height);
            Main.spriteBatch.Draw(Top, topPos, null, lightColor, Projectile.rotation + topRotAdd, topOrigin, overallScale, SpriteEffects.None, 0);

            return false;
        }
    }

    public class KarmaPanelTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

            Projectile.width = Projectile.height = 2;
        }


        int timer = 0;

        float progress = 0f;
        public override void AI()
        {

            timer++;
        }

        Effect myEffect = null;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = CommonTextures.Pixel.Value;

            float alpha = 1f;

            Color borderCol = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.15f) * alpha;
            Color innerCol = Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, 0.25f) * alpha;

            Vector2 topLeft = Projectile.Center - Main.screenPosition;

            float Width = 100f + GeneralUtilities.OscSine(-50f, 50f, 1f, 0f);
            float Height = 100f + GeneralUtilities.OscSine(-50f, 50f, 0.5f, 0f);
            float thickness = 4f;


            //Border
            Main.spriteBatch.Draw(Tex, topLeft + new Vector2(0f, -Height / 2f), null, borderCol, 0f, Tex.Size() / 2f, new Vector2(Width + thickness, thickness), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, topLeft + new Vector2(0f, Height / 2f), null, borderCol, 0f, Tex.Size() / 2f, new Vector2(Width + thickness, thickness), SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex, topLeft + new Vector2(-Width / 2f, 0f), null, borderCol, MathHelper.PiOver2, Tex.Size() / 2f, new Vector2(Height + thickness, thickness), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, topLeft + new Vector2(Width / 2f, 0f), null, borderCol, MathHelper.PiOver2, Tex.Size() / 2f, new Vector2(Height + thickness, thickness), SpriteEffects.None, 0f);

            //Inner
            Main.spriteBatch.Draw(Tex, topLeft, null, innerCol, 0f, Tex.Size() / 2f, new Vector2(Width - thickness, Height - thickness), SpriteEffects.None, 0f);

            //Lines
            int reps = (int)((Height / thickness) / 2f);

            for (int i = 0; i < reps; i++)
            {
                float prog = (float)i / (float)reps;

                Vector2 pos = topLeft + new Vector2(0f, -Height / 2f);
                pos += new Vector2(0f, thickness + (thickness * 2 * i));

                Main.spriteBatch.Draw(Tex, pos, null, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f), 0f, Tex.Size() / 2f, new Vector2(Width - thickness * 3f, thickness), SpriteEffects.None, 0f);

            }

            return false;
        }
    }
}