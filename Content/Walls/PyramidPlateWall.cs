using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Macrocosm.Content.Walls
{
	public class PyramidPlateWall : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			ItemDrop = ItemType<Items.Placeables.BlocksAndWalls.PyramidPlateWall>();
			AddMapEntry(new Color(65, 65, 65));
		}
	}
}