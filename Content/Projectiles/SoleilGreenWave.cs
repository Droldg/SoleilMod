using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Soleil.Content.Projectiles
{
	public class SoleilGreenWave : ModProjectile
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
			Projectile.timeLeft = 210;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Projectile.ai[0]++;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Vector2 wave = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * (float)System.Math.Sin(Projectile.ai[0] * 0.18f) * 0.18f;
			Projectile.velocity += wave;

			if (!Main.dedServ && Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch);
				dust.noGravity = true;
				dust.velocity *= 0.2f;
			}
		}
	}
}
