using Lima.API;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace Lima
{
  [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
  public class TouchButtonPadSession : MySessionComponentBase
  {
    public TouchUiKit Api { get; private set; }
    public static TouchButtonPadSession Instance;

    public BlockStorageHandler BlockHandler;
    public NetworkHandler<BlockStorageContent> NetBlockHandler;

    public LCDTextureHandler TextureHandler;

    public override void BeforeStart()
    {
      if (MyAPIGateway.Utilities.IsDedicated)
        return;

      TextureHandler = new LCDTextureHandler();
    }

    public override void LoadData()
    {
      // For server and clients
      BlockHandler = new BlockStorageHandler();
      NetBlockHandler = new NetworkHandler<BlockStorageContent>(041444);
      NetBlockHandler.Init();

      Instance = this;

      if (MyAPIGateway.Utilities.IsDedicated)
      {
        // Only for server
        NetBlockHandler.MessageReceivedEvent += NetwrokBlockReceivedServer;
        return;
      }

      // Only for clients
      Api = new TouchUiKit();
      Api.Load();
    }

    private void NetwrokBlockReceivedServer(BlockStorageContent blockContent)
    {
      var block = MyAPIGateway.Entities.GetEntityById(blockContent.BlockId) as IMyCubeBlock;
      if (block != null)
        BlockHandler.SaveBlockContent(block, blockContent);
    }

    protected override void UnloadData()
    {
      if (NetBlockHandler != null)
        NetBlockHandler.MessageReceivedEvent -= NetwrokBlockReceivedServer;

      NetBlockHandler?.Dispose();
      TextureHandler?.Dispose();
      Api?.Unload();
      TextureHandler = null;
      Instance = null;
    }
  }
}