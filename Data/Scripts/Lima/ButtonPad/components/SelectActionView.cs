using Lima.API;
using Lima.ButtonPad;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRageMath;

namespace Lima
{
  public class SelectActionView : ScrollView
  {
    private ButtonPadApp _padApp;
    private List<ITerminalAction> _terminalActions = new List<ITerminalAction>();
    private List<Button> _buttons = new List<Button>();

    private float _step = 0;

    public SelectActionView(ButtonPadApp pad)
    {
      _padApp = pad;

      ScrollBar.Pixels = new Vector2(24, 0);
      Gap = 2;
    }

    public void Dispose()
    {
      _terminalActions.Clear();
      _buttons.Clear();
      _terminalActions = null;
      _buttons = null;
    }

    public void UpdateItemsForButton(ActionButton actionBt, IMyBlockGroup blockGroup)
    {
      _terminalActions.Clear();
      Utils.GetBlockGroupActions(blockGroup, _terminalActions);

      RemoveAllChildren();
      _buttons.Clear();
      foreach (var act in _terminalActions)
      {
        var bt = new Button(act.Name.ToString(), () => SelectGroupAction(blockGroup, actionBt, act));
        AddButton(bt);
      }
    }

    public void UpdateItemsForButton(ActionButton actionBt, IMyTerminalBlock block)
    {
      _terminalActions.Clear();
      block.GetActions(_terminalActions, (a) => a.IsEnabled(block));

      RemoveAllChildren();
      _buttons.Clear();
      foreach (var act in _terminalActions)
      {
        var bt = new Button(act.Name.ToString(), () => SelectAction(block, actionBt, act));
        AddButton(bt);
      }
    }

    private void AddButton(Button button)
    {
      var height = _padApp.Screen.Surface.SurfaceSize.Y;
      var smallHeight = height < 128;
      var smallWidth = _padApp.Screen.Surface.SurfaceSize.X <= 128;

      var h = smallWidth ? 68f : 36f;
      if (smallHeight)
        h = ((height / 2) - 2) / _padApp.Theme.Scale;

      ScrollBar.Pixels = new Vector2(smallWidth && !smallHeight ? 8 : 24, 0);
      button.Padding = new Vector4(4);
      button.Label.Alignment = smallWidth ? TextAlignment.CENTER : TextAlignment.LEFT;
      button.Label.FontSize = smallWidth ? 0.5f : 0.8f;
      if (button.Label.Text.Length > 20)
        button.Label.FontSize = smallWidth ? 0.4f : 0.6f;
      button.Label.AutoBreakLine = true;
      button.Label.MaxLines = 3;
      button.Pixels = new Vector2(0, h);
      button.Flex = new Vector2(1, 0);
      AddChild(button);
      _buttons.Add(button);

      _step = button.Pixels.Y + Gap;
      UpdateScrollStep();
    }

    public void UpdateScrollStep()
    {
      ScrollWheelStep = _step * _padApp.Theme.Scale;
    }

    public void RemoveAllChildren()
    {
      Scroll = 0;
      // TODO: Pool
      foreach (var bt in _buttons)
        RemoveChild(bt);
      _buttons.Clear();
    }

    private void SelectGroupAction(IMyBlockGroup blockGroup, ActionButton actionBt, ITerminalAction action)
    {
      actionBt.SetAction(blockGroup, action);
      _padApp.ShowSelectLayoutView(actionBt);
    }

    private void SelectAction(IMyCubeBlock block, ActionButton actionBt, ITerminalAction action)
    {
      actionBt.SetAction(block, action);

      if (block is IMyProgrammableBlock && action.Id == "Run")
        _padApp.ShowSelectArgumentView(actionBt);
      else
        _padApp.ShowSelectLayoutView(actionBt);
    }
  }
}
