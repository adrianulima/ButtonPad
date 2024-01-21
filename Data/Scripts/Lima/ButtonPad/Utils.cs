using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.Game;
using Sandbox.Game.Entities;
using VRageMath;

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

    public static Vector3I GetPositionRelativeTo(IMyCubeBlock referenceBlock, IMyCubeBlock targetBlock)
    {
      Matrix mtr;
      referenceBlock.Orientation.GetMatrix(out mtr);
      mtr.TransposeRotationInPlace();

      Vector3I lcdPos = referenceBlock.Position;
      Vector3I blockPos = targetBlock.Position;

      var position = new Vector3I(lcdPos.X - blockPos.X, lcdPos.Y - blockPos.Y, lcdPos.Z - blockPos.Z);
      Vector3I.Transform(ref position, ref mtr, out position);

      return position;
    }

    public static IMyCubeBlock GetBlockFromRelativePositionTo(IMyCubeBlock referenceBlock, Vector3I? position)
    {
      Matrix mtr;
      referenceBlock.Orientation.GetMatrix(out mtr);
      mtr.TransposeRotationInPlace();
      mtr = Matrix.Invert(mtr);

      Vector3I lcdPos = referenceBlock.Position;
      Vector3I pos = position ?? Vector3I.Zero;
      Vector3I.Transform(ref pos, ref mtr, out pos);
      Vector3I blockPos = new Vector3I(lcdPos.X - pos.X, lcdPos.Y - pos.Y, lcdPos.Z - pos.Z);

      var slimBlock = referenceBlock.CubeGrid.GetCubeBlock(blockPos);

      return slimBlock?.FatBlock;
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