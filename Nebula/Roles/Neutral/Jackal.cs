﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using Hazel;
using Nebula.Objects;

namespace Nebula.Roles.Neutral
{
    public class Jackal : Role
    {
        static public Color Color = new Color(0, 162, 211);

        static private CustomButton killButton;

        public override void MyPlayerControlUpdate()
        {
            Game.MyPlayerData data = Game.GameData.data.myData;
            data.currentTarget = Patches.PlayerControlPatch.SetMyTarget();
            Patches.PlayerControlPatch.SetPlayerOutline(data.currentTarget, Palette.ImpostorRed);
        }

        public override void ButtonInitialize(HudManager __instance)
        {
            if (killButton != null)
            {
                killButton.Destroy();
            }
            killButton = new CustomButton(
                () =>
                {
                    byte targetId = Game.GameData.data.myData.currentTarget.PlayerId;

                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(PlayerControl.LocalPlayer.PlayerId);
                    killWriter.Write(targetId);
                    killWriter.Write(byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCEvents.UncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, targetId, Byte.MaxValue);


                    killButton.Timer = killButton.MaxTimer;
                    Game.GameData.data.myData.currentTarget = null;
                },
                () => { return !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Game.GameData.data.myData.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { killButton.Timer = killButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                new Vector3(0f, 1f, 0),
                __instance,
                KeyCode.Q
            );
            killButton.MaxTimer = 20;
        }

        public override void CleanUp()
        {
            if (killButton != null)
            {
                killButton.Destroy();
                killButton = null;
            }
        }

        public Jackal()
            : base("Jackal", "jackal", Color, RoleCategory.Neutral, Side.Jackal, Side.Jackal,
                 new HashSet<Side>() { Side.Jackal }, new HashSet<Side>() { Side.Jackal },
                 new HashSet<Patches.EndCondition>() { Patches.EndCondition.JackalWin },
                 true, true, true, true, true)
        {
            killButton = null;
        }
    }
}
