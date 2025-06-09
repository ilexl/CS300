using System.Collections.Generic;

namespace Ballistics.Database
{
    public enum ProjectileKey
    {
        M62,
        M829A4,
        T99APT,
    }

    public class ProjectileDefinition
    {
        public string Name { get; set; }
        public MaterialKey MaterialKey { get; set; }
        public Material Material => MaterialDatabase.GetMaterial(MaterialKey);
        public float DiameterMm { get; set; }
        public float LengthMm { get; set; }
        public float VelocityMs { get; set; }

        public ProjectileDefinition(string name, MaterialKey materialKey, float diameterMm, float lengthMm,
            float velocityMs)
        {
            Name = name;
            MaterialKey = materialKey;
            DiameterMm = diameterMm;
            LengthMm = lengthMm;
            VelocityMs = velocityMs;
        }
    }
    public static class ProjectileDatabase
    {
        private static readonly Dictionary<ProjectileKey, ProjectileDefinition> ProjectileDefinition = new()
        {
            { ProjectileKey.M62 , new ProjectileDefinition("M62 (US)", MaterialKey.HighCarbonSteel, 76.2f, 355, 792)},
            { ProjectileKey.M829A4 , new ProjectileDefinition("M829A4 (US)", MaterialKey.DepletedUranium, 25f, 850, 1570)},
            { ProjectileKey.T99APT, new ProjectileDefinition("T99 AP-T (US)", MaterialKey.HighCarbonSteel, 120f, 965, 945)}
        };

        public static ProjectileDefinition GetProjectile(ProjectileKey key)
        {
            return ProjectileDefinition[key];
        }
    }
}