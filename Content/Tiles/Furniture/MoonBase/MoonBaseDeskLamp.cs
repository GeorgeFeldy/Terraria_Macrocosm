﻿using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;
using Macrocosm.Content.Dusts;

namespace Macrocosm.Content.Tiles.Furniture.MoonBase
{
	internal class MoonBaseDeskLamp : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileWaterDeath[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.WaterDeath = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

			// To ensure the right-facing style is properly registered, not the left-facing style "turned off" frame
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleWrapLimit = 4;

			// Place right alternate
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(2); // Skip style 1 "turned off" frame

			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			AdjTiles = new int[] { TileID.Candelabras };

			DustType = ModContent.DustType<MoonBasePlatingDust>();

			AddMapEntry(new Color(253, 221, 3), Language.GetText("MapObject.Candelabra"));
		}

		public override void HitWire(int i, int j)
		{
			int leftX = i - Main.tile[i, j].TileFrameX / 18 % 2;
			int topY = j - Main.tile[i, j].TileFrameY / 18 % 2;

			for (int x = leftX; x < leftX + 2; x++)
			{
				for (int y = topY; y < topY + 2; y++)
				{
					// There must be a better way to determine this lol
					// No, I am not doing bitwise boolean algebra tho  -- Feldy
					if (Main.tile[x, y].TileFrameX / 18 is 2 or 3 or 6 or 7 )
						Main.tile[x, y].TileFrameX -= 36;
					else
						Main.tile[x, y].TileFrameX += 36;
				}
			}
			if (Wiring.running)
			{
				Wiring.SkipWire(leftX, topY);
				Wiring.SkipWire(leftX, topY + 1);
				Wiring.SkipWire(leftX + 1, topY);
				Wiring.SkipWire(leftX + 1, topY + 1);
			}

			if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, leftX, topY + 1, 2);
 		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX == 0)
			{
				r = 1f;
				g = 1f;
				b = 1f;
			}
		}
	}
}
