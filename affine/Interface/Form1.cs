using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Render;

namespace Interface
{
    public partial class Form1 : Form
    {
        private Figure f;

        private Graphics g;

        private bool is_selected = false;

        private bool clipping = false;

        private int tab_ind;
        private Render.Point3d _prev_scale = new Point3d(1, 1, 1);

        private Point3d _prev_angle = new Point3d(0, 0, 0);

        public Form1()
        {
            InitializeComponent();
            ClearScreen();
            Figure figure = new Figure();
            double px = -5;
            double py = Math.Cos(-5);
            for (double x = -5; x < 5; x += 0.1)
            {
                double cx = x;
                double cy = Math.Sin(cx);
                //figure.Add(new Line(new Point3d(20*px, 20*py - 50, 0), new Point3d(20*cx, 20*cy -50, 0)));
                px = cx;
                py = cy;
            }
            f = figure;
            DrawFigure();

        }

        private void ClearScreen()
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.Silver);
        }

        private Figure Hexahedron()
        {
            Figure figure = new Figure();
            Point3d p1 = new Point3d(100 / 2, -100 / 2, 100 / 2);
            Point3d p2 = new Point3d(100 / 2, -100 / 2, -100 / 2);
            Point3d p3 = new Point3d(-100 / 2, -100 / 2, -100 / 2);
            Point3d p4 = new Point3d(-100 / 2, -100 / 2, 100 / 2);

            Point3d p5 = new Point3d(100 / 2, 100 / 2, 100 / 2);
            Point3d p6 = new Point3d(100 / 2, 100 / 2, -100 / 2);
            Point3d p7 = new Point3d(-100 / 2, 100 / 2, -100 / 2);
            Point3d p8 = new Point3d(-100 / 2, 100 / 2, 100 / 2);

            figure.Add(new Line(p1, p2));
            figure.Add(new Line(p2, p3));
            figure.Add(new Line(p1, p5));
            figure.Add(new Line(p2, p6));

			figure.Add(new Line(p3, p4));
			figure.Add(new Line(p4, p1));
			figure.Add(new Line(p7, p8));

			figure.Add(new Line(p5, p6));
			figure.Add(new Line(p6, p7));
			figure.Add(new Line(p8, p5));
			figure.Add(new Line(p3, p7));
			figure.Add(new Line(p4, p8));

            return figure;

        }

        private Figure Tetrahedron()
        {
            Figure figure = new Figure();
            double coef = 50 * Math.Sqrt(3);
            Point3d p1 = new Point3d(0, coef / 2, 0);
            Point3d p2 = new Point3d(-50, -coef / 2, 0);
            Point3d p3 = new Point3d(50, -coef / 2, 0);
            Point3d p4 = new Point3d(0, -coef / 2, -coef);

			figure.Add(new Line(p3, p4));
			figure.Add(new Line(p2, p4));
			figure.Add(new Line(p3, p1));
			figure.Add(new Line(p2, p3));
			figure.Add(new Line(p1, p4));
			figure.Add(new Line(p1, p2));

            return figure;
        }

        private void Displacement(double x, double y, double z)
        {
            f.Smestchenie(x, y, z);
        }

        private void Rotate(int axis, double deg)
        {
            if (axis == 0)
            {
                f.RotateX(deg);
            }
            else if (axis == 1)
            {
                f.RotateY(deg);
            }
            else if (axis == 2)
            {
                f.RotateZ(deg);
            }
            else if (axis == 3)
            {
                Point3d start = new Point3d();
                if (!Double.TryParse(textBox_customX1.Text, out start.x))
                    start.x = 0;
                if (!Double.TryParse(textBox_customY1.Text, out start.y))
                    start.y = 0;
                if (!Double.TryParse(textBox_customZ1.Text, out start.z))
                    start.z = 0;

                Point3d end = new Point3d();
                if (!Double.TryParse(textBox_customX2.Text, out end.x))
                    end.x = 0;
                if (!Double.TryParse(textBox_customY2.Text, out end.y))
                    end.y = 0;
                if (!Double.TryParse(textBox_customZ2.Text, out end.z))
                    end.z = 0;
                Line _lin = new Line(start, end);
                f.Rotate(_lin, deg);
            }
        }

        private Point ToPBPoint(Point3d p)
        {
            int c = 200;
            return new Point((int)((c * p.x / (p.z + c)) + pictureBox1.Width / 2),
                (int)((-1) * (c * p.y / (p.z + c)) + pictureBox1.Height / 2));
        }

        void DrawFigure()
        {
            ClearScreen();
            Pen pen = new Pen(Color.DarkRed);
            if (!clipping)
            {
                foreach (var edge in f.GetLines())
                {
                    g.DrawLine(pen, ToPBPoint(edge.start), ToPBPoint(edge.end));
                }
                pictureBox1.Invalidate();
            }
            else
            {
                var f2 = f.toVersion2();

                foreach (var fac in f2.Faces())
                {

                    var ugol = Point3d.Ugol(fac.Normal(), new Point3d(0, 0, 1));
                    if (Math.Abs(ugol) > (3.0 / 2))
                        continue;

                    Point3d start = fac.Points()[0];
                    for (int i = 0; i < (fac.Points().Count - 1); i++)
                    {
                        var edge = fac.Points()[i];
                        g.DrawLine(pen, ToPBPoint(edge), ToPBPoint(fac.Points()[i + 1]));
                    }
                    g.DrawLine(pen, ToPBPoint(start), ToPBPoint(fac.Points()[fac.Points().Count - 1]));
                }
                pictureBox1.Invalidate();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex == 0)
            {
                f = Tetrahedron();
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                f = Hexahedron();
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                f = Figure.Generate(((double x, double y) => { return x - y; }), -30, -30, 0, 0, 10, 10);
            }
            DrawFigure();
            is_selected = true;
        }



        private void textBox_displX_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            int num;
            if (int.TryParse(tb.Text, out num) && is_selected)
                button_move.Enabled = true;
            else
                button_move.Enabled = false;
        }

        private void button_move_Click(object sender, EventArgs e)
        {
            int x;
            int y;
            int z;
            if (!int.TryParse(textBox_displX.Text, out x))
                x = 0;
            if (!int.TryParse(textBox_displY.Text, out y))
                y = 0;
            if (!int.TryParse(textBox_displZ.Text, out z))
                z = 0;
            Displacement(x, y, z);
            DrawFigure();
        }

        private void radioButton_rotateX_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            button_rotate.Enabled = true;
            tab_ind = rb.TabIndex;
            if (rb.TabIndex == 3)
            {
                textBox_customX1.Enabled = true;
                textBox_customY1.Enabled = true;
                textBox_customZ1.Enabled = true;
                textBox_customX2.Enabled = true;
                textBox_customY2.Enabled = true;
                textBox_customZ2.Enabled = true;
            }
            else
            {
                textBox_customX1.Enabled = false;
                textBox_customY1.Enabled = false;
                textBox_customZ1.Enabled = false;
                textBox_customX2.Enabled = false;
                textBox_customY2.Enabled = false;
                textBox_customZ2.Enabled = false;
            }
        }

        private void button_rotate_Click(object sender, EventArgs e)
        {
            double angle;
            if (!double.TryParse(textBox_rotateAngle.Text, out angle))
                angle = 0;
            angle = angle * Math.PI / 180;
            Rotate(tab_ind, angle);
            DrawFigure();
        }

        private void trackBar_scaleX_Scroll(object sender, EventArgs e)
        {
            double x = trackBar_scaleX.Value / 10.0;
            double y = trackBar_scaleY.Value / 10.0;
            double z = trackBar_scaleZ.Value / 10.0;
            
			// Перед тем, как повернуть нужно обязательно вернуться к предыдущему размеру
			// Иначе изменяем размер уже измененной фигуры
			f.Scale (1 / _prev_scale.x, 1 / _prev_scale.y, 1 / _prev_scale.z);
			f.Scale(x, y, z);
			_prev_scale.x = x;
			_prev_scale.y = y;
			_prev_scale.z = z;
            DrawFigure();
        }

    }
  
}
