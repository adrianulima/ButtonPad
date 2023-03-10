using Lima.API;
using System.Collections.Generic;
using VRageMath;

namespace Lima
{
  public class ButtonPadView : View
  {
    private ButtonPadApp _padApp;

    private int _rows = 3;
    private int _cols = 3;
    private List<ActionButton> _actionBts = new List<ActionButton>();
    public List<ActionButton> ActionButtons { get { return _actionBts; } }

    public ButtonPadView(ButtonPadApp pad)
    {
      _padApp = pad;

      Direction = ViewDirection.Column;
      Padding = new Vector4(4);
      Gap = 4;

      UpdateItems();
    }

    public bool Reset()
    {
      RemoveAllChildren();

      var old = _actionBts;
      _actionBts = null;
      var changed = UpdateItems(old);

      foreach (var actBt in old)
        actBt.Dispose();
      old.Clear();

      return changed;
    }

    private bool UpdateItems(List<ActionButton> previous = null)
    {
      var maxSide = MathHelper.Max(_padApp.Viewport.Width, _padApp.Viewport.Height);
      var minSide = MathHelper.Min(_padApp.Viewport.Width, _padApp.Viewport.Height);
      var minSize = MathHelper.Min(minSide, maxSide > 256 ? 128 : 64) * _padApp.CustomScale;

      var cols = _cols;
      var rows = _rows;
      _cols = MathHelper.FloorToInt(_padApp.Viewport.Width / minSize);
      _rows = MathHelper.FloorToInt(_padApp.Viewport.Height / minSize);

      if (_cols < 1) _cols = 1;
      if (_rows < 1) _rows = 1;

      if (_actionBts == null)
        _actionBts = new List<ActionButton>();

      for (int i = 0; i < _rows; i++)
      {
        var rowView = new View(ViewDirection.Row);
        rowView.Gap = 4;
        AddChild(rowView);

        for (int j = 0; j < _cols; j++)
        {
          var index = i * _cols + j;
          var actionBt = GetActionByIndex(previous, index);
          _actionBts.Add(actionBt);
          rowView.AddChild(actionBt.Button);
        }
      }

      return _rows != rows || _cols != cols;
    }

    private ActionButton GetActionByIndex(List<ActionButton> previous, int index)
    {
      var act = new ActionButton(_padApp, index);
      act.CloneFrom(previous?.Find(item => item.Index == index));
      return act;
    }

    private void RemoveAllChildren()
    {
      foreach (var row in Children)
        RemoveChild(row);
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
