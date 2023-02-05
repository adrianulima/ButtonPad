using Lima.API;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Lima
{
  public class ButtonPadApp : TouchApp
  {
    private ButtonPadView _buttonsView;
    private SelectActionView _actionsView;
    private SelectBlockView _blocksView;

    public ButtonPadApp()
    {
      Direction = ViewDirection.Column;
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
      if (MyAPIGateway.Gui.IsCursorVisible || (!Screen.IsOnScreen && MyAPIGateway.Input.IsAnyMouseOrJoystickPressed()))
        SelectActionConfirm();
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

    public void SelectActionConfirm()
    {
      _buttonsView.Enabled = true;
      _actionsView.Enabled = false;
      _blocksView.Enabled = false;
    }
  }
}