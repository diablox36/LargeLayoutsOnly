using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace LargeLayoutsOnly
{
    [UpdateBefore(typeof(HandleLayoutRequests))]
    public class HandleLayoutRequestsPatch : FranchiseSystem, IModSystem
    {
        private EntityQuery LayoutUpgrades;

        protected override void Initialise()
        {
            base.Initialise();
            LayoutUpgrades = GetEntityQuery(typeof(CLayoutUpgrade));
        }

        protected override void OnUpdate()
        {
            if (LayoutUpgrades.CalculateEntityCount() == 1)
            {
                using (NativeArray<CLayoutUpgrade> layouts = LayoutUpgrades.ToComponentDataArray<CLayoutUpgrade>(Allocator.Temp))
                {
                    if (layouts[0].LayoutID == AssetReference.HugeLayout)
                    {
                        return;
                    }
                }
            }

            EntityManager.DestroyEntity(LayoutUpgrades);
            Entity layoutUpgrade = EntityManager.CreateEntity(typeof(CLayoutUpgrade));

            EntityManager.SetComponentData(layoutUpgrade, new CLayoutUpgrade 
            {
                LayoutID = AssetReference.HugeLayout
            });
        }
    }
}