﻿using Macrocosm.Common.DataStructures;
using Macrocosm.Common.Utils;
using Macrocosm.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Macrocosm.Content.NPCs.Bosses.CraterDemon
{
    public class PhantasmalPortal : ModProjectile
    {
        public ref float AITimer => ref Projectile.ai[0];

        public bool Phase2
        {
            get => Projectile.ai[1] > 0f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        public int TargetPlayer
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        public int SpawnPeriod => 28;

        protected int defWidth;
        protected int defHeight;

        protected bool spawned;

        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            defWidth = defHeight = Projectile.width = Projectile.height = 52;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = CraterDemon.PortalTimerMax;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            if (!spawned)
            {
                spawned = true;
                AITimer = 255f;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item78, Projectile.Center);
            }

            Projectile.rotation -= MathHelper.ToRadians(7.4f);

            if (Projectile.timeLeft >= CraterDemon.PortalTimerMax - 90)
                AITimer -= 2.83333325f;
            else if (Projectile.timeLeft <= 90)
                AITimer += 2.83333325f;
            else
            {
                AITimer = 0f;
                if (Projectile.timeLeft % SpawnPeriod == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 velocity = (Projectile.Center - Main.player[TargetPlayer].Center).RotatedByRandom(20) * Main.rand.NextFloat(6f, 9.25f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<PhantasmalImpLarge>(),
                            (int)(Projectile.damage * 0.4f), Projectile.knockBack, Projectile.owner, 0f, 0f);
                    }

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
                }
            }

            SpawnDusts();

            Projectile.alpha = (int)MathHelper.Clamp((int)AITimer, 0f, 255f);

            Vector2 center = Projectile.Center;
            Projectile.scale = 0.05f + 0.95f * (1f - Projectile.alpha / 255f);
            Projectile.width = (int)(defWidth * Projectile.scale);
            Projectile.height = (int)(defHeight * Projectile.scale);
            Projectile.Center = center;
        }

        public override Color? GetAlpha(Color lightColor)
            => Color.White * (1f - Projectile.alpha / 255f);

        private SpriteBatchState state;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Color color = Color.White;

            state.SaveState(Main.spriteBatch);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(BlendState.AlphaBlend, state);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, (color * 1f).WithOpacity(0.5f - 0.5f * Projectile.alpha / 255f), (0f - Projectile.rotation) * 0.65f, texture.Size() / 2f, Projectile.scale * 1.4f, SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, (color * 0.84f).WithOpacity(0.5f - 0.5f * Projectile.alpha / 255f), Projectile.rotation, texture.Size() / 2f, Projectile.scale * 1.2f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(BlendState.Additive, state);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, color * 1f, (0f - Projectile.rotation) * 0.65f, texture.Size() / 2f, Projectile.scale * 0.8f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(BlendState.AlphaBlend, state);

            return false;
        }

        /// <summary> Adapted from Lunar Portal Staff </summary>
        private void SpawnDusts()
        {

            for (int i = 0; i < (int)(5 * (1f - AITimer / 255f)); i++)
            {
                int type = ModContent.DustType<PortalLightGreenDust>();

                Vector2 rotVector1 = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * 1.6f * (1f - AITimer / 255f);
                Dust lightDust = Main.dust[Dust.NewDust(Projectile.Center - rotVector1 * 30f, 0, 0, type)];
                lightDust.noGravity = true;
                lightDust.position = Projectile.Center - rotVector1 * Main.rand.Next(17, 19);
                lightDust.velocity = rotVector1.RotatedBy(MathHelper.PiOver2) * 2.4f;
                lightDust.scale = (0.8f + Main.rand.NextFloat()) * (1f - AITimer / 255f);
                lightDust.fadeIn = 0.5f;
                lightDust.customData = Projectile.Center;
            }

            if (Main.rand.NextBool())
            {
                int type = ModContent.DustType<PortalDarkGreenDust>();

                Vector2 rotVector1 = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * 1.6f * (1f - AITimer / 255f);
                Dust lightDust = Main.dust[Dust.NewDust(Projectile.Center - rotVector1 * 30f, 0, 0, type)];
                lightDust.noGravity = true;
                lightDust.position = Projectile.Center - rotVector1 * Main.rand.Next(30, 40);
                lightDust.velocity = rotVector1.RotatedBy(MathHelper.PiOver2) * 2f;
                lightDust.scale = (0.8f + Main.rand.NextFloat()) * (1f - AITimer / 255f);
                lightDust.fadeIn = 0.5f;
                lightDust.customData = Projectile.Center;
            }
        }
    }
}
