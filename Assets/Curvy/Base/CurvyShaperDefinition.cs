// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Asset-Class containing Superformula parameters
/// </summary>
public class CurvyShaperDefinition : ScriptableObject 
{
    /// <summary>
    /// Name of this definition
    /// </summary>
    public string Name;
    /// <summary>
    /// Resolution / Number of Control Points
    /// </summary>
    public int Resolution;
    /// <summary>
    /// Range in full circles (e.g. 2=720°)
    /// </summary>
    public float Range = 1;
    /// <summary>
    /// Initial Radius
    /// </summary>
    public float Radius;
    /// <summary>
    /// Radius modifier mode
    /// </summary>
    public SplineShaper.ModifierMode RadiusModifier;
    /// <summary>
    /// Radius modifier curve (optional)
    /// </summary>
    public AnimationCurve RadiusModifierCurve;
    /// <summary>
    ///  Initial Z
    /// </summary>
    public float Z;
    /// <summary>
    /// Z modifier mode
    /// </summary>
    public SplineShaper.ModifierMode ZModifier;
    /// <summary>
    /// Z modifier curve (optional)
    /// </summary>
    public AnimationCurve ZModifierCurve;
    public float m;
    public float n1;
    public float n2;
    public float n3;
    public float a;
    public float b;
    
    
    public static CurvyShaperDefinition Create() { return ScriptableObject.CreateInstance<CurvyShaperDefinition>(); }
    /// <summary>
    /// Creates a shaper definition
    /// </summary>
    /// <param name="shape">the shape to copy the settings of</param>
    public static CurvyShaperDefinition Create(SplineShaper shape) 
    {
        var def = Create();
        def.Name = shape.Name;
        def.Resolution = shape.Resolution;
        def.Range = shape.Range;
        def.Radius = shape.Radius;
        def.RadiusModifier = shape.RadiusModifier;
        def.RadiusModifierCurve = shape.RadiusModifierCurve;
        def.Z = shape.Z;
        def.ZModifier = shape.ZModifier;
        def.ZModifierCurve = shape.ZModifierCurve;
        def.m = shape.m;
        def.n1 = shape.n1;
        def.n2 = shape.n2;
        def.n3 = shape.n3;
        def.a = shape.a;
        def.b = shape.b;
        
        return def;
    }

    /// <summary>
    /// Loads a definition into a SplineShaper class
    /// </summary>
    /// <param name="shape">the shape to copy values into</param>
    /// <param name="loadGeneral">whether general parameters should be copied as well</param>
    public void LoadInto(SplineShaper shape, bool loadGeneral)
    {
        if (loadGeneral) {
            shape.Name = Name;
            shape.Resolution = Resolution;
            shape.Range = Range;
            shape.Radius = Radius;
            shape.RadiusModifier = RadiusModifier;
            shape.RadiusModifierCurve = RadiusModifierCurve;
            shape.Z = Z;
            shape.ZModifier = ZModifier;
            shape.ZModifierCurve = ZModifierCurve;
        }
        shape.m = m;
        shape.n1 = n1;
        shape.n2 = n2;
        shape.n3 = n3;
        shape.a = a;
        shape.b = b;
    }
}
