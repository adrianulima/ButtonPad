using Lima.API;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRageMath;

namespace Lima
{
  public class SelectBlockView : View
  {
    private ButtonPadApp _padApp;
    private List<Button> _buttons = new List<Button>();

    private Button _button;
    private TextField _textField;
    private View _searchView;
    private ScrollView _scrollView;

    private List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
    private List<View> _views = new List<View>();
    private List<IMyBlockGroup> _blockGroups = new List<IMyBlockGroup>();

    private float _step = 0;

    private ActionButton _lastActionBt;
    private IMyCubeGrid _lastCubeGrid;

    public SelectBlockView(ButtonPadApp pad)
    {
      _padApp = pad;

      Gap = 2;
      Padding = new Vector4(2);

      _searchView = new View(ViewDirection.Row);
      _searchView.Gap = 2;
      _searchView.Flex = new Vector2(1, 0);
      _searchView.Pixels = new Vector2(0, 24);
      _searchView.Alignment = ViewAlignment.Center;

      _textField = new TextField();
      _textField.Label.Alignment = TextAlignment.LEFT;
      _textField.Pixels = Vector2.Zero;
      _textField.Flex = Vector2.One;
      _textField.OnSubmit = (_text) => UpdateFilter();
      _searchView.AddChild(_textField);

      _button = new Button("Search", () => UpdateFilter());
      _button.Pixels = new Vector2(60, 0);
      _button.Flex = Vector2.UnitY;
      _searchView.AddChild(_button);

      _scrollView = new ScrollView();
      _scrollView.ScrollBar.Pixels = new Vector2(24, 0);
      _scrollView.Padding = new Vector4(0, 0, 1, 0);

      AddChild(_searchView);
      AddChild(_scrollView);
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
      _button = null;
      _textField = null;
      _searchView = null;
      _scrollView = null;
      _lastActionBt = null;
      _lastCubeGrid = null;
    }

    private void UpdateFilter()
    {
      UpdateItemsForButton(_lastActionBt, _lastCubeGrid, true);
    }

    public void UpdateItemsForButton(ActionButton actionBt, IMyCubeGrid cubeGrid, bool filtering = false)
    {
      _lastActionBt = actionBt;
      _lastCubeGrid = cubeGrid;

      var filter = "";
      if (!filtering)
        _textField.Text = "";
      else
        filter = _textField.Text.ToLower().Trim();

      if (filter == "")
        filtering = false;

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
      View lastView = null;
      foreach (var blgr in _blockGroups)
      {
        if (filtering && !blgr.Name.ToLower().Contains(filter))
          continue;
        var bt = new Button($"*{blgr.Name}*", () => SelectBlockGroup(blgr, actionBt));
        bt.BorderColor = darker7;
        bt.Border = border;
        lastView = AddButton(bt, TouchButtonPadSession.Instance.Api.GetBlockGroupIconSprite(blgr), odd, lastView, gray);
        odd++;
      }

      foreach (var bl in _blocks)
      {
        var name = bl.DisplayNameText.ToString();
        if (filtering && !name.ToLower().Contains(filter))
          continue;
        var same = cubeGrid.EntityId == bl.CubeGrid.EntityId;
        var bt = new Button(name, () => SelectBlock(bl, actionBt));
        bt.BorderColor = same ? darker7 : golden;
        bt.Border = border;
        lastView = AddButton(bt, TouchButtonPadSession.Instance.Api.GetBlockIconSprite(bl), odd, lastView, gray);
        odd++;
      }
    }

    private View AddButton(Button button, string iconString, int odd, View lastView, Color color)
    {
      var smallHeight = _padApp.Screen.Surface.SurfaceSize.Y < 128;
      var smallWidth = _padApp.Screen.Surface.SurfaceSize.X < 128;

      if (smallWidth)
      {
        _searchView.Enabled = false;
      }
      else
      {
        _searchView.Enabled = true;
        _searchView.Pixels = new Vector2(0, smallHeight ? 14 : 22);
        _button.Label.FontSize = _textField.Label.FontSize = smallHeight ? 0.4f : 0.6f;
      }


      var cols = 2;
      var small = _padApp.Theme.Scale < 1;
      var height = _padApp.Screen.Surface.SurfaceSize.Y - (Padding.Y + Padding.W) - (_searchView.Enabled ? _searchView.Pixels.Y : 0);

      button.Gap = 4;
      button.Padding = new Vector4(4);
      button.Pixels = Vector2.Zero;
      button.Flex = new Vector2(small || smallWidth ? 1 : 0.5f, 1);
      button.Label.AutoEllipsis = LabelEllipsis.Left;
      button.Label.AutoBreakLine = true;
      if (smallWidth)
      {
        button.Label.Alignment = TextAlignment.CENTER;
        button.Direction = ViewDirection.Column;
        button.Label.FontSize = 0.45f;
        _scrollView.ScrollBar.Pixels = new Vector2(8, 0);
      }
      else
      {
        button.Label.Alignment = TextAlignment.LEFT;
        button.Direction = ViewDirection.Row;
        button.Label.FontSize = 0.6f;
        _scrollView.ScrollBar.Pixels = new Vector2(24, 0);
      }
      if (button.Label.Text.Length > 20)
        button.Label.FontSize = 0.45f;

      _buttons.Add(button);

      var iconSize = smallHeight ? height / 2 - 9 : 31;
      if (smallWidth && button.Label.Text.Length > 22)
        iconSize -= 16 * _padApp.Theme.Scale;
      var icon = new Icon(iconString, new Vector2(iconSize), 0, color);
      icon.Absolute = false;
      icon.Pixels = new Vector2(iconSize);
      icon.Flex = Vector2.Zero;
      button.AddChild(icon, 0);

      var w = smallWidth ? 72 : 40;
      var h = smallHeight ? ((height / 2) - 2) / _padApp.Theme.Scale : w;
      if (!small && !smallWidth && odd % cols != 0 && lastView != null)
        lastView.AddChild(button);
      else
      {
        var view = new View(ViewDirection.Row);
        view.Gap = 2;
        view.Flex = new Vector2(1, 0);
        view.Pixels = new Vector2(0, h);
        view.AddChild(button);
        _scrollView.AddChild(view);
        _views.Add(view);
        lastView = view;
      }

      _step = lastView.Pixels.Y + _scrollView.Gap;
      UpdateScrollStep();

      return lastView;
    }

    public void UpdateScrollStep()
    {
      _scrollView.ScrollWheelStep = _step * _padApp.Theme.Scale;
    }

    public void RemoveAllChildren()
    {
      _scrollView.Scroll = 0;
      // TODO: Pool
      foreach (var v in _views)
        _scrollView.RemoveChild(v);
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
