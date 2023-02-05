using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using System;
using VRage.Game.ModAPI;

namespace Lima.ButtonPad
{
  internal static class Utils
  {
    internal static string GetBlockTexture(IMyCubeBlock block)
    {
      var subType = block.BlockDefinition.SubtypeName;
      if (subType == "")
        subType = block.BlockDefinition.TypeIdString;
      return $"Lima/Blocks/{subType}";
    }

    internal static string GetBlockGroupTexture(IMyBlockGroup blockGroup)
    {
      List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
      blockGroup.GetBlocks(blocks);

      HashSet<Type> hashSet = new HashSet<Type>();
      foreach (var myTerminalBlock in blocks)
        hashSet.Add(myTerminalBlock.GetType());

      if (hashSet.Count == 1)
        return GetBlockTexture(blocks[0]);
      else
        return "Lima/Blocks/Group";
    }

    internal static void GetBlockGroupActions(IMyBlockGroup blockGroup, List<ITerminalAction> actions)
    {
      List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
      blockGroup.GetBlocks(blocks);

      List<ITerminalAction> actionsList = new List<ITerminalAction>();
      blocks[0].GetActions(actionsList, (a) => a.IsEnabled(blocks[0]));

      //(a) => a.IsEnabled(block)
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
  }
}