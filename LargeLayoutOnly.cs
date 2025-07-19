using Kitchen;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace LargeLayoutsOnly
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
			MethodInfo method = typeof(CreateLayoutSlots).GetMethod("CreateMapSource", BindingFlags.Instance | BindingFlags.NonPublic);

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
				method.Invoke(LayoutSlotsSystem, new object[] { office + list[i] });
			}
		}
	}
}