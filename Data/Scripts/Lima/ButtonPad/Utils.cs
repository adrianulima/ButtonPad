using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.Game;
using Sandbox.Game.Entities;

namespace Lima.ButtonPad
{
  internal static class Utils
  {
    internal static void GetBlockGroupActions(IMyBlockGroup blockGroup, List<ITerminalAction> actions)
    {
      List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
      blockGroup.GetBlocks(blocks);

      List<ITerminalAction> actionsList = new List<ITerminalAction>();
      blocks[0].GetActions(actionsList, (a) => a.IsEnabled(blocks[0]));

      List<ITerminalAction> list2 = Enumerable.ToList<ITerminalAction>(actionsList);

      foreach (IMyTerminalBlock myTerminalBlock2 in Enumerable.Skip<IMyTerminalBlock>(blocks, 1))
      {
        List<ITerminalAction> list3 = new List<ITerminalAction>();

        List<ITerminalAction> actionsList2 = new List<ITerminalAction>();
        myTerminalBlock2.GetActions(actionsList2, (a) => a.IsEnabled(myTerminalBlock2));
        List<ITerminalAction> list4 = Enumerable.ToList<ITerminalAction>(actionsList2);
        foreach (ITerminalAction terminalAction in list2)
        {
          using (List<ITerminalAction>.Enumerator enumerator4 = list4.GetEnumerator())
          {
            while (enumerator4.MoveNext())
            {
              if (enumerator4.Current.Id == terminalAction.Id)
              {
                list3.Add(terminalAction);
                break;
              }
            }
          }
        }
        list2 = list3;
      }
      actions.AddRange(list2);
    }

    public static bool IsOwnerOrFactionShare(IMyCubeBlock block, IMyPlayer player)
    {
      var relation = (block.OwnerId > 0 ? player.GetRelationTo(block.OwnerId) : MyRelationsBetweenPlayerAndBlock.NoOwnership);
      if (relation == MyRelationsBetweenPlayerAndBlock.Owner || relation == MyRelationsBetweenPlayerAndBlock.NoOwnership)
        return true;

      var shareMode = Utils.GetBlockShareMode(block);
      if (shareMode == MyOwnershipShareModeEnum.All || (relation == MyRelationsBetweenPlayerAndBlock.FactionShare && shareMode == MyOwnershipShareModeEnum.Faction))
        return true;

      return false;
    }

    public static MyOwnershipShareModeEnum GetBlockShareMode(IMyCubeBlock block)
    {
      var internalBlock = block as MyCubeBlock;
      if (internalBlock != null && internalBlock.IDModule != null)
        return internalBlock.IDModule.ShareMode;
      return MyOwnershipShareModeEnum.None;
    }
  }
}