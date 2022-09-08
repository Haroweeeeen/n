﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Nebula.Objects;
using Nebula.Module;

namespace Nebula.Roles.CrewmateRoles
{
    public class Guardian : Role
    {
        static public Color RoleColor = new Color(171f / 255f, 131f / 255f, 85f / 255f);

        private CustomButton antennaButton;
        private CustomButton guardButton;
        private HashSet<Objects.CustomObject> myAntennaSet = new HashSet<CustomObject>();
        private Utilities.ObjectPool<SpriteRenderer>? indicatorsPool=null;

        private CustomOption maxAntennaOption;
        private CustomOption placeCoolDownOption;
        private CustomOption antennaEffectiveRangeOption;

        public int remainAntennasId { get; private set; }

        private PlayerControl? guardPlayer;

        public override void OnRoleRelationSetting()
        {
            RelatedRoles.Add(Roles.Navvy);
            RelatedRoles.Add(Roles.NiceTrapper);
            RelatedRoles.Add(Roles.EvilTrapper);
        }

        public override void LoadOptionData()
        {
            maxAntennaOption = CreateOption(Color.white, "maxAntennas", 3f, 1f, 15f, 1f);
            placeCoolDownOption = CreateOption(Color.white, "placeCoolDown", 15f, 5f, 60f, 2.5f);
            placeCoolDownOption.suffix = "second";
            antennaEffectiveRangeOption = CreateOption(Color.white, "antennaEffectiveRange", 5f, 1.25f, 20f, 1.25f);
            antennaEffectiveRangeOption.suffix = "cross";
        }

        public override void Initialize(PlayerControl __instance)
        {
            indicatorsPool = null;
            myAntennaSet.Clear();
            guardPlayer = null;
        }

        public override void FinalizeInGame(PlayerControl __instance)
        {
            if (indicatorsPool != null) indicatorsPool.Destroy();
            if (guardPlayer != null) RPCEventInvoker.RemoveGuardian(guardPlayer,PlayerControl.LocalPlayer);
        }

        public override void MyMapUpdate(MapBehaviour mapBehaviour)
        {
            if (!mapBehaviour.GetTrackOverlay()) return;
            if (!mapBehaviour.GetTrackOverlay().activeSelf) return;

            if (indicatorsPool == null)
            {
                indicatorsPool = new Utilities.ObjectPool<SpriteRenderer>(mapBehaviour.HerePoint, mapBehaviour.GetTrackOverlay().transform);
                indicatorsPool.SetInitializer((renderer) =>
                {
                    PlayerMaterial.SetColors(Palette.DisabledGrey, renderer);
                });
            }

            indicatorsPool.Reclaim();

            if (MeetingHud.Instance) return;

            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == PlayerControl.LocalPlayer) continue;
                if (p.Data.IsDead || !p.Visible || p.GetModData().isInvisiblePlayer) continue;

                bool showFlag = false;
                foreach (var a in myAntennaSet)
                {
                    if (a.PassedMeetings == 0) continue;

                    var vec = p.transform.position - a.GameObject.transform.position;
                    float mag = vec.magnitude;
                    if (mag > antennaEffectiveRangeOption.getFloat()) continue;
                    if (PhysicsHelpers.AnyNonTriggersBetween(a.GameObject.transform.position, vec.normalized, mag, Constants.ShipAndAllObjectsMask)) continue;

                    showFlag = true;
                    break;
                }
                if (showFlag) indicatorsPool.Get().transform.localPosition = MapBehaviourExpansion.ConvertMapLocalPosition(p.transform.position, p.PlayerId);
            }

