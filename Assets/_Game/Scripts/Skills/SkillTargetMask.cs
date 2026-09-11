using System;

[Flags]
public enum SkillTargetMask
{
    None = 0,
    Enemy = 1 << 0,
    Elite = 1 << 1,
    Boss = 1 << 2
}
