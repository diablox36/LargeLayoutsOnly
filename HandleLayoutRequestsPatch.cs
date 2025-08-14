using Kitchen;
using KitchenData;
using KitchenMods;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;

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
            RequireForUpdate(Requests);
            RequireForUpdate(Slots);

            HandleLayoutSystem = World.GetExistingSystem<HandleLayoutRequests>();

            if (HandleLayoutSystem != null)
            {
                HandleLayoutSystem.Enabled = false;
            }
        }

        protected override void OnUpdate()
        {
            if (!Require<HandleLayoutRequests.SLayoutRequest>(out var comp) || comp.HasBeenCreated)
            {
                return;
            }

            base.EntityManager.DestroyEntity(MapItems);
            int setting_id = CSettingSelector.IDFromQuery(SettingSelectors);
            using (NativeArray<Entity> nativeArray = Slots.ToEntityArray(Allocator.Temp))
            {
                foreach (Entity item in nativeArray)
                {
                    int source = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                    LayoutSeed ls = new LayoutSeed(source, new[] { AssetReference.HugeLayout });

                    Entity entity = ls.GenerateMap(base.EntityManager, setting_id);
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
