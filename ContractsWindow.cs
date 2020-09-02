using Sources.Metrica;
using UnityEngine;
using UnityEngine.UI;
using ZombieShooter.Core.Model;
using AiryCat.UtilitiesForUnity.Localization;
using System.Threading.Tasks;

namespace ZombieShooter.UI
{
    public class ContractsWindow : MonoBehaviour
    {
        [SerializeField] private GameObject bossUiPrefab;
        [SerializeField] private GameObject listContent;
        [SerializeField] private Button closeBtn;

        [SerializeField] private Button undercoverBtn;

        //[SerializeField] private Text bossesCount;
        //[SerializeField] private Text locationName;
        [SerializeField] private Animator animator;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private OpenWindowsChecker openWindowsChecker;
        [SerializeField] private GoldReceivedPushController goldReceivedPushController;

        #region Properties

        public bool IsVisible => gameObject.activeInHierarchy;
        private MetaContext meta;
        private int activeBossesCount;

        #endregion


        public void Show()
        {
            if (IsVisible || openWindowsChecker.AnyWindowOpened())
            {
                return;
            }

            // gridLayoutGroup.constraintCount = Camera.main.aspect > 0.65f ? 3 : 2;
            AnalyticsWrapper.SendEvent(EAnalyticsKeys.UI_ContractsOpened);
            meta = Contexts.sharedInstance.meta;

            FillContent();

            Tools.Helper.TapTic(Tools.Helper.TapTicType.ImpactLight);
            closeBtn.onClick.AddListener(Hide);
            undercoverBtn.onClick.AddListener(Hide);
            //bossesCount.text = activeBossesCount + " / " + Settings.Phase[meta.locationSaveData.value.CurrentLocation][meta.locationSaveData.value.CurrentZone].Count(x => x.Boss != null);
            //locationName.text = $"locationName{meta.locationSaveData.value.CurrentLocation}".Localized();
            gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }


        private void Instantiate(BossDeathsData boss, bool isComplete, int childNumber, int zone)
        {
            var obj = Instantiate(bossUiPrefab);
            obj.transform.SetParent(listContent.transform.GetChild(childNumber));
            obj.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            var bossUi = obj.transform.GetChild(0).GetComponent<BossModalUi>();
            bossUi.SetBoss(boss, this, ClaimMoney, isComplete, zone);

            obj.SetActive(true);
            obj.transform.localScale = Vector3.one;
        }

        private void Hide()
        {
            animator.Play(Animator.StringToHash("Settings_out"));

        }

        private void ClaimMoney(int money)
        {
            meta.ReplaceGameData(meta.gameData.Members, meta.gameData.Gold += money, meta.gameData.BestScore,
                meta.gameData.PlayerExp, meta.gameData.TutorialStageNumber, meta.gameData.BattleCount, meta.gameData.PlayerLvl,
                meta.gameData.BossDeathsCounts,
                meta.gameData.SavedQuestsFromGroup);
            goldReceivedPushController.SetPush(money);
            Tools.Helper.TapTic(Tools.Helper.TapTicType.NotificationSuccess);
        }

        public void DestroyContent()
        {
            BossModalUi[] prefabs = listContent.GetComponentsInChildren<BossModalUi>();

            foreach (BossModalUi it in prefabs)
            {
                Destroy(it.transform.parent.gameObject);
            }

        }

        public async void FillContent()
        {

            DestroyContent();

            while (listContent.GetComponentsInChildren<BossModalUi>().Length != 0)
            {
                await Task.Yield();
            }

            Text[] zoneNames = listContent.transform.GetComponentsInChildren<Text>();

            int currentZone = GetCurrentZone();            
            for (int i = 0; i < zoneNames.Length; i++)
            {
                if (currentZone == meta.locationSaveData.value.CurrentZone)
                {
                    InstatiateBossPrefabs(currentZone, i);
                    zoneNames[i].text = $"zoneName_{0}_{currentZone}".Localized();
                    currentZone++;
                    if (currentZone > zoneNames.Length - 1)
                    {
                        currentZone = 0;
                    }
                    continue;
                }

                zoneNames[i].text = $"zoneName_{0}_{currentZone}".Localized();

                InstatiateBossPrefabs(currentZone, i);

                currentZone++;
                if (currentZone > zoneNames.Length - 1)
                {
                    currentZone = 0;
                }
            }

        }

        void InstatiateBossPrefabs(int zone, int childNum)
        {
            activeBossesCount = 0;
            bool isComplete = false;
            foreach (var it in meta.gameData.BossDeathsCounts[meta.locationSaveData.value.CurrentLocation][
                zone])
            {
                isComplete = false;
                if (it.completePhase)
                {
                    activeBossesCount++;
                }

                if (it.countCompleteReward >= Settings.Main.KillsBossesToReward.Length)
                {
                    isComplete = true;
                }
                else if (it.completePhase)
                {
                    activeBossesCount++;
                }


                Instantiate(it, isComplete, childNum * 2 + 1, zone);

            }
        }

        private int GetCurrentZone()
        {
            int retVal = meta.locationSaveData.value.CurrentZone;

            int untakenRewards = 0;
            
            for (int i = 0; i < 4; i++)
            {
                foreach (var it in meta.gameData.BossDeathsCounts[meta.locationSaveData.value.CurrentLocation][i])
                {
                    if (it.countCompleteReward >= Settings.Main.KillsBossesToReward.Length)
                    {
                        continue;
                    }

                    int count;
                    int countReawardForAllDeaths = 0;
                    count = it.countDeaths;
                    for (int j = 0; j < Settings.Main.KillsBossesToReward.Length; j++)
                    {
                        if (Settings.Main.KillsBossesToReward[j] <= count)
                        {
                            count -= Settings.Main.KillsBossesToReward[j];
                            countReawardForAllDeaths++;
                        }
                    }

                    untakenRewards += countReawardForAllDeaths - it.countCompleteReward;                   
                   
                }

                if (untakenRewards > 0)
                {
                    retVal = i;
                    break;
                }
            }

            return retVal;
        }
    }
}