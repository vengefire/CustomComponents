﻿#undef CCDEBUG

using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// error for category check
    /// </summary>
    internal enum CategoryError
    {
        None,
        AreadyEquiped,
        MaximumReached,
        AlreadyEquipedLocation,
        MaximumReachedLocation,
        AllowMix
    }

    /// <summary>
    /// class to handle category interaction
    /// </summary>
    public static class CategoryController
    {
        //private static List<string> fcache = new List<string>();


        /// <summary>
        /// validate mech and fill errors
        /// </summary>
        /// <param name="errors">errors by category</param>
        /// <param name="validationLevel"></param>
        /// <param name="mechDef">mech to validate</param>
        internal static void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {

            var items_by_category = mechDef.Inventory
                .Select(item => new { item, def = item.Def.GetComponents<Category>() })
                .Where(@t => @t.def != null)
                .SelectMany(@t => t.def.Select(item => new
                    {
                        category = item.CategoryDescriptor,
                        itemdef = @t.item.Def,
                        itemref = @t.item,
                        mix = item.GetTag()
                    }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key, i => i.ToList());


            //fcache.Clear();

            //check each "required" category
            foreach (var category in Control.GetCategories().Where(i => i.Required))
            {
                if (!items_by_category.ContainsKey(category) || items_by_category[category].Count < category.MinEquiped)
                    if (category.MinEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(category.ValidateRequred, category.DisplayName.ToUpper(), category.DisplayName)));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(category.ValidateMinimum, category.DisplayName.ToUpper(), category.DisplayName, category.MinEquiped)));
            }

            foreach (var pair in items_by_category)
            {
                //check if too mant items of same category
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                    if (pair.Key.MaxEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(pair.Key.ValidateUnique,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName)));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(pair.Key.ValidateMaximum,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquiped)));

                //check if cateory mix tags
                if (!pair.Key.AllowMixTags)
                {
                    string def = pair.Value[0].mix;

                    bool flag = pair.Value.Any(i => i.mix != def);
                    if (flag)
                    {
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(pair.Key.ValidateMixed,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName)));
                    }
                }

                // check count items per location
                if (pair.Key.MaxEquipedPerLocation > 0)
                {
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Count());
                    if (max > pair.Key.MaxEquipedPerLocation)
                        if (pair.Key.MaxEquipedPerLocation == 1)
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(pair.Key.ValidateUniqueLocation,
                                pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName)));
                        else
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(pair.Key.ValidateMaximumLocation,
                                pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquipedPerLocation)));
                }
            }
        }

        /// <summary>
        /// check if mech can be fielded
        /// </summary>
        /// <param name="mechDef"></param>
        /// <returns></returns>
        internal static bool ValidateMechCanBeFielded(MechDef mechDef)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"- Category");
#endif
            //var items_by_category = (from item in mechDef.Inventory
            //                         let def = item.Def.GetComponent<Category>()
            //                         where def != null
            //                         select new
            //                         {
            //                             category = def.CategoryDescriptor,
            //                             itemdef = item.Def,
            //                             itemref = item,
            //                             mix = def.GetTag()
            //                         }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

            var items_by_category = mechDef.Inventory
                .Select(item => new { item, def = item.Def.GetComponents<Category>() })
                .Where(@t => @t.def != null)
                .SelectMany(@t => t.def.Select(item => new
                {
                    category = item.CategoryDescriptor,
                    itemdef = @t.item.Def,
                    itemref = @t.item,
                    mix = item.GetTag()
                }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key, i => i.ToList());

            // if all required category present
            foreach (var category in Control.GetCategories().Where(i => i.Required))
            {
#if CCDEBUG
                Control.Logger.LogDebug($"-- MinEquiped for {category.displayName}");
#endif
                if (!items_by_category.ContainsKey(category) || items_by_category[category].Count < category.MinEquiped)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"--- not passed {items_by_category[category].Count}/{category.MinEquiped}");
#endif
                    return false;
                }
            }

            foreach (var pair in items_by_category)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"-- MaxEquiped for {pair.Key.displayName}");
#endif                
                // if too many equiped
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"--- not passed {pair.Value.Count}/{pair.Key.MaxEquiped}");
#endif
                    return false;
                }

                //if mixed
                if (!pair.Key.AllowMixTags)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"-- AllowMixTags for {pair.Key.displayName}");
#endif
                    string def = pair.Value[0].mix;

                    bool flag = pair.Value.Any(i => i.mix != def);
                    if (flag)
                    {
#if CCDEBUG
                        Control.Logger.LogDebug($"--- not passed {def}");
#endif
                        return false;
                    }
                }

                // if too many per location
                if (pair.Key.MaxEquipedPerLocation > 0)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"-- MaxEquipedPerLocation for {pair.Key.displayName}");
#endif
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Count());
                    if (max > pair.Key.MaxEquipedPerLocation)
                    {
#if CCDEBUG
                        Control.Logger.LogDebug($"--- not passed {max}/{pair.Key.MaxEquipedPerLocation}");
#endif
                        return false;
                    }
                }
            }
#if CCDEBUG
            Control.Logger.LogDebug($"--- all passed");
#endif
            return true;
        }

        public static bool IsCategory(this MechComponentDef cdef, string category)
        {
            return cdef.GetComponents<Category>().Any(c => c.CategoryID == category);

            // Is<Category>
            // GetComponent<Category>
        }

        public static bool IsCategory(this MechComponentRef cref, string category)
        {
            return cref.GetComponents<Category>().Any(c => c.CategoryID == category);
        }


        public static bool IsCategory(this MechComponentDef cdef, string categoryid, out Category category)
        {
            category = cdef.GetComponents<Category>().FirstOrDefault(c => c.CategoryID == categoryid);
            return category != null;
            // Is<Category>
            // GetComponent<Category>
        }

        public static bool IsCategory(this MechComponentRef cref, string categoryid, out Category category)
        {
            category = cref.GetComponents<Category>().FirstOrDefault(c => c.CategoryID == categoryid);
            return category != null;
        }

        public static bool need_remove(MechComponentRef item, MechDef mech)
        {
            if (!item.IsFixed)
                return false;

            if (item.IsModuleFixed(mech))
                return false;

            if (!item.IsDefault())
                return false;

            if (item.Is<Category>(out var category))
            {
                if (category.CategoryDescriptor.MaxEquiped > 0 || category.CategoryDescriptor.MaxEquipedPerLocation > 0)
                    return true;
            }

            return false;
        }

        public static void RemoveExcessDefaults(MechDef mechDef)
        {
            mechDef.SetInventory(mechDef.Inventory.Where(i => !need_remove(i, mechDef)).ToArray());
            mechDef.Refresh();
        }
    }
}