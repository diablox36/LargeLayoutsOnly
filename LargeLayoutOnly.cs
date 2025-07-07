using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace LargeLayoutsOnly
{
    public class PopulateMapPedestals : FranchiseSystem, IModSystem
    {
        private EntityQuery LayoutSlots;
        private EntityQuery MapItems;
        private EntityQuery SettingSelectors;
        private int SettingsId;

        protected override void Initialise()
        {
            base.Initialise();
            LayoutSlots = GetEntityQuery(typeof(CreateLayoutSlotsPatch.CLayoutSlot), typeof(CItemHolder));
            MapItems = GetEntityQuery(typeof(CItemLayoutMap), typeof(CClearOnLayoutRequest));
            SettingSelectors = GetEntityQuery(typeof(CSettingSelector));
            SettingsId = CSettingSelector.IDFromQuery(SettingSelectors);
        }

        protected override void OnUpdate()
        {
            int settingId = CSettingSelector.IDFromQuery(SettingSelectors);

            if (settingId == SettingsId)
            {
                return;
            }

            SettingsId = settingId;

            EntityManager.DestroyEntity(MapItems);

            NativeArray<Entity> pedestalEntities = LayoutSlots.ToEntityArray(Allocator.Temp);

            foreach (Entity pedestal in pedestalEntities)
            {
                if (Require(pedestal, out CItemHolder itemHolder))
                {
                    int randomSeed = Random.Range(int.MinValue, int.MaxValue);
                    var layoutSeed = new LayoutSeed(randomSeed, new[] { AssetReference.HugeLayout });
                    Entity map = layoutSeed.GenerateMap(EntityManager, settingId);

                    EntityManager.AddComponent<CClearOnLayoutRequest>(map);
                    EntityManager.SetComponentData<CItemHolder>(pedestal, map);
                    EntityManager.SetComponentData<CHeldBy>(map, pedestal);
                    EntityManager.SetComponentData<CHome>(map, pedestal);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct CClearOnLayoutRequest : IComponentData
        {
        }
    }

    [UpdateAfter(typeof(CreateOffice))]
    public class CreateLayoutSlotsPatch : FranchiseFirstFrameSystem, IModSystem
    {
        private EntityQuery LayoutSizeUpgrades;
        private CreateLayoutSlots LayoutSlotsSystem;

        protected override void Initialise()
        {
            base.Initialise();
            LayoutSizeUpgrades = GetEntityQuery(typeof(CUpgradeExtraLayout));
            LayoutSlotsSystem = World.GetExistingSystem<CreateLayoutSlots>();

            if (LayoutSlotsSystem != null)
            {
                LayoutSlotsSystem.Enabled = false;
            }
        }

        protected override void OnUpdate()
        {
            Vector3 office = LobbyPositionAnchors.Office;
            List<Vector3> list = new List<Vector3>
            {
                new Vector3(-0f, 0f, -5f),
                new Vector3(-1f, 0f, -5f),
                new Vector3(-2f, 0f, -5f),
                new Vector3(-3f, 0f, -5f),
                new Vector3(-4f, 0f, -5f),
                new Vector3(-4f, 0f, -4f)
            };

            for (int i = 0; i < Mathf.Min(6, 4 + LayoutSizeUpgrades.CalculateEntityCount()); i++)
            {
                CreateMapSource(office + list[i]);
            }
        }

        private void CreateMapSource(Vector3 location)
        {
            EntityManager entityManager = EntityManager;
            Entity entity = entityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition), typeof(CItemHolder), typeof(CLayoutSlot));
            entityManager.SetComponentData(entity, new CCreateAppliance
            {
                ID = AssetReference.LayoutPedestal
            });
            entityManager.SetComponentData(entity, new CPosition(location));
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct CLayoutSlot : IComponentData
        {
        }
    }
}