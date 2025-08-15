using Kitchen;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static Kitchen.CreateLayoutSlots;

namespace LargeLayoutsOnly
{
    [UpdateAfter(typeof(CreateOffice))]
    public class CreateLayoutSlotsPatch : FranchiseFirstFrameSystem, IModSystem
    {
        private EntityQuery LayoutSlots;

        private EntityQuery LayoutSizeUpgrades;

        private CreateLayoutSlots LayoutSlotsSystem;

        protected override void Initialise()
        {
            base.Initialise();
            LayoutSizeUpgrades = GetEntityQuery(typeof(CUpgradeExtraLayout));
            LayoutSlots = GetEntityQuery(typeof(CLayoutSlot));
            LayoutSlotsSystem = World.GetExistingSystem<CreateLayoutSlots>();

            if (LayoutSlotsSystem != null)
            {
                LayoutSlotsSystem.Enabled = false;
            }
        }

        protected override void OnUpdate()
        {
            using (NativeArray<Entity> layoutSlots = LayoutSlots.ToEntityArray(Allocator.Temp))
            {
                if (layoutSlots.Any())
                {
                    return;
                }
            }

            MethodInfo method = typeof(CreateLayoutSlots).GetMethod("CreateMapSource", BindingFlags.Instance | BindingFlags.NonPublic);

            Vector3 office = LobbyPositionAnchors.Office;
            var list = new List<Vector3>
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
                method?.Invoke(LayoutSlotsSystem, new object[] { office + list[i] });
            }
        }
    }
}