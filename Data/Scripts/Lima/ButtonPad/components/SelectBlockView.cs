using Lima.API;
using Lima.ButtonPad;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRageMath;

namespace Lima
{
  public class SelectBlockView : TouchScrollView
  {
    private ButtonPadApp _padApp;
    private List<TouchButton> _buttons = new List<TouchButton>();

    private List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
    private List<TouchView> _views = new List<TouchView>();
    private List<IMyBlockGroup> _blockGroups = new List<IMyBlockGroup>();

    public SelectBlockView(ButtonPadApp pad)
    {
      _padApp = pad;

      ScrollBar.Pixels = new Vector2(24, 0);
      Gap = 2;
      Padding = new Vector4(2, 1, 2, 1);
    }

    public void Dispose()
    {
      _buttons.Clear();
      _blocks.Clear();
      _blockGroups.Clear();
      _views.Clear();
      _padApp = null;
      _buttons = null;
      _blocks = null;
      _blockGroups = null;
      _views = null;
    }

    public void UpdateItemsForButton(ActionButton actionBt, IMyCubeGrid cubeGrid)
    {
      HandleGrid(cubeGrid);

      RemoveAllChildren();

      _buttons.Clear();

      _blockGroups.Sort((pair1, pair2) => pair1.Name.CompareTo(pair2.Name));
      _blocks.Sort((pair1, pair2) => pair1.DisplayNameText.CompareTo(pair2.DisplayNameText));

      var gray = new Color(128, 128, 128);
      var darker7 = _padApp.Theme.GetMainColorDarker(7);
      var golden = new Color(Color.DarkGoldenrod * 0.2f, 1);
      var border = new Vector4(2, 0, 0, 0);

      var odd = 0;
      TouchView lastView = null;
      foreach (var blgr in _blockGroups)
      {
        var bt = new TouchButton($"*{blgr.Name}*", () => SelectBlockGroup(blgr, actionBt));
        bt.BorderColor = darker7;
        bt.Border = border;
        lastView = AddButton(bt, Utils.GetBlockGroupTexture(blgr), odd, lastView, gray);
        odd++;
      }

      foreach (var bl in _blocks)
      {
        var same = cubeGrid.EntityId == bl.CubeGrid.EntityId;
        var bt = new TouchButton(bl.DisplayNameText.ToString(), () => SelectBlock(bl, actionBt));
        bt.BorderColor = same ? darker7 : golden;
        bt.Border = border;
        lastView = AddButton(bt, Utils.GetBlockTexture(bl), odd, lastView, gray);
        odd++;
      }
    }

    private TouchView AddButton(TouchButton button, string iconString, int odd, TouchView lastView, Color color)
    {
      var cols = 2;
      var small = _padApp.Theme.Scale < 1;
      var height = _padApp.Screen.Surface.SurfaceSize.Y;
      var smallHeight = height < 128;
      var smallWidth = _padApp.Screen.Surface.SurfaceSize.X < 128;

      button.Gap = 4;
      button.Padding = new Vector4(4);
      button.Pixels = Vector2.Zero;
      button.Scale = new Vector2(small ? 1 : 0.5f, 1);
      button.Label.AutoEllipsis = LabelEllipsis.Left;
      button.Label.AutoBreakLine = true;
      if (smallWidth)
      {
        button.Label.Alignment = TextAlignment.CENTER;
        button.Direction = ViewDirection.Column;
        button.Label.FontSize = 0.45f;
        ScrollBar.Pixels = new Vector2(8, 0);
      }
      else
      {
        button.Label.Alignment = TextAlignment.LEFT;
        button.Direction = ViewDirection.Row;
        button.Label.FontSize = 0.7f;
        ScrollBar.Pixels = new Vector2(24, 0);
      }
      _buttons.Add(button);

      var iconSize = smallHeight ? height / 2 - 9 : 31;
      if (smallWidth && button.Label.Text.Length > 22)
        iconSize -= 16 * _padApp.Theme.Scale;
      var icon = new Icon(iconString, new Vector2(iconSize), 0, color);
      icon.Absolute = false;
      icon.Pixels = new Vector2(iconSize);
      icon.Scale = Vector2.Zero;
      button.AddChild(icon, 0);

      var w = smallWidth ? 72 : 40;
      var h = smallHeight ? ((height / 2) - 2) / _padApp.Theme.Scale : w;
      if (!small && odd % cols != 0 && lastView != null)
        lastView.AddChild(button);
      else
      {
        var view = new TouchView(ViewDirection.Row);
        view.Gap = 2;
        view.Scale = new Vector2(1, 0);
        view.Pixels = new Vector2(0, h);
        view.AddChild(button);
        AddChild(view);
        _views.Add(view);
        lastView = view;
      }

      ScrollWheelStep = (lastView.Pixels.Y + Gap) * _padApp.Theme.Scale;

      return lastView;
    }

    public void RemoveAllChildren()
    {
      Scroll = 0;
      // TODO: Pool
      foreach (var v in _views)
        RemoveChild(v);
      _buttons.Clear();
    }

    private void SelectBlockGroup(IMyBlockGroup blockGroup, ActionButton actionBt)
    {
      _padApp.ShowSelectActionView(actionBt, blockGroup);
    }

    private void SelectBlock(IMyTerminalBlock block, ActionButton actionBt)
    {
      _padApp.ShowSelectActionView(actionBt, block);
    }

    private void HandleGrid(IMyCubeGrid cubeGrid)
    {
      _blocks.Clear();
      _blockGroups.Clear();

      var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
      terminalSystem.GetBlockGroups(_blockGroups);
      terminalSystem.GetBlocks(_blocks);
    }

  }
}
