using Godot;
using System;

public abstract partial class TowerBase : Node2D
{
	public abstract void SpecializationOne();
	public abstract void SpecializationTwo();
	public abstract void AttackMob(Mob target);
	[Signal]
	public delegate void SelectedTowerEventHandler(TowerBase tower);
    [Export]
	public float targetingRangePx = 0;
	[Export]
	public float AttackCooldown = 1;
	[Export] 
	public int AttackDamage = 0;
	[Export]
	public int cost = 0;
	public UpgradeSet Upgrades { get; set; }
	public string name = "TowerBase";
	public bool onCooldown = true;
	public bool isSelected = true;
	public bool hasBeenPlaced = false;
	public Mob target;

	public override void _Draw()
	{
		if(isSelected)
			DrawCircle(Vector2.Zero, targetingRangePx, new Color(0,0,0,0.35f));
	}
	
	public void FindTarget()
	{
		var mobs = GetTree().GetNodesInGroup("mobGroup");
			if(mobs.Count != 0)
			{
				float highestProgress = -1f;
				Mob currentTarget = null;
				foreach(Mob mob in mobs)
				{
					//GD.Print("Mob at "+mob.Position);
					if((this.Position - mob.Position).Length() < targetingRangePx && mob.progressRatio > highestProgress)
					{
						currentTarget = mob;
						highestProgress = mob.progressRatio;
					}
				}
				target = currentTarget;
			}
			else
				target = null;
	}

}

public class UpgradeSet
{
	public byte Level { get; set; } = 1;
	public byte MaxLevel { get; set; }
	public int[] Cost { get; set; } 
	public int[] DamageAdded { get; set; }
	public float[] CooldownMultiplier { get; set; }
	public float[] RangeMultiplier { get; set; }
	public String[] Specializations { get; set; }
	public int Specialization = -1;

	public UpgradeSet(byte maxLevel, int[] cost, int[] damageAdded, float[] cooldownMultiplier, float[] rangeMultiplier, String[] specializations)
	{
		MaxLevel = maxLevel;
		Cost = cost;
		DamageAdded = damageAdded;
		CooldownMultiplier = cooldownMultiplier;
		RangeMultiplier = rangeMultiplier;
		Specializations = specializations;
	}
}