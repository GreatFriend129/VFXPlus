using System.ComponentModel;
using Terraria.ModLoader.Config;

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
        [ReloadRequired]
        [DefaultValue(true)]
        public bool WandOfSparkingToggle = true;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool WandOfFrostingToggle = true;

        #region Gem Staves
        [ReloadRequired]
        [DefaultValue(true)]
        public bool AmethystStaffToggle = true;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool TopazStaffToggle = true;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool SapphireStaffToggle = true;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EmeraldStaffToggle = true;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool RubyStaffToggle = true;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool AmberStaffToggle = true;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool DiamondStaffToggle = true;
        #endregion

        
        [ReloadRequired]
        [DefaultValue(true)]
        public bool ThunderZapperToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool VilethornToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool AquaScepter = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool WeatherPain = true;

        #endregion

        #region Hardmode Staves
        [ReloadRequired]
        [DefaultValue(true)]
        public bool SkyFractureToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool CrystalSerpentToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool FrostStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool PoisonStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MeteorStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool CrystalVileShardToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ClingerStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LifeDrainToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool UnholyTridentToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool TomeOfInfiniteWisdomToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NettleBurstToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool VenomStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool SpecterStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ShadowbeamStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool InfernoForkToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BatScepterToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BlizzardStaffToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool RazorpineToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool StaffOfEarthToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BetsysWrathToggle = true;
        #endregion

        //-----------------------------------------------------------
        [Header("MagicGuns")]

        #region PreHM Magic Guns
        [DefaultValue(true)]
        [ReloadRequired]
        public bool SpaceGunToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ZapinatorToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BeeGunToggle = true;
        #endregion

        #region Hardmode Magic Guns
        [DefaultValue(true)]
        [ReloadRequired]
        public bool LaserRifleToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool WaspGunToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LeafBlowerToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool RainbowGunToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool HeatRayToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LaserMachinegunToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ChargedBlasterCannonToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BubbleGunToggle = true;
        #endregion

        //-----------------------------------------------------------
        [Header("Tomes")]

        #region PreHM Tomes
        [DefaultValue(true)]
        [ReloadRequired]
        public bool WaterboltToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BookOfSkullsToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool DemonScytheToggle = true;
        #endregion

        #region Hardmode Tomes
        //[ReloadRequired]
        [DefaultValue(true)] //TODO
        public bool CrystalStormToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool CursedFlamesToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool GoldenShowerToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MagnetSphereToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool RazorbladeTyphoonToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LunarFlareToggle = true;
        #endregion

        //-----------------------------------------------------------
        [Header("Misc")]

        #region Misc
        [DefaultValue(true)]
        [ReloadRequired]
        public bool CrimsonRodToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NimbusRodToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool FlowerOfFireToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool FlowerOfFrostToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ShadowflameHexDollToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MagicDaggerToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MedusaHeadToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool IceRodToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool SpiritFlameToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool BloodThornToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool MagicHarpToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool ToxicFlaskToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NightglowToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool StellarTuneToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NebulaArcanumToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool NebulaBlazeToggleToggle = true;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool LastPrismToggle = true;

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