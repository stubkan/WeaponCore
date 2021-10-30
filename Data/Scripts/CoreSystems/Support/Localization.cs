﻿using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage;

namespace CoreSystems.Support
{
    public static class Localization
    {
        private static readonly Dictionary<MyLanguagesEnum, Dictionary<string, string>> I18NDictionaries =
            new Dictionary<MyLanguagesEnum, Dictionary<string, string>>
            {
                {
                    MyLanguagesEnum.English, new Dictionary<string, string>
                    {
                        { "TerminalSwitchOn", "On" },
                        { "TerminalSwitchOff", "Off" },
                        { "TerminalGuidanceTitle", "Enable Guidance" },
                        { "TerminalGuidanceTooltip", "Enable Guidance" },
                        { "TerminalWeaponDamageTitle", "Change Damage Per Shot" },
                        { "TerminalWeaponDamageTooltip", "Change the damage per shot" },
                        { "TerminalWeaponROFTitle", "Change Rate of Fire" },
                        { "TerminalWeaponROFTooltip", "Change rate of fire" },
                        { "TerminalOverloadTitle", "Overload Damage" },
                        { "TerminalOverloadTooltip", "Overload damage" },
                        { "TerminalDetonationTitle", "Detonation time" },
                        { "TerminalDetonationTooltip", "Detonation time" },
                        { "TerminalStartCountTitle", "Start Countdown" },
                        { "TerminalStartCountTooltip", "Start Countdown" },
                        { "TerminalStopCountTitle", "Stop Countdown" },
                        { "TerminalStopCountTooltip", "Stop Countdown" },
                        { "TerminalArmTitle", "Arm Reaction" },
                        { "TerminalArmTooltip", "Arm Reaction" },
                        { "TerminalTriggerTitle", "Trigger" },
                        { "TerminalTriggerTooltip", "Trigger" },
                        { "TerminalWeaponRangeTitle", "Aiming Radius" },
                        { "TerminalWeaponRangeTooltip", "Change the min/max targeting range" },
                        { "TerminalNeutralsTitle", "Target Neutrals" },
                        { "TerminalNeutralsTooltip", "Fire on targets that are neutral" },
                        { "TerminalUnownedTitle", "Target Unowned" },
                        { "TerminalUnownedTooltip", "Fire on targets with no owner" },
                        { "TerminalBiologicalsTitle", "Target Biologicals" },
                        { "TerminalBiologicalsTooltip", "Fire on players and biological NPCs" },
                        { "TerminalProjectilesTitle", "Target Projectiles" },
                        { "TerminalProjectilesTooltip", "Fire on incoming projectiles" },
                        { "TerminalMeteorsTitle", "Target Meteors" },
                        { "TerminalMeteorsTooltip", "Target Meteors" },
                        { "TerminalGridsTitle", "Target Grids" },
                        { "TerminalGridsTooltip", "Target Grids" },
                        { "TerminalFocusFireTitle", "Target FocusFire" },
                        { "TerminalFocusFireTooltip", "Focus all fire on the specified target" },
                        { "TerminalSubSystemsTitle", "Target SubSystems" },
                        { "TerminalSubSystemsTooltip", "Target specific SubSystems of a target" },
                        { "TerminalRepelTitle", "Repel Mode" },
                        { "TerminalRepelTooltip", "Aggressively focus and repel small threats" },
                        { "TerminalPickAmmoTitle", "Pick Ammo" },
                        { "TerminalPickAmmoTooltip", "Select the ammo type to use" },
                        { "TerminalPickSubSystemTitle", "Pick SubSystem" },
                        { "TerminalPickSubSystemTooltip", "Select the target subsystem to focus fire on" },
                        { "TerminalTrackingModeTitle", "Tracking Mode" },
                        { "TerminalTrackingModeTooltip", "Movement fire control requirements" },
                        { "TerminalControlModesTitle", "Control Mode" },
                        { "TerminalControlModesTooltip", "Select the aim control mode for the weapon" },
                        { "TerminalCameraChannelTitle", "Weapon Camera Channel" },
                        { "TerminalCameraChannelTooltip", "Assign this weapon to a camera channel" },
                        { "TerminalTargetGroupTitle", "Target Lead Group" },
                        { "TerminalTargetGroupTooltip", "Assign this weapon to target lead group" },
                        { "TerminalDecoyPickSubSystemTitle", "Pick SubSystem" },
                        { "TerminalDecoyPickSubSystemTooltip", "Pick what subsystem this decoy will imitate" },
                        { "TerminalCameraCameraChannelTitle", "Camera Channel" },
                        { "TerminalCameraCameraChannelTooltip", "Assign the camera weapon channel to this camera" },
                        { "TerminalDebugTitle", "Debug" },
                        { "TerminalDebugTooltip", "Debug On/Off" },
                        { "TerminalShootTitle", "Shoot" },
                        { "TerminalShootTooltip", "Shoot On/Off" },
                        { "ActionWC_Shoot_Click", "Toggle Click To Fire" },
                        { "ActionShootOnce", "Shoot Once" },
                        { "ActionShoot", "Shoot On/Off" },
                        { "ActionShoot_Off", "Shoot Off" },
                        { "ActionShoot_On", "Shoot On" },
                        { "ActionSubSystems", "Cycle SubSystems" },
                        { "ActionNextCameraChannel", "Next Channel" },
                        { "ActionPreviousCameraChannel", "Previous Channel" },
                        { "ActionControlModes", "Control Mode" },
                        { "ActionNeutrals", "Neutrals On/Off" },
                        { "ActionProjectiles", "Projectiles On/Off" },
                        { "ActionBiologicals", "Biologicals On/Off" },
                        { "ActionMeteors", "Meteors On/Off" },
                        { "ActionGrids", "Grids On/Off" },
                        { "ActionFriendly", "Friendly On/Off" },
                        { "ActionUnowned", "Unowned On/Off" },
                        { "ActionFocusTargets", "FocusTargets On/Off" },
                        { "ActionFocusSubSystem", "FocusSubSystem On/Off" },
                        { "ActionMaxSizeIncrease", "MaxSize Increase" },
                        { "ActionMaxSizeDecrease", "MaxSize Decrease" },
                        { "ActionMinSizeIncrease", "MinSize Increase" },
                        { "ActionMinSizeDecrease", "MinSize Decrease" },
                        { "ActionTrackingMode", "Tracking Mode" },
                        { "ActionWC_CycleAmmo", "Cycle Consumable" },
                        { "ActionWC_RepelMode", "Repel Mode" },
                        { "ActionWC_Increase_CameraChannel", "Next Camera Channel" },
                        { "ActionWC_Decrease_CameraChannel", "Previous Camera Channel" },
                        { "ActionWC_Increase_LeadGroup", "Next Lead Group" },
                        { "ActionWC_Decrease_LeadGroup", "Previous Lead Group" },
                        { "ActionMask", "Select Mask Type" },
                        { "ActionWC_Toggle", "Toggle On/Off" },
                        { "ActionWC_Toggle_On", "On" },
                        { "ActionWC_Toggle_Off", "Off" },
                        { "ActionWC_Increase", "Increase" },
                        { "ActionWC_Decrease", "Decrease" }
                    }
                },
                {
                    MyLanguagesEnum.ChineseChina, new Dictionary<string, string>
                    {
                        { "TerminalSwitchOn", "开启" },
                        { "TerminalSwitchOff", "关闭" },
                        { "TerminalGuidanceTitle", "启用制导" },
                        { "TerminalGuidanceTooltip", "启用制导" },
                        { "TerminalWeaponDamageTitle", "改变每发的伤害" },
                        { "TerminalWeaponDamageTooltip", "改变每发的伤害" },
                        { "TerminalWeaponROFTitle", "改变射速" },
                        { "TerminalWeaponROFTooltip", "改变射速" },
                        { "TerminalOverloadTitle", "过载伤害" },
                        { "TerminalOverloadTooltip", "过载伤害" },
                        { "TerminalDetonationTitle", "引爆时间" },
                        { "TerminalDetonationTooltip", "引爆时间" },
                        { "TerminalStartCountTitle", "开始倒计时" },
                        { "TerminalStartCountTooltip", "开始倒计时" },
                        { "TerminalStopCountTitle", "停止倒计时" },
                        { "TerminalStopCountTooltip", "停止倒计时" },
                        { "TerminalArmTitle", "装备" },
                        { "TerminalArmTooltip", "装备反应" },
                        { "TerminalTriggerTitle", "触发" },
                        { "TerminalTriggerTooltip", "触发" },
                        { "TerminalWeaponRangeTitle", "瞄准半径" },
                        { "TerminalWeaponRangeTooltip", "改变最小/最大瞄准范围" },
                        { "TerminalNeutralsTitle", "攻击中立方" },
                        { "TerminalNeutralsTooltip", "向中立目标开火" },
                        { "TerminalUnownedTitle", "攻击无主" },
                        { "TerminalUnownedTooltip", "向无主目标开火" },
                        { "TerminalBiologicalsTitle", "攻击角色" },
                        { "TerminalBiologicalsTooltip", "向玩家和生物性非玩家角色开火" },
                        { "TerminalProjectilesTitle", "攻击导弹" },
                        { "TerminalProjectilesTooltip", "向来袭的投射物开火" },
                        { "TerminalMeteorsTitle", "攻击流星" },
                        { "TerminalMeteorsTooltip", "向流星开火" },
                        { "TerminalGridsTitle", "攻击网格" },
                        { "TerminalGridsTooltip", "向网格开火" },
                        { "TerminalFocusFireTitle", "集火" },
                        { "TerminalFocusFireTooltip", "将所有火力集中在指定目标上" },
                        { "TerminalSubSystemsTitle", "瞄准子系统" },
                        { "TerminalSubSystemsTooltip", "瞄准目标上的特定子系统" },
                        { "TerminalRepelTitle", "击退模式" },
                        { "TerminalRepelTooltip", "积极关注并击退小型威胁" },
                        { "TerminalPickAmmoTitle", "选择弹药" },
                        { "TerminalPickAmmoTooltip", "选择要使用的弹药类型" },
                        { "TerminalPickSubSystemTitle", "选择子系统" },
                        { "TerminalPickSubSystemTooltip", "选择要集火的子系统" },
                        { "TerminalTrackingModeTitle", "追踪模式" },
                        { "TerminalTrackingModeTooltip", "移动火控需求" },
                        { "TerminalControlModesTitle", "控制模式" },
                        { "TerminalControlModesTooltip", "选择武器的瞄准控制模式" },
                        { "TerminalCameraChannelTitle", "武器摄像头频道" },
                        { "TerminalCameraChannelTooltip", "将此武器分配到一个摄像头频道" },
                        { "TerminalTargetGroupTitle", "目标引导组" },
                        { "TerminalTargetGroupTooltip", "将此武器分配到目标引导组" },
                        { "TerminalDecoyPickSubSystemTitle", "选择子系统" },
                        { "TerminalDecoyPickSubSystemTooltip", "选择这个诱饵将模仿的子系统" },
                        { "TerminalCameraCameraChannelTitle", "摄像机频道" },
                        { "TerminalCameraCameraChannelTooltip", "将武器摄像头频道绑定到此摄像头上" },
                        { "TerminalDebugTitle", "调试" },
                        { "TerminalDebugTooltip", "调试 开启/关闭" },
                        { "TerminalShootTitle", "射击" },
                        { "TerminalShootTooltip", "射击 开启/关闭" },
                        { "ActionWC_Shoot_Click", "切换点击开火" },
                        { "ActionShootOnce", "射击一次" },
                        { "ActionShoot", "射击 开启/关闭" },
                        { "ActionShoot_Off", "射击 关闭" },
                        { "ActionShoot_On", "射击 开启" },
                        { "ActionSubSystems", "循环子系统" },
                        { "ActionNextCameraChannel", "下一个频道" },
                        { "ActionPreviousCameraChannel", "上一个频道" },
                        { "ActionControlModes", "控制模式" },
                        { "ActionNeutrals", "攻击中立方 开启/关闭" },
                        { "ActionProjectiles", "攻击导弹 开启/关闭" },
                        { "ActionBiologicals", "攻击角色 开启/关闭" },
                        { "ActionMeteors", "攻击流星 开启/关闭" },
                        { "ActionGrids", "攻击网格 开启/关闭" },
                        { "ActionFriendly", "攻击友方 开启/关闭" },
                        { "ActionUnowned", "攻击无主 开启/关闭" },
                        { "ActionFocusTargets", "集火 开启/关闭" },
                        { "ActionFocusSubSystem", "瞄准子系统 开启/关闭" },
                        { "ActionMaxSizeIncrease", "最大大小增加" },
                        { "ActionMaxSizeDecrease", "最大大小减少" },
                        { "ActionMinSizeIncrease", "最小大小增加" },
                        { "ActionMinSizeDecrease", "最小大小减少" },
                        { "ActionTrackingMode", "追踪模式" },
                        { "ActionWC_CycleAmmo", "循环消耗品" },
                        { "ActionWC_RepelMode", "击退模式" },
                        { "ActionWC_Increase_CameraChannel", "下一个摄像头频道" },
                        { "ActionWC_Decrease_CameraChannel", "上一个摄像头频道" },
                        { "ActionWC_Increase_LeadGroup", "下一个目标引导组" },
                        { "ActionWC_Decrease_LeadGroup", "上一个目标引导组" },
                        { "ActionMask", "选择伪装类型" },
                        { "ActionWC_Toggle", "切换 开启/关闭" },
                        { "ActionWC_Toggle_On", "开启" },
                        { "ActionWC_Toggle_Off", "关闭" },
                        { "ActionWC_Increase", "增加" },
                        { "ActionWC_Decrease", "减少" }
                    }
                }
            };

        private const MyLanguagesEnum FallbackLanguage = MyLanguagesEnum.English;

        private static readonly MyLanguagesEnum Language = MyAPIGateway.Session.Config.Language;

        private static readonly Dictionary<string, string> FallbackI18NDictionary = I18NDictionaries[FallbackLanguage];

        private static readonly Dictionary<string, string> I18NDictionary =
            I18NDictionaries.GetValueOrDefault(Language, FallbackI18NDictionary);

        internal static string GetText(string text)
        {
            string value;
            if (I18NDictionary.TryGetValue(text, out value))
            {
                return value;
            }

            if (FallbackI18NDictionary.TryGetValue(text, out value))
            {
                return value;
            }

            return text;
        }
    }
}