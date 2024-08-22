using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace VFXPlus
{
	public class VFXPlus : Mod
	{
        internal static VFXPlus Instance { get; set; }
        public VFXPlus()
        {
            Instance = this;
        }

        public static Effect BasicTrailShader;
        public static Effect TrailShaderGradient;

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                BasicTrailShader = Instance.Assets.Request<Effect>("Effects/TrailShaders/BasicTrailShader", AssetRequestMode.ImmediateLoad).Value;
                TrailShaderGradient = Instance.Assets.Request<Effect>("Effects/TrailShaders/TrailShaderGradient", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public override void Unload()
        {
            BasicTrailShader = null;
            TrailShaderGradient = null;

            Instance = null;
        }
    }
}
