using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Soleil.Content.Projectiles;

namespace Soleil.Content.NPCs
{
	public class SoleilBoss : ModNPC
	{
		private enum AttackState
		{
			Hover = 0,
			Dash = 1,
			Spiral = 2
		}

		private const int HoverDuration = 360;
		private const int SpiralDuration = 210;
		private const int DashChargeTime = 45;
		private const int DashMoveTime = 34;
		private const int DashRestTime = 24;

		private ref float State => ref NPC.ai[0];
		private ref float Timer => ref NPC.ai[1];
		private ref float Counter => ref NPC.ai[2];
		private ref float LocalTimer => ref NPC.ai[3];

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
		}

		public override void SetDefaults()
		{
			NPC.width = 82;
			NPC.height = 82;
			NPC.damage = 72;
			NPC.defense = 32;
			NPC.lifeMax = 34000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.value = Item.buyPrice(gold: 15);
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.lavaImmune = true;
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.netAlways = true;
			Music = MusicID.Boss3;
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
		{
			NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance * bossAdjustment);
			NPC.damage = (int)(NPC.damage * 0.85f);
		}

		public override void AI()
		{
			NPC.TargetClosest(false);
			Player target = Main.player[NPC.target];

			if (!IsValidTarget(target))
			{
				Despawn();
				return;
			}

			Lighting.AddLight(NPC.Center, 0.45f, 0.12f, 0.45f);
			Timer++;
			LocalTimer++;

			switch ((AttackState)(int)State)
			{
				case AttackState.Hover:
					DoHover(target);
					break;
				case AttackState.Dash:
					DoDash(target);
					break;
				case AttackState.Spiral:
					DoSpiral(target);
					break;
			}

			NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.025f, -0.28f, 0.28f);
			SpawnAmbientDust();
		}

		private bool IsValidTarget(Player target)
		{
			return target.active && !target.dead && Vector2.Distance(NPC.Center, target.Center) < 4200f;
		}

		private void Despawn()
		{
			NPC.TargetClosest(false);
			NPC.velocity.Y -= 0.2f;
			NPC.timeLeft = 10;
		}

		private void DoHover(Player target)
		{
			float phaseSpeed = Phase3 ? 1.28f : Phase2 ? 1.14f : 1f;
			Vector2 hoverOffset = new Vector2((float)System.Math.Sin(Timer / 48f) * 180f, -220f);
			MoveToward(target.Center + hoverOffset, 0.34f * phaseSpeed, 14f * phaseSpeed);

			int fireRate = Phase3 ? 44 : Phase2 ? 54 : 66;
			if (Timer % fireRate == 0)
			{
				FireAimedProjectile(target, Counter % 2 == 0);
				Counter++;
			}

			if (Timer >= HoverDuration)
			{
				ChangeState(AttackState.Dash);
			}
		}

		private void DoDash(Player target)
		{
			int dashesNeeded = Phase3 ? 4 : Phase2 ? 3 : 2;
			int cycle = DashChargeTime + DashMoveTime + DashRestTime;
			int cycleTimer = (int)Timer % cycle;

			if (cycleTimer == 1)
			{
				SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
				ChargeDust();
				NPC.velocity *= 0.25f;
			}

			if (cycleTimer < DashChargeTime)
			{
				Vector2 chargePosition = target.Center + new Vector2(target.Center.X > NPC.Center.X ? -260f : 260f, -80f);
				MoveToward(chargePosition, 0.55f, 12f);
				return;
			}

			if (cycleTimer == DashChargeTime)
			{
				Vector2 predicted = target.Center + target.velocity * 18f;
				float dashSpeed = Phase3 ? 23f : Phase2 ? 20f : 17f;
				NPC.velocity = NPC.DirectionTo(predicted) * dashSpeed;
				SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
				NPC.netUpdate = true;
			}

			if (cycleTimer > DashChargeTime + DashMoveTime)
			{
				NPC.velocity *= 0.9f;
			}

			if (Timer >= cycle * dashesNeeded)
			{
				ChangeState(AttackState.Spiral);
			}
		}

		private void DoSpiral(Player target)
		{
			MoveToward(target.Center + new Vector2(0f, -260f), 0.28f, Phase3 ? 11f : 9f);

			int rate = Phase3 ? 18 : Phase2 ? 24 : 30;
			if (Timer % rate == 0)
			{
				FireSpiralVolley();
			}

			if (Timer >= SpiralDuration)
			{
				ChangeState(AttackState.Hover);
			}
		}

		private void MoveToward(Vector2 destination, float acceleration, float maxSpeed)
		{
			Vector2 desiredVelocity = NPC.DirectionTo(destination) * maxSpeed;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, acceleration / maxSpeed);
		}

		private void FireAimedProjectile(Player target, bool green)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			int type = green ? ModContent.ProjectileType<SoleilGreenWave>() : ModContent.ProjectileType<SoleilOrangeWave>();
			int damage = Main.expertMode ? 32 : 44;
			Vector2 velocity = NPC.DirectionTo(target.Center) * (green ? 8.2f : 7.4f);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, damage, 0f, Main.myPlayer);
			SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
		}

		private void FireSpiralVolley()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			int shots = Phase3 ? 9 : Phase2 ? 8 : 7;
			float rotation = Timer * 0.035f;
			for (int i = 0; i < shots; i++)
			{
				if (!Phase3 && i % 4 == 2)
				{
					continue;
				}

				bool green = i % 2 == 0;
				int type = green ? ModContent.ProjectileType<SoleilGreenWave>() : ModContent.ProjectileType<SoleilOrangeWave>();
				Vector2 velocity = Vector2.UnitX.RotatedBy(rotation + MathHelper.TwoPi * i / shots) * 6.5f;
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, Main.expertMode ? 30 : 40, 0f, Main.myPlayer);
			}

			SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
		}

		private void ChangeState(AttackState nextState)
		{
			State = (float)nextState;
			Timer = 0f;
			Counter = 0f;
			LocalTimer = 0f;
			NPC.netUpdate = true;
		}

		private bool Phase2 => NPC.life < NPC.lifeMax * 0.5f;
		private bool Phase3 => NPC.life < NPC.lifeMax * 0.24f;

		private void SpawnAmbientDust()
		{
			if (Main.dedServ || Main.rand.NextBool(4))
			{
				return;
			}

			int dustType = Main.rand.NextBool(3) ? DustID.GemAmethyst : Main.rand.NextBool() ? DustID.GreenTorch : DustID.OrangeTorch;
			Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, Scale: 1.1f);
			dust.noGravity = true;
			dust.velocity *= 0.25f;
		}

		private void ChargeDust()
		{
			if (Main.dedServ)
			{
				return;
			}

			for (int i = 0; i < 28; i++)
			{
				Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 28f) * 3f;
				Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.PurpleTorch, velocity);
				dust.noGravity = true;
			}
		}

		public override void OnKill()
		{
			if (Main.dedServ)
			{
				return;
			}

			for (int i = 0; i < 60; i++)
			{
				int dustType = i % 3 == 0 ? DustID.GreenTorch : i % 3 == 1 ? DustID.OrangeTorch : DustID.GemAmethyst;
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);
				dust.noGravity = true;
				dust.velocity *= 2.4f;
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			LeadingConditionRule normalMode = new LeadingConditionRule(new Conditions.NotExpert());
			normalMode.OnSuccess(ItemDropRule.Common(ItemID.HallowedBar, 1, 15, 30));
			normalMode.OnSuccess(ItemDropRule.Common(ItemID.SoulofMight, 1, 12, 20));
			normalMode.OnSuccess(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 5, 15));
			npcLoot.Add(normalMode);

			LeadingConditionRule expertMode = new LeadingConditionRule(new Conditions.IsExpert());
			expertMode.OnSuccess(ItemDropRule.Common(ItemID.HallowedBar, 1, 20, 34));
			expertMode.OnSuccess(ItemDropRule.Common(ItemID.SoulofMight, 1, 18, 28));
			expertMode.OnSuccess(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 5, 15));
			npcLoot.Add(expertMode);
		}
	}
}
