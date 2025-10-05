using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace LargeLayoutsOnly.Refresher
{
    [UpdateInGroup(typeof(InteractionGroup))]
    public class LayoutRefreshInteraction : InteractionSystem, IModSystem
    {
        private CItemHolder ItemHolder;

        protected override void Initialise()
        {
            base.Initialise();
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
            Set(default(HandleLayoutRequests.SLayoutRequest));
        }
    }
}