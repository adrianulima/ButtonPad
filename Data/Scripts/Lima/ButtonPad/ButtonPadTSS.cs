using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using System.Collections.Generic;
using VRage;

namespace Lima
{
  [MyTextSurfaceScript("Touch_ButtonPad", "Button Pad")]
  public class ButtonPadTSS : MyTSSCommon
  {
    public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update10;

    IMyCubeBlock _block;
    IMyTerminalBlock _terminalBlock;
    IMyTextSurface _surface;

    ButtonPadApp _app;

    bool _init = false;
    int ticks = 0;

    public ButtonPadTSS(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
      _block = block;
      _surface = surface;
      _terminalBlock = (IMyTerminalBlock)block;

      surface.ScriptBackgroundColor = Color.Black;
      Surface.ScriptForegroundColor = new Color(20, 30, 40);//Color.SteelBlue;
    }

    public void Init()
    {
      if (!ButtonPadSession.Instance.Api.IsReady)
        return;

      if (_init)
        return;
      _init = true;

      _app = new ButtonPadApp(SaveConfigAction);
      _app.InitApp(this.Block, this.Surface);
      // var max = Math.Max(this.Surface.SurfaceSize.X, this.Surface.SurfaceSize.Y);
      if (this.Surface.SurfaceSize.X <= 256)
        _app.Theme.Scale = _app.Cursor.Scale = 0.75f;
      _app.CreateElements();
      // _app.Theme.Scale = Math.Min(Math.Max(Math.Min(this.Surface.SurfaceSize.X, this.Surface.SurfaceSize.Y) / 512, 0.4f), 2);
      // _app.Cursor.Scale = _app.Theme.Scale;

      var appContent = ButtonPadSession.Instance.BlockHandler.LoadAppContent(_block, _surface.Name);
      if (appContent != null)
        _app.ApplySettings(appContent.GetValueOrDefault());

      ButtonPadSession.Instance.NetBlockHandler.MessageReceivedEvent += OnBlockContentReceived;
      _terminalBlock.OnMarkForClose += BlockMarkedForClose;
    }

    private bool IsOwnerOrFactionShare()
    {
      var player = MyAPIGateway.Session.Player;
      var relation = (_block.OwnerId > 0 ? player.GetRelationTo(_block.OwnerId) : MyRelationsBetweenPlayerAndBlock.NoOwnership);

      return relation == MyRelationsBetweenPlayerAndBlock.Owner || relation == MyRelationsBetweenPlayerAndBlock.FactionShare;
    }

    private void SaveConfigAction()
    {
      var buttons = new List<MyTuple<int, string, long, string>>();
      foreach (var actBt in _app.ActionButtons)
      {
        var tup = actBt.GetTuple();
        var blGrpName = tup.Item2;
        var blockId = tup.Item3;
        var actionName = tup.Item4;
        if ((blGrpName == "" && blockId == 0) || actionName == "")
          continue;
        buttons.Add(tup);
      }

      var appContent = new AppContent()
      {
        SurfaceName = _surface.Name,
        Buttons = buttons
      };

      var blockContent = ButtonPadSession.Instance.BlockHandler.SaveAppContent(_block, appContent);
      if (MyAPIGateway.Multiplayer.MultiplayerActive && blockContent != null)
      {
        blockContent.NetworkId = MyAPIGateway.Session.Player.SteamUserId;
        ButtonPadSession.Instance.NetBlockHandler.Broadcast(blockContent);
      }
    }

    private void OnBlockContentReceived(BlockStorageContent blockContent)
    {
      if (blockContent.BlockId != _block.EntityId)
        return;

      var appContent = blockContent.GetAppContent(_surface.Name);
      if (appContent != null)
        _app.ApplySettings(appContent.GetValueOrDefault());
    }

    public override void Dispose()
    {
      base.Dispose();

      _app?.Dispose();
      _terminalBlock.OnMarkForClose -= BlockMarkedForClose;
      ButtonPadSession.Instance.NetBlockHandler.MessageReceivedEvent -= OnBlockContentReceived;
    }

    void BlockMarkedForClose(IMyEntity ent)
    {
      Dispose();
    }

    public override void Run()
    {
      try
      {
        if (!_init && ticks++ < (6 * 2)) // 2 secs
          return;

        Init();

        if (_app == null)
          return;

        base.Run();

        using (var frame = m_surface.DrawFrame())
        {
          _app.ForceUpdate();
          frame.AddRange(_app.GetSprites());
          frame.Dispose();
        }
      }
      catch (Exception e)
      {
        _app = null;
        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

        if (MyAPIGateway.Session?.Player != null)
          MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} ]", 5000, MyFontEnum.Red);
      }
    }
  }
}