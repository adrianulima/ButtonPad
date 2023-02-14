using System;
using System.Collections.Generic;
using ProtoBuf;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Lima
{
  [ProtoContract(UseProtoMembersOnly = true)]
  public class BlockStorageContent : NetworkMessage
  {
    [ProtoMember(1)]
    public List<AppContent> Apps = new List<AppContent>();

    [ProtoMember(2)]
    public long BlockId;

    public BlockStorageContent() { }

    public AppContent? GetAppContent(string surfaceName)
    {
      foreach (var app in Apps)
      {
        if (app.SurfaceName == surfaceName)
          return app;
      }
      return null;
    }

    public void AddOrUpdateAppContent(AppContent appContent)
    {
      int index = Apps.FindIndex(app => app.SurfaceName == appContent.SurfaceName);
      if (index != -1)
        Apps[index] = appContent;
      else
        Apps.Add(appContent);
    }
  }

  [ProtoContract(UseProtoMembersOnly = true)]
  public struct AppContent
  {
    [ProtoMember(1)]
    public string SurfaceName;

    [ProtoMember(2)]
    public List<MyTuple<int, string, long, string>> Buttons;

    [ProtoMember(3)]
    public float CustomScale;
  }

  public class BlockStorageHandler
  {
    protected readonly Guid StorageGuid = new Guid("AD71A300-1594-456C-8622-CECD5B4F97F2");

    public AppContent? LoadAppContent(IMyCubeBlock block, string surfaceName)
    {
      if (block.Storage == null)
        return null;

      string rawData;
      if (!block.Storage.TryGetValue(StorageGuid, out rawData))
        return null;

      try
      {
        var blockContent = MyAPIGateway.Utilities.SerializeFromBinary<BlockStorageContent>(Convert.FromBase64String(rawData));
        if (blockContent != null)
          return blockContent.GetAppContent(surfaceName);
      }
      catch (Exception e)
      {
        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");
      }

      return null;
    }

    public BlockStorageContent SaveAppContent(IMyCubeBlock block, AppContent appContent)
    {
      BlockStorageContent blockContent = null;
      string rawData;
      if (block.Storage == null)
      {
        block.Storage = new MyModStorageComponent();
        blockContent = new BlockStorageContent();
        blockContent.NetworkId = MyAPIGateway.Session.Player.SteamUserId;
        blockContent.BlockId = block.EntityId;
      }
      else if (block.Storage.TryGetValue(StorageGuid, out rawData))
      {
        blockContent = MyAPIGateway.Utilities.SerializeFromBinary<BlockStorageContent>(Convert.FromBase64String(rawData));
      }
      else
      {
        blockContent = new BlockStorageContent();
        blockContent.NetworkId = MyAPIGateway.Session.Player.SteamUserId;
        blockContent.BlockId = block.EntityId;
      }

      if (blockContent != null)
      {
        blockContent.AddOrUpdateAppContent(appContent);
        block.Storage.SetValue(StorageGuid, Convert.ToBase64String(MyAPIGateway.Utilities.SerializeToBinary(blockContent)));
      }

      return blockContent;
    }

    public void SaveBlockContent(IMyCubeBlock block, BlockStorageContent blockContent)
    {
      if (block.Storage == null)
        block.Storage = new MyModStorageComponent();

      block.Storage.SetValue(StorageGuid, Convert.ToBase64String(MyAPIGateway.Utilities.SerializeToBinary(blockContent)));
    }
  }
}