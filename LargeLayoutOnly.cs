using Kitchen;
using Kitchen.NetworkSupport;
using Kitchen.Transports;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace LargeLayoutOnly
{
    [UpdateBefore(typeof(HandleLayoutRequests))]
    public class HandleLayoutRequestsPatch : FranchiseSystem, IModSystem
    {
        private EntityQuery SettingSelectors;

        // BaseNetworkTransport<INetworkTarget, ILiveness> NetworkTransport;

        protected override void Initialise()
        {
            base.Initialise();
            SettingSelectors = GetEntityQuery(typeof(CSettingSelector));


        }

        protected override void OnUpdate()
        {
            INetworkTransport transport = NetworkPlatforms.Get<INetworkPlatform>() as INetworkTransport;
            if (transport != null && !transport.IsHosting)
            {
                return;
            }
                int settingId = CSettingSelector.IDFromQuery(SettingSelectors);
                RestaurantSetting restaurantSetting = GameData.Main.Get<RestaurantSetting>(settingId);
                restaurantSetting.ForceLayout = GameData.Main.Get<LayoutProfile>(AssetReference.HugeLayout);
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
            LayoutSlotsSystem.Enabled = false;
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