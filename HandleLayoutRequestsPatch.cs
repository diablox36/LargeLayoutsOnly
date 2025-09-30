using Kitchen;
using KitchenData;
using KitchenMods;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace LargeLayoutsOnly
{
    public class HandleLayoutRequestsPatch : FranchiseSystem, IModSystem
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct CClearOnLayoutRequest : IComponentData
        {
        }

        private EntityQuery Requests;

        private EntityQuery Slots;

        private EntityQuery MapItems;

        private EntityQuery SettingSelectors;

        private HandleLayoutRequests HandleLayoutSystem;

        public static bool Initialised { get; private set; }

        protected override void Initialise()
        {
            base.Initialise();
            Requests = GetEntityQuery(typeof(HandleLayoutRequests.SLayoutRequest));
            Slots = GetEntityQuery(typeof(CreateLayoutSlots.CLayoutSlot), typeof(CItemHolder));
            MapItems = GetEntityQuery(typeof(CItemLayoutMap), typeof(CClearOnLayoutRequest));
            SettingSelectors = GetEntityQuery(typeof(CSettingSelector));
            HandleLayoutSystem = World.GetExistingSystem<HandleLayoutRequests>();
            RequireForUpdate(Requests);
            RequireForUpdate(Slots);

            if (HandleLayoutSystem != null)
            {
                HandleLayoutSystem.Enabled = false;
            }
        }

        protected override void OnUpdate()
        {
            if (!Require(out HandleLayoutRequests.SLayoutRequest comp) || comp.HasBeenCreated)
            {
                return;
            }

            EntityManager.DestroyEntity(MapItems);
            int settingId = CSettingSelector.IDFromQuery(SettingSelectors);
            using (NativeArray<Entity> nativeArray = Slots.ToEntityArray(Allocator.Temp))
            {
                foreach (Entity item in nativeArray)
                {
                    int randomSeed = Random.Range(int.MinValue, int.MaxValue);
                    var ls = new LayoutSeed(randomSeed, new[] { AssetReference.HugeLayout });

                    Entity entity = ls.GenerateMap(EntityManager, settingId);
                    EntityManager.AddComponent<CClearOnLayoutRequest>(entity);
                    EntityManager.SetComponentData(item, (CItemHolder)entity);
                    EntityManager.SetComponentData(entity, (CHeldBy)item);
                    EntityManager.SetComponentData(entity, (CHome)item);
                }

                comp.HasBeenCreated = true;
                Set(comp);
                Initialised = true;
            }
        }
    }
}