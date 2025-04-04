using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum MaterialKey
{
    MildSteel,
    HighCarbonSteel,
    StainlessSteel304,
    AR500Steel,
    Aluminum6061T6,
    Aluminum7075T6,
    TitaniumGrade2,
    TitaniumGrade5,
    Tungsten,
    TungstenCarbide,
    TungstenAlloy,
    Glass,
    Quartz,
    Alumina,
    SiliconCarbide,
    Kevlar,
    CarbonFiber,
    Rubber,
    Nylon,
    WoodOak,
    WoodPine
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
    public float Friction { get; }  // Static coefficient
    public float TensileStrength { get; }  // MPa (Megapascals)
    public float MeltingPoint { get; }  // °C
    public float ImpactResistance { get; }  // J/cm²

    public Material(string name, MaterialType type, float hardness, float density, float friction,
        float tensileStrength, float meltingPoint, float impactResistance)
    {
        Name = name;
        Type = type;
        Hardness = hardness;
        Density = density;
        Friction = friction;
        TensileStrength = tensileStrength;
        MeltingPoint = meltingPoint;
        ImpactResistance = impactResistance;
    }
}

public static class MaterialDatabase
{
    public static readonly Dictionary<MaterialKey, Material> Materials = new()
    {
        // STEEL GRADES
        { MaterialKey.MildSteel, new Material("Mild Steel (A36)", MaterialType.Metal, 4.5f, 7850f, 0.5f, 400f, 1510f, 150f) },
        { MaterialKey.HighCarbonSteel, new Material("High-Carbon Steel", MaterialType.Metal, 6.5f, 7800f, 0.6f, 800f, 1480f, 120f) },
        { MaterialKey.StainlessSteel304, new Material("Stainless Steel (304)", MaterialType.Metal, 5.5f, 8000f, 0.6f, 520f, 1450f, 200f) },
        { MaterialKey.AR500Steel, new Material("AR500 Steel", MaterialType.Metal, 7.0f, 7850f, 0.55f, 1250f, 1510f, 250f) },

        // OTHER METALS
        { MaterialKey.Aluminum6061T6, new Material("Aluminum 6061-T6", MaterialType.Metal, 2.8f, 2700f, 0.35f, 310f, 660f, 60f) },
        { MaterialKey.Aluminum7075T6, new Material("Aluminum 7075-T6", MaterialType.Metal, 3.0f, 2810f, 0.3f, 570f, 635f, 50f) },
        { MaterialKey.TitaniumGrade2, new Material("Titanium Grade 2", MaterialType.Metal, 6.0f, 4500f, 0.4f, 350f, 1660f, 250f) },
        { MaterialKey.TitaniumGrade5, new Material("Titanium Grade 5 (Ti-6Al-4V)", MaterialType.Metal, 6.0f, 4430f, 0.35f, 950f, 1660f, 300f) },
        { MaterialKey.Tungsten, new Material("Tungsten", MaterialType.Metal, 7.5f, 19300f, 0.4f, 980f, 3422f, 100f) },
        { MaterialKey.TungstenCarbide, new Material("Tungsten Carbide", MaterialType.Metal, 9.5f, 15600f, 0.1f, 2500f, 2870f, 6700f) },
        { MaterialKey.TungstenAlloy, new Material("Tungsten Heavy Alloy", MaterialType.Metal, 7.5f, 19300f, 0.4f, 900f, 3410f, 5200f) },
        
        // CERAMICS & COMPOSITES
        { MaterialKey.Glass, new Material("Glass", MaterialType.Ceramic, 5.5f, 2500f, 0.9f, 50f, 1400f, 5f) },
        { MaterialKey.Quartz, new Material("Quartz", MaterialType.Ceramic, 7.0f, 2650f, 0.8f, 80f, 1700f, 10f) },
        { MaterialKey.Alumina, new Material("Alumina (Al₂O₃)", MaterialType.Ceramic, 9.0f, 4000f, 0.2f, 400f, 2050f, 30f) },
        { MaterialKey.SiliconCarbide, new Material("Silicon Carbide", MaterialType.Ceramic, 9.5f, 3200f, 0.3f, 450f, 2700f, 40f) },
        { MaterialKey.Kevlar, new Material("Kevlar", MaterialType.Composite, 3.5f, 1440f, 0.5f, 3750f, 500f, 500f) },
        { MaterialKey.CarbonFiber, new Material("Carbon Fiber", MaterialType.Composite, 4.5f, 1800f, 0.4f, 3500f, 650f, 600f) },

        // POLYMERS & ORGANIC MATERIALS
        { MaterialKey.Rubber, new Material("Rubber", MaterialType.Polymer, 1.0f, 1100f, 1.0f, 25f, 300f, 200f) },
        { MaterialKey.Nylon, new Material("Nylon", MaterialType.Polymer, 2.0f, 1200f, 0.25f, 80f, 220f, 100f) },
        { MaterialKey.WoodOak, new Material("Wood (Oak)", MaterialType.Organic, 2.0f, 800f, 0.6f, 90f, 600f, 250f) },
        { MaterialKey.WoodPine, new Material("Wood (Pine)", MaterialType.Organic, 1.5f, 500f, 0.5f, 50f, 500f, 150f) }
    };

    public static Material GetMaterial(MaterialKey key)
    {
        return Materials.TryGetValue(key, out var material) ? material : null;
    }
}