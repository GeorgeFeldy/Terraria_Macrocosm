using Macrocosm.Common.Bases;
using Macrocosm.Common.TileFrame;
using Macrocosm.Content.Dusts;
using Macrocosm.Content.Items.Placeable.Furniture.MoonBase;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Macrocosm.Content.Tiles.Furniture.MoonBase
{
    public class MoonBaseBulkheadOpen : ModTile, IDoorTile
	{
		public int Height => 5;
		public int Width => 1;
        public bool IsClosed => false;
        public int StyleCount => 1;
        public int TileAnimationID => TileAnimation.RegisterTileAnimation(2, 60, [2, 1, 0]);

        public override void SetStaticDefaults()
		{
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileLavaDeath[Type] = true;
			Main.tileNoSunLight[Type] = true;
			TileID.Sets.HousingWalls[Type] = true;  
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.CloseDoorID[Type] = ModContent.TileType<MoonBaseBulkheadClosed>();

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

			DustType = ModContent.DustType<MoonBasePlatingDust>();
			AdjTiles = [TileID.OpenDoor];

			RegisterItemDrop(ModContent.ItemType<MoonBaseBulkhead>(), 0);

			AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));

            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.DrawYOffset = 0;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];

            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;

            TileObjectData.addTile(Type);
        }

        public Rectangle ModifyAutoDoorPlayerCollisionRectangle(Point tileCoords, Rectangle original)
        {
            Rectangle result = original;
            result.Y -= 16;
            result.Height = Height * 5;
            return result;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX % (18 * Width);
            int frameY = tile.TileFrameY % (18 * Height);
            if (TileAnimation.GetTemporaryFrame(i - frameX / 18, j - frameY / 18, out var frame))
                tile.TileFrameY = (short)(18 * frame + frameY);
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override void NumDust(int i, int j, bool fail, ref int num) 
		{
			num = 1;
		}

		public override void MouseOver(int i, int j) 
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<MoonBaseBulkhead>();
		}
	}
}