//  Program.cs
using Raylib_cs;
using System.Numerics;
using UI;
public class EsField
{
    Func<float, float, float> fx;
    Func<float, float, float> fy;
    int[] Xrange;
    int[] Yrange;
    int density;
    public EsField(Func<float, float, float> fx, Func<float, float, float> fy, int[] Xrange, int[] Yrange, int density)
    {
        this.fx = fx;
        this.fy = fy;
        this.Xrange = Xrange;
        this.Yrange = Yrange;
        this.density = density;
    }
    public void draw(Color color)
    {
        for (int i = Xrange[0]; i < Xrange[1]; i += density)
        {
            for (int j = Yrange[0]; j < Yrange[1]; j += density)
            {
                Vector2 pos = new(i, j);
                Arrow(pos, pos + 3 * density * Vector2.Normalize(new Vector2(fx(pos.X, pos.Y), fy(pos.X, pos.Y))) / 4, color);
            }
        };
    }
    private void Arrow(Vector2 posA, Vector2 posB, Color pcolor)
    {
        float angle = MathF.Atan2(posB.Y - posA.Y, posB.X - posA.X);
        Raylib.DrawLineEx(posA, posB, 4, pcolor);
        Raylib.DrawLineEx(posB, posB + 8 * new Vector2(-MathF.Cos(angle + (float)Math.PI / 4), -MathF.Sin(angle + (float)Math.PI / 4)), 2, pcolor);
        Raylib.DrawLineEx(posB, posB + 8 * new Vector2(-MathF.Cos(angle - (float)Math.PI / 4), -MathF.Sin(angle - (float)Math.PI / 4)), 2, pcolor);
    }
    public EsField LinearCombine(EsField fields)
    {
        return new EsField((x, y) => { return this.fx(x, y) + fields.fx(x, y); }, (x, y) => { return this.fy(x, y) + fields.fy(x, y); }, this.Xrange, this.Yrange, this.density);
    }
}
public class Particle
{
    public List<Vector2> positions;
    public Vector2 pos;
    public Vector2 vel;
    public Vector2 acc;
    public float charge;
    public string vec_mode;
    public const float k = 8.998400e+9f;
    public const float e = 1.60217733e-19f;
    public Particle(Vector2 pos, Vector2 vel, Vector2 acc, float charge)
    {
        this.pos = pos;
        this.vel = vel;
        this.acc = acc;
        this.charge = charge;
        this.vec_mode = "velocity";
        positions = [pos];
    }
    public void Draw(bool active)
    {
        Color clr = Raylib.ColorFromHSV(Raymath.Lerp(0, 360, MathF.Abs(charge) / 36),
            Raymath.Clamp(MathF.Abs(charge), 0, 1), 1);
        Raylib.DrawCircleGradient((int)pos.X, (int)pos.Y, charge * 20, clr, Raylib.ColorFromNormalized(Vector4.Zero));
        if (active) positions.Add(pos);
        if (active && positions.Count >= 200)
            _ = positions.Remove(positions.First());
        TjDraw(clr);
        if (vec_mode == "acceleration")
            Acc_repr(this.acc);
        else Acc_repr(this.vel);
        Raylib.DrawCircleV(pos, 12, clr);
        Raylib.DrawText(charge > 0 ? "+" : charge == 0 ? "0" : "-", (int)pos.X - 6, (int)pos.Y - 11, 26, Color.Black);
    }
    public void Update(float deltaTime, Particle[] system)
    {
        // verte velocity integral (v(t + Δt) = v(t) + a(t)Δt + 1/2 a(t)Δt^2)
        Vector2 acceleration = acc;

        pos += vel * deltaTime + 0.5f * acceleration * deltaTime * deltaTime;

        Vector2 newForce = new(0, 0);
        for (var i = 0; i < system.Length; i++)
        {
            newForce += CalculateForce(system[i]);
        }
        Vector2 newAcceleration = newForce; // Assuming newForce is already acceleration

        // Update velocity (v(t + Δt) = v(t) + 1/2 (a(t) + a(t + Δt))Δt)
        vel += 0.5f * (acceleration + newAcceleration) * deltaTime;
        // update the acceleration
        acc = newAcceleration;

    }
    // Method to calculate the force acting on the particle
    // You need to implement this based on your simulation's forces
    private Vector2 CalculateForce(Particle other)
    {
        // Implement the logic to calculate the force acting on this particle
        // For example, it could be gravitational, electrostatic, etc.
        Vector2 dp = pos - other.pos;
        float a = MathF.Atan2(dp.Y, dp.X);
        float F = k * (charge * other.charge) * e / (dp.LengthSquared() * 4e-10f);
        return new Vector2(F * MathF.Cos(a), F * MathF.Sin(a)); // Placeholder
    }
    private void TjDraw(Color clr)
    {
        for (var i = 1; i < positions.Count; i++)
        {
            var m_color = Raylib.ColorNormalize(clr);
            Raylib.DrawLineEx(positions[i - 1], positions[i], MathF.Log(positions.Count - i),
            Raylib.ColorFromNormalized(new(m_color.X, m_color.Y, m_color.Z, i / 200f)));
        }
    }
    private void Acc_repr(Vector2 Force)
    {
        Vector2 end = this.pos + 55 * Vector2.Normalize(Force);
        float angle = MathF.Atan2(Force.Y, Force.X);
        Raylib.DrawLineEx(this.pos, end, 5, Color.Red);
        Raylib.DrawLineEx(end, end + 11 * new Vector2(-MathF.Cos(angle + (float)Math.PI / 4), -MathF.Sin(angle + (float)Math.PI / 4)), 2, Color.Red);
        Raylib.DrawLineEx(end, end + 11 * new Vector2(-MathF.Cos(angle - (float)Math.PI / 4), -MathF.Sin(angle - (float)Math.PI / 4)), 2, Color.Red);
    }
}
public class Generator
{
    private List<Particle> particles { get; set; }

    public Generator() { particles = []; }
    public Particle[] GetParticles()
    {
        return particles.ToArray();
    }

    public void Generate(int maxParticles, Vector2 spacing, Vector2 offset)
    {
        for (var i = 0; i < maxParticles; i++)
        {
            var RandmonessRange = MathF.Log2(maxParticles);
            var x = offset.X + spacing.X * i % Raylib.GetRenderWidth();
            var y = offset.Y + MathF.Floor(spacing.X * i / Raylib.GetRenderWidth()) * spacing.Y;
            particles.Add(new Particle(new Vector2(x, y), new Vector2(0, i % 3),
            new Vector2(0, 0), Random.Shared.Next(-(int)RandmonessRange, (int)RandmonessRange)));
        };
    }
    public void DrawElements(bool active)
    {
        foreach (var p in particles)
        {
            p.Draw(active);
        }
    }
    public void EnableInteractions(float dt)
    {
        foreach (var p in particles)
        {
            Particle[] other = particles.Where(x => x != p).ToArray();
            p.Update(dt, other);
        };
    }
}
class Controller
{
    private readonly List<Particle> particles;
    int currentId;
    public Controller(List<Particle> particles)
    {
        this.particles = particles;
        currentId = 0;
    }
    public Controller(Generator gen)
    {
        particles = [.. gen.GetParticles()];
        currentId = 0;
    }
    public int getCurrentID()
    {
        return currentId;
    }
    public Particle[] getSystem()
    {
        return [.. particles];
    }
    public void Control()
    {
        // Left mouse click means add velocity 
        // right mouse click means add acceleration
        // Spacebar means toggle active
        // use the a q to change the current Properties
        if (Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            Vector2 dp = Raylib.GetMousePosition() - particles[currentId].pos;
            float a = MathF.Atan2(dp.Y, dp.X);
            particles[currentId].vel = new Vector2(MathF.Cos(a), MathF.Sin(a)) * dp.Length() / 160;
        }
        if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            Vector2 dp = Raylib.GetMousePosition() - particles[currentId].pos;
            float a = MathF.Atan2(dp.Y, dp.X);
            particles[currentId].acc = new Vector2(MathF.Cos(a), MathF.Sin(a)) * dp.Length() / 280;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Up))
        {
            currentId++;
            currentId %= particles.Count;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Down))
        {
            currentId--;
            if (currentId < 0)
            {
                currentId *= -1;
            }
            currentId %= particles.Count;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.LeftControl))
        {
            if (particles[currentId].vec_mode == "velocity")
            {
                particles[currentId].vec_mode = "acceleration";
            }
            else
            {
                particles[currentId].vec_mode = "velocity";
            }
        }
    }
}
class Graph
{
    Particle p;
    List<float> val_vels;
    List<float> val_accs;
    int index;
    float factor;
    List<string> cache;
    public Graph(Particle particle)
    {
        p = particle;
        val_accs = [];
        val_vels = [];
        cache = [];
        index = 0;
        factor = 1;
    }
    private void Arrow(Vector2 posA, Vector2 posB, Color pcolor)
    {
        float angle = MathF.Atan2(posB.Y - posA.Y, posB.X - posA.X);
        Raylib.DrawLineEx(posA, posB, 2.75f, pcolor);
        Raylib.DrawLineEx(posB, posB + 8 * new Vector2(-MathF.Cos(angle + (float)Math.PI / 4), -MathF.Sin(angle + (float)Math.PI / 4)), 2.2F, pcolor);
        Raylib.DrawLineEx(posB, posB + 8 * new Vector2(-MathF.Cos(angle - (float)Math.PI / 4), -MathF.Sin(angle - (float)Math.PI / 4)), 2.2F, pcolor);
    }
    public void Update()
    {
        float vl = p.vel.Length();
        float ac = p.acc.Length();
        val_vels.Add(vl);
        val_accs.Add(ac);
        index++;
        cache.Add($"{index}:{vl},{ac}");
        if (val_vels.Count > 415)
        {
            _ = val_vels.Remove(val_vels.First());
        }
        if (val_accs.Count > 415)
        {
            _ = val_accs.Remove(val_accs.First());
        }
    }
    public void Draw_UI()
    {
        Raylib.DrawRectangleRounded(
                new Rectangle(5, Raylib.GetScreenHeight() - 205, 450, 200),
                    0.25f, 100, Raylib.ColorFromNormalized(new(0f, 0f, 0f, 0.65f))
        );
        Raylib.DrawText($"f(t)=t*{p.vec_mode}", 55, Raylib.GetScreenHeight() - 190, 24, Color.Gold);
        Arrow(new Vector2(25, Raylib.GetScreenHeight() - 15), new Vector2(425, Raylib.GetScreenHeight() - 15), Color.White);
        Arrow(new Vector2(25, Raylib.GetScreenHeight() - 15), new Vector2(25, Raylib.GetScreenHeight() - 175), Color.White);
        if (val_accs.Count > 1 || val_vels.Count > 1)
        {
            if (p.vec_mode == "velocity")
            {
                factor = val_vels.Max() + 0.25f;
                for (int i = 1; i < val_vels.Count; i++)
                {
                    Raylib.DrawLineEx(
                        new Vector2(25 + i - 1.15f, Raylib.GetScreenHeight() - 15 - 150 / factor * val_vels[i - 1]),
                        new Vector2(25 + i, Raylib.GetScreenHeight() - 15 - 150 / factor * val_vels[i]), 3f, Color.White);
                }
            }
            else
            {
                factor = val_accs.Max() + 0.025f;
                for (int i = 1; i < val_accs.Count; i++)
                {
                    Raylib.DrawLineEx(
                    new Vector2(25 + i - 1.15f, Raylib.GetScreenHeight() - 15 - 150 / factor * val_accs[i - 1]),
                    new Vector2(25 + i, Raylib.GetScreenHeight() - 15 - 150 / factor * val_accs[i]), 3f, Color.White);
                }
            }
        }

    }
    public void ExportData(string fileOutput)
    {
        File.AppendAllLines(fileOutput, cache);
    }
}
partial class HelpMenu
{
    private bool show;
    private bool gshow;
    private Texture2D up;
    private Texture2D down;
    private Texture2D space;
    private Texture2D Lmouse;
    private Texture2D Rmouse;
    private Texture2D shift;
    private int currentId;
    public Graph[] graphs;
    private Particle[] particles;
    public Controller ctr;
    public HelpMenu(Controller ctr, Graph[] graphs)
    {
        this.ctr = ctr;
        this.graphs = graphs;
        currentId = ctr.getCurrentID();
        particles = ctr.getSystem();
        show = false;
        gshow = false;
        up = Raylib.LoadTexture("./assets/Up.png");
        down = Raylib.LoadTexture("./assets/Down.png");
        space = Raylib.LoadTexture("./assets/Space.png");
        Lmouse = Raylib.LoadTexture("./assets/Mouse_Left.png");
        Rmouse = Raylib.LoadTexture("./assets/Mouse_Right.png");
        shift = Raylib.LoadTexture("./assets/Shift.png");
    }
    public void Toggle()
    {
        show = !show;
    }
    public void ToggleG()
    {
        gshow = !gshow;
    }
    public void Draw()
    {
        currentId = ctr.getCurrentID();
        if (show)
        {
            Raylib.DrawRectangleRounded(
                new Rectangle(20, 20, Raylib.GetScreenWidth() - 40, Raylib.GetScreenHeight() - 40),
                    0.25f, 100, Raylib.ColorFromNormalized(new(0f, 0f, 0f, 0.65f))
            );
            Raylib.DrawTexture(up, 105, 60, Color.White);
            Raylib.DrawTexture(down, 205, 60, Color.White);
            Raylib.DrawText("Change Current Particle", 420, 100, 36, Color.White);
            Raylib.DrawTexture(Lmouse, 150, 260 - 50, Color.White);
            Raylib.DrawText("Modify velocity ", 420, 240, 36, Color.White);
            Raylib.DrawTexture(Rmouse, 150, 420 - 50, Color.White);
            Raylib.DrawText("Modify accelerationn", 420, 400, 36, Color.White);
            Raylib.DrawTexture(space, 150, 580 - 50, Color.White);
            Raylib.DrawText("Pause and Play", 420, 560, 36, Color.White);
        }
        else
        {
            Raylib.DrawRectangleRounded(
                new Rectangle(5, 5, 600, 140),
                    0.25f, 100, Raylib.ColorFromNormalized(new(0f, 0f, 0f, 0.65f))
            );
            Raylib.DrawText($"Particle {currentId}", 30, 20, 20, Color.White);
            Raylib.DrawText($"  |  Vector Mode : {particles[currentId].vec_mode}", 130, 20, 20, Color.White);
            Raylib.DrawText($"Particle charge : {particles[currentId].charge}", 30, 40, 20, Color.White);
            Raylib.DrawText($"Particle velocity : {particles[currentId].vel}", 30, 60, 20, Color.White);
            Raylib.DrawText($"Particle acceleration : {particles[currentId].acc}", 30, 80, 20, Color.White);
            Raylib.DrawText($"Particle position : {particles[currentId].pos}", 30, 100, 20, Color.White);
            Raylib.DrawTexture(shift, Raylib.GetScreenWidth() - 100, -15, Color.White);
            Raylib.DrawText("Help", Raylib.GetScreenWidth() - 80, 65, 28, Color.White);
            Raylib.DrawFPS(30, 120);
            if (gshow)
            {
                graphs[currentId].Draw_UI();
            }
        }
    }
}
public interface Win
{
    public void Draw(Win_Mng win);
}
class EngineWindow : Win
{
    public record Settings(int V_Density);
    public Particle p, e, e2;
    public Controller ctr;
    public Graph gp, ge1, ge2;
    public bool active;
    public HelpMenu menu;
    public EngineWindow()
    {
        p = new(new Vector2(500, 340), new Vector2(0, 0), new Vector2(0, 0), 20);
        e = new(new Vector2(260, 340), new Vector2(0, 0), new Vector2(0, 2.5f), -12);
        e2 = new(new Vector2(740, 340), new Vector2(0, 0), new Vector2(0, -2.5f), -12);
        ctr = new([p, e, e2]);
        gp = new(p);
        ge1 = new(e);
        ge2 = new(e2);
        menu = new(ctr, [gp, ge1, ge2]);
        active = false;
    }
    public void Draw(Win_Mng seer)
    {
        float dt = 90 * Raylib.GetFrameTime();
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            active = !active;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.LeftShift))
        {
            menu.Toggle();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.G))
        {
            menu.ToggleG();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.F11))
        {
            Raylib.TakeScreenshot("./img.png");
        }
        if (Raylib.IsKeyPressed(KeyboardKey.F1))
        {
            seer.C_index = 0;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.S))
        {
            active = false;
            gp.ExportData("proton.txt");
            ge1.ExportData("electron1.txt");
            ge2.ExportData("electron2.txt");
        }
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.SetMouseCursor(MouseCursor.Crosshair);

        EsField esField = new(
        (x, y) => { return p.charge * (x - p.pos.X) / MathF.Pow(MathF.Sqrt((x - p.pos.X) * (x - p.pos.X) + (y - p.pos.Y) * (y - p.pos.Y)), 3) + e.charge * (x - e.pos.X) / MathF.Pow(MathF.Sqrt((x - e.pos.X) * (x - e.pos.X) + (y - e.pos.Y) * (y - e.pos.Y)), 3) + e2.charge * (x - e2.pos.X) / MathF.Pow(MathF.Sqrt((x - e2.pos.X) * (x - e2.pos.X) + (y - e2.pos.Y) * (y - e2.pos.Y)), 3); },
        (x, y) => { return p.charge * (y - p.pos.Y) / MathF.Pow(MathF.Sqrt((x - p.pos.X) * (x - p.pos.X) + (y - p.pos.Y) * (y - p.pos.Y)), 3) + e2.charge * (y - e2.pos.Y) / MathF.Pow(MathF.Sqrt((x - e2.pos.X) * (x - e2.pos.X) + (y - e2.pos.Y) * (y - e2.pos.Y)), 3) + e.charge * (y - e.pos.Y) / MathF.Pow(MathF.Sqrt((x - e.pos.X) * (x - e.pos.X) + (y - e.pos.Y) * (y - e.pos.Y)), 3); },
        [15, Raylib.GetScreenWidth()], [15, Raylib.GetScreenHeight()], seer.P_index);
        esField.draw(new Color(215, 55, 255, 240));

        ctr.Control();
        p.Draw(active);
        e.Draw(active);
        e2.Draw(active);
        menu.Draw();
        if (active)
        {
            Parallel.Invoke(() => { gp.Update(); p.Update(dt, [e, e2]); }, () => { ge1.Update(); e.Update(dt, [p, e2]); }, () => { ge2.Update(); e2.Update(dt, [p, e]); });
        }
        Raylib.EndDrawing();
    }
}
public class StartWin : Win{
    Button start;
    Slider sld;
    public StartWin()
    {
        start = new(new(Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 160), new(250, 50), "Start", Color.White, Color.Blue);
        sld = new(new(Raylib.GetScreenWidth() / 2 - 150, Raylib.GetScreenHeight() / 2 ),new(25,100),300,150);
    }
    public void Draw(Win_Mng seer)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawCircleGradient(0, 0, Raylib.GetScreenWidth() + Raylib.GetScreenHeight() / 2, Color.SkyBlue, Color.Magenta);
        Raylib.DrawRectangleRounded(
                new Rectangle(50, 50, Raylib.GetScreenWidth() - 100, Raylib.GetScreenHeight() - 100),
                    0.075f, 100, Raylib.ColorFromNormalized(new(0f, 0f, 0f, 0.55f))
            );
        Raylib.DrawText("Electro static Simulator", Raylib.GetScreenWidth() / 2 - 300, Raylib.GetScreenHeight() / 2 - 250, 54, Color.RayWhite);
        start.Draw(true);
        sld.Update();
        Raylib.DrawText("Vector Field Spacing :", Raylib.GetScreenWidth() / 2 - 250, Raylib.GetScreenHeight() / 2 - 100, 46, Color.RayWhite);
        sld.Draw(Color.Blue,Color.RayWhite);
        Raylib.EndDrawing();
        if (start.IsClick()){
            seer.C_index = 1;
        }
        seer.P_index = (int)sld.Value();
        
    }
}
public class Win_Mng(Win[] obj)
{
    public int C_index = 0;
    public int P_index = 0;
    public Win[] wins = obj;

