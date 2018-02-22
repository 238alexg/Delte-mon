using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUI : UIScreen
{

    public override void Close()
    {
        base.Close();
        PlayerMovement.Inst.ResumeMoving();
    }

    public void DriveToLocation(TownRecoveryLocation townRecov)
    {
        Close();
        UIManager.Inst.SwitchLocationAndScene(townRecov.RecovX, townRecov.RecovY, townRecov.townName);
    }
}