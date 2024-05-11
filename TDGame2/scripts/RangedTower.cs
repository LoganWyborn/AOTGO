using Godot;
using System;

public partial class RangedTower : TowerBase
{
	[Export]
	public PackedScene projScene { get; set; }
	[Export]
	public float projSpeed { get; set; }
	
	public override void _Ready()
	{
		this.name = "Long Range Tower";
		this.Upgrades = new UpgradeSet(
			5,
			new int[]{0, 30, 50, 80, 125},
			new int[]{0, 2, 3, 3},
			new float[]{0, .90f, .90f, .85f},
			new float[]{0, 1.1f, 1.2f, 1.2f},
			new string[]{"Double Shot Tower", "Sniper Tower"}
		);
		GetNode<Timer>("Cooldown").WaitTime = AttackCooldown;
	}
	
	public override void _Process(double delta)
	{
		if(!onCooldown)
		{
			FindTarget();
			if(target != null)
				AttackMob(target);
		}
	}

	public override void SpecializationOne()
	{
		Upgrades.Specialization = 1;
		name = Upgrades.Specializations[0];
	}

	public override void SpecializationTwo()
	{
		Upgrades.Specialization = 2;
		name = Upgrades.Specializations[1];
		targetingRangePx *= 1.7f;
		AttackCooldown *= 1.25f;
		AttackDamage += 6;
		GetNode<Timer>("Cooldown").WaitTime = AttackCooldown;
	}
	
	public override async void AttackMob(Mob target)
	{
		Projectile proj = projScene.Instantiate<Projectile>();
		proj.target = target;
		proj.damage = AttackDamage;
		proj.Position = Position;
		proj.speed = projSpeed;
		GetNode<Node>("../../Main").AddChild(proj);
		onCooldown = true;
		if(Upgrades.Specialization == 1)
		{
			await ToSignal(GetTree().CreateTimer(0.2f), Timer.SignalName.Timeout);
			proj = projScene.Instantiate<Projectile>();
			proj.target = target;
			proj.damage = AttackDamage;
			proj.Position = Position;
			GetNode<Node>("../../Main").AddChild(proj);
		}
		GetNode<Timer>("Cooldown").Start();
	}
	
	private void OnSelectorPressed()
	{
		GD.Print(name + "clicked on");
		EmitSignal(SignalName.SelectedTower, this);
	}

	private void OnCooldownTimeout()
	{
		onCooldown = false;
	}
}
