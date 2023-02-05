using Lima.API;
using VRage.Game.Components;

namespace Lima
{
  [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
  public class ButtonPadSession : MySessionComponentBase
  {
    public TouchUiKit Api { get; private set; }
    public static ButtonPadSession Instance;

    public override void LoadData()
    {
      Instance = this;

      Api = new TouchUiKit();
      Api.Load();
    }

    protected override void UnloadData()
    {
      Api?.Unload();
      Instance = null;
    }
  }
}