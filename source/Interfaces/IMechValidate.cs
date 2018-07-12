﻿using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    /// <summary>
    /// component need to validate mech state
    /// </summary>
    public interface IMechValidate
    {
        /// <summary>
        /// validate mech
        /// </summary>
        /// <param name="errors">list of errors</param>
        /// <param name="validationLevel">level of validation</param>
        /// <param name="mechDef">mech to check</param>
        void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef);

        bool ValidateMechCanBeFielded(MechDef mechDef);
    }

    /// <summary>
    /// delegate for mech valudation
    /// </summary>
    /// <param name="errors">list of errors</param>
    /// <param name="validationLevel">level of validation</param>
    /// <param name="mechDef">mech to validate</param>
    public delegate void ValidateMechDelegate(Dictionary<MechValidationType, List<string>> errors,
        MechValidationLevel validationLevel, MechDef mechDef);


    public delegate bool ValidateMechCanBeFieldedDelegate(MechDef mechDef);
}