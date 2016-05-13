﻿using System.Collections.Generic;
using mze9412.SEScripts.Libraries;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace mze9412.SEScripts.InventoryManager.Actions
{
    /**Begin copy here**/

    /// <summary>
    /// Collects items from all inventories in grid and collects them into target containers
    /// </summary>
    public sealed class CollectItemsAction : InventoryManagerAction
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="gridProgram"></param>
        public CollectItemsAction(MyGridProgram gridProgram) : base(gridProgram, "CollectItems")
        {

        }

        /// <summary>
        /// Action implementation ;)
        /// </summary>
        /// <param name="argument"></param>
        protected override bool RunCore(string argument)
        {
            //get all blocks with inventories which are not a gun (i.e. turrets) and which do not use the Ignore keyworkd
            var sources = new List<IMyTerminalBlock>(100);
            GridProgram.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(sources, x => x.CubeGrid == GridProgram.Me.CubeGrid && x is IMyInventoryOwner&& !(x is IMyUserControllableGun || x is IMyReactor) && !x.CustomName.Contains(InventoryManagerConfig.IgnoreContainerTag));

            //go through all blocks and empty them
            foreach (var source in sources)
            {
                //assemblers and refineries are only emptied from their second output inventory
                var inventoryIndex = 0;
                if (source is IMyAssembler || source is IMyRefinery)
                {
                    inventoryIndex = 1;
                }

                //get inventory and items
                var sourceInventory = source.GetInventory(inventoryIndex);
                var sourceItems = sourceInventory.GetItems();

                //iterate backwords over items (because removal of items messes up loop otherwise)
                for (int i = sourceItems.Count - 1; i >= 0; i--)
                {
                    //get item type
                    var item = sourceItems[i];
                    var type = ItemIdHelper.GetItemType(item.Content.TypeId.ToString());

                    //do not sort if already in correct container
                    if (type == 0 && source.CustomName.Contains(InventoryManagerConfig.OreContainerTag)
                        || type == 1 && source.CustomName.Contains(InventoryManagerConfig.IngotsContainerTag)
                        || type == 2 && source.CustomName.Contains(InventoryManagerConfig.ComponentsContainerTag)
                        || type == 3 && source.CustomName.Contains(InventoryManagerConfig.MiscContainerTag)
                        )
                    {
                        continue;
                    }
                    
                    //find cargo target and transfer
                    var target = GetTargetForItem(sourceInventory, type);
                    if (target != null)
                    {
                        TransferItem(sourceInventory, target.GetInventory(0), i);
                    }

                    //cancel if too close to instruction count
                    if (!CanContinueRun)
                    {
                        return false;
                    }
                }

                //cancel if too close to instruction count
                if (!CanContinueRun)
                {
                    return false;
                }
            }

            //all done
            return true;
        }

        /**End copy here**/
    }
}
