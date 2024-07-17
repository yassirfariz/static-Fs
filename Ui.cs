using System.Numerics;
using Raylib_cs;
namespace UI
{
    class Button
    {
        public Color bg;
        public Color fg;
        public Vector2 pos;
        public Vector2 dim;
        public string text;
        public bool toggled;
        public Button(Vector2 pos, Vector2 dim, string Text, Color fg, Color bg)
        {
            this.pos = pos;
            this.dim = dim;
            this.text = Text;
            this.fg = fg;
            this.bg = bg;
        }
        public void Update(Vector2 pos, Vector2 dim)
        {
            this.pos = pos;
            this.dim = dim;
        }
        public bool IsHover()
        {
            Vector2 mpos = Raylib.GetMousePosition();
            if ((mpos - pos).Y > 0 && (mpos - pos).X > 0 && (mpos - pos).X < dim.X && (mpos - pos).Y < dim.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsClick()
        {
            if (this.IsHover() && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Selected()
        {
            if (IsClick() && toggled)
            {
                toggled = false;
            }
            if (IsClick() && !toggled)
            {
                toggled = true;
            }
            return toggled;
        }
        public void Draw()
        {
            if (this.IsHover())
                Raylib.DrawRectangleRoundedLines(
                    new Rectangle(pos, dim), 0.5f, 54, 4, fg
                );
            if (this.Selected())
            {
                Raylib.DrawRectangleRounded(
                    new Rectangle(pos, dim),
                        0.5f, 54, Raylib.ColorTint(bg, Color.Red)
                );
            }
            else
            {
                Raylib.DrawRectangleRounded(
                    new Rectangle(pos, dim),
                        0.5f, 54, bg
                );
            }

            Raylib.DrawText(text, (int)(pos.X + 20), (int)(pos.Y + dim.Y / 4), 24, fg);
        }
    }
    class Slider{
        Vector2 pos; 
        int width;
        Vector2 extrenums;
        
        public Slider(Vector2 pos,Vector2 extrenums,int width){
            this.pos = pos;
            this.extrenums = extrenums;
            this.width = width;

        }
        
        public void Update(){
            
        }
        public void Draw(){

        }
        
    }
}