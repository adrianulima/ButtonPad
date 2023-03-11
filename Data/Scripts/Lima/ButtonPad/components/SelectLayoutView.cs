using Lima.API;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace Lima
{
  public class SelectLayoutView : ScrollView
  {
    private ButtonPadApp _padApp;
    private List<Button> _buttons = new List<Button>();

    private string[] _options;

    private int _selected = 0;
    private float _step = 0;

    public SelectLayoutView(ButtonPadApp pad)
    {
      _padApp = pad;

      ScrollBar.Pixels = new Vector2(24, 0);
      Gap = 2;

      _options = new string[] { "Show Current Value", "Show Block Name", "Show Name and Value", "Show Action and Value", "Show Icon Only" };

      for (int i = 0; i < 5; i++)
      {
        var bt = new Button($"{_options[i]}", null);
        AddButton(bt);
      }
    }

    public void Dispose()
    {
      _buttons.Clear();
      _buttons = null;
    }

    public void UpdateItemsForButton(ActionButton actionBt)
    {
      Scroll = 0;
      for (int i = 0; i < _buttons.Count; i++)
      {
        var index = i;
        _buttons[i].OnChange = () =>
        {
          actionBt.TextMode = index;
          _padApp.SelectActionConfirm();
        };
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
      button.Label.AutoBreakLine = true;
      button.Label.MaxLines = 3;
      button.Pixels = new Vector2(0, h);
      button.Flex = new Vector2(1, 0);
      AddChild(button);
      _buttons.Add(button);

      _step = (button.Pixels.Y + Gap);
      UpdateScrollStep();
    }

    public void UpdateScrollStep()
    {
      ScrollWheelStep = _step * _padApp.Theme.Scale;
    }
  }
}
