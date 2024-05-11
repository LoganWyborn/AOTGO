using Godot;
using System;
using System.Dynamic;

public partial class ShortRangeTower : TowerBase
{
	[Export] 
	public PackedScene projScene { get; set; }
	[Export]
	public float projSpeed { get; set; }
	[Export]
	public float projLifetime { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.name = "Short Range Tower";
		this.Upgrades = new UpgradeSet(
			5,
			new int[]{0, 20, 40, 70, 125},
			new int[]{0, 1, 2, 2},
			new float[]{0, .90f, .85f, .85f},
			new float[]{0, 1.1f, 1.05f, 1.05f},
			new string[]{"Minigun Tower", "Shotgun Tower"}
		);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!onCooldown)
		{
			FindTarget();
			if(target != null)
				AttackMob(target);
		}
	}

    public override void AttackMob(Mob target)
    {
		onCooldown = true;
		Node main = GetNode<Node>("../../Main");
        Projectile proj = projScene.Instantiate<Projectile>();
		proj.targetDir = Position.AngleToPoint(target.Position);
		proj.damage = AttackDamage;
		proj.Position = Position;
		proj.speed = projSpeed;
		proj.lifetime = projLifetime;
		main.AddChild(proj);
		if(Upgrades.Specialization == 2)
		{
			Projectile proj2 = projScene.Instantiate<Projectile>();
			Projectile proj3 = projScene.Instantiate<Projectile>();
			proj2.targetDir = Position.AngleToPoint(target.Position) - 0.35f;
			proj3.targetDir = Position.AngleToPoint(target.Position) + 0.35f;
			proj2.damage = AttackDamage - 2;
			proj3.damage = AttackDamage - 2;
			proj2.Position = Position;
			proj3.Position = Position;
			proj2.speed = projSpeed;
			proj3.speed = projSpeed;
			proj2.lifetime = projLifetime;
			proj3.lifetime = projLifetime;
			main.AddChild(proj2);
			main.AddChild(proj3);
		}
		GetNode<Timer>("Cooldown").Start();
    }
   
    public override void SpecializationOne()
    {
        Upgrades.Specialization = 1;
		name = Upgrades.Specializations[0];
		targetingRangePx *= 1.1f;
		AttackCooldown /= 2;
		GetNode<Timer>("Cooldown").WaitTime = AttackCooldown;

    }

    public override void SpecializationTwo()
    {
        Upgrades.Specialization = 2;
		name = Upgrades.Specializations[1];
		targetingRangePx *= 0.9f;
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
