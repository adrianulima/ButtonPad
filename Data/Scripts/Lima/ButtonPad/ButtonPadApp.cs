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
    private ParameterView _paramView;
    private SelectLayoutView _layoutView;
    private AppContent? _loadadeAppContent;

    private IMyHudNotification _notification;

    public ButtonPadApp(IMyCubeBlock block, IMyTextSurface surface, Action saveConfigAction) : base(block, surface)
    {
      Direction = ViewDirection.Column;

      _notification = MyAPIGateway.Utilities.CreateNotification(string.Empty);
      SaveConfigAction = saveConfigAction;

      _buttonsView = new ButtonPadView(this);
      _buttonsView.Enabled = true;
      AddChild(_buttonsView);

      _actionsView = new SelectActionView(this);
      _actionsView.Enabled = false;
      AddChild(_actionsView);

      _blocksView = new SelectBlockView(this);
      _blocksView.Enabled = false;
      AddChild(_blocksView);

      _paramView = new ParameterView(this);
      _paramView.Enabled = false;
      AddChild(_paramView);

      _layoutView = new SelectLayoutView(this);
      _layoutView.Enabled = false;
      AddChild(_layoutView);

      RegisterUpdate(Update);

      // this.Screen.InteractiveDistance = 6;
    }

    int _prevWheel = 0;
    bool _pressedInside = false;
    private void Update()
    {
      ApplyLoadedContent();
      _loadadeAppContent = null;

      if (_blocksView.Enabled)
        _blocksView.UpdateScrollStep();
      if (_actionsView.Enabled)
        _actionsView.UpdateScrollStep();
      if (_layoutView.Enabled)
        _layoutView.UpdateScrollStep();

      var anyPressed = Screen.Mouse1.IsPressed || MyAPIGateway.Input.IsAnyMouseOrJoystickPressed();
      if (MyAPIGateway.Gui.IsCursorVisible || (!_pressedInside && !Screen.IsOnScreen && anyPressed) || Screen.Mouse2.IsPressed)
        SelectActionConfirm(false);

      _pressedInside = _pressedInside || (Screen.IsOnScreen && anyPressed);
      if (!anyPressed)
        _pressedInside = false;

      var wheelDelta = MyAPIGateway.Input.MouseScrollWheelValue();
      if (_buttonsView.Enabled && Screen.IsOnScreen && wheelDelta != _prevWheel && MyAPIGateway.Input.IsAnyCtrlKeyPressed() && MyAPIGateway.Input.IsAnyShiftKeyPressed())
      {
        var newScale = MathHelper.Min(2.25f, MathHelper.Max(0.5f, CustomScale + Math.Sign(wheelDelta - _prevWheel) * 0.25f));
        var changed = false;
        var shouldSave = false;
        while (newScale != CustomScale && !changed)
        {
          shouldSave = true;
          CustomScale = newScale;
          changed = _buttonsView.Reset();
          newScale = MathHelper.Min(2.25f, MathHelper.Max(0.75f, CustomScale + Math.Sign(wheelDelta - _prevWheel) * 0.25f));
        }
        if (shouldSave)
          SaveConfigAction?.Invoke();
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

      var themeScale = _loadadeAppContent?.ThemeScale ?? 0;
      Theme.Scale = themeScale > 0 ? themeScale : 1;
      Cursor.Scale = Theme.Scale;

      var customScale = _loadadeAppContent?.CustomScale ?? 0;
      CustomScale = customScale > 0 ? customScale : 1;
      if (CustomScale != 1)
        _buttonsView.Reset();

      var count = _loadadeAppContent?.ButtonsList.Count ?? 0;
      var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Screen.Block.CubeGrid as IMyCubeGrid);
      if (terminalSystem == null)
        return;

      count = (int)MathHelper.Min(count, _buttonsView.ActionButtons.Count);
      for (int i = 0; i < count; i++)
      {
        var index = _loadadeAppContent?.ButtonsList[i].Item1 ?? 0;
        var blGrpName = _loadadeAppContent?.ButtonsList[i].Item2;
        var blockId = _loadadeAppContent?.ButtonsList[i].Item3;
        var splitActionAndParam = _loadadeAppContent?.ButtonsList[i].Item4.Split('|');
        var position = _loadadeAppContent?.ButtonsList[i].Item5;
        var actionName = splitActionAndParam[0];

        if ((blGrpName == "" && blockId == 0 && position == Vector3I.MaxValue) || actionName == "")
          continue;

        IMyCubeBlock block;
        if (position != Vector3I.MaxValue)
        {
          Matrix mtr;
          Screen.Block.Orientation.GetMatrix(out mtr);
          mtr.TransposeRotationInPlace();
          mtr = Matrix.Invert(mtr);

          Vector3I lcdPos = Screen.Block.Position;
          Vector3I pos = position ?? Vector3I.Zero;
          Vector3I.Transform(ref pos, ref mtr, out pos);
          Vector3I blockPos = new Vector3I(lcdPos.X - pos.X, lcdPos.Y - pos.Y, lcdPos.Z - pos.Z);

          var slimBlock = Screen.Block.CubeGrid.GetCubeBlock(blockPos);
          block = slimBlock?.FatBlock as IMyCubeBlock;
        }
        else
        {
          block = MyAPIGateway.Entities.GetEntityById(blockId) as IMyCubeBlock;
        }

        if (block == null)
        {
          // TODO: Implement pending to construct state (not found instead of ignore)
          continue;
        }

        var terminalAction = actionName != "" ? MyAPIGateway.TerminalActionsHelper.GetActionWithName(actionName, block.GetType()) : null;
        var blGrp = blGrpName != "" ? terminalSystem.GetBlockGroupWithName(blGrpName) : null;

        var len = splitActionAndParam.Length;
        if (blGrp != null)
          _buttonsView.ActionButtons[index].SetAction(blGrp, terminalAction);
        else
        {
          _buttonsView.ActionButtons[index].SetAction(block, terminalAction);
          if (len > 2)
            _buttonsView.ActionButtons[index].Param = splitActionAndParam[2];
        }

        if (len > 1)
          _buttonsView.ActionButtons[index].TextMode = int.Parse(splitActionAndParam[1]);
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
      _paramView.Dispose();
      _buttonsView = null;
      _blocksView = null;
      _paramView = null;
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
      _paramView.Enabled = false;
      _blocksView.Enabled = true;
      _layoutView.Enabled = false;

      _blocksView.UpdateItemsForButton(actionBt, Screen.Block.CubeGrid as IMyCubeGrid);
    }

    public void ShowSelectActionView(ActionButton actionBt, IMyTerminalBlock block)
    {
      _buttonsView.Enabled = false;
      _actionsView.Enabled = true;
      _paramView.Enabled = false;
      _blocksView.Enabled = false;
      _layoutView.Enabled = false;

      _actionsView.UpdateItemsForButton(actionBt, block);
    }

    public void ShowSelectActionView(ActionButton actionBt, IMyBlockGroup blockGroup)
    {
      _buttonsView.Enabled = false;
      _actionsView.Enabled = true;
      _paramView.Enabled = false;
      _blocksView.Enabled = false;
      _layoutView.Enabled = false;

      _actionsView.UpdateItemsForButton(actionBt, blockGroup);
    }

    public void ShowSelectArgumentView(ActionButton actionBt)
    {
      _buttonsView.Enabled = false;
      _actionsView.Enabled = false;
      _blocksView.Enabled = false;
      _paramView.Enabled = true;
      _layoutView.Enabled = false;

      _paramView.UpdateForButton(actionBt);
    }

    public void ShowSelectLayoutView(ActionButton actionBt)
    {
      _buttonsView.Enabled = false;
      _actionsView.Enabled = false;
      _blocksView.Enabled = false;
      _paramView.Enabled = false;
      _layoutView.Enabled = true;

      _layoutView.UpdateItemsForButton(actionBt);
    }

    public void SelectActionConfirm(bool save = true)
    {
      _paramView.CancelTextfield();

      _buttonsView.Enabled = true;
      _actionsView.Enabled = false;
      _paramView.Enabled = false;
      _blocksView.Enabled = false;
      _layoutView.Enabled = false;

      if (save)
        SaveConfigAction?.Invoke();
    }
  }
}