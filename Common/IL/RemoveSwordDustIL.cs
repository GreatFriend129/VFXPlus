using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using VFXPlus.Common.Interfaces;

namespace VFXPlus.Common.IL
{
    public class RemoveSwordDustIL : ModSystem
    {
        public override void OnModLoad()
        {
            IL_Player.ItemCheck_EmitUseVisuals += RemoveStarfurySwingDust;
        }

        private void RemoveStarfurySwingDust(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(65)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveStarfurySwingDust Edit failed.");
                return;
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
        }
    }
}
