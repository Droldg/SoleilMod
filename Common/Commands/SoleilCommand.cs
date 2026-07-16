using Soleil.Content.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Soleil.Common.Commands
{
	public class SoleilCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "soleil";
		public override string Usage => "/soleil";
		public override string Description => "Giver dig et Soleil Sigil.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player;
			player.QuickSpawnItem(new EntitySource_Misc("SoleilCommand"), ModContent.ItemType<SoleilSigil>());
			caller.Reply("Du modtog et Soleil Sigil.");
		}
	}
}
