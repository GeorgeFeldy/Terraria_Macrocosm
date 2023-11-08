using Macrocosm.Common.Utils;
using Macrocosm.Content.LoadingScreens;
using Macrocosm.Content.Players;
using Macrocosm.Content.Rockets;
using Macrocosm.Content.Subworlds;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.IO;

namespace Macrocosm.Common.Subworlds
{
    public enum MapColorType
    {
        SkyUpper,
        SkyLower,
        UndergroundUpper,
        UndergroundLower,
        CavernUpper,
        CavernLower,
        Underworld
    }

    public partial class MacrocosmSubworld
    {
        /// <summary> Get the current <c>MacrocosmSubworld</c> active instance. 
        /// Earth returns null! You should check for <see cref="SubworldSystem.AnyActive"/> for <b>Macrocosm</b> before accessing this. </summary>
        public static MacrocosmSubworld Current => SubworldSystem.AnyActive<Macrocosm>() ? SubworldSystem.Current as MacrocosmSubworld : null;

        /// <summary> 
        /// Get the current active Macrocosm subworld string ID, matching the subworld class name. 
        /// Returns <c>Earth</c> if none active. 
        /// Use this for situations where we want other mods' subworlds to behave like Earth.
        /// </summary>
        public static string CurrentMacrocosmID => SubworldSystem.AnyActive<Macrocosm>() ? Current.Name : "Earth";

        /// <summary>
        /// Get the current active subworld string ID, matching the subworld class name. 
        /// If it's from another mod, not Macrocosm, returns the subworld name with the mod name prepended. 
        /// Returns <c>Earth</c> if none active.
        /// Use this for situations where other mods' subworlds should behave differently from Earth (the main world).
        /// </summary>
        public static string CurrentID =>
            SubworldSystem.AnyActive() && !SubworldSystem.AnyActive<Macrocosm>() ?
            SubworldSystem.Current.Mod.Name + ":" + SubworldSystem.Current.Name :
            CurrentMacrocosmID;

        public static bool IsValidMacrocosmID(string id) => SubworldSystem.GetIndex(Macrocosm.Instance.Name + "/" + id) >= 0 || id is "Earth";

        // TODO: We could protect the original properties get them only via statics?
        public static double CurrentTimeRate => SubworldSystem.AnyActive<Macrocosm>() ? Current.TimeRate : Earth.TimeRate;
        public static double CurrentDayLenght => SubworldSystem.AnyActive<Macrocosm>() ? Current.DayLenght : Earth.DayLenght;
        public static double CurrentNightLenght => SubworldSystem.AnyActive<Macrocosm>() ? Current.NightLenght : Earth.NightLenght;
        public static float CurrentGravityMultiplier => SubworldSystem.AnyActive<Macrocosm>() ? Current.GravityMultiplier : Earth.GravityMultiplier;

        /// <summary> The loading screen. </summary>
        public static LoadingScreen LoadingScreen { get; set; }

        public static bool Travel(string targetWorld, Rocket rocket = null, bool trigger = true)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                UpdateLoadingScreen(rocket, targetWorld);

                Main.LocalPlayer.GetModPlayer<MacrocosmPlayer>().TriggeredSubworldTravel = trigger;
                Main.LocalPlayer.GetModPlayer<MacrocosmPlayer>().SetReturnSubworld(targetWorld);

                if (targetWorld == "Earth")
                {
                    SubworldSystem.Exit();
                    LoadingScreen?.SetTargetWorld("Earth");
                    LoadingTitleSequence.SetTargetWorld("Earth");
                    return true;
                }

                string subworldId = Macrocosm.Instance.Name + "/" + targetWorld;
                bool entered = SubworldSystem.Enter(subworldId);

                if (entered)
                {
                    LoadingScreen?.SetTargetWorld(targetWorld);
                    LoadingTitleSequence.SetTargetWorld(targetWorld);
                }
                else
                {
                    WorldTravelFailure("Error: Failed entering target subworld: " + targetWorld + ", staying on " + CurrentID);
                }

                return entered;
            }
            else
            {
                return true;
            }
        }

        private static void UpdateLoadingScreen(Rocket rocket, string targetWorld)
        {
            bool flightAnimation = rocket is not null;

            if(flightAnimation)
            {
                if (!SubworldSystem.AnyActive<Macrocosm>())
                     LoadingScreen = new EarthLoadingScreen();
                else LoadingScreen = CurrentMacrocosmID switch
                {
                    "Moon" => new MoonLoadingScreen(),
                    _ => null,
                };
            }
            else
            {
                LoadingScreen = targetWorld switch
                {
                    "Earth" => new EarthLoadingScreen(),
                    "Moon" => new MoonLoadingScreen(),
                    _ => null,
                };
            }

            if (flightAnimation)
                LoadingScreen?.SetRocket(rocket);

            LoadingScreen?.Setup();
        }

        // Called if travel to the target subworld fails
        public static void WorldTravelFailure(string message)
        {
            Utility.Chat(message, Color.Red);
            Macrocosm.Instance.Logger.Error(message);
        }
    }
}
