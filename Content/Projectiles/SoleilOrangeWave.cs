using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Soleil.Content.Projectiles
{
	public class SoleilOrangeWave : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 8;
			ProjectileID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 190;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Projectile.ai[0]++;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Projectile.ai[0] < 45f)
			{
				Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
				if (target.active && !target.dead)
				{
					Vector2 desired = Projectile.DirectionTo(target.Center) * Projectile.velocity.Length();
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.018f);
				}
			}

			Projectile.velocity *= 1.002f;

			if (!Main.dedServ && Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.OrangeTorch);
				dust.noGravity = true;
				dust.velocity *= 0.2f;
			}
		}
	}
}
