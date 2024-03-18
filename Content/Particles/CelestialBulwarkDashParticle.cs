﻿using Macrocosm.Common.Drawing.Particles;
using Macrocosm.Common.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Macrocosm.Content.Players;
using Macrocosm.Common.Drawing;
using Macrocosm.Common.DataStructures;
using Macrocosm.Common.Graphics;
using Macrocosm.Content.Items.Accessories;
using Terraria.ID;
using System;

namespace Macrocosm.Content.Particles
{
	public class CelestialBulwarkDashParticle : Particle
	{
		public override int SpawnTimeLeft => 1000;
		public override string TexturePath => Macrocosm.EmptyTexPath;
		public override ParticleDrawLayer DrawLayer => ParticleDrawLayer.BeforeNPCs;
        public override int TrailCacheLenght => 24;

        public int PlayerID;
		public Color Color;
		public Color? SecondaryColor;
        public float Opacity;

        private float defScale;
        private float defRotation;
        private bool spawned;
        private bool collided;

        private BlendState blendStateOverride;
        private bool rainbow;

        public Player Player => Main.player[PlayerID];
        public DashPlayer DashPlayer => Player.GetModPlayer<DashPlayer>();
        public float Progress => DashPlayer.DashProgress;

        SpriteBatchState state;
        public override bool PreDrawAdditive(SpriteBatch spriteBatch, Vector2 screenPosition, Color lightColor)
		{
            bool specialRainbow = false;
            Texture2D slash = ModContent.Request<Texture2D>(Macrocosm.TextureEffectsPath + "Slash1").Value;
            Texture2D glow = ModContent.Request<Texture2D>(Macrocosm.TextureEffectsPath + "Circle5").Value;

            if (blendStateOverride is not null)
            {
                state.SaveState(spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin(blendStateOverride, state);
            }
            
            for (int i = 0; i < TrailCacheLenght; i++)
            {
                float trailProgress = MathHelper.Clamp((float)i / TrailCacheLenght, 0f, 1f);
                float scale = defScale - (Scale * trailProgress * 5f);

                bool even = i % 2 == 0;
                Color baseColor;
                if (SecondaryColor.HasValue)
                {
                    baseColor = even ? Color : SecondaryColor.Value;
                }
                else
                {
                    baseColor = Color;
                }


                if (rainbow)
                {
                    float rainbowProgress = Utility.WrapProgress(trailProgress + CelestialDisco.CelestialStyleProgress);
                    baseColor = Utility.MultiLerpColor(rainbowProgress, new(255,0,0), new(0,255,0), new(0,0,255));

                    #region Special code for Subtractive + Rainbow

                    specialRainbow = blendStateOverride == CustomBlendStates.Subtractive && SecondaryColor.HasValue;
                    if (specialRainbow && even)
                    {
                        baseColor = SecondaryColor.Value * (1f - trailProgress);
                        spriteBatch.End();
                        spriteBatch.Begin(blendStateOverride, state);
                    }
                    #endregion
                }

                Color color = scale < 0 ? baseColor * Progress * (1f - trailProgress) : baseColor * Progress;

                Vector2 position = scale < 0 ? OldPositions[i] + new Vector2(0, 55).RotatedBy(OldRotations[i]) : Vector2.Lerp(OldPositions[i], Center, Progress * (1f - trailProgress));
                spriteBatch.Draw(slash, position - screenPosition, null, color, OldRotations[i], slash.Size() / 2, scale, SpriteEffects.None, 0f);

                #region Special code for Midnight Rainbow
                if (specialRainbow && even)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(state);
                }
                #endregion
            }

            spriteBatch.Draw(slash, Center - screenPosition, null, Color * Progress, Rotation, slash.Size() / 2, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, Center - screenPosition, null, Color.Lerp(Color.White, Color, 0.75f).WithOpacity(0.5f) * Progress, defRotation, glow.Size() / 2, Utility.QuadraticEaseIn(Progress) * 0.7f, SpriteEffects.None, 0f);

            if (blendStateOverride is not null)
            {           
                spriteBatch.End();
                spriteBatch.Begin(state);
            }

            return false;
		}

        public override void AI()
        {
            if (!spawned)
            {
                spawned = true;
                defScale = Scale;
                defRotation = Rotation;

                CelestialBulwark.GetEffectColor(Player, out Color, out SecondaryColor, out blendStateOverride, out _, out rainbow);
            }

            if (DashPlayer.DashTimer <= 0)
                Kill();
            Scale = MathHelper.Lerp(Scale * 0.8f, defScale, Progress);

            Lighting.AddLight(Player.Center, Color.ToVector3() * 2f * Utility.QuadraticEaseIn(Progress));

            if (collided)
                 Color *= 0.9f;
            else
                 collided = DashPlayer.CollidedWithNPC;
 
            if (Player.velocity.Length() > 0.5f)
            {
                if(!collided)
                    Rotation = Player.velocity.ToRotation() - MathHelper.PiOver2;

                Position = Player.Center + new Vector2(0, 15).RotatedBy(Rotation);
            }
		}
	}
}
