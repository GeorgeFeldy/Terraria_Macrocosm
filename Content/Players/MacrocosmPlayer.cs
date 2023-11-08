using Macrocosm.Common.Config;
using Macrocosm.Common.Subworlds;
using Macrocosm.Common.Systems;
using Macrocosm.Common.Utils;
using Macrocosm.Content.Biomes;
using Macrocosm.Content.Buffs.Debuffs;
using Macrocosm.Content.LoadingScreens;
using Macrocosm.Content.Subworlds;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace Macrocosm.Content.Players
{
    public enum SpaceProtection
    {
        None,
        Tier1,
        Tier2,
        Tier3
    }

    /// <summary>
    /// Miscenllaneous class for storing custom player data. 
    /// Complex, very specific systems should be implemented in a separate ModPlayer.
    /// </summary>
    public class MacrocosmPlayer : ModPlayer
    {
        /// <summary>
        /// Whether the player has subworld travel activated (used item, in rocket etc.)
        /// If this is false, SubworldSystem.Exit() will return to the main menu.
        /// Not synced.
        /// </summary>
        public bool TriggeredSubworldTravel { get; set; }

        /// <summary> 
        /// The player's space protection level.
        /// Not synced.
        /// </summary>
        public SpaceProtection SpaceProtection { get; set; } = SpaceProtection.None;

        /// <summary> 
        /// The radiation effect intensity for this player. 
        /// Not synced.
        /// </summary>
        public float RadNoiseIntensity = 0f;

        /// <summary> 
        /// Chandrium whip hit stacks. 
        /// Not synced.
        /// </summary>
        public int ChandriumWhipStacks = 0;

        /// <summary> 
        /// Whether this player is aware that they can use zombie fingers to unlock chests.
        /// Persistent. Not synced.
        /// </summary>
        public bool KnowsToUseZombieFinger = false;

        /// <summary> 
        /// Chance to not consume ammo from equipment and weapons, stacks additively with the vanilla chance 
        /// Not synced.
        /// </summary>
        public float ChanceToNotConsumeAmmo
        {
            get => chanceToNotConsumeAmmo;
            set => chanceToNotConsumeAmmo = MathHelper.Clamp(value, 0f, 1f);
        }

        private float chanceToNotConsumeAmmo = 0f;

        /// <summary>
        /// The subworlds this player has visited.
        /// Persistent. Not synced.
        /// </summary>
        //TODO: sync this if needed
        private List<string> visitedSubworlds = new();

        /// <summary>
        /// A dictionary of this player's last known subworld, by each Terraria world file visited.
        /// Not synced.
        /// </summary>
        private readonly Dictionary<Guid, string> lastSubworldNameByWorldUniqueId = new();

        public override void ResetEffects()
        {
            SpaceProtection = SpaceProtection.None;
            RadNoiseIntensity = 0f;
            ChanceToNotConsumeAmmo = 0f;
            Player.buffImmune[BuffType<Depressurized>()] = false;
            TriggeredSubworldTravel = false;
        }

        public bool HasVisitedSubworld(string subworldId) => visitedSubworlds.Contains(subworldId);

        public void SetReturnSubworld(string subworldId)
        {
            Guid guid = SubworldSystem.AnyActive<Macrocosm>() ? MacrocosmSubworld.Current.MainWorldUniqueId : Main.ActiveWorldFileData.UniqueId;
            lastSubworldNameByWorldUniqueId[guid] = subworldId;
        }

        public bool TryGetReturnSubworld(Guid worldUniqueId, out string subworldId) => lastSubworldNameByWorldUniqueId.TryGetValue(worldUniqueId, out subworldId);

        public override void OnEnterWorld()
        {
            if (lastSubworldNameByWorldUniqueId.TryGetValue(Main.ActiveWorldFileData.UniqueId, out string lastSubworldId))
                 if (!SubworldSystem.AnyActive<Macrocosm>() && lastSubworldId is not "Earth")
                     MacrocosmSubworld.Travel(lastSubworldId, trigger: false);
 
            if (SubworldSystem.AnyActive<Macrocosm>())
            {
                LoadingTitleSequence.StartSequence(noTitle: HasVisitedSubworld(MacrocosmSubworld.CurrentMacrocosmID) && !MacrocosmConfig.Instance.AlwaysDisplayTitleScreens);
                visitedSubworlds.Add(MacrocosmSubworld.CurrentMacrocosmID);
            }
            else if (TriggeredSubworldTravel)
            {                
                LoadingTitleSequence.StartSequence(noTitle: !MacrocosmConfig.Instance.AlwaysDisplayTitleScreens);
            }
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            bool consumeAmmo = true;

            if (Main.rand.NextFloat() < ChanceToNotConsumeAmmo)
                consumeAmmo = false;

            return consumeAmmo;
        }

        public override void PostUpdateBuffs()
        {
            if (SubworldSystem.AnyActive<Macrocosm>())
            {
                UpdateSpaceEnvironmentalDebuffs();
            }
        }

        public void UpdateSpaceEnvironmentalDebuffs()
        {
            if (!Player.GetModPlayer<RocketPlayer>().InRocket)
            {
                if (SubworldSystem.IsActive<Moon>())
                {
                    if (SpaceProtection == SpaceProtection.None)
                        Player.AddBuff(BuffType<Depressurized>(), 2);
                    //if (protectTier <= SpaceProtection.Tier1)
                    //Player.AddBuff(BuffTpye<Irradiated>(), 2);
                }
                //else if (SubworldSystem.IsActive<Mars>())
            }
        }

        public override void PostUpdateMiscEffects()
        {
            //UpdateGravity();
            UpdateFilterEffects();
        }

        public override void PostUpdateEquips()
        {
            UpdateSpaceArmourImmunities();
            if (Player.GetModPlayer<MacrocosmPlayer>().SpaceProtection > SpaceProtection.None)
                Player.setBonus = Language.GetTextValue("Mods.Macrocosm.Items.SetBonuses.SpaceProtection_" + SpaceProtection.ToString());
        }

        public void UpdateSpaceArmourImmunities()
        {
            if (SpaceProtection > SpaceProtection.None)
                Player.buffImmune[BuffType<Depressurized>()] = true;
            if (SpaceProtection > SpaceProtection.Tier1)
            {
            }
        }

        //private void UpdateGravity()
        //{
        //	if (MacrocosmSubworld.AnyActive)
        //		Player.gravity = Player.defaultGravity * MacrocosmSubworld.Current.GravityMultiplier;
        //}

        private void UpdateFilterEffects()
        {
            if (Main.dedServ)
                return;

            if (Player.InModBiome<IrradiationBiome>())
            {
                if (!Filters.Scene["Macrocosm:RadiationNoise"].IsActive())
                    Filters.Scene.Activate("Macrocosm:RadiationNoise");

                RadNoiseIntensity += 0.189f * Utility.InverseLerp(400, 10000, TileCounts.Instance.IrradiatedRockCount, clamped: true);

                Filters.Scene["Macrocosm:RadiationNoise"].GetShader().UseIntensity(RadNoiseIntensity);
            }
            else
            {
                if (Filters.Scene["Macrocosm:RadiationNoise"].IsActive())
                    Filters.Scene.Deactivate("Macrocosm:RadiationNoise");
            }
        }

        public override void SaveData(TagCompound tag)
        {
            if (KnowsToUseZombieFinger)
                tag[nameof(KnowsToUseZombieFinger)] = true;

            if(visitedSubworlds.Any())
                tag[nameof(visitedSubworlds)] = visitedSubworlds;

            TagCompound lastSubworldsByWorld = new();
            foreach (var kvp in lastSubworldNameByWorldUniqueId)
                lastSubworldsByWorld[kvp.Key.ToString()] = kvp.Value;

            tag[nameof(lastSubworldNameByWorldUniqueId)] = lastSubworldsByWorld;
        }

        public override void LoadData(TagCompound tag)
        {
            KnowsToUseZombieFinger = tag.ContainsKey(nameof(KnowsToUseZombieFinger));

            if (tag.ContainsKey(nameof(visitedSubworlds)))
                visitedSubworlds = tag.GetList<string>(nameof(visitedSubworlds)).ToList();

            if (tag.ContainsKey(nameof(lastSubworldNameByWorldUniqueId)))
            {
                TagCompound lastSubworldsByWorld = tag.GetCompound(nameof(lastSubworldNameByWorldUniqueId));
                foreach (var kvp in lastSubworldsByWorld)
                    lastSubworldNameByWorldUniqueId[new Guid(kvp.Key)] = lastSubworldsByWorld.GetString(kvp.Key);
            }
        }
    }
}