            //死体も表示
            foreach (var p in Helpers.AllDeadBodies())
            {
                bool showFlag = false;
                foreach (var a in myAntennaSet)
                {
                    if (a.PassedMeetings == 0) continue;

                    var vec = p.transform.position - a.GameObject.transform.position;
                    float mag = vec.magnitude;
                    if (mag > antennaEffectiveRangeOption.getFloat()) continue;
                    if (PhysicsHelpers.AnyNonTriggersBetween(a.GameObject.transform.position, vec.normalized, mag, Constants.ShipAndAllObjectsMask)) continue;

                    showFlag = true;
                    break;
                }
                indicatorsPool.Get().transform.localPosition = MapBehaviourExpansion.ConvertMapLocalPosition(p.transform.position, p.ParentId);
            }
        }

        public override void GlobalInitialize(PlayerControl __instance)
        {
            __instance.GetModData().SetRoleData(remainAntennasId, (int)maxAntennaOption.getFloat());
        }

        private static Sprite placeButtonSprite;
        public static Sprite getPlaceButtonSprite()
        {
            if (placeButtonSprite) return placeButtonSprite;
            placeButtonSprite = Helpers.loadSpriteFromResources("Nebula.Resources.AntennaButton.png", 115f);
            return placeButtonSprite;
        }

        private static Sprite guardButtonSprite;
        public static Sprite getGuardButtonSprite()
        {
            if (guardButtonSprite) return guardButtonSprite;
            guardButtonSprite = Helpers.loadSpriteFromResources("Nebula.Resources.GuardButton.png", 115f);
            return guardButtonSprite;
        }

        public override void MyPlayerControlUpdate()
        {
            if (guardPlayer==null)
            {
                Game.MyPlayerData data = Game.GameData.data.myData;
                data.currentTarget = Patches.PlayerControlPatch.SetMyTarget();
                Patches.PlayerControlPatch.SetPlayerOutline(data.currentTarget, Color.yellow);
            }
        }

        public override void OnAnyoneGuarded(byte murderId, byte targetId)
        {
            if (guardPlayer == null) return;
            if (targetId == guardPlayer.PlayerId)
            {
                Helpers.PlayQuickFlash(RoleColor);
            }
        }

        public override void ButtonInitialize(HudManager __instance)
        {
            if (antennaButton != null)
            {
                antennaButton.Destroy();
            }
            antennaButton = new CustomButton(
                () =>
                {
                    var obj=RPCEventInvoker.ObjectInstantiate(CustomObject.Type.Antenna, PlayerControl.LocalPlayer.transform.position);
                    RPCEventInvoker.AddAndUpdateRoleData(PlayerControl.LocalPlayer.PlayerId, remainAntennasId, -1);
                    myAntennaSet.Add(obj);
                    antennaButton.Timer = antennaButton.MaxTimer;
                },
                () => {
                    return !PlayerControl.LocalPlayer.Data.IsDead && Game.GameData.data.myData.getGlobalData().GetRoleData(remainAntennasId) > 0;
                },
                () => {
                    int total = (int)maxAntennaOption.getFloat();
                    int remain = Game.GameData.data.myData.getGlobalData().GetRoleData(remainAntennasId);
                    antennaButton.UpperText.text = $"{remain}/{total}";

                    return remain > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { antennaButton.Timer = antennaButton.MaxTimer; },
                getPlaceButtonSprite(),
                new Vector3(-1.8f, 0, 0),
                __instance,
                KeyCode.F,
                false, 
                "button.label.place"
            );
            antennaButton.MaxTimer = placeCoolDownOption.getFloat();

            if (guardButton != null)
            {
                guardButton.Destroy();
            }
            guardButton = new CustomButton(
                () =>
                {
                    var target=Game.GameData.data.myData.currentTarget;
                    RPCEventInvoker.AddGuardian(target,PlayerControl.LocalPlayer);
                    guardButton.UpperText.text = target.name;
                    guardPlayer = target;
                },
                () => {
                    return !PlayerControl.LocalPlayer.Data.IsDead;
                },
                () => {
                    return guardPlayer==null && Game.GameData.data.myData.currentTarget!=null && PlayerControl.LocalPlayer.CanMove;
                },
                () => { guardButton.Timer = guardButton.MaxTimer; },
                getGuardButtonSprite(),
                new Vector3(0, 1f, 0),
                __instance,
                KeyCode.G,
                false,
                "button.label.guard"
            );
            guardButton.MaxTimer = 10f;
        }

        public override void CleanUp()
        {
            indicatorsPool = null;
            myAntennaSet.Clear();

            if (antennaButton != null)
            {
                antennaButton.Destroy();
                antennaButton = null;
            }

            if (guardButton != null)
            {
                guardButton.Destroy();
                guardButton = null;
            }
        }

        public Guardian()
            : base("Guardian", "guardian", RoleColor, RoleCategory.Crewmate, Side.Crewmate, Side.Crewmate,
                 Crewmate.crewmateSideSet, Crewmate.crewmateSideSet, Crewmate.crewmateEndSet,
                 false, VentPermission.CanNotUse, false, false, false)
        {

            remainAntennasId = Game.GameData.RegisterRoleDataId("guardian.remainAntennas");
        }
    }
}