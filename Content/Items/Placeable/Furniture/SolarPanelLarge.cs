﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Macrocosm.Content.Items.Placeable.Furniture
{
    internal class SolarPanelLarge : ModItem
    {
		public override void SetStaticDefaults()
		{
 		}

		public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 34;
            Item.maxStack = Item.CommonMaxStack;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.rare = ItemRarityID.White;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Furniture.SolarPanelLarge>();
        }
    }
}