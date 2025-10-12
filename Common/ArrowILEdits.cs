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

namespace VFXPlus.Common
{
    public class ArrowILEdits : ModSystem
    {
        public override void OnModLoad()
        {
            //IL_Projectile.Update += RemoveFlamingArrowDust;
            //IL_Projectile.Update += RemoveFrostburnArrowDust;
            //IL_Projectile.Update += RemoveCursedArrowDust;
            //IL_Projectile.Update += RemoveIchorArrowDust;
            //IL_Projectile.Update += RemoveUnholyArrowDust;
            //IL_Projectile.Update += RemoveJesterArrowDust;
        }

        // So for some reason a lot of arrows have their dust run in the Update function instead of AI
        // This IL Edit is designed to remove that dust
        private void RemoveFlamingArrowDust(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            for (int i = 0; i < 12; i++)
            {
                if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
                {
                    VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                    return;
                }
            }


            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);


            //WORKS FOR HOSTILE FIRE ARROW

            /*
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(82)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
            */
        }

        private void RemoveFrostburnArrowDust(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(172)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFrostburnArrowDust Edit failed.");
                return;
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
        }

        private void RemoveCursedArrowDust(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(103)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveCursedArrowDust Edit failed.");
                return;
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
        }

        private void RemoveIchorArrowDust(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(278)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveIchorArrowDust Edit failed.");
                return;
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
        }

        private void RemoveUnholyArrowDust(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            for (int i = 0; i < 2; i++)
            {
                if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(4)))
                {
                    VFXPlus.Instance.Logger.Warn("RemoveUnholyArrowDust Edit failed.");
                    return;
                }
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
        }

        private void RemoveJesterArrowDust(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            for (int i = 0; i < 3; i++)
            {
                if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(5)))
                {
                    VFXPlus.Instance.Logger.Warn("RemoveJesterArrowDust Edit failed.");
                    return;
                }
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
        }
    }
}
