using Soleil.Content.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Soleil.Content.Items
{
	public class SoleilSigil : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(silver: 50);
			Item.UseSound = SoundID.Item41;
			Item.consumable = false;
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(ModContent.NPCType<SoleilBoss>());
		}

		public override bool? UseItem(Player player)
		{
			int bossType = ModContent.NPCType<SoleilBoss>();
			SoundEngine.PlaySound(SoundID.Roar, player.Center);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: bossType);
			}
			else
			{
				NPC.SpawnOnPlayer(player.whoAmI, bossType);
			}

			return true;
		}
	}
}
