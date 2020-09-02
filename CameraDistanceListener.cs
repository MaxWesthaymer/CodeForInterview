
using Entitas;
using UnityEngine;
using Cinemachine;

namespace ZombieShooter.MonoBeh.Listener
{
    public class CameraDistanceListener : MonoBehaviour, IEventListener, IMembersDistanceListener
    {
        [Tooltip("Minimum distance with 0-1 squad member.")] [SerializeField]
        private int baseDistance = 25;

        [Tooltip("Maximum distance.")] [SerializeField]
        private int maxDistance = 70;

        [Tooltip("The multiplier of the number of squad members for the surplus distance.")] [SerializeField]
        private float multiplierMember = 3;

        [SerializeField] private float zoomDuration = 1f;
        [SerializeField] private bool isLobby;
        private CinemachineVirtualCamera virtualCamera;

        public void RegisterEventListeners(IEntity entity)
        {
            Contexts.sharedInstance.meta.membersDistanceEntity.AddMembersDistanceListener(this);
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        public void OnMembersDistance(MetaEntity entity, float value)
        {
            var newDistance =
                Mathf.Clamp(
                    (isLobby ? baseDistance : PerksHandler.CheckEaglesEyePerk()) +
                    value * (isLobby ? multiplierMember : Settings.Main.multiplierMember),
                    isLobby ? baseDistance : PerksHandler.CheckEaglesEyePerk(), isLobby ? maxDistance : Settings.Main.maxDistance);
            if (newDistance > virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance ||
                newDistance < virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance)
            {
                var currentDist = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;
                virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance =
                    Mathf.Lerp(currentDist, newDistance, Time.deltaTime * Settings.Main.zoomDuration);
            }
        }
    }
}