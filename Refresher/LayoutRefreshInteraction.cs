using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace LargeLayoutsOnly.Refresher
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
            {
                return false;
            }

            return Has<CLayoutRefresher>(data.Target) && ItemHolder.HeldItem == Entity.Null;
        }

        protected override void Perform(ref InteractionData data)
        {
            EntityManager.DestroyEntity(MapItems);
            int settingId = CSettingSelector.IDFromQuery(SettingSelectors);

            using (NativeArray<Entity> nativeArray = LayoutSlots.ToEntityArray(Allocator.Temp))
            {
                foreach (Entity item in nativeArray)
                {
                    int randomSeed = Random.Range(int.MinValue, int.MaxValue);
                    var ls = new LayoutSeed(randomSeed, new[] { AssetReference.HugeLayout });

                    Entity entity = ls.GenerateMap(EntityManager, settingId);
                    EntityManager.AddComponent<HandleLayoutRequestsPatch.CClearOnLayoutRequest>(entity);
                    EntityManager.SetComponentData(item, (CItemHolder)entity);
                    EntityManager.SetComponentData(entity, (CHeldBy)item);
                    EntityManager.SetComponentData(entity, (CHome)item);
                }
            }
        }
    }
}