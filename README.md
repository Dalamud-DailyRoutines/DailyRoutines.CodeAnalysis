# DailyRoutines 代码规范分析器

这个项目包含为 DailyRoutines 设计的代码规范分析器，用于确保代码风格一致性。

## 项目结构

```
DailyRoutines.CodeAnalysis/
├── Common/                     # 通用基础设施
│   ├── BaseAnalyzer.cs         # 分析器基类
│   ├── BaseCodeFixProvider.cs  # 代码修复提供者基类
│   ├── DiagnosticCategories.cs # 诊断类别常量
│   ├── DiagnosticDescriptorFactory.cs # 诊断描述符工厂
│   └── DiagnosticRules.cs      # 所有规则的描述符集合
├── Rules/                      # 规则实现
│   ├── Naming/                 # 命名规则相关
│   │   ├── DoNotUseUnderscorePrefixInNamesAnalyzer.cs
│   │   └── DoNotUseUnderscorePrefixInNamesCodeFixProvider.cs
│   ├── Usage/                  # 用法规则相关
│   │   ├── UseNintInsteadOfIntPtrAnalyzer.cs
│   │   └── UseNintInsteadOfIntPtrCodeFixProvider.cs
│   ├── Performance/            # 性能规则相关（待添加）
│   └── Design/                 # 设计规则相关（待添加）
└── Templates/                  # 规则模板
    ├── TemplateAnalyzer.cs     # 分析器模板
    └── TemplateCodeFixProvider.cs # 代码修复提供者模板
```

## 当前规则

| 规则 ID | 类别 | 描述 | 严重性 |
|---------|------|------|--------|
| DR0001 | 用法 | 使用 `nint` 代替 `IntPtr` | 错误 |
| DR0002 | 命名 | 不允许命名以下划线开头 | 错误 |
| DR0004 | 设计 | 控制语句的语句体必须另起一行 | 警告 |
| DR0005 | 设计 | 只有一行代码的控制语句不应使用大括号 | 警告 |

## 使用方法

这个分析器已经通过 `