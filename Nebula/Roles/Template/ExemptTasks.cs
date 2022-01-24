﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Nebula.Patches;
using Nebula.Objects;
using HarmonyLib;
using Hazel;
using Nebula.Game;

namespace Nebula.Roles.Template
{
    public class ExemptTasks : Role
    {
        private Module.CustomOption exemptTasksOption;
        //初期設定でのタスク免除数
        protected int InitialExemptTasks { get; set; } = 1;
        //最高タスク免除数
        protected int MaxExemptTasks { get; set; } = 10;
        public override void IntroInitialize(PlayerControl __instance)
        {
            int exempt = (int)exemptTasksOption.getFloat();
            byte result = 0;
            int tasks = PlayerControl.LocalPlayer.myTasks.Count;
            tasks -= exempt;
            if (tasks < 0) tasks = 0;
            RPCEventInvoker.ExemptTasks(__instance.PlayerId, tasks, tasks);
        }

        public override void LoadOptionData()
        {
            exemptTasksOption = CreateOption(Color.white, "exemptTasks", (float)InitialExemptTasks, 0f, (float)MaxExemptTasks, 1f);
        }

        //インポスターはModで操作するFakeTaskは所持していない
        protected ExemptTasks(string name, string localizeName, Color color, RoleCategory category,
            Side side, Side introMainDisplaySide, HashSet<Side> introDisplaySides, HashSet<Side> introInfluenceSides,
            HashSet<Patches.EndCondition> winReasons,
            bool hasFakeTask, bool canUseVents, bool canMoveInVents,
            bool ignoreBlackout, bool useImpostorLightRadius) :
            base(name, localizeName, color, category,
                side, introMainDisplaySide, introDisplaySides, introInfluenceSides,
                winReasons,
                hasFakeTask, canUseVents, canMoveInVents,
                ignoreBlackout, useImpostorLightRadius)
        {
        }
    }
}