using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace LargeLayoutsOnly
{
    public struct CLayoutRefresher : IComponentData
    {
    }

    [UpdateAfter(typeof(CreateOffice))]
    public class LayoutRefreshAppliance : FranchiseFirstFrameSystem, IModSystem
    {
        protected override void OnUpdate()
        {
            Vector3 position = LobbyPositionAnchors.Office + new Vector3(-4f, -0.3f, -2f);

            Entity entity = EntityManager.CreateEntity(
                typeof(CCreateAppliance),
                typeof(CPosition),
                typeof(CLayoutRefresher)
            );

            EntityManager.SetComponentData(entity, new CCreateAppliance
            {
                ID = AssetReference.ResearchAppliance
            });

            EntityManager.SetComponentData(entity, new CPosition(position));
            EntityManager.SetComponentData(entity, new CLayoutRefresher());
        }
    }
}