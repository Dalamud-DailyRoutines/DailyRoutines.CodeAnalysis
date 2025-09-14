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
        "ID", "NPC", "API", "URL", "HTTP", "HTTPS", "JSON", "XML", "SQL", "UI", "DB",
        "CPU", "GPU", "RAM", "ROM", "USB", "DVD", "CD", "HD", "SSD", "HDD",
        "HTML", "CSS", "JS", "TS", "PHP", "ASP", "JSP", "CGI", "FTP", "SSH",
        "TCP", "UDP", "IP", "DNS", "DHCP", "VPN", "LAN", "WAN", "WIFI", "GPS",
        "AI", "ML", "DL", "CNN", "RNN", "GAN", "NLP", "OCR", "AR", "VR",
        "OS", "SDK", "IDE", "CLI", "GUI", "REST", "SOAP", "JWT", "OAuth", "CORS",
        "CRUD", "MVC", "MVP", "MVVM", "OOP", "AOP", "DI", "IoC", "ORM", "DAO",
        "DTO", "VO", "POJO", "POCO", "UUID", "GUID", "MD5", "SHA", "AES", "RSA"
    };
}
