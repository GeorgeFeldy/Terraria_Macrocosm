using Macrocosm.Content.Biomes;
using Macrocosm.Content.Projectiles.Global;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Macrocosm.Content.NPCs.Global
{
	public interface IMoonEnemy 
	{

	}

	public class MoonEnemyNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.ModNPC is IMoonEnemy;

		public override void SetDefaults(NPC entity)
		{
			entity.ModNPC.SpawnModBiomes = entity.ModNPC.SpawnModBiomes.Prepend(ModContent.GetInstance<MoonBiome>().Type).ToArray();
		}
	}
}