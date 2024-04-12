﻿using Macrocosm.Content.Items.Materials.Tech;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace Macrocosm.Content.Rockets.Modules
{
    public class ReactorModule : RocketModule
    {
        private static Asset<Texture2D> glowmask;
        public override int DrawPriority => 2;

        public override int Width => 84;
        public override int Height => 80;

        public override Rectangle Hitbox => base.Hitbox with { Y = base.Hitbox.Y + 4 };

        public override AssemblyRecipe Recipe { get; } = new AssemblyRecipe()
        {
            new(ModContent.ItemType<RocketPlating>(), 50),
            new(ModContent.ItemType<ReactorComponent>()),
            new(ModContent.ItemType<ReactorHousing>(), 2),
        };

        // Reactor glowmask
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 position, bool inWorld)
        {
            if (inWorld)
            {
                glowmask ??= ModContent.Request<Texture2D>(TexturePath + "_Glow");
                spriteBatch.Draw(glowmask.Value, position, null, Color.White, 0f, Origin, 1f, SpriteEffects.None, 0f);
            }  
        }
    }
}
