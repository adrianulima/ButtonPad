using Lima.API;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace Lima
{
  public class ParameterView : TouchView
  {
    private ButtonPadApp _padApp;
    private TouchLabel _label;
    private TouchTextField _textField;
    private TouchButton _button;

    private ActionButton _actionbutton;

    public ParameterView(ButtonPadApp pad)
    {
      _padApp = pad;

      Gap = 4;
      Alignment = ViewAlignment.Center;
      Anchor = ViewAnchor.Center;
      Padding = new Vector4(8, 4, 8, 4);

      _label = new TouchLabel("Argument", 0.5f, TextAlignment.CENTER);
      AddChild(_label);
      _textField = new TouchTextField("", (string s, bool b) => { });
      AddChild(_textField);
      _button = new TouchButton("Confirm", OnConfirm);
      AddChild(_button);
    }

    public void UpdateForButton(ActionButton actionBt)
    {
      _actionbutton = actionBt;
      _textField.Text = "";

      var sizeX = GetSize().X;
      var w = new Vector2(MathHelper.Min(256, sizeX), _textField.Pixels.Y);
      _textField.Pixels = w;
      _textField.Scale = Vector2.Zero;
      _button.Pixels = w;
      _button.Scale = Vector2.Zero;
    }

    public void OnConfirm()
    {
      _actionbutton.Param = _textField.Text;
      _padApp.SelectActionConfirm();
    }

    public void CancelTextfield()
    {
      _textField?.CancelEdit();
    }

    public void Dispose()
    {
      _padApp = null;
      _label = null;
      _textField = null;
      _button = null;
      _actionbutton = null;
    }

  }
}