    public void Draw()
    {
        wins[C_index].Draw(this);
    }
}
// this is a playground for testing UI feature 17/07/2024
public class  TestUiWin :Win{
    readonly Slider sl,dl;
    readonly Button btn; 
    float num;
    public TestUiWin(){
        sl = new Slider(new(400,300),new(0,30),200,10);
        dl = new Slider(new(400,600),new(0,10),200,25);
        btn = new Button(new Vector2(400,100),new(125,55),"APPLY",Color.Blue,Color.Green);
    }
    public void Draw(Win_Mng seer){
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawFPS(10,10);
        sl.Update();
        sl.Draw(Color.Beige,Color.Gold);
        dl.Update();
        dl.Draw(Color.Beige,Color.Gold);
        btn.Draw(false);
        if (btn.IsClick()){
            num = sl.Value()+dl.Value();
        }
        Raylib.DrawText($"{num}",200,200,32,Color.Beige);
        Raylib.EndDrawing();
    }
}


partial class Program{
    public static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.ResizableWindow | ConfigFlags.MaximizedWindow);
        Raylib.InitWindow(1000, 680, "Es force sim");// math 1000x680
        StartWin Main = new();
        EngineWindow Engine = new();
        TestUiWin WN = new ();
        Win_Mng seer = new([Main,Engine]);
        while (!Raylib.WindowShouldClose())
        {
            seer.Draw();
        }
        Raylib.CloseWindow();
    }
}