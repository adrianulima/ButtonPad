using Lima.API;
using Lima.ButtonPad;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage;
using VRageMath;
using TerminalActionParameter = Sandbox.ModAPI.Ingame.TerminalActionParameter;

namespace Lima
{
  public class ActionButton
  {
    public TouchEmptyButton Button;
    private TouchLabel _statusLabel;
    private Icon _icon;

    private ButtonPadApp _padApp;
    private ITerminalAction _terminalAction;
    private IMyCubeBlock _block;
    private IMyBlockGroup _blockGroup;

    private string _iconString = "";
    private string _statusText = "";
    private StringBuilder _name = new StringBuilder("");
    private Color _gray = new Color(128, 128, 128);

    private List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
    public int Index = -1;

    private ListReader<TerminalActionParameter> _paramList;
    private string _param = "";
    public string Param
    {
      get { return _param; }
      set
      {
        _param = value;
        var list = new List<TerminalActionParameter>() { TerminalActionParameter.Get(_param) };
        _paramList = new ListReader<TerminalActionParameter>(list);
      }
    }

    public ActionButton(ButtonPadApp pad, int index)
    {
      _padApp = pad;
      Index = index;

      Button = new TouchEmptyButton(OnClickButton);
      Button.UseThemeColors = false;
      Button.Alignment = ViewAlignment.Center;
      Button.Anchor = ViewAnchor.SpaceAround;
      Button.Pixels = Vector2.Zero;
      Button.Scale = Vector2.One;

      _icon = new Icon("", new Vector2(40), 0, _gray);
      _icon.Absolute = false;
      _icon.Pixels = Vector2.Zero;
      _icon.Scale = Vector2.One;
      Button.AddChild(_icon);

      _statusLabel = new TouchLabel("");
      _statusLabel.FontSize = 0.8f;
      Button.AddChild(_statusLabel);

      Button.RegisterUpdate(Update);
    }

    private void Update()
    {
      var ctrl = _padApp.Screen.IsOnScreen && MyAPIGateway.Input.IsAnyCtrlKeyPressed() && !MyAPIGateway.Input.IsAnyShiftKeyPressed();

      if ((!ctrl || !Button.Handler.IsMouseOver) && _icon.SpriteImage != _iconString)
      {
        _icon.SpriteImage = _iconString;
        SetBtText(_statusText);
        UpdateBorderColor();
      }

      var disabled = false;
      if (_block != null)
      {
        Button.Disabled = !_block.IsFunctional;
        disabled = Button.Disabled;
      }

      UpdateValue();

      if (ctrl)
        Button.Disabled = false;

      if (Button.Handler.IsMousePressed)
        Button.BgColor = _padApp.Theme.GetMainColorDarker(8);
      else if (Button.Handler.IsMouseOver)
      {
        var hoverColor = _padApp.Theme.GetMainColorDarker(5);
        if (_terminalAction != null && ctrl)
        {
          hoverColor = new Color(10, 0, 0);
          if (_icon.SpriteImage != "Cross")
          {
            _icon.SpriteColor = _gray;
            _icon.SpriteImage = "Cross";
            Button.Border = Vector4.Zero;
            SetBtText("Clear");
            _statusLabel.TextColor = _padApp.Theme.WhiteColor;
          }
        }
        else if (_terminalAction != null & _block != null)
        {
          var text = _blockGroup != null ? $"*{_blockGroup.Name}*" : $"{_block.DisplayNameText}";
          _padApp.ShowNotification($"{text} - {_terminalAction.Name.ToString()}");
        }
        Button.BgColor = disabled ? _padApp.Theme.GetMainColorDarker(3) : hoverColor;
      }
      else
        Button.BgColor = disabled ? _padApp.Theme.GetMainColorDarker(3) : _padApp.Theme.GetMainColorDarker(4);

    }

    private void OnClickButton()
    {
      if (_terminalAction == null)
      {
        _padApp.ShowSelectBlockView(this);
        return;
      }
      else if (_padApp.Screen.IsOnScreen && MyAPIGateway.Input.IsAnyCtrlKeyPressed())
      {
        ClearAction();
        _padApp.SelectActionConfirm();
        return;
      }

      if (_blockGroup != null)
      {
        _blocks.Clear();
        _blockGroup.GetBlocks(_blocks);
        foreach (var bl in _blocks)
          ApplyAction(bl);
      }
      else
      {
        var hasConnection = MyAPIGateway.GridGroups.HasConnection(_padApp.Screen.Block.CubeGrid as IMyCubeGrid, _block.CubeGrid, GridLinkTypeEnum.Logical);
        if (hasConnection && Utils.IsOwnerOrFactionShare(_block, MyAPIGateway.Session.Player))
          ApplyAction(_block);
        else
          MyAPIGateway.Utilities.ShowNotification("Activation Failed", 3000, "Red");
        // TODO: Consider move to app._notification
      }

      UpdateValue();
    }

