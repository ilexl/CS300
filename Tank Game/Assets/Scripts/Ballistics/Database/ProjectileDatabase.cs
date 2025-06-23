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
        public MaterialKey MaterialKey { get; set; } // The material key to search the database for when referencing the material of this projectile.
        /// <summary>
        /// The material composition of the projectile.
        /// <para>
        /// Material properties affect several key behaviors:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <b>Higher density materials</b> increase the projectile's kinetic energy (KE),
        /// resulting in greater penetration at the same velocity.
        /// </item>
        /// <item>
        /// <b>Higher hardness materials</b> convert kinetic energy more efficiently on impact,
        /// improving armor penetration.
        /// </item>
        /// <item>
        /// <b>Materials with high impact resistance</b> are less likely to shatter on impact,
        /// modeled in-game as a flat bonus to penetration.
        /// </item>
        /// </list>
        /// </summary>
        public Material Material => MaterialDatabase.GetMaterial(MaterialKey);

        /// <summary>
        /// The diameter of the projectile, in millimeters (mm).
        /// <para>
        /// Effects of higher projectile diameters:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// Create larger, more lethal spalling in a wider cone.
        /// </item>
        /// <item>
        /// Ricochet more easily.
        /// </item>
        /// <item>
        /// Have worse penetration against sloped armor compared to lower-diameter projectiles of the same length.
        /// </item>
        /// <item>
        /// Raw penetration value is unchanged.
        /// </item>
        /// </list>
        /// <para>
        /// Effects of lower projectile diameters:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// Create smaller, less lethal spalling in a tighter cone.
        /// </item>
        /// <item>
        /// Ricochet less easily (better resistance to deflection).
        /// </item>
        /// <item>
        /// Have better penetration against sloped armor compared to higher-diameter projectiles of the same length.
        /// </item>
        /// <item>
        /// Raw penetration value is unchanged.
        /// </item>
        /// </list>
        /// </summary>
        public float DiameterMm { get; set; }

        /// <summary>
        /// The length of the projectile, in millimeters (mm).
        /// <para>
        /// Longer projectiles demonstrate several advantages:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// Lower ricochet chance.
        /// </item>
        /// <item>
        /// Improved penetration against sloped armor.
        /// </item>
        /// <item>
        /// Higher penetration at the same velocity.
        /// </item>
        /// </list>
        /// <para>
        /// Shorter projectiles are lighter and may achieve higher muzzle velocities, but with inferior performance against sloped or complex armor profiles.
        /// </para>
        /// </summary>
        public float LengthMm { get; set; }

        /// <summary>
        /// The velocity of the projectile, in meters per second (m/s).
        /// <para>
        /// Effects of higher projectile velocities:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// Increase kinetic energy, resulting in greater armor penetration.
        /// </item>
        /// <item>
        /// Create tighter, faster-moving spall cones upon penetration.
        /// </item>
        /// <item>
        /// Increase ricochet resistance, reducing the chance of deflection.
        /// </item>
        /// </list>
        /// <para>
        /// Lower velocities result in reduced penetration performance, slower spall formation, and higher ricochet chance.
        /// </para>
        /// </summary>
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
        // The master projectile list. Contains a ref to every definition.
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