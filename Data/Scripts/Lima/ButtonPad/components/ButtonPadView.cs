using Lima.API;
using System.Collections.Generic;
using VRageMath;

namespace Lima
{
  public class ButtonPadView : TouchView
  {
    private ButtonPadApp _padApp;

    private int _rows = 3;
    private int _cols = 3;
    private List<ActionButton> _actionBts = new List<ActionButton>();

    public ButtonPadView(ButtonPadApp pad)
    {
      _padApp = pad;

      Direction = ViewDirection.Column;
      Padding = new Vector4(4);
      Gap = 4;

      var maxSide = MathHelper.Max(pad.Viewport.Width, pad.Viewport.Height);
      var minSide = MathHelper.Min(pad.Viewport.Width, pad.Viewport.Height);
      var minSize = MathHelper.Min(minSide, maxSide > 256 ? 128 : 64);

      _cols = MathHelper.FloorToInt(pad.Viewport.Width / minSize);
      _rows = MathHelper.FloorToInt(pad.Viewport.Height / minSize);

      UpdateItems();
    }

    public void UpdateItems()
    {
      for (int i = 0; i < _rows; i++)
      {
        var rowView = new TouchView(ViewDirection.Row);
        rowView.Gap = 4;
        AddChild(rowView);

        for (int j = 0; j < _cols; j++)
        {
          var actionBt = new ActionButton(_padApp);
          _actionBts.Add(actionBt);
          actionBt.Button.Pixels = Vector2.Zero;
          actionBt.Button.Scale = Vector2.One;
          rowView.AddChild(actionBt.Button);
        }
      }
    }

    public void Dispose()
    {
      foreach (var actBt in _actionBts)
        actBt.Dispose();

      _actionBts.Clear();
      _actionBts = null;
    }
  }
}
