# CodexRemaining

一个使用 C# + WPF 构建的桌面悬浮工具，用于显示 Codex 使用限制和剩余额度。

## 已实现能力

- 总在最上层、可拖拽、可调大小、半透明悬浮窗。
- 6 个关键指标（包含你要求的 5 类限制 + 剩余信用）以进度条展示：
  - 5小时使用限制
  - 每周使用限制
  - GPT-5.3 Codex-Spark 5小时限制
  - GPT-5.3 Codex-Spark 每周限制
  - 代码审查使用限制
  - 剩余信用
- 自动刷新（默认 10 秒）+ 手动立即刷新。
- 颜色阈值：绿色（健康）/黄色（接近）/红色（告急）。
- 接近阈值时弹窗提醒（默认 <= 10%）。
- `appsettings.json` 配置透明度、刷新频率、API 地址等。
- API 拉取失败时自动回退到模拟数据，并在 UI 显示状态。

## 运行说明

> WPF 仅支持 Windows。请在 Windows + .NET 8 SDK 环境下构建和运行。

```bash
dotnet build CodexRemaining.sln
```

## API 对接说明

当前通过 `CodexUsageApiClient` 读取：

- Base URL: `ApiBaseUrl`
- Endpoint: `UsageEndpoint`
- Header: `Authorization: Bearer $OPENAI_API_KEY`（若环境变量存在）

示例 payload 字段（可按真实 API 调整）：

```json
{
  "limit5hRemainingPercent": 72,
  "limit5hResetAt": "2026-01-10T09:00:00Z",
  "weeklyRemainingPercent": 55,
  "weeklyResetAt": "2026-01-12T00:00:00Z",
  "spark5hRemainingPercent": 44,
  "spark5hResetAt": "2026-01-10T10:00:00Z",
  "sparkWeeklyRemainingPercent": 68,
  "sparkWeeklyResetAt": "2026-01-12T00:00:00Z",
  "codeReviewRemainingPercent": 18,
  "codeReviewResetAt": "2026-01-11T00:00:00Z",
  "creditRemainingPercent": 33,
  "creditResetAt": "2026-02-01T00:00:00Z"
}
```

## 后续建议

- 用 `TaskDialog`/系统通知替代 MessageBox，减少打断。
- 使用托盘图标支持隐藏后恢复。
- 补充 ViewModel 单元测试与 API 反序列化测试。
