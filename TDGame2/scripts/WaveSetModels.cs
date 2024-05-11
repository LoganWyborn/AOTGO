using Godot;
using System;

// These models are for structural reference, probably won't actually be necessary in code. 
// ^^ *Clown Emoji*

public partial class WaveSet : Node
{ // WaveSet is a collection of Wave objs
	public Wave[] Waves { get; set; } = new Wave[0];
	public int WaveCounter { get; set; } = 0;
}

public partial class Wave : Node
{ // Wave is a collection of WaveUnit objs
	public WaveUnit[] WaveUnits { get; set; } = new WaveUnit[0];
}

public partial class WaveUnit : Node
{ 
	[Signal]
	public delegate void SpawnMobEventHandler(MobPropsContainer mobToSpawn);
/*
	Each WaveUnit represents a pack of mobs to be spawned during a wave. 
	Starting at StartDelay seconds into the wave, one MobToSpawn will be spawned every SpawnDelay seconds
	until NumToSpawn is reached.
*/
	public MobPropsContainer MobToSpawn { get; set; } = new MobPropsContainer();
	public int NumToSpawn { get; set; }
	public float StartDelay { get; set; }
	public float SpawnDelay { get; set; }
	public Timer SpawnTimer { get; set; } = new Timer();

	public override void _Ready()
	{
		SpawnTimer.OneShot = false;
		SpawnTimer.WaitTime = SpawnDelay;
		AddChild(SpawnTimer);
		AddToGroup("waveUnitGroup");
		SpawnMobs();
	}

	public async void SpawnMobs()
	{
		await ToSignal(GetTree().CreateTimer(StartDelay), SceneTreeTimer.SignalName.Timeout);
		SpawnTimer.Start();
		while(NumToSpawn-- > 0)
        {
			EmitSignal(WaveUnit.SignalName.SpawnMob, MobToSpawn);
            await ToSignal(SpawnTimer, Timer.SignalName.Timeout);
        }
		QueueFree();
	}
}
/* Template for WaveSet json
{
	"Waves": 
	[
		{
			"WaveUnits": 
			[
				{
					"MobToSpawn": 
					{
						"speed": 0,
						"maxHitPoints": 0,
						"goldValue": 0,
						"Modulate": "#000000"
					},
					"NumToSpawn": 0,
					"StartDelay": 0,
					"SpawnDelay": 0
				}
			]
		}
	]
}
*/
