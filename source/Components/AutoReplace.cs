﻿using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    [CustomComponent("Replace")]
    public class AutoReplace : SimpleCustomComponent, IOnItemGrabbed, IOnInstalled, IClearInventory
    {
        public string ComponentDefId { get; set; }
        public ChassisLocations Location { get; set; }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget widget)
        {
            Control.Logger.LogDebug($"- AutoReplace");
            Control.Logger.LogDebug($"-- search replace for {item.ComponentRef.ComponentDefID}");
            if (string.IsNullOrEmpty(ComponentDefId))
            {
                Control.Logger.LogDebug($"-- no replacement, skipping");
                return;
            }
            Control.Logger.LogDebug($"-- {widget}");
            var location = Location == ChassisLocations.None ? widget.loadout.Location : Location;

            DefaultHelper.AddMechLab(ComponentDefId, Def.ComponentType, new MechLabHelper(mechLab),location);
            Control.Logger.LogDebug($"-- added {ComponentDefId} to {location}");
            mechLab.ValidateLoadout(false);
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.Logger.LogDebug($"- AutoReplace");
            Control.Logger.LogDebug($"-- search replace for {order.MechComponentRef.ComponentDefID}");
            if (order.PreviousLocation != ChassisLocations.None)
            {
                var location = Location == ChassisLocations.None ? order.PreviousLocation : Location;
                Control.Logger.LogDebug($"-- found, adding {ComponentDefId} to {location}");
                DefaultHelper.AddInventory(ComponentDefId, mech, location, order.MechComponentRef.ComponentDefType, state);
            }
            else
            {
                Control.Logger.LogDebug($"-- new component, not replacement needed");
            }
        }

        public void ClearInventory(List<MechComponentRef> result, SimGameState state, MechComponentRef source)
        {
            var ref_item = new MechComponentRef(ComponentDefId, state.GenerateSimGameUID(), source.ComponentDefType,
                Location == ChassisLocations.None ? source.MountedLocation : Location, isFixed: true)
            {
                DataManager = state.DataManager
            };
            ref_item.RefreshComponentDef();
            result.Add(ref_item);
            Control.Logger.LogDebug($"-- Replace with {ref_item.ComponentDefID} - {ref_item.SimGameUID}");

        }
    }
}
