using Lima.API;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace Lima
{
  public class Icon : TouchEmptyElement
  {
    public string SpriteImage;
    public Vector2 SpriteSize;
    public Vector2 SpritePosition;
    public float SpriteRotation;
    public Color? SpriteColor;

    public Icon(string image, Vector2 size, float rotation = 0, Color? color = null)
    {
      SpriteImage = image;
      SpriteSize = size;
      SpritePosition = Vector2.Zero;
      SpriteRotation = rotation;
      SpriteColor = color;

      Pixels = size;
      Scale = Vector2.Zero;

      RegisterUpdate(Update);
    }

    private void Update()
    {
      var scale = (App?.Theme?.Scale ?? 1);
      var imageSprite = new MySprite()
      {
        Type = SpriteType.TEXTURE,
        Data = SpriteImage,
        RotationOrScale = SpriteRotation,
        Color = SpriteColor ?? App.Theme.WhiteColor,
        Size = SpriteSize * scale,
        Position = Position + Vector2.UnitY * SpriteSize.Y * 0.5f + SpritePosition
      };

      GetSprites().Clear();
      GetSprites().Add(imageSprite);
    }
  }
}