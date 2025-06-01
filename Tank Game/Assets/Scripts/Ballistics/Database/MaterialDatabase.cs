using System;
using System.Collections.Generic;

namespace Ballistics
{
    public enum MaterialKey
    {
        MildSteel,
        RolledHomogenousSteel,
        HighCarbonSteel,
        StainlessSteel304,
        AR500Steel,
        Aluminum6061T6,
        Aluminum7075T6,
        TitaniumGrade2,
        TitaniumGrade5,
        TungstenCarbide,
        TungstenAlloy,
        DepletedUranium,
        Glass,
        Quartz,
        Alumina,
        SiliconCarbide,
        Kevlar,
        CarbonFiber,
        Rubber,
        Nylon,
        WoodOak,
        WoodPine,
        HumanFlesh
    
    }
    public enum MaterialType
    {
        Metal,
        Ceramic,
        Composite,
        Polymer,
        Organic
    }
    public class Material
    {
        public string Name { get; }
        public MaterialType Type { get; }  // Now an enum
        public float Hardness { get; }  // Mohs scale
        public float Density { get; }  // kg/m³
        public float ImpactResistance { get; }  // J/cm²

        public Material(string name, MaterialType type, float hardness, float density, float impactResistance)
        {
            Name = name;
            Type = type;
            Hardness = hardness;
            Density = density;
            ImpactResistance = impactResistance;
        }

        public float GetMaterialToughnessCoefficient(float resistanceFactor, float mul)
        {
            return Math.Min((ImpactResistance / resistanceFactor) / Hardness, 1) * Hardness * mul;
        }
    }

    public static class MaterialDatabase
    {
        private static readonly Dictionary<MaterialKey, Material> Materials = new()
        {
            // STEELS
            
            { MaterialKey.MildSteel,       new Material("Mild Steel (A36)",                 MaterialType.Metal, hardness: 4.5f, density: 7850, impactResistance: 150) },
            { MaterialKey.RolledHomogenousSteel, new Material("Rolled Homogeneous Steel",        MaterialType.Metal, hardness: 6.0f, density: 7850, impactResistance: 210) },
            { MaterialKey.HighCarbonSteel, new Material("High-Carbon Steel",              MaterialType.Metal, hardness: 6.5f, density: 7800,  impactResistance: 180) },
            { MaterialKey.StainlessSteel304, new Material("Stainless Steel (304)",          MaterialType.Metal, hardness: 5.5f, density: 8000,  impactResistance: 200) },
            { MaterialKey.AR500Steel,      new Material("AR500 Steel",                     MaterialType.Metal, hardness: 7.0f, density: 7850,  impactResistance: 250) },
    
            // OTHER METALS
            { MaterialKey.Aluminum6061T6,   new Material("Aluminum 6061-T6",               MaterialType.Metal, hardness: 2.8f, density: 2700,  impactResistance:  60) },
            { MaterialKey.Aluminum7075T6,   new Material("Aluminum 7075-T6",               MaterialType.Metal, hardness: 3.0f, density: 2810,  impactResistance:  50) },
            { MaterialKey.TitaniumGrade2,   new Material("Titanium Grade 2",                MaterialType.Metal, hardness: 6.0f, density: 4500,  impactResistance: 250) },
            { MaterialKey.TitaniumGrade5,   new Material("Titanium Grade 5 (Ti-6Al-4V)", MaterialType.Metal, hardness: 6.0f, density: 4430,  impactResistance: 300) },
            { MaterialKey.TungstenCarbide,  new Material("Tungsten Carbide",               MaterialType.Metal, hardness: 9.0f, density: 15600, impactResistance:  50) },
            { MaterialKey.TungstenAlloy,    new Material("Tungsten Heavy Alloy",          MaterialType.Metal, hardness: 7.5f, density: 18000, impactResistance: 200) },
            { MaterialKey.DepletedUranium,  new Material("Depleted Uranium",              MaterialType.Metal, hardness: 6.0f, density: 19050, impactResistance: 220) },

            // CERAMICS & COMPOSITES
            { MaterialKey.Glass,            new Material("Glass",                          MaterialType.Ceramic, hardness: 5.5f, density: 2500,  impactResistance:   5) },
            { MaterialKey.Quartz,           new Material("Quartz",                         MaterialType.Ceramic, hardness: 7.0f, density: 2650,  impactResistance:  10) },
            { MaterialKey.Alumina,          new Material("Alumina (Al₂O₃)",                MaterialType.Ceramic, hardness: 9.0f, density: 4000,  impactResistance:  30) },
            { MaterialKey.SiliconCarbide,    new Material("Silicon Carbide",               MaterialType.Ceramic, hardness: 9.5f, density: 3200,  impactResistance:  40) },
            { MaterialKey.Kevlar,           new Material("Kevlar",                         MaterialType.Composite, hardness: 3.5f, density: 1440,  impactResistance: 500) },
            { MaterialKey.CarbonFiber,      new Material("Carbon Fiber",                   MaterialType.Composite, hardness: 4.5f, density: 1800,  impactResistance: 600) },

            // POLYMERS & ORGANIC MATERIALS
            { MaterialKey.Rubber,           new Material("Rubber",                         MaterialType.Polymer,   hardness: 1.0f, density: 1100,  impactResistance: 200) },
            { MaterialKey.Nylon,            new Material("Nylon",                          MaterialType.Polymer,   hardness: 2.0f, density: 1200,  impactResistance: 100) },
            { MaterialKey.WoodOak,          new Material("Wood (Oak)",                     MaterialType.Organic,   hardness: 2.0f, density:  800,  impactResistance: 250) },
            { MaterialKey.WoodPine,         new Material("Wood (Pine)",                    MaterialType.Organic,   hardness: 1.5f, density:  500,  impactResistance: 150) },
            { MaterialKey.HumanFlesh,       new Material("Human Flesh",                    MaterialType.Organic,   hardness: 0.5f, density:  985,  impactResistance: 5) },
            
            
        };

        public static Material GetMaterial(MaterialKey key)
        {
            return Materials.GetValueOrDefault(key);
        }
    }
}