using Kitchen;
using KitchenData;
using KitchenMods;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace LargeLayoutsOnly
{
    [UpdateInGroup(typeof(InteractionGroup))]
    public class LayoutRefreshInteraction : InteractionSystem, IModSystem
    {
        private CItemHolder ItemHolder;
        private EntityQuery LayoutSlots;
        private EntityQuery MapItems;
        private EntityQuery SettingSelectors;

        protected override void Initialise()
        {
            base.Initialise();
            LayoutSlots = GetEntityQuery(typeof(CreateLayoutSlots.CLayoutSlot), typeof(CItemHolder));
            MapItems = GetEntityQuery(typeof(CItemLayoutMap), typeof(HandleLayoutRequestsPatch.CClearOnLayoutRequest));
            SettingSelectors = GetEntityQuery(typeof(CSettingSelector));
        }

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require(data.Target, out ItemHolder))
                return false;

            return Has<CLayoutRefresher>(data.Target) && ItemHolder.HeldItem == Entity.Null;
        }

        protected override void Perform(ref InteractionData data)
        {
            EntityManager.DestroyEntity(MapItems);
            int settingId = CSettingSelector.IDFromQuery(SettingSelectors);

            using (NativeArray<Entity> slotEntities = LayoutSlots.ToEntityArray(Allocator.Temp))
            {
                foreach (Entity slotEntity in slotEntities)
                {
                    int randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                    LayoutSeed layoutSeed = new LayoutSeed(randomSeed, new[] { AssetReference.HugeLayout });

                    Entity mapEntity = layoutSeed.GenerateMap(EntityManager, settingId);

                    EntityManager.AddComponent<HandleLayoutRequestsPatch.CClearOnLayoutRequest>(mapEntity);
                    EntityManager.SetComponentData(slotEntity, (CItemHolder)mapEntity);
                    EntityManager.SetComponentData(mapEntity, (CHeldBy)slotEntity);
                    EntityManager.SetComponentData(mapEntity, (CHome)slotEntity);
                }
            }
        }
    }
}