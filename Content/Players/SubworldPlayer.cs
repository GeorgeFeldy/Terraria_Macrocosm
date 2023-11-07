using Macrocosm.Common.Config;
using Macrocosm.Common.Subworlds;
using Macrocosm.Content.LoadingScreens;
using Macrocosm.Content.Rockets.LaunchPads;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Macrocosm.Content.Players
{
    public class SubworldPlayer : ModPlayer
    {
        public Dictionary<string, string> LastSubworldsByWorld = new();
        public List<string> VisitedSubworlds = new();
        public bool UsedTravelItem { get; set; }

        public override void Load()
        {
            On_UIWorldListItem.DrawSelf += On_UIWorldListItem_DrawSelf;
        }

        public override void Unload()
        {
            On_UIWorldListItem.DrawSelf -= On_UIWorldListItem_DrawSelf;
        }

        public override void Initialize()
        {
        }

        public override void ResetEffects()
        {
            UsedTravelItem = false;
        }

        public void RegisterTravel(string subworldId)
        {
            LastSubworldsByWorld[Main.ActiveWorldFileData.GetWorldName()] = subworldId;
        }

        public override void OnEnterWorld()
        {
            string worldName = Main.worldName;
            if (LastSubworldsByWorld.TryGetValue(worldName, out string lastSubworldID))
            {
                if (!SubworldSystem.AnyActive<Macrocosm>() && lastSubworldID != "Earth")
                {
                    SubworldSystem.Enter(Macrocosm.Instance.Name + "/" + lastSubworldID);
                }
            }

            if (SubworldSystem.AnyActive<Macrocosm>())
            {
                bool visited = VisitedSubworlds.Contains(MacrocosmSubworld.CurrentMacrocosmID);
                LoadingTitleSequence.StartSequence(noTitle: visited && !MacrocosmConfig.Instance.AlwaysDisplayTitleScreens);
                VisitedSubworlds.Add(MacrocosmSubworld.CurrentMacrocosmID);
            }
            else
            {
                // Travelling to Earth from another planet
                if (Player.GetModPlayer<RocketPlayer>().InRocket || UsedTravelItem)
                    LoadingTitleSequence.StartSequence(noTitle: !MacrocosmConfig.Instance.AlwaysDisplayTitleScreens);
            }
        }

        private void On_UIWorldListItem_DrawSelf(On_UIWorldListItem.orig_DrawSelf orig, UIWorldListItem self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            orig(self, spriteBatch);

            var player = Main.LocalPlayer.GetModPlayer<SubworldPlayer>();
            string worldName = self.Data.GetWorldName();
            Texture2D texture = ModContent.Request<Texture2D>(Macrocosm.TextureAssetsPath + "Icons/Earth").Value;

            var dims = self.GetOuterDimensions();
            var pos = new Vector2(dims.X + texture.Width + 102, dims.Y + dims.Height - texture.Height + 1);

            if (player.LastSubworldsByWorld.ContainsKey(worldName) && ModContent.RequestIfExists<Texture2D>(Macrocosm.TextureAssetsPath + "Icons/" + player.LastSubworldsByWorld[worldName], out var asset))
                texture = asset.Value;

            spriteBatch.Draw(texture, pos, null, Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
        }

        public override void SaveData(TagCompound tag)
        {
            TagCompound lastSubworldsByWorld = new();
            foreach (var kvp in LastSubworldsByWorld)
                 lastSubworldsByWorld[kvp.Key] = kvp.Value;
 
            tag[nameof(LastSubworldsByWorld)] = lastSubworldsByWorld;
            tag[nameof(VisitedSubworlds)] = VisitedSubworlds;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey(nameof(LastSubworldsByWorld)))
            {
                TagCompound lastSubworldsByWorld = tag.GetCompound(nameof(LastSubworldsByWorld));

                foreach (var kvp in lastSubworldsByWorld)
                    LastSubworldsByWorld[kvp.Key] = lastSubworldsByWorld.GetString(kvp.Key);
            }

            if (tag.ContainsKey(nameof(VisitedSubworlds)))
                VisitedSubworlds = tag.GetList<string>(nameof(VisitedSubworlds)).ToList();               
        }
    }
}
