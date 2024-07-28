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
        float i; float j;
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
        public void Draw(bool tb)
        {
            
            if (IsHover()){
                if (i<=1){i+=0.025f;}
                Raylib.DrawRectangleRoundedLines(
                    new Rectangle(pos, dim), 0.5f, 54, 4, Raylib.ColorAlpha(fg,i)
                );
            }
            if(!IsHover()){
                if (i>=0){i-=0.025f;}
                Raylib.DrawRectangleRoundedLines(
                    new Rectangle(pos, dim), 0.5f, 54, 4, Raylib.ColorAlpha(fg,i)
                );
            }
            Raylib.DrawRectangleRounded(
                new Rectangle(pos, dim),
                0.5f, 54,bg
            );
            if (Selected() && tb)
            {
                if (j<=1){j+=0.01f;}
                Raylib.DrawRectangleRounded(
                    new Rectangle(pos, dim),
                        0.5f, 54, Raylib.ColorAlpha(Raylib.ColorTint(bg,Color.Red),j)
                );
            }
            if (!Selected()){
                if (j>=0){j-=0.01f;}
                Raylib.DrawRectangleRounded(
                    new Rectangle(pos, dim),
                    0.5f, 54, Raylib.ColorAlpha(Raylib.ColorTint(bg,Color.Red),j)
                );
            }
            Raylib.DrawText(text, (int)(pos.X + 20), (int)(pos.Y + dim.Y / 4), 24, fg);
        }
    }
    class Slider{
        Vector2 pos; 
        int width;
        const int height = 10;
        Vector2 extrenums; // a range of Values as [0,5] from 0,5
        int subDivs;// number of possible values <=> 1/deltaL
        Vector2 dotPos;
        const int Radius=8;
        float Result;
        float Divstep;
        private float i;
        bool fact;
        public Slider(Vector2 pos,Vector2 extrenums,int width,int subDivs){
            this.pos = pos;
            this.extrenums = extrenums;
            this.width = width;
            this.subDivs = subDivs;
            this.dotPos = this.pos+new Vector2(0,height/2);
        }
        private bool Hover(){
            Vector2 mpos = Raylib.GetMousePosition();
            bool V = (mpos - pos).Y > -15 && (mpos - pos).Y < height+15;
            bool H = (mpos - pos).X > -1 && (mpos - pos).X <= width+1;
            if ( V|(fact && Raylib.IsMouseButtonDown(MouseButton.Left)) && H)
            {
                fact = true;
                return true;
            }
            else
            {
                fact = false;
                return false;
            }
        }
        private bool Held(){
            if (Hover() && Raylib.IsMouseButtonDown(MouseButton.Left)){
                return true;
            }
            else{
                return false;
            }
        }
        public float Value(){
            return Result;
        }
        public void Update(){
            Divstep = (extrenums.Y-extrenums.X)/subDivs;
            if (Held()){dotPos.X = Raylib.GetMousePosition().X;}
            Result = extrenums.X+Divstep*MathF.Floor((extrenums.Y-extrenums.X)*(dotPos.X-pos.X)/(width*Divstep));
        }
        public void Draw(Color prm,Color sec){
            Raylib.DrawRectangleRounded(
                new Rectangle(pos, width,height),
                    1.5f, 5, Raylib.ColorAlpha(prm,0.4f)
            );
            if (Hover()){
                if( i<=3) i+=Raylib.GetFrameTime()/0.075f; 
                Raylib.DrawCircleGradient((int)dotPos.X, (int)dotPos.Y, Radius+i*i,sec,Raylib.ColorAlpha(sec,0.1f));
                Raylib.DrawCircleV(dotPos,Radius+i,sec);
                Raylib.DrawCircleLinesV(dotPos,Radius+i,prm);
                int L = Raylib.MeasureText($"{Result}",26);
                Raylib.DrawText($"{Result}",(int)(pos.X+width/2-L/2),(int)(pos.Y-35),26,
                    Raylib.ColorAlpha(Color.White,i/3));
            }
            else{
                if(i>=0) i-=Raylib.GetFrameTime()/0.075f;
                Raylib.DrawCircleV(dotPos,Radius+i,sec);
                Raylib.DrawCircleLinesV(dotPos,Radius+i,prm);
                int L = Raylib.MeasureText($"{Result}",26);
                Raylib.DrawText($"{Result}",(int)(pos.X+width/2-L/2),(int)(pos.Y-35),26,
                    Raylib.ColorAlpha(Color.White,i/3));
            }
            
        }
        
    }
}
