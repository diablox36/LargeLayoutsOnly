using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace LargeLayoutsOnly.Systems
{
    [UpdateBefore(typeof(HandleLayoutRequests))]
    public class FilterLayoutUpgrades : FranchiseSystem, IModSystem
    {
        private EntityQuery LayoutUpgrades;

        protected override void Initialise()
        {
            base.Initialise();
            LayoutUpgrades = GetEntityQuery(typeof(CLayoutUpgrade));
        }

        protected override void OnUpdate()
        {
            using (NativeArray<Entity> entities = LayoutUpgrades.ToEntityArray(Allocator.Temp))
            {
                EntityManager.DestroyEntity(entities);

                Entity layoutUpgrade = EntityManager.CreateEntity(typeof(CLayoutUpgrade));
                EntityManager.SetComponentData(layoutUpgrade, new CLayoutUpgrade
                {
                    LayoutID = AssetReference.HugeLayout
                });
            }
        }
    }
}
