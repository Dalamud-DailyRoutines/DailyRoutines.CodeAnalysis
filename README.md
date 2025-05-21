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
│   ├── Design/                 # 设计规则相关
│   │   ├── ControlStatementBodyMustBeOnNewLineAnalyzer.cs
│   │   ├── ControlStatementBodyMustBeOnNewLineCodeFixProvider.cs
│   │   ├── SingleLineControlStatementMustNotUseBlockAnalyzer.cs
│   │   └── SingleLineControlStatementMustNotUseBlockCodeFixProvider.cs
│   └── Performance/            # 性能规则相关（待添加）
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

## 添加新规则

要添加新的规则，请按照以下步骤操作:

1. 在 `Common/DiagnosticRules.cs` 中添加新的规则描述符
2. 选择适当的规则类别文件夹（或创建新文件夹）
3. 复制 `Templates` 目录下的模板文件，根据你的规则进行修改
4. 编译并测试新规则

### 添加分析器示例

1. 复制 `Templates/TemplateAnalyzer.cs` 到适当的类别文件夹中，例如 `Rules/Performance/AvoidUnnecessaryAllocationAnalyzer.cs`
2. 修改命名空间、类名和规则实现
3. 在 `Common/DiagnosticRules.cs` 中添加规则描述符：

```csharp
/// <summary>
/// 诊断规则：避免不必要的内存分配
/// </summary>
public static readonly DiagnosticDescriptor AvoidUnnecessaryAllocation = DiagnosticDescriptorFactory.Create(
    id: "0003",
    title: "避免不必要的内存分配",
    messageFormat: "这段代码可能导致不必要的内存分配",
    category: DiagnosticCategories.Performance,
    description: "避免在性能敏感的代码路径中进行不必要的内存分配。"
);
```

4. 实现新的分析器：

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidUnnecessaryAllocationAnalyzer : BaseAnalyzer
{
    public AvoidUnnecessaryAllocationAnalyzer() 
        : base(DiagnosticRules.AvoidUnnecessaryAllocation)
    {
    }

    protected override void RegisterAnalyzers(AnalysisContext context)
    {
        // 实现分析逻辑...
    }
}
```

5. 如果需要，创建对应的代码修复提供程序

### 规则命名约定

- 分析器类命名格式：`<描述性名称>Analyzer`
- 代码修复提供者命名格式：`<与分析器相同的描述性名称>CodeFixProvider`
- 规则ID格式：`DR<四位数字>` (例如 `DR0001`)

## 构建和测试

```powershell
# 构建项目
dotnet build

# 生成NuGet包
dotnet pack -c Release
```

## 许可证

本项目采用MIT许可证。有关详细信息，请参阅 [LICENSE](LICENSE) 文件。