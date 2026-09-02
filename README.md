# 勇者试炼｜Unity2D RPG Demo
> 个人练手项目，Unity C# 2D客户端游戏，求职练习demo

## 项目简介
2D角色扮演小游戏，实现背包系统、敌人AI状态机、商店系统、多账号存档、UGUI全套界面。

## 技术栈
Unity 2022 LTS | C# | UGUI | Addressables  可寻址资源管理 | 面向对象、状态机模式、MVC架构
泛型、委托事件、Lambda；集合容器（List/Dictionary) | 本地多账号数据持久化

## 已实现功能
1. **背包&快捷栏系统（MVC架构）**：物品拾取、拖拽交换、半透明拖拽悬浮效果、物品堆叠合并、快捷栏优先存取，分离数据层、视图层、控制层。
2. **敌人AI有限状态机**：实现巡逻、追击、攻击、受击状态切换，动画状态同步，参数驱动敌人行为逻辑。
3. **UGUI游戏UI体系**：动态血条滑块、商店交易系统、登录注册面板，处理UI层级、事件系统、跨Canvas拖拽问题。
4. **存档持久化**：基于PlayerPrefs封装多账号独立存档方案，切换账号数据隔离。
5. **游戏核心玩法**：场景双向传送、角色受伤判定、消耗类道具回血、怪物掉落物品逻辑。
6.** NPC对话功能 **

## 体验游戏
 [Windows可运行Demo下载](https://github.com/pjttt/Hero-Trial/releases/download/v1/demo.zip) 

 [demo运行视频](https://github.com/pjttt/Hero-Trial/releases/download/v1/demo.mp4)
> 下载压缩包解压，双击 `勇者试炼.exe` 直接运行。

## 重点代码位置
- `Assets/Scripts/Inventory`：背包系统全部核心脚本
- `Assets/Scripts/Enemy`：敌人AI状态机脚本
- `Assets/Scripts/UI`：UGUI界面逻辑
- `Assets/Scripts/Save`：存档持久化逻辑

## 运行源码方法
1. Git clone下载本项目源码
2. 使用Unity2022 LTS打开项目
