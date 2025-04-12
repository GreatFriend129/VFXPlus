using System.ComponentModel;
using Terraria.ModLoader.Config;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using SteelSeries.GameSense;

namespace VFXPlus
{
    public class VFXPlusToggles : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        //[Header("SeparatePageExamples")]
        // Using SeparatePage, an object will be presented to the user as a button. That button will lead to a separate page where the usual UI will be presented. Useful for organization.

        public MagicToggles MagicToggle = new MagicToggles();
    }

    [SeparatePage]
    [BackgroundColor(211, 156, 186)]
    public class MagicToggles
    {
        //-----------------------------------------------------------
        [Header("Staves")]

        #region PreHM Staves
        [DefaultValue(true)]
        [ReloadRequired]
        public bool WandOfSparkingToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool WandOfFrostingToggle;

        #region Gem Staves
        [DefaultValue(true)]
        [ReloadRequired]
        public bool AmethystStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool TopazStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool SapphireStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool EmeraldStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool RubyStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool AmberStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool DiamondStaffToggle;
        #endregion

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ThunderZapperToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool VilethornToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool AquaScepter;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool WeatherPain;

        #endregion

        #region Hardmode Staves
        [DefaultValue(true)]
        [ReloadRequired]
        public bool SkyFractureToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool CrystalSerpentToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool FrostStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool PoisonStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MeteorStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool CrystalVileShardToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ClingerStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LifeDrainToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool UnholyTridentToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool TomeOfInfiniteWisdomToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NettleBurstToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool VenomStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool SpecterStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ShadowbeamStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool InfernoForkToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BatScepterToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BlizzardStaffToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool RazorpineToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool StaffOfEarthToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BetsysWrathToggle;
        #endregion

        //-----------------------------------------------------------
        [Header("MagicGuns")]

        #region PreHM Magic Guns
        [DefaultValue(true)]
        [ReloadRequired]
        public bool SpaceGunToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ZapinatorToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BeeGunToggle;
        #endregion

        #region Hardmode Magic Guns
        [DefaultValue(true)]
        [ReloadRequired]
        public bool LaserRifleToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool WaspGunToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LeafBlowerToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool RainbowGunToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool HeatRayToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LaserMachinegunToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ChargedBlasterCannonToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BubbleGunToggle;
        #endregion

        //-----------------------------------------------------------
        [Header("Tomes")]

        #region PreHM Tomes
        [DefaultValue(true)]
        [ReloadRequired]
        public bool WaterboltToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BookOfSkullsToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool DemonScytheToggle;
        #endregion

        #region Hardmode Tomes
        [DefaultValue(true)]
        [ReloadRequired]
        public bool CrystalStormToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool CursedFlamesToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool GoldenShowerToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MagnetSphereToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool RazorbladeTyphoonToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LunarFlareToggle;
        #endregion

        //-----------------------------------------------------------
        [Header("Misc")]

        #region Misc
        [DefaultValue(true)]
        [ReloadRequired]
        public bool CrimsonRodToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NimbusRodToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool FlowerOfFireToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool FlowerOfFrostToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ShadowflameHexDollToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MagicDaggerToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MedusaHeadToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool IceRodToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool SpiritFlameToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BloodThornToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MagicHarpToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ToxicFlaskToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NightglowToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool StellarTuneToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NebulaArcanumToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NebulaBlazeToggleToggle;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LastPrismToggle;

        #endregion
    }

    public class MiscCustomization : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static MiscCustomization Instance;


        [DrawTicks]
        [OptionStrings(new string[] { "None", "Lesbian", "Bisexual", "Trans", "Non-Binary", "Asexual", "Aromantic", "Aroace" })]
        [DefaultValue("None")]
        public string LastPrismPrideColor;
    }

}