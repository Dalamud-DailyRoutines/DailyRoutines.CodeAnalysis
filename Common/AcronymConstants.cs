using System;
using System.Collections.Generic;

namespace DailyRoutines.CodeAnalysis.Common;

/// <summary>
/// 英文缩写常量定义
/// </summary>
public static class AcronymConstants
{
    public static readonly HashSet<string> CommonAcronyms = new(StringComparer.OrdinalIgnoreCase)
    {
        "ID", "UI", "NPC", "HQ", "NQ", "DC", "DPS"
    };
}
