#region Using directives

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

#endregion

namespace VFXPlus.Common.Interfaces
{
    //Based off SLRs IDrawAdditive which I think is based off Spirit's IDrawAdditive
    interface IDrawAdditive
    {
        void DrawAdditive(SpriteBatch spriteBatch);
    }
}
