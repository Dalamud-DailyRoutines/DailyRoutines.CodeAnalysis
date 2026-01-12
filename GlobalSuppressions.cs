// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// 禁用所有代码分析警告
[assembly: SuppressMessage("Style", "IDE0001:Simplify name",                     Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Style", "IDE0002:Simplify member access",            Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Style", "IDE0003:Remove qualification",              Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Style", "IDE0004:Remove unnecessary cast",           Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Style", "IDE0005:Remove unnecessary imports",        Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "项目级别禁用警告")]

// 禁用所有代码质量分析警告
[assembly: SuppressMessage("Design",      "CA1000-CA9999:设计规则",  Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Performance", "CA1800-CA1899:性能规则",  Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Reliability", "CA2000-CA2999:可靠性规则", Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Usage",       "CA1801-CA1899:用法规则",  Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Naming",      "CA1700-CA1799:命名规则",  Justification = "项目级别禁用警告")]

// 禁用分析器规则
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1000-RS9999:分析器规则", Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisDesign",      "RS1000-RS9999:分析器设计", Justification = "项目级别禁用警告")]

// 禁用Roslyn规则
[assembly: SuppressMessage("Roslyn", "RS0001-RS9999:Roslyn规则", Justification = "项目级别禁用警告")]

// 禁用特定见过的警告
[assembly: SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Usage",       "CA2227:集合属性应为只读",                  Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Usage",       "CA2234:传递系统 URI 对象而非字符串",          Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Design",      "CA1031:不捕获常规异常类型",                 Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Naming",      "CA1707:标识符不应包含下划线",                Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Naming",      "CA1716:标识符不应与关键字匹配",               Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Naming",      "CA1724:类型名称不应与命名空间名称冲突",           Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Naming",      "CA1725:参数名称应与基方法中的参数名称匹配",         Justification = "项目级别禁用警告")]
[assembly: SuppressMessage("Design",      "CA1062:验证公共方法的参数",                 Justification = "项目级别禁用警告")]
