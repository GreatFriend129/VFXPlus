using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using static Terraria.NPC;

namespace AerovelenceMod.Content.Items.Weapons.AreaPistols.ErinGun
{
    public class BulletCasing : ModGore
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasing";
        public override bool Update(Gore gore)
        {

            if (gore.frameCounter == 0)
                gore.alpha = 255;

            if (gore.frameCounter <= 30)
                gore.alpha -= 24;
            if (gore.frameCounter > 30)
            {
                gore.alpha += 10;

                if (gore.alpha >= 250)
                    gore.active = false;
            }
            
            gore.frameCounter++;
            return base.Update(gore);
        }
    }
}

