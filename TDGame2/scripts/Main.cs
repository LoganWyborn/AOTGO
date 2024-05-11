using Godot;
using System;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public partial class Main : Node
{
	[Export]
	public PackedScene MobScene { get; set; }
	[Export]
	public PackedScene ShortRangeTowerScene { get; set; }
	[Export]
	public PackedScene RangedTowerScene { get; set; }
	[Export]
	public PackedScene MeleeTowerScene { get; set; }
	private TowerBase heldTower { get; set; }
	private TowerBase selectedTower { get; set; }
	public WaveSet waveSet { get; set; }
	public int health = 10;
	public int gold = 50;
	public bool waveCompleted = true;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Label>("HUD/CashLabel").Text = this.gold.ToString();
		GetNode<Label>("HUD/HealthLabel").Text = this.health.ToString();
		// All the stuff to read the json and prepare the waveset obj
		LoadLevelDataFromJson("res://LevelData.json");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!waveCompleted && GetTree().GetNodesInGroup("mobGroup").Count == 0 && GetTree().GetNodesInGroup("waveUnitGroup").Count == 0 && health > 0)
		{
			waveCompleted = true;
			if(waveSet.WaveCounter == waveSet.Waves.Length)
			{
				AnnouncementLabel announcement = GetNode<AnnouncementLabel>("HUD/AnnouncementLabel");
				announcement.Text = "Game Completed! Congrats!!";
				announcement.GetNode<Label>("SubAnnouncementLabel").Text = "Thanks for Playing!!";
				announcement.StopSlide();
			}
			else
			{
				AnnouncementLabel announcement = GetNode<AnnouncementLabel>("HUD/AnnouncementLabel");
				announcement.Text = "Wave " + waveSet.WaveCounter + " Completed!";
				announcement.GetNode<Label>("SubAnnouncementLabel").Text = "Congrats!";
				announcement.StartSlide();
			}
		}

		if(GetNode<TileMap>("TextureMap").Modulate.G < 1)
		{
			TileMap texMap = GetNode<TileMap>("TextureMap");
			texMap.Modulate = texMap.Modulate + (new Color("#000101"));
		}
	}
	
	public override void _Input(InputEvent ev)
	{
		if(IsInstanceValid(heldTower))
		{
			if(ev is InputEventMouseButton evMouseButton && evMouseButton.Pressed)
			{
				TileMap towerMap = GetNode<TileMap>("TowerMap");
				Vector2I towerMapCoords = towerMap.LocalToMap(towerMap.ToLocal(evMouseButton.Position));
				TileData td = towerMap.GetCellTileData(0, towerMapCoords);
				if(td != null && (bool)td.GetCustomData("canPlaceOn") && gold >= heldTower.cost)
				{
					heldTower.Position = towerMap.ToGlobal(towerMap.MapToLocal(towerMapCoords));
					heldTower.GetNode<Timer>("Cooldown").Start();
					EarnedGold(-heldTower.cost);
					heldTower.GetNode<Button>("Selector").Disabled = false;
					heldTower.SelectedTower += (tower) => SelectTower(tower);
					if(heldTower is MeleeTower)
						heldTower.GetNode<Sword>("Sword").Freeze = false;
					heldTower = null;
					towerMap.EraseCell(0, towerMapCoords);
					towerMap.Hide();
					GetNode<Button>("HUD/PlaceShortRangedButton").ButtonPressed = false;
					GetNode<Button>("HUD/PlaceRangedButton").ButtonPressed = false;
					GetNode<Button>("HUD/PlaceMeleeButton").ButtonPressed = false;
				}
			}
			else if(ev is InputEventMouseMotion evMouseMotion)
			{
				heldTower.Position = evMouseMotion.Position;
			}
		}
		else if(ev is InputEventMouseButton evMouseButton && evMouseButton.Pressed && evMouseButton.Position.Y < 800)
			DeselectTower();
	}

	private void OnUpgradeButtonPressed()
	{
		if(selectedTower is null || selectedTower.Upgrades.Level == selectedTower.Upgrades.MaxLevel 
								 || gold < selectedTower.Upgrades.Cost[selectedTower.Upgrades.Level])
			return;
		else if(selectedTower.Upgrades.Level == selectedTower.Upgrades.MaxLevel - 1)
		{
			EarnedGold(-selectedTower.Upgrades.Cost[selectedTower.Upgrades.Level]);
			selectedTower.Upgrades.Level++;
			selectedTower.Upgrades.Specialization = 0;
			SelectTower(selectedTower);
		}
		else
		{
			EarnedGold(-selectedTower.Upgrades.Cost[selectedTower.Upgrades.Level]);
			selectedTower.AttackDamage += selectedTower.Upgrades.DamageAdded[selectedTower.Upgrades.Level];
			selectedTower.AttackCooldown *= selectedTower.Upgrades.CooldownMultiplier[selectedTower.Upgrades.Level];
			selectedTower.GetNode<Timer>("Cooldown").WaitTime = selectedTower.AttackCooldown;
			selectedTower.targetingRangePx *= selectedTower.Upgrades.RangeMultiplier[selectedTower.Upgrades.Level];
			selectedTower.Upgrades.Level++;
			SelectTower(selectedTower);
		}
	}

	private void OnSpecializationOneButtonPressed()
	{
		if(selectedTower is null || selectedTower.Upgrades.Specialization != 0)
			return;
		selectedTower.SpecializationOne();
		SelectTower(selectedTower);
	}

	private void OnSpecializationTwoButtonPressed()
	{
		if(selectedTower is null || selectedTower.Upgrades.Specialization != 0)
			return;
		selectedTower.SpecializationTwo();
		SelectTower(selectedTower);
	}

	private void SelectTower(TowerBase tower)
	{
		if(selectedTower is not null)
		{
			selectedTower.isSelected = false;
			selectedTower.QueueRedraw();
		}
		selectedTower = tower;
		tower.isSelected = true;
		selectedTower.QueueRedraw();
		
		Control selectedTowerPanel = GetNode<Control>("HUD/SelectedTowerPanel");
		selectedTowerPanel.Show();
		selectedTowerPanel.GetNode<Label>("TitleLabel").Text = tower.name;
		float atkSpeed = Mathf.Round(100*(1/tower.AttackCooldown))/100f;
		selectedTowerPanel.GetNode<Label>("StatValuesLabel").Text = tower.Upgrades.Level +"\n"+atkSpeed+"/s\n"+tower.AttackDamage+"\n"+Math.Round(tower.targetingRangePx);
		if(tower.Upgrades.Level == tower.Upgrades.MaxLevel)
		{
			selectedTowerPanel.GetNode<Label>("UpgradeValuesLabel").Hide();
			selectedTowerPanel.GetNode<Label>("UpgradeCostLabel").Hide();
			selectedTowerPanel.GetNode<Label>("SpecializationUnlockLabel").Hide();
			selectedTowerPanel.GetNode<Button>("UpgradeButton").Disabled = true;
		}
		else if(tower.Upgrades.MaxLevel - tower.Upgrades.Level == 1)
		{
			selectedTowerPanel.GetNode<Label>("UpgradeValuesLabel").Show();
			selectedTowerPanel.GetNode<Label>("UpgradeCostLabel").Show();
			selectedTowerPanel.GetNode<Label>("SpecializationUnlockLabel").Show();
			selectedTowerPanel.GetNode<Button>("UpgradeButton").Disabled = false;
			selectedTowerPanel.GetNode<Label>("UpgradeValuesLabel").Text = "+1\t\t Specialization Unlocked";
			selectedTowerPanel.GetNode<Label>("UpgradeCostLabel").Text = tower.Upgrades.Cost[tower.Upgrades.Level].ToString();
		}
		else
		{
			selectedTowerPanel.GetNode<Label>("UpgradeValuesLabel").Show();
			selectedTowerPanel.GetNode<Label>("UpgradeCostLabel").Show();
			selectedTowerPanel.GetNode<Label>("SpecializationUnlockLabel").Show();
			selectedTowerPanel.GetNode<Button>("UpgradeButton").Disabled = false;
			int atkSpeedMulti = Mathf.RoundToInt(100*(1 - tower.Upgrades.CooldownMultiplier[tower.Upgrades.Level]));
			selectedTowerPanel.GetNode<Label>("UpgradeValuesLabel").Text = "+1\n"+"+"+atkSpeedMulti+"%\n"+"+"+tower.Upgrades.DamageAdded[tower.Upgrades.Level]+"\nx"+tower.Upgrades.RangeMultiplier[tower.Upgrades.Level];
			selectedTowerPanel.GetNode<Label>("UpgradeCostLabel").Text = tower.Upgrades.Cost[tower.Upgrades.Level].ToString();
		}

		Button specialOne = selectedTowerPanel.GetNode<Button>("SpecializationOneButton");
		Button specialTwo = selectedTowerPanel.GetNode<Button>("SpecializationTwoButton");
		specialOne.Text = tower.Upgrades.Specializations[0];
		specialTwo.Text = tower.Upgrades.Specializations[1];
		switch(tower.Upgrades.Specialization)
		{
			case 0:
				specialOne.Disabled = false;
				specialOne.Modulate = new Color("#000000");
				specialTwo.Disabled = false;
				specialTwo.Modulate = new Color("#000000");
				break;
			case 1:
				specialOne.Disabled = true;
				specialOne.Modulate = new Color("#00ff00");
				specialTwo.Disabled = true;
				specialTwo.Modulate = new Color("#ff0000");
				break;
			case 2:
				specialOne.Disabled = true;
				specialOne.Modulate = new Color("#ff0000");
				specialTwo.Disabled = true;
				specialTwo.Modulate = new Color("#00ff00");
				break;
			default:
				specialOne.Disabled = true;
				specialOne.Modulate = new Color("#000000");
				specialTwo.Disabled = true;
				specialTwo.Modulate = new Color("#000000");
				break;
		}
	}

	private void DeselectTower()
	{
		if(IsInstanceValid(heldTower))
			heldTower.QueueFree();
		if(!IsInstanceValid(selectedTower))
			return;
		selectedTower.isSelected = false;
		selectedTower.QueueRedraw();
		selectedTower = null;
		GetNode<Control>("HUD/SelectedTowerPanel").Hide();
	}

	private void LoadLevelDataFromJson(string filePath)
	{
		if(!FileAccess.FileExists(filePath))
		{
			GD.Print("File at '"+filePath+"' not found, unable to load level data.");
			return;
		}
		using var levelData = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

		bool inWaves = false, inWaveUnits = false, inWaveUnit = false, inMobToSpawn = false; 
		waveSet = new WaveSet();
		Wave wave = new Wave();
		WaveUnit unit = new WaveUnit();
		
		while(levelData.GetPosition() < levelData.GetLength())
		{
			string jsonString = levelData.GetLine();
			jsonString = jsonString.Dedent();

			if(inWaves)
			{
				if(inWaveUnits)
				{
					if(inWaveUnit)
					{
						if(inMobToSpawn)
						{
							if(jsonString.Contains("speed"))
							{
								jsonString = jsonString.TrimPrefix("\"speed\":").TrimSuffix(",");
								unit.MobToSpawn.speed = jsonString.ToFloat();
							}
							else if(jsonString.Contains("maxHitPoints"))
							{
								jsonString = jsonString.TrimPrefix("\"maxHitPoints\":").TrimSuffix(",");
								unit.MobToSpawn.maxHitPoints = jsonString.ToInt();
							}
							else if(jsonString.Contains("goldValue"))
							{
								jsonString = jsonString.TrimPrefix("\"goldValue\":").TrimSuffix(",");
								unit.MobToSpawn.goldValue = jsonString.ToInt();
							}
							else if(jsonString.Contains("Modulate"))
							{
								jsonString = jsonString.TrimPrefix("\"Modulate\":").TrimSuffix(",");
								jsonString = jsonString.Dedent().TrimPrefix("\"").TrimSuffix("\"");
								unit.MobToSpawn.Modulate = new Color(jsonString);
							}
							else if(jsonString.Contains('}'))
							{
								inMobToSpawn = false;
							}
						}
						else if(jsonString.Contains("NumToSpawn"))
						{
							jsonString = jsonString.TrimPrefix("\"NumToSpawn\":").TrimSuffix(",");
							unit.NumToSpawn = jsonString.ToInt();
						}
						else if(jsonString.Contains("StartDelay"))
						{
							jsonString = jsonString.TrimPrefix("\"StartDelay\":").TrimSuffix(",");
							unit.StartDelay = jsonString.ToFloat();
						}
						else if(jsonString.Contains("SpawnDelay"))
						{
							jsonString = jsonString.TrimPrefix("\"SpawnDelay\":").TrimSuffix(",");
							unit.SpawnDelay = jsonString.ToFloat();
						}
						else if(jsonString.Contains("MobToSpawn"))
						{
							inMobToSpawn = true;
						}
						else if(jsonString.Contains('}'))
						{
							inWaveUnit = false;
							wave.WaveUnits = wave.WaveUnits.Append<WaveUnit>(unit).ToArray<WaveUnit>();
						}
					}
					else if(jsonString.Contains('{'))
					{
						inWaveUnit = true;
						unit = new WaveUnit();
					}
					else if(jsonString.Contains(']'))
					{
						inWaveUnits = false;
						waveSet.Waves = waveSet.Waves.Append<Wave>(wave).ToArray<Wave>();
					}
				}
				else if(jsonString.Contains("WaveUnits"))
				{
					inWaveUnits = true;
					wave = new Wave();
				}
				else if(jsonString.Contains(']'))
				{
					inWaves = false;
					break;
				}
			}
			else if(jsonString.Contains("Waves"))
			{
				inWaves = true;
			}
		}
		GD.Print("Finished LevelLoading");
	}

	private void StartWave()
	{
		waveCompleted = false;
		if(waveSet.WaveCounter == waveSet.Waves.Length)
			return;

		foreach(WaveUnit unit in waveSet.Waves[waveSet.WaveCounter].WaveUnits)
		{
			AddChild(unit);
			unit.SpawnMob += (MobPropsContainer mob) => SpawnMob(mob);
		}
		waveSet.WaveCounter++;
		waveCompleted = false;
		GetNode<Label>("HUD/WaveLabel").Text = "Wave " + waveSet.WaveCounter;
		AnnouncementLabel announcement = GetNode<AnnouncementLabel>("HUD/AnnouncementLabel");
		announcement.Text = "Wave " + waveSet.WaveCounter;
		announcement.GetNode<Label>("SubAnnouncementLabel").Text = "Slimes incoming...";
		if(waveSet.WaveCounter == waveSet.Waves.Length)
			announcement.GetNode<Label>("SubAnnouncementLabel").Text += "\nFinal Wave!!";
		announcement.StartSlide();
	}

	private void SpawnMob(MobPropsContainer mob)
	{
		Mob mobInstance = MobScene.Instantiate<Mob>();
		mobInstance.maxHitPoints = mob.maxHitPoints;
		mobInstance.speed = mob.speed;
		mobInstance.goldValue = mob.goldValue;
		mobInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Modulate = mob.Modulate;
		mobInstance.MobKilled += (int gold) => EarnedGold(gold);
		mobInstance.MobEscaped += (int damage) => TakenDamage(damage); 
		AddChild(mobInstance);
	}

	private void EarnedGold(int gold)
	{
		this.gold += gold;
		GetNode<Label>("HUD/CashLabel").Text = this.gold.ToString();
	}

	private void TakenDamage(int damage)
	{
		this.health -= damage;
		GetNode<Label>("HUD/HealthLabel").Text = this.health.ToString();
		if(health <= 0)
		{
			AnnouncementLabel announcement = GetNode<AnnouncementLabel>("HUD/AnnouncementLabel");
			announcement.Text = "Game Over!";
			announcement.GetNode<Label>("SubAnnouncementLabel").Text = "Better luck next time!";
			announcement.StopSlide();
			announcement.Show();
		}
		GetNode<TileMap>("TextureMap").Modulate = new Color("#ff8080");
	}
	
	private void OnSingleButtonPressed()
	{
		Mob mobInstance = MobScene.Instantiate<Mob>();
		mobInstance.MobKilled += (int gold) => EarnedGold(gold);
		mobInstance.MobEscaped += (int damage) => TakenDamage(damage); 
		AddChild(mobInstance);
	}

	private void OnWaveButtonPressed()
	{
		StartWave();
	}

	private void OnWaveButtonToggled(bool button_pressed)
	{
		Timer timer = GetNode<Timer>("MobSpawnTimer");
		if(timer.IsStopped())
			timer.Start();
		else
			timer.Stop();
	}
	
	private void OnMobSpawnTimerTimeout()
	{
		OnSingleButtonPressed();
	}
	
	private void OnClearButtonPressed()
	{
		GetTree().CallGroup("mobGroup", Node.MethodName.QueueFree);
	}

	private void OnResetButtonPressed()
	{
		GetNode<ConfirmationDialog>("ResetPopup").Show();
	}

	private void OnResetPopupConfirmed()
	{
		GetTree().ChangeSceneToFile("res://Main.tscn");
	}

	private void OnPlaceShortRangeButtonToggled(bool button_pressed)
	{
		if(button_pressed)
		{
			DeselectTower();
			GetNode<TileMap>("TowerMap").Show();
			heldTower = ShortRangeTowerScene.Instantiate<ShortRangeTower>();
			AddChild(heldTower);
			if(heldTower.cost > gold)
			{
				GetNode<TileMap>("TowerMap").Hide();
				heldTower.QueueFree();
				GetNode<Button>("HUD/PlaceShortRangedButton").ButtonPressed = false;
			}
		}
		else
		{
			if(IsInstanceValid(heldTower))
			{
				heldTower.QueueFree();
			}
			GetNode<TileMap>("TowerMap").Hide();
		}
	}
	
	private void OnPlaceRangedButtonToggled(bool button_pressed)
	{
		if(button_pressed)
		{
			DeselectTower();
			GetNode<TileMap>("TowerMap").Show();
			heldTower = RangedTowerScene.Instantiate<RangedTower>();
			AddChild(heldTower);
			if(heldTower.cost > gold)
			{
				GetNode<TileMap>("TowerMap").Hide();
				heldTower.QueueFree();
				GetNode<Button>("HUD/PlaceRangedButton").ButtonPressed = false;
			}
		}
		else
		{
			if(IsInstanceValid(heldTower))
			{
				heldTower.QueueFree();
			}
			GetNode<TileMap>("TowerMap").Hide();
		}
	}

	private void OnPlaceMeleeButtonToggled(bool button_pressed)
	{
		if(button_pressed)
		{
			DeselectTower();
			GetNode<TileMap>("TowerMap").Show();
			heldTower = MeleeTowerScene.Instantiate<MeleeTower>();
			AddChild(heldTower);
			if(heldTower.cost > gold)
			{
				GetNode<TileMap>("TowerMap").Hide();
				heldTower.QueueFree();
				GetNode<Button>("HUD/PlaceMeleeButton").ButtonPressed = false;
			}
		}
		else
		{
			if(heldTower != null)
			{
				heldTower.QueueFree();
				heldTower = null;
			}
			GetNode<TileMap>("TowerMap").Hide();
		}
	}
}
