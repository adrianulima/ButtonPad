using Lima.API;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System;
using VRage.Game.ModAPI;
using VRageMath;

namespace Lima
{
  public class ButtonPadApp : TouchApp
  {
    public List<ActionButton> ActionButtons { get { return _buttonsView.ActionButtons; } }
    public Action SaveConfigAction;

    public float CustomScale = 1;

    private ButtonPadView _buttonsView;
    private SelectActionView _actionsView;
    private SelectBlockView _blocksView;
    private AppContent? _loadadeAppContent;

    private IMyHudNotification _notification;

    public ButtonPadApp(Action saveConfigAction)
    {
      Direction = ViewDirection.Column;

      _notification = MyAPIGateway.Utilities.CreateNotification(string.Empty);
      SaveConfigAction = saveConfigAction;
    }

    public void CreateElements()
    {
      _buttonsView = new ButtonPadView(this);
      _buttonsView.Enabled = true;
      AddChild(_buttonsView);

      _actionsView = new SelectActionView(this);
      _actionsView.Enabled = false;
      AddChild(_actionsView);

      _blocksView = new SelectBlockView(this);
      _blocksView.Enabled = false;
      AddChild(_blocksView);

      RegisterUpdate(Update);
    }

    int _prevWheel = 0;
    bool _pressedInside = false;
    private void Update()
    {
      ApplyLoadedContent();
      _loadadeAppContent = null;

      var anyPressed = MyAPIGateway.Input.IsAnyMouseOrJoystickPressed();
      if (MyAPIGateway.Gui.IsCursorVisible || (!_pressedInside && !Screen.IsOnScreen && anyPressed))
        SelectActionConfirm(false);

      _pressedInside = _pressedInside || (Screen.IsOnScreen && anyPressed);
      if (!anyPressed)
        _pressedInside = false;

      var wheelDelta = MyAPIGateway.Input.MouseScrollWheelValue();
      if (_buttonsView.Enabled && Screen.IsOnScreen && wheelDelta != _prevWheel && MyAPIGateway.Input.IsAnyCtrlKeyPressed() && MyAPIGateway.Input.IsAnyShiftKeyPressed())
      {
        var newScale = MathHelper.Min(2.25f, MathHelper.Max(0.75f, CustomScale + Math.Sign(wheelDelta - _prevWheel) * 0.25f));
        var changed = false;
        while (newScale != CustomScale && !changed)
        {
          CustomScale = newScale;
          changed = _buttonsView.Reset();
          newScale = MathHelper.Min(2.25f, MathHelper.Max(0.75f, CustomScale + Math.Sign(wheelDelta - _prevWheel) * 0.25f));
        }
      }
      _prevWheel = wheelDelta;
    }

    public void ApplySettings(AppContent content)
    {
      _loadadeAppContent = content;
    }

    private void ApplyLoadedContent()
    {
      if (_loadadeAppContent == null)
        return;

      var customScale = _loadadeAppContent?.CustomScale ?? 0;
      CustomScale = customScale > 0 ? customScale : 1;
      if (CustomScale != 1)
        _buttonsView.Reset();
      var count = _loadadeAppContent?.Buttons?.Count ?? 0;
      var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Screen.Block.CubeGrid as IMyCubeGrid);
      if (terminalSystem == null)
        return;

      count = (int)MathHelper.Min(count, _buttonsView.ActionButtons.Count);
      for (int i = 0; i < count; i++)
      {
        var index = _loadadeAppContent?.Buttons[i].Item1 ?? 0;
        var blGrpName = _loadadeAppContent?.Buttons[i].Item2;
        var blockId = _loadadeAppContent?.Buttons[i].Item3;
        var actionName = _loadadeAppContent?.Buttons[i].Item4;

        if ((blGrpName == "" && blockId == 0) || actionName == "")
          continue;

        var block = MyAPIGateway.Entities.GetEntityById(blockId) as IMyCubeBlock;
        if (block == null)
          continue;

        var terminalAction = actionName != "" ? MyAPIGateway.TerminalActionsHelper.GetActionWithName(actionName, block.GetType()) : null;
        var blGrp = blGrpName != "" ? terminalSystem.GetBlockGroupWithName(blGrpName) : null;
        if (blGrp != null)
          _buttonsView.ActionButtons[index].SetAction(blGrp, terminalAction);
        else
          _buttonsView.ActionButtons[index].SetAction(block, terminalAction);
      }
    }

    public void ShowNotification(string text)
    {
      _notification.Hide();
      _notification.Text = text;
      _notification.AliveTime = 160;
      _notification.Show();
    }

    public void Dispose()
    {
      UnregisterUpdate(Update);
      _buttonsView.Dispose();
      _blocksView.Dispose();
      _actionsView.Dispose();
      _buttonsView = null;
      _blocksView = null;
      _actionsView = null;
      _loadadeAppContent = null;
      _notification = null;
      SaveConfigAction = null;
      this.ForceDispose();
    }

    public void ShowSelectBlockView(ActionButton actionBt)
    {
      _buttonsView.Enabled = false;
      _actionsView.Enabled = false;
      _blocksView.Enabled = true;

      _blocksView.UpdateItemsForButton(actionBt, Screen.Block.CubeGrid as IMyCubeGrid);
    }

    public void ShowSelectActionView(ActionButton actionBt, IMyTerminalBlock block)
    {
      _buttonsView.Enabled = false;
      _actionsView.Enabled = true;
      _blocksView.Enabled = false;

      _actionsView.UpdateItemsForButton(actionBt, block);
    }

    public void ShowSelectActionView(ActionButton actionBt, IMyBlockGroup blockGroup)
    {
      _buttonsView.Enabled = false;
      _actionsView.Enabled = true;
      _blocksView.Enabled = false;

      _actionsView.UpdateItemsForButton(actionBt, blockGroup);
    }

    public void SelectActionConfirm(bool save = true)
    {
      _buttonsView.Enabled = true;
      _actionsView.Enabled = false;
      _blocksView.Enabled = false;

      if (save)
        SaveConfigAction?.Invoke();
    }
  }
}