﻿using Macrocosm.Common.Bases.Projectiles;
using Macrocosm.Content.Items.GrabBags;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Macrocosm.Content.Projectiles.Environment.Meteors
{
    public class SolarMeteor : BaseMeteor
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.width = 52;
            Projectile.height = 44;

            ScreenshakeMaxDist = 140f * 16f;
            ScreenshakeIntensity = 100f;

            RotationMultiplier = 0.01f;
            BlastRadiusMultiplier = 3.5f;
        }

        public override void MeteorAI()
        {
            float DustScaleMin = 1f;
            float DustScaleMax = 1.6f;

            if (Main.rand.NextBool(1))
            {
                Dust dust = Dust.NewDustDirect(
                        new Vector2(Projectile.position.X, Projectile.position.Y),
                        Projectile.width,
                        Projectile.height,
                        DustID.SolarFlare,
                        0f,
                        0f,
                        Scale: Main.rand.NextFloat(DustScaleMin, DustScaleMax)
                    );

                dust.noGravity = true;
            }
        }

        public override void ImpactEffects()
        {
            int ImpactDustCount = Main.rand.Next(140, 160);
            Vector2 ImpactDustSpeed = new Vector2(3f, 10f);
            float DustScaleMin = 1f;
            float DustScaleMax = 1.6f;

            for (int i = 0; i < ImpactDustCount; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    new Vector2(Projectile.Center.X, Projectile.Center.Y),
                    Projectile.width,
                    Projectile.height,
                    DustID.SolarFlare,
                    Main.rand.NextFloat(-ImpactDustSpeed.X, ImpactDustSpeed.X),
                    Main.rand.NextFloat(0f, -ImpactDustSpeed.Y),
                    Scale: Main.rand.NextFloat(DustScaleMin, DustScaleMax)
                );

                dust.noGravity = true;
            }
        }

        public override void SpawnItems()
        {
            int type = ModContent.ItemType<SolarChunk>();
            Vector2 position = new Vector2(Projectile.position.X + Projectile.width / 2, Projectile.position.Y - Projectile.height);
            int itemIdx = Item.NewItem(Projectile.GetSource_FromThis(), position, new Vector2(Projectile.width, Projectile.height), type);
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIdx, 1f);
        }
    }
}
