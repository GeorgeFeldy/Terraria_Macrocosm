using Macrocosm.Common.Drawing.Particles;
using Macrocosm.Content.Items.Bars;
using Macrocosm.Content.Particles;
using Macrocosm.Content.Projectiles.Friendly.Melee;
using Macrocosm.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Macrocosm.Content.Items.Weapons.Melee
{
    public class ArtemiteSword : ModItem
    {
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Item.damage = 225;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5;
            Item.value = 10000;
            Item.rare = ModContent.RarityType<MoonRarityT1>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ArtemiteSwordSwing>();
            Item.shootSpeed = 10f;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Particle.CreateParticle<ArtemiteStar>((p) =>
            {
                p.Position = target.Center;
                p.Velocity = -Vector2.UnitY * 0.4f;
                p.Scale = 1f;
                p.Rotation = MathHelper.PiOver4;
            }, shouldSync: true
            );
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, velocity, ModContent.ProjectileType<ArtemiteSwordProjectile>(), damage, knockback, player.whoAmI); //, player.GetAdjustedItemScale(Item));
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<ArtemiteSwordSwing>(), damage, knockback, player.whoAmI, (float)player.direction * player.gravDir, 24); //, player.GetAdjustedItemScale(Item));
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient<ArtemiteBar>(12)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        }
    }
}