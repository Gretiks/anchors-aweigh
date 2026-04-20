using System;

public class PlayerShip : BaseShip
{
	public PlayerShip()
	{
		this.hitPoints = 100;
	}

	public override void TakeDamage(int damage)
	{
		this.hitPoints -= damage;
		MenuManager.Instance.RefreshShipHealth(this);
	}
	
	protected override void DestroyShip()
	{
	}
}
