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
        "ID", "NPC", "API", "URL", "XML", "SQL", "UI", "DB",
        "CPU", "GPU", "RAM", "ROM", "USB", "DVD", "CD", "HD", "SSD", "HDD",
        "CSS", "JS", "TS", "PHP", "ASP", "JSP", "CGI", "FTP", "SSH",
        "TCP", "UDP", "IP", "DNS", "VPN", "GPS",
        "AI", "ML", "DL", "CNN", "RNN", "GAN", "NLP", "OCR", "AR", "VR",
        "OS", "SDK", "IDE", "CLI", "GUI", "JWT",
        "MVC", "MVP", "OOP", "AOP", "DI", "ORM", "DAO",
        "DTO", "VO", "MD5", "SHA", "AES", "RSA"
    };
}
