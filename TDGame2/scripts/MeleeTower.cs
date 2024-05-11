using Godot;
using System;

public partial class MeleeTower : TowerBase
{
	public float swingSpeed;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.name = "Sword Tower";
		this.swingSpeed = 2f*Mathf.Pi;
		this.Upgrades = new UpgradeSet(
			5,
			new int[]{0, 30, 60, 95, 150},
			new int[]{0, 2, 2, 2},
			new float[]{0, .95f, .95f, .90f},
			new float[]{0, 1f, 1f, 1f},
			new string[]{"Cleaver Tower", "Orbiter Tower"}
		);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!onCooldown && Upgrades.Specialization != 2)
		{
			FindTarget();
			if(target != null)
				AttackMob(target);
		}
	}

    public override async void AttackMob(Mob target)
    {
		Sword sword = GetNode<Sword>("Sword");
		onCooldown = true;
		if(Upgrades.Specialization == 2)
		{
			sword.Show();
			sword.AngularVelocity = swingSpeed;
			return;
		}

		await ToSignal(sword, Sword.SignalName.SwordRotationSet);
		GD.Print("[Before Attack]Tower's position: "+Position+", Target's position: "+target.Position+", Sword's Position: "+sword.Position+", Sword Rotation: "+sword.Rotation);
		sword.Show();
		sword.AngularVelocity = swingSpeed;
		await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);

		if(Upgrades.Specialization != 2)
		{
			sword.AngularVelocity = 0;
			sword.Hide();
			GetNode<Timer>("Cooldown").Start();
		}
		
		if(IsInstanceValid(target))
			GD.Print("[After Attack]Tower's position: "+Position+", Target's position: "+target.Position+", Sword's Position: "+sword.Position+", Sword Rotation: "+sword.Rotation);
		else
			GD.Print("[After Attack]Target is disposed");
    }

    public override void SpecializationOne()
    {
        Upgrades.Specialization = 1;
		name = Upgrades.Specializations[0];
		GetNode<CollisionShape2D>("Sword/CollisionShape2D").Scale *= new Vector2(2,2);
		GetNode<Sprite2D>("Sword/Sprite2D").Scale *= new Vector2(2,2);
		targetingRangePx *= 2;
		AttackDamage *= 2;
		AttackCooldown *= 1.5f;
		GetNode<Timer>("Cooldown").WaitTime = AttackCooldown;
    }

    public override void SpecializationTwo()
    {
        Upgrades.Specialization = 2;
		name = Upgrades.Specializations[1];
		//GetNode<CollisionShape2D>("Sword/CollisionShape2D").Scale *= new Vector2(1.2f,1.2f);
		//GetNode<Sprite2D>("Sword/Sprite2D").Scale *= new Vector2(1.2f,1.2f);
		GetNode<Sword>("Sword").Show();
		//targetingRangePx *= 1.2f;
		onCooldown = true;
    }

	private void OnSelectorPressed()
	{
		GD.Print(name + " clicked on");
		EmitSignal(SignalName.SelectedTower, this);
	}
	
	private void OnCooldownTimeout()
	{
		GD.Print("Cooldown Timeout");
		onCooldown = false;
	}
}
