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
using VFXPlus.Common;
using System.Reflection;
using VFXPlus.Common.Interfaces;
using System.IO;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace VFXPlus
{
	public class VFXPlus : Mod
	{
        private List<IOrderedLoadable> loadCache;

        internal static VFXPlus Instance { get; set; }
        public VFXPlus()
        {
            Instance = this;
        }

        public static Effect GlowingTrailShader;

        public static Effect BasicTrailShader;
        public static Effect TrailShaderGradient;

        public override void Load()
        {
            // Literally ripped from SLR
            #region IOrderedLoadable Loading
            loadCache = new List<IOrderedLoadable>();

            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IOrderedLoadable)))
                {
                    object instance = Activator.CreateInstance(type);
                    loadCache.Add(instance as IOrderedLoadable);
                }
            }

            for (int k = 0; k < loadCache.Count; k++)
            {
                loadCache[k].Load();
                SetLoadingText("Loading: " + loadCache[k].GetType().Name);
            }
            #endregion

            if (Main.netMode != NetmodeID.Server)
            {
                BasicTrailShader = Instance.Assets.Request<Effect>("Effects/TrailShaders/BasicTrailShader", AssetRequestMode.ImmediateLoad).Value;
                TrailShaderGradient = Instance.Assets.Request<Effect>("Effects/TrailShaders/TrailShaderGradient", AssetRequestMode.ImmediateLoad).Value;
            }

            var screenRef = new Ref<Effect>(VFXPlus.Instance.Assets.Request<Effect>("Effects/GlowDustShader", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
            Filters.Scene["GlowDustShader"] = new Filter(new ScreenShaderData(screenRef, "ArmorBasic"), EffectPriority.High);
            Filters.Scene["GlowDustShader"].Load();

        }

        public static void SetLoadingText(string text)
        {
            FieldInfo Interface_loadMods = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface")!.GetField("loadMods", BindingFlags.NonPublic | BindingFlags.Static)!;
            MethodInfo UIProgress_set_SubProgressText = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIProgress")!.GetProperty("SubProgressText", BindingFlags.Public | BindingFlags.Instance)!.GetSetMethod()!;

            UIProgress_set_SubProgressText.Invoke(Interface_loadMods.GetValue(null), new object[] { text });
        }

        public override void Unload()
        {
            if (loadCache != null)
            {
                foreach (IOrderedLoadable loadable in loadCache)
                {
                    loadable.Unload();
                }

                loadCache = null;
            }
            else
            {
                Logger.Warn("load cache was null, ILoadables may not have been unloaded...");
            }

            if (!Main.dedServ)
            {
                BasicTrailShader = null;
                TrailShaderGradient = null;
                Instance ??= null;
            }
        }
    }
}
