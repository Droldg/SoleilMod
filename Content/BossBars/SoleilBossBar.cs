using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Soleil.Content.NPCs;
using Terraria;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace Soleil.Content.BossBars
{
	public class SoleilBossBar : ModBossBar
	{
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
		{
			return ModContent.Request<Texture2D>("Soleil/Content/NPCs/SoleilBoss_Head_Boss");
		}

		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
		{
			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active || npc.ModNPC is not SoleilBoss)
			{
				return false;
			}

			life = npc.life;
			lifeMax = npc.lifeMax;
			shield = 0f;
			shieldMax = 0f;
			return true;
		}
	}
}
