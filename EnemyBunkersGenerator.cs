using System;
using UnityEngine;
using GameStatics;

public class EnemyBunkersGenerator : MonoBehaviour {

    #region Private Fields
    private static EnemyBunkersGenerator _instance;
    #endregion
	
    #region Unity Methods
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        RemoveBunkerTimer();
        StartBunkersTimer();
    }

    private void Update()
    {
        if (StageHelper.corral.EnemyBunkers.Count < 1)
        {
            return;
        }
        RemoveBunkerTimer();
    }
    #endregion
    
    #region Private Methods
    private void StartBunkersTimer()
    {
        var ts = Convert.ToInt32((DateTime.Now - BunkersTimeHelper.bunkersTime.Value).TotalSeconds);
        var count = ts / BunkersTimeHelper.FrequencyCheckOfBunker;

        var remain = ts < BunkersTimeHelper.FrequencyCheckOfBunker
            ? BunkersTimeHelper.FrequencyCheckOfBunker - ts
            : ts % BunkersTimeHelper.FrequencyCheckOfBunker;

        for (var i = 0; i < count; i++)
        {                    
            SetBunkerArray(i);
        }
        CancelInvoke(nameof(SetBunker));
        InvokeRepeating(nameof(SetBunker), remain, BunkersTimeHelper.FrequencyCheckOfBunker);
    }
    private bool CanCreate() => StageHelper.attackingZombie.bunkerCounter.Value < BunkersTimeHelper.MaxCountOfBunkers;

    private void SetBunker()
    {
        BunkersTimeHelper.bunkersTime.Value = DateTime.Now;
        if (!CanCreate())
        {
            return;
        }     
        var enemyBunker = new EnemyBunker();
        StageHelper.corral.AddEnemyBunker(enemyBunker);
        StageHelper.attackingZombie.bunkerCounter.Value++;    
    }
    
    private void SetBunkerArray(int value)
    {
        BunkersTimeHelper.bunkersTime.Value = DateTime.Now;
        if (!CanCreate())
        {
            return;
        }
   
        var enemyBunker = new EnemyBunker();

        enemyBunker.SetStartTime(-BunkersTimeHelper.FrequencyCheckOfBunker * value);

        StageHelper.corral.AddEnemyBunker(enemyBunker);
        StageHelper.attackingZombie.bunkerCounter.Value++;    
    }

    private void RemoveBunkerTimer()
    {
        foreach (var it in StageHelper.corral.EnemyBunkers)
        {
            var ts = Convert.ToInt32((DateTime.Now - it.StartTime).TotalSeconds);
            if (ts > BunkersTimeHelper.FrequencyAvailibleBunker)
            {
                StageHelper.corral.RemoveEnemyBunker(it);             
                break;
            }
        }
    }
    #endregion
}
