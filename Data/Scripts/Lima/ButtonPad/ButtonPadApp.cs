using System;
using System.Collections.Generic;
using Lima.API;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Lima
{
  public class ButtonPadApp : TouchApp
  {
    public List<ActionButton> ActionButtons { get { return _buttonsView.ActionButtons; } }
    public Action SaveConfigAction;

    private ButtonPadView _buttonsView;
    private SelectActionView _actionsView;
    private SelectBlockView _blocksView;
    private AppContent? _loadadeAppContent;

    public ButtonPadApp(Action saveConfigAction)
    {
      Direction = ViewDirection.Column;

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

    private void Update()
    {
      ApplyLoadedContent();
      _loadadeAppContent = null;

      if (MyAPIGateway.Gui.IsCursorVisible || (!Screen.IsOnScreen && MyAPIGateway.Input.IsAnyMouseOrJoystickPressed()))
        SelectActionConfirm(false);
    }

    public void ApplySettings(AppContent content)
    {
      _loadadeAppContent = content;
    }

    private void ApplyLoadedContent()
    {
      if (_loadadeAppContent == null)
        return;

      var count = _loadadeAppContent?.Buttons?.Count ?? 0;
      var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Screen.Block.CubeGrid as IMyCubeGrid);
      if (terminalSystem == null)
        return;

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