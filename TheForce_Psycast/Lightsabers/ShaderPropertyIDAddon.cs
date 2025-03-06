using UnityEngine;
using Verse;

[StaticConstructorOnStartup]
public static class ShaderPropertyIDAddon
{
    // Texture properties
    private static readonly string CoreTexName = "_CoreTex";
    private static readonly string BladeTexName = "_BladeTex";
    private static readonly string GlowTexName = "_GlowTex";

    // Color properties
    private static readonly string CoreColorName = "_CoreColor";
    private static readonly string BladeColorName = "_BladeColor";

    // Glow properties
    private static readonly string GlowIntensityName = "_GlowIntensity";
    private static readonly string GlowSpreadName = "_GlowSpread";

    // Public property IDs
    public static readonly int CoreTex = Shader.PropertyToID(CoreTexName);
    public static readonly int BladeTex = Shader.PropertyToID(BladeTexName);
    public static readonly int GlowTex = Shader.PropertyToID(GlowTexName);

    public static readonly int CoreColor = Shader.PropertyToID(CoreColorName);
    public static readonly int BladeColor = Shader.PropertyToID(BladeColorName);

    public static readonly int GlowIntensity = Shader.PropertyToID(GlowIntensityName);
    public static readonly int GlowSpread = Shader.PropertyToID(GlowSpreadName);
}
