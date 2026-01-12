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
        "ID", "UI", "NPC", "HQ", "NQ", "DC", "DPS",
        "XML", "HTTP", "FTP", "URL", "HTML", "JSON", "CSV", "SQL", "DB", "API",
        "CPU", "RAM", "OS", "IP", "TCP", "UDP", "SSL", "TLS", "SSH",
        "AES", "DES", "RSA", "MD5", "SHA",
        "GUI", "CLI", "SDK", "DLL", "EXE", "URI", "UTC", "GMT", "GUID", "UUID",
        "DTO", "VO", "POJO", "DAO"
    };
}
