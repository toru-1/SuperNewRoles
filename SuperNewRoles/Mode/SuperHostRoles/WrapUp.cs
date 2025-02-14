﻿using Hazel;
using InnerNet;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class WrapUpClass
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            /*
            new LateTask(() =>
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    byte reactorId = 3;
                    if (PlayerControl.GameOptions.MapId == 2) reactorId = 21;
                    MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(p.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, p.getClientId());
                    MessageExtensions.WriteNetObject(MurderWriter, BotHandler.Bot);
                    AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
                    MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                    SabotageWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageWriter, p);
                    SabotageWriter.Write((byte)128);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                    SabotageFixWriter.Write((byte)16);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                    if (PlayerControl.GameOptions.MapId == 4)
                    {
                        MessageWriter SabotageFixWriter2 = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                        SabotageFixWriter2.Write(reactorId);
                        MessageExtensions.WriteNetObject(SabotageFixWriter2, p);
                        SabotageFixWriter2.Write((byte)17);
                        AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter2);
                    }
                }
            }, 5f, "AntiBlack");*/
            SuperNewRolesPlugin.Logger.LogInfo("WrapUp");
            foreach (PlayerControl p in RoleClass.RemoteSheriff.RemoteSheriffPlayer)
            {
                SuperNewRolesPlugin.Logger.LogInfo("ALL");
                if (p.isAlive() && !p.IsMod())
                {
                    p.RpcProtectPlayer(p, 0);
                    SuperNewRolesPlugin.Logger.LogInfo("プロテクト");
                    new LateTask(() =>
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("マーダー");
                        p.RpcMurderPlayer(p);
                    }, 0.5f);
                }
            }
            AmongUsClient.Instance.StartCoroutine(nameof(ResetName));
            IEnumerator ResetName()
            {
                yield return new WaitForSeconds(1);
                FixedUpdate.SetRoleNames();
            }
            Roles.BestFalseCharge.WrapUp();
            if (exiled == null) return;
            exiled.Object.Exiled();
            if (exiled.Object.isRole(RoleId.Sheriff) || exiled.Object.isRole(RoleId.truelover) || exiled.Object.isRole(RoleId.MadMaker))
            {
                exiled.Object.RpcSetRoleDesync(RoleTypes.GuardianAngel);
            }
            if (RoleClass.Lovers.SameDie && exiled.Object.IsLovers())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    PlayerControl SideLoverPlayer = exiled.Object.GetOneSideLovers();
                    if (SideLoverPlayer.isAlive())
                    {
                        SideLoverPlayer.RpcMurderPlayer(SideLoverPlayer);
                    }
                }
            }
            if (exiled.Object.IsQuarreled())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    var Side = RoleHelpers.GetOneSideQuarreled(exiled.Object);
                    if (Side.isDead())
                    {
                        RPCProcedure.ShareWinner(exiled.Object.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                        Writer.Write(exiled.Object.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                        Writer.Write((byte)CustomGameOverReason.QuarreledWin);
                        Writer.EndRPC();
                        CustomRPC.RPCProcedure.SetWinCond((byte)CustomGameOverReason.QuarreledWin);
                        var winplayers = new List<PlayerControl>();
                        winplayers.Add(exiled.Object);
                        //EndGameCheck.WinNeutral(winplayers);
                        Chat.WinCond = CustomGameOverReason.QuarreledWin;
                        Chat.Winner = new List<PlayerControl>();
                        Chat.Winner.Add(exiled.Object);
                        RoleClass.Quarreled.IsQuarreledWin = true;
                        SuperHostRoles.EndGameCheck.CustomEndGame(ShipStatus.Instance, GameOverReason.HumansByTask, false);
                    }
                }
            }
            Roles.Jester.WrapUp(exiled);
            Roles.Nekomata.WrapUp(exiled);
        }
    }
}
