namespace gmtk2024.Runtime.Stat;

[Stat]
[IconIdentifier("💖")]
[RoundTo(RoundingMode.Floor)]
[Serializable]
public sealed class Health : Resource { }

[Stat]
[IconIdentifier("⚔️")]
[Serializable]
public sealed class Damage : Stat { }

[Stat]
[IconIdentifier("💥")]
[ClampMinMax(0, 100)]
[RoundTo(RoundingMode.Floor)]
[Serializable]
public sealed class Crit : Stat { }