    private void ApplyAction(IMyCubeBlock block)
    {
      if (block.IsFunctional)
      {
        if (Param == "")
          _terminalAction.Apply(block);
        else
          _terminalAction.Apply(block, _paramList);
      }
    }

    public void CloneFrom(ActionButton actBt)
    {
      if (actBt?._blockGroup != null)
        SetAction(actBt._blockGroup, actBt._terminalAction);
      else if (actBt?._block != null)
        SetAction(actBt._block, actBt._terminalAction);
    }

    public void SetAction(IMyBlockGroup blockGroup, ITerminalAction terminalAction)
    {
      _blockGroup = blockGroup;
      _terminalAction = terminalAction;
      Param = "";

      _blocks.Clear();
      blockGroup.GetBlocks(_blocks);
      _block = _blocks[0];

      Button.Border = new Vector4(0, 2, 0, 0);
      var darker7 = _padApp.Theme.GetMainColorDarker(7);
      Button.BorderColor = darker7;

      UpdateAction(Utils.GetBlockGroupTexture(blockGroup, TouchButtonPadSession.Instance.LcdTextureDefinitions));
    }

    public void SetAction(IMyCubeBlock block, ITerminalAction terminalAction)
    {
      _block = block;
      _terminalAction = terminalAction;
      Param = "";

      UpdateBorderColor();
      UpdateAction(Utils.GetBlockTexture(block, TouchButtonPadSession.Instance.LcdTextureDefinitions));
    }

    private void UpdateBorderColor()
    {
      var same = _block.CubeGrid.EntityId == _padApp.Screen.Block.CubeGrid.EntityId;
      var golden = new Color(Color.DarkGoldenrod * 0.2f, 1);
      Button.Border = new Vector4(0, 2, 0, 0);
      var darker7 = _padApp.Theme.GetMainColorDarker(7);
      Button.BorderColor = same ? darker7 : golden;
    }

    private void UpdateAction(string iconString)
    {
      _icon.SpriteImage = _iconString = iconString;
      UpdateValue();
    }

    private void UpdateValue()
    {
      if (_terminalAction == null)
        return;

      if (_icon.SpriteImage == "Cross")
      {
        SetBtText("Clear");
        return;
      }

      _icon.SpriteColor = Button.Disabled ? _padApp.Theme.GetMainColorDarker(4) : _gray;
      _statusLabel.TextColor = Button.Disabled ? _padApp.Theme.GetMainColorDarker(2) : _padApp.Theme.WhiteColor;

      _name.Clear();
      if (_block != null)
      {
        _terminalAction.WriteValue(_block, _name);
        _statusText = _name.ToString();
        if (_statusText == "")
          _statusText = _terminalAction.Name.ToString();
        SetBtText(_statusText);
      }
    }

    private void ClearAction()
    {
      _statusLabel.Text = _icon.SpriteImage = _statusText = _iconString = "";
      _terminalAction = null;
      _block = null;
      _blockGroup = null;
      _blocks.Clear();
      Button.Border = Vector4.Zero;
    }

    Vector2 _prevSize = Vector2.Zero;
    private void SetBtText(string text)
    {
      var scale = (_padApp.Theme?.Scale ?? 1);
      var sizeIcon = _icon.GetSize() / scale;
      if (_statusLabel.Text == text && _prevSize == sizeIcon)
        return;

      _prevSize = sizeIcon;

      var size = Button.GetSize();
      var defSize = size.X >= 100 ? 0.7f : 0.5f;
      _statusLabel.FontSize = MathHelper.Max(defSize, MathHelper.Min(1.5f, (size.X * defSize) / 100));
      _statusLabel.Text = text;

      _icon.SpriteSize = new Vector2(MathHelper.Min(sizeIcon.X, sizeIcon.Y));
      _icon.SpritePosition = (sizeIcon - _icon.SpriteSize) * 0.5f;
    }

    public void Dispose()
    {
      Button.UnregisterUpdate(Update);
      ClearAction();
      _padApp = null;
      _statusLabel = null;
      _icon = null;
      Button = null;
      _name = null;
      _blocks = null;
    }

    public MyTuple<int, string, long, string> GetTuple()
    {
      return new MyTuple<int, string, long, string>(Index, _blockGroup?.Name ?? "", _block?.EntityId ?? 0, _terminalAction?.Id ?? "");
    }
  }
}