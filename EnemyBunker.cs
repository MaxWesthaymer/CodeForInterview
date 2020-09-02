using System;
using GameStatics;
using GameConstants;
using Newtonsoft.Json;
using UniRx;
using System.Collections.Generic;
using BalanceSettings;
using UnityEngine;

public class EnemyBunker : ACharacter 
{    
    #region Private Fields   

    private static readonly int LootCount = Balance.PVP.stat.LootCount;
    private static readonly int LootMin = Balance.PVP.stat.LootMin;
    private static readonly int LootMax = Balance.PVP.stat.LootMax;
    private static readonly float MultiplierHealth = Balance.PVP.stat.MultiplierHealth;
    private readonly List<int> _bunkerGenerateLevel = Balance.PVP.stat.BunkerGenerateLevel;
    #endregion
    
    #region Properties
    [JsonProperty] public EEnemyType Type { get; private set; }
    [JsonProperty] public int Loot { get; private set; }
    [JsonProperty] public int Id { get; private set; }
    [JsonProperty] public bool CanDisplay { get; set; }
    [JsonProperty] public string CreatorName { get; private set; }
    [JsonProperty] public new bool IsAlive => CurrentHealth.Value > 0;
    [JsonProperty] public BoolReactiveProperty IsResearching { get; private set; }
    [JsonProperty] public ReactiveProperty<DateTime> TimeLeft { get; private set; }
    [JsonProperty] public bool IsWin { get; private set; }
    
    [JsonProperty]
    public DateTime StartTime { get; private set; }
    [JsonProperty]
    public bool IsForeignAttackWin { get; set; }
    [JsonProperty]
    public bool IsRevengeWin { get; set; }

    [JsonProperty]
    public bool IsRevenged { get; set; } = false;
    #endregion

    #region Constructors & Destructor

    public EnemyBunker(bool isLoser = false)
    {
        Id = StageHelper.corral.GenerateId(true);
        Level.Value = GenerateLevel();
        GenerateName();
        StartTime = DateTime.Now;
        IsForeignAttackWin = false;
        IsRevengeWin = false;
        //IsRevenged = false;
        IsResearching = new BoolReactiveProperty(false);
        TimeLeft = new ReactiveProperty<DateTime>();

        Loot = LootCount;
        CurrentHealth.Value = isLoser ? 0 : Health.Value = CalculateHealth();
        Debug.Log("CurrentHealth.Value = " + CurrentHealth.Value);
        Strength.Value = CalculateStrength();
        CanDisplay = true;
        SetWin(false);
    }

    #endregion

    #region Public Methods
    public void Rebirth() => CurrentHealth.Value = Health.Value;
    public int GetMinLootLevel() => LootMin;
    public int GetMaxLootLevel() => LootMax;
    
    public void SetWin(bool value)
    {
        IsWin = value;
        if (StageHelper.attackingZombie.attakingZombies.Contains(this))
        {
            IsRevengeWin = !value;
        }
    }

    public void SetResearch(bool isResearching)
    {
        IsResearching.Value = isResearching;
    }
    public void SetRevengeResultForTest()
    {
        IsRevenged = true;
        IsRevengeWin = true;
        StageHelper.attackingZombie.SaveRevengedZombie(this);
    }

    public void SetTimeLeft(float newTime)
    {
        var dt = new DateTime().AddSeconds(newTime);
        TimeLeft.Value = dt;
    }
    
    public void SetStartTime(float newTime)
    {
        StartTime = DateTime.Now.AddSeconds(newTime);
    }

    public int GetGenerateLevel() => _bunkerGenerateLevel[Level.Value - 2];
    #endregion

    #region Private Methods
    private int CalculateHealth() => (int)(_bunkerGenerateLevel[Level.Value-2] * MultiplierHealth);
    private int CalculateStrength() => (int) (_bunkerGenerateLevel[Level.Value - 2] * Balance.PVP.stat.MultiplierStrength);  
    private void GenerateName() => Name = $"{"PlayerName"}{RandomHelper.random.Next(1,10)}";
    
    private int GenerateLevel()
     {
         var level = StageHelper.bunker.Level.Value;
         var enemyLevel = RandomHelper.random.Next(2, level + 2);
         return enemyLevel > 5 ? 5 : enemyLevel;
     }
    #endregion	
}
