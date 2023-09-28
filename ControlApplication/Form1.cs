using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Threading;

namespace GyroControl
{
    public partial class GyroForm : Form
    {

        CommunicationManager comm = new CommunicationManager();
        static Gyroscope gyro = new Gyroscope();
        Boolean runcontrol;
        string InvokedMsg;
        string LastLine;
        string[] SubLines;
        static Boolean logging;
        Boolean painting;
        Boolean WithCaption;
        public static  Int32 LogTime;
        Int32 LogBeginTime;
        Boolean DataInvoked;
       static  public Rectangle RectForDraw;
        class ValueGraph
        {
            public Boolean visible;
            public Color LineColor;
            public Pen linePen;
            public int penWidth;
            public double timeScale;
            public double valueScale;
            public int GtimeOffset;
            public int GvalueOffset;
            public Dictionary<Int32, float> Values;
            public Dictionary<Int32, Point > Points;
            public Dictionary<Int32, int> GraphValues;
            public Dictionary<Int32, int> GraphTimes;
            public float BeginPointValue;
            public string Caption;
            public string Unit;
            Button ShowSettings;
            public Graphics grapher;
            CheckBox LineVisible;

            Boolean ControlPanelVisible;
            Panel ControlPanel;
            Label ColorText;
            Label TickText;
            Label ScaleText;
            Panel ColorChanger;
            NumericUpDown TickChanger;
            TextBox ScaleChanger;
            public Dictionary<Int32, float> AverageValues;
            public Dictionary<Int32, float> DefaultValues;
            public int AveragedBy=1;
            public void SetAverage(int AverageCount)
            {
                         
                
                if (AveragedBy == 1)
                {
                    DefaultValues = new Dictionary<Int32, float>();
                    DefaultValues = Values;
                }
                if  (AverageCount!=1)
                {
                    AverageValues = new Dictionary<Int32, float>();
                    AveragedBy = AverageCount;
                    float CurrValue = 0;
                    int AverValueNums = 0;
                    Int32 AverValueTime = 0;

                    foreach (Int32 key in DefaultValues.Keys)
                    {
                        CurrValue = 0;
                        AverValueNums = 0;
                        AverValueTime = 0;
                        int A = 0;
                        AverValueTime = key;
                        while ((A < AverageCount ) && (AverValueTime > 0))
                        {
                            if (DefaultValues.ContainsKey(AverValueTime))
                            {
                                CurrValue += DefaultValues[AverValueTime];
                                AverValueNums++;
                                A++;
                            }

                            AverValueTime--;
                        }
                        A = 0;
                        AverValueTime = key;
                        while ((A < AverageCount ) && (AverValueTime < LogTime))
                        {
                            if (DefaultValues.ContainsKey(AverValueTime))
                            {
                                CurrValue += DefaultValues[AverValueTime];
                                AverValueNums++;
                                A++;
                            }

                            AverValueTime++;
                        }
                        CurrValue = CurrValue / AverValueNums;
                        AverageValues.Add(key, CurrValue);
                    }
                    Values = AverageValues;
                    
                }
                else if (AverageCount == 1)
                {
                    AveragedBy = 1;
                    Values = DefaultValues;
                }
                Repoint(timeScale, valueScale, GtimeOffset, GvalueOffset);
            }
            public void AddControls(Control ParentPanel, int leftpoint, int toppoint)
            {
                //int leftpoint=200;
                //int toppoint=10;
                toppoint -= 3;
                LineVisible = new CheckBox() { Text = "", Left = leftpoint, Top = toppoint+1, AutoSize = false, Width=14,Checked = visible, Parent = ParentPanel };
                LineVisible.CheckedChanged += delegate { visible = LineVisible.Checked; GraphValues(); };
                LineVisible.BringToFront();
                ShowSettings = new Button() { Text = "...", Left = leftpoint + 14, Top = toppoint+5, BackColor=LineColor, AutoSize = false, FlatStyle = FlatStyle.Flat, Width = 22, Height = 15, Parent = ParentPanel };
                ShowSettings.BringToFront();

                //ShowSettings=_ShowSettings;
               // ShowSettings.MouseUp += ShowControlPanel;
                ShowSettings.Click += new EventHandler(this.ShowControlPanel); 
                //ShowSettings.Click += this.ShowControlPanel; 
               // LineVisible = _LineVisible;
            }
            int ClickNum;
            public void setVisible(Boolean _visible)
            {
                visible = _visible;
                LineVisible.Checked = _visible;
            }

            public void ShowControlPanel(object sender, EventArgs e)
            {
                ClickNum++;
                //if (ClickNum == 2)
                {
                    ClickNum = 0;
                    if (ControlPanelVisible)
                    {

                        this.ControlPanel.Visible = false;
                        ControlPanelVisible = false;
                        return;
                    }
                    if (!ControlPanelVisible)
                    {

                        ControlPanelVisible = true;
                        //ControlPanel.Visible = true;
                        //AngXsettpanel.Visible = !AngXsettpanel.Visible;
                        ControlPanel = new Panel();
                        ControlPanel.Name = "AngXControlPanel";
                        
                        ControlPanel.Width = 155;
                        ControlPanel.Height = 76;

                        ControlPanel.Left = ShowSettings.Right - ControlPanel.Width;
                        ControlPanel.Top = ShowSettings.Bottom;

                        ControlPanel.BorderStyle = BorderStyle.FixedSingle;
                        ControlPanel.Parent = ShowSettings.Parent;
                        ControlPanel.BringToFront();
                        int row1x=2;
                        int row2x=120;
                        int col1y=2;
                        int col2y=26;
                        int col3y=50;
                        ColorText = new Label() { Text = "Цвет линии", Top = col1y+2, Left = row1x, Parent = ControlPanel };
                        TickText = new Label() { Text = "Толщина линии", Top = col2y+2, Left = row1x, Parent = ControlPanel };
                        ScaleText = new Label() { Text = "Макс. значение", Top = col3y+2, Left = row1x, AutoSize=false, Width=115, Parent = ControlPanel };
                        ColorChanger = new Panel() {BorderStyle = BorderStyle.FixedSingle, 
                                                    BackColor=LineColor,
                                                    AutoSize=false,
                                                    Width=30,
                                                    Height=22,
                                                    Left = row2x, 
                                                    Top = col1y,
                                                    Parent = ControlPanel };
                        ColorChanger.Click += new EventHandler(ColorChangerClick);
                        TickChanger = new NumericUpDown() { Left = row2x, 
                                                            Top=col2y,
                                                            Value=penWidth,
                                                            AutoSize=false,
                                                            Size=new Size(30,22),
                                                            Padding = new Padding(0, 1, 0, 1),
                                                            TextAlign=HorizontalAlignment.Right,
                                                            Minimum=1,
                                                            Maximum=9,
                                                            Increment=1,
                                                            Parent=ControlPanel};
                        //TickChanger.ValueChanged += delegate { penWidth = int.Parse(TickChanger.Value.ToString());linePen = new Pen(LineColor, penWidth); };
                        TickChanger.ValueChanged += new EventHandler(this.TickChangerChanged); 
                       /* TickChanger = new NumericUpDown();
                        TickChanger.Name = "TicknessChanger";
                        TickChanger.Left = row2x;
                        TickChanger.Top = col2y;
                        TickChanger.Value = penWidth;
                        TickChanger.Parent = ControlPanel;
                        //GyroForm.add.Add(TickChanger);
                        ControlPanel.Controls.Add(TickChanger);*/

                        string MaxVal = ((Yposmax - Yposmin)*0.5 / valueScale).ToString();
                        ScaleChanger = new TextBox() { Left = row2x, 
                                                       Top = col3y,
                                                       Width=30,
                                                       Height=22,
                                                       TextAlign = HorizontalAlignment.Right,

                                                       Text = (((Yposmax - Yposmin) * 0.5 / valueScale).ToString()),
                                                       Parent = ControlPanel};
                        ScaleChanger.KeyUp += new KeyEventHandler(this.ScaleChangerKeyUp);
                       // ControlPanel.Parent = ShowSettings.Parent;
                       /* ShowSettings.Parent.Controls.Add(ControlPanel);
                        ControlPanel.BringToFront();*/
                       // ScaleChanger.Refresh();
                        //ColorText.Parent = ControlPanel;
                        //return;
                        
                    }
                }
            }
            public void ColorChangerClick(object sender, EventArgs e)
            {
                ColorDialog MyDialog = new ColorDialog();

                MyDialog.AllowFullOpen = false;
                MyDialog.ShowHelp = true;
                MyDialog.Color = LineColor;

                // Update the text box color if the user clicks OK 
                if (MyDialog.ShowDialog() == DialogResult.OK)
                    LineColor = MyDialog.Color;
                ColorChanger.BackColor = LineColor;
                ShowSettings.BackColor = LineColor;
                linePen = new Pen(LineColor, penWidth);
                GraphValues();

            }

            public void TickChangerChanged(object sender, EventArgs e)
            {
                penWidth = int.Parse(TickChanger.Value.ToString());
                linePen = new Pen(LineColor, penWidth);
                GraphValues();
            }

            private void ScaleChangerKeyUp(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    
                    if (String.IsNullOrEmpty(ScaleChanger.Text) || !double.TryParse(ScaleChanger.Text, out valueScale))
                    {
                        return;
                    }
                    else
                    {
                        valueScale = ((Yposmax - Yposmin) * 0.5 / valueScale);
                        this.Repoint(timeScale,valueScale,GtimeOffset,GvalueOffset);
                    }
                }
                GraphValues();
            }

    
            public void HideControlPanel(object sender, EventArgs e)
            {
                if (ControlPanelVisible)
                {
                    this.ControlPanel.Visible = false;
                    ControlPanelVisible = false;
                    ShowSettings.Click -= this.HideControlPanel;
                    ShowSettings.Click += new EventHandler(this.ShowControlPanel);
                }
            }
            public ValueGraph( Color _LineColor,int _penWidth, string _Caption,string _Unit)
            {
                visible = true;
                ControlPanelVisible = false;
                penWidth = _penWidth;
                LineColor=_LineColor;
                timeScale = 0.01F;
                valueScale = 1;
                GtimeOffset = 0;
                GvalueOffset = 0;
                Caption = _Caption;
                Unit = _Unit;
                
                linePen = new Pen(_LineColor, penWidth);
                Values = new Dictionary<Int32, float >();
                GraphValues = new Dictionary<Int32, int>();
                GraphTimes = new Dictionary<Int32, int>();
                Points = new Dictionary<Int32, Point>();
            }
            public void Clear()
            {
                timeScale = 0.01F;
                valueScale = 1;
                GtimeOffset = 0;
                GvalueOffset = 0;
                Values.Clear();
                GraphValues.Clear();
                GraphTimes.Clear();
                Points.Clear();
            }
            public void AddValue(Int32 _time,  float _value)
            {
                Values[_time] = _value;
                GraphValues[_time] = GvalueOffset + (int)(Values[_time] * valueScale);
                GraphTimes[_time] = GtimeOffset + (int)(_time * timeScale);
                Points[_time] = new Point(PaintX + ZeroX + GraphTimes[_time], PaintY + ZeroY -  GraphValues[_time]);
            }

            public void Repoint(double _timeScale, double _valueScale, int _GtimeOffset, int _GvalueOffset)  // Пересчитываем с новыми масштабами
            {
                timeScale = _timeScale;
                valueScale = _valueScale;
                GtimeOffset = _GtimeOffset;
                GvalueOffset = _GvalueOffset;
                GraphValues.Clear();
                GraphTimes.Clear();
                Points.Clear();
                foreach (Int32 key in Values.Keys)
                {
                    GraphValues[key] = GvalueOffset + (int)(Values[key] * valueScale);
                    GraphTimes[key] = GtimeOffset + (int)(key * timeScale);
                    Points[key] = new Point(PaintX + ZeroX + GraphTimes[key], PaintY + ZeroY - GraphValues[key]);
                }
            }
            public void PaintValueGraph(Int32 From,Int32 To)
            {
                if (visible)
                {
                    List< Point> PoitsForPaint=new List<Point> ();

                    //IEnumerable<Point> PoitsForPaint=Points.SkipWhile((time, index) =>index <From );
                   // 
                    if (Values.Count<3)
                    {
                        PoitsForPaint.Add(new Point(ZeroX+GtimeOffset, ZeroY+GvalueOffset));
                        PoitsForPaint.Add(new Point(ZeroX+GtimeOffset, ZeroY+GvalueOffset));
                    }
                    foreach (Int32 key in Values.Keys)
                    {
                        int addX = Points[key].X;
                        int addY = Points[key].Y;
                        if (addX <= Xposmin) addX = Xposmin;
                        if (addX >= Xposmax) addX = Xposmax;
                        if (addY <= Yposmin) addY = Yposmin;
                        if (addY >= Yposmax) addY = Yposmax;
                        //if (RectForDraw.Contains(Points[key]))
                        //if ((key >= ) && (key <= To))
                        {
                            PoitsForPaint.Add(new Point(addX,addY));

                        }
                    }
                    //TakeWhile<KeyValuePair<TKey, TValue>>(Func<KeyValuePair<TKey, TValue>, Int32, Boolean>)
                    Point[] Pointlist = PoitsForPaint.ToArray();
                    if (smoothMode)
                    graph.DrawCurve(linePen, Pointlist.ToArray(), 0.6F);
                    else
                    graph.DrawLines(linePen, Pointlist.ToArray());

                }
            }

        }


        static int vertcalDistance = 10;
        static int text_padding = 2;
        static int text_fontsize_name = 10;
        static int text_fontsize_value = 12;
        static Font nameFont = new Font("Arial", text_fontsize_name);

        static Font valueFont = new Font("Consolas", text_fontsize_value, FontStyle.Bold);
        static Font unitFont = new Font("Arial", text_fontsize_name);
        //static Dictionary<int, Rectangle> ValueBoxes;
         Rectangle[] ValueBoxes;
        class ValueBox
        {
            public Boolean blocked;
            public Boolean visible;
            public Point InsertPoint;
            public Rectangle TextRect;
            public string caption;
            public string text;
            public string unit;
            public Int32 time;
            public int gtime;
            public Color LineColor;
            public ValueGraph ParentValueGraph;
            int InsertDistance;
            public Graphics grapher;
           public Point LineBegin;

            public ValueBox(ref Graphics grapher, ref ValueGraph _ParentValueGraph, Int32 _time, Point _InsertPoint,int _InsertDistance, string _caption, string _text,string _unit)
            {
                blocked = false;
                ParentValueGraph = _ParentValueGraph;
                time = _time;
                InsertPoint = _InsertPoint;
                caption = _caption;
                text = _text;
                unit = _unit;
                InsertDistance = _InsertDistance;
                 LineBegin = new Point(0, 0);
            }



            public void PaintValueBox()
            {

                if (InsertPoint.Y >= TextRect.Y)
                    LineBegin.Y = TextRect.Y + TextRect.Height;
                else
                    LineBegin.Y = TextRect.Y;


                float oldW = ParentValueGraph.linePen.Width;
                ParentValueGraph.linePen.Width = 1;
                //LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                //LineBegin.Y=

                graph.DrawLine(ParentValueGraph.linePen, InsertPoint, LineBegin);
                //LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                graph.FillRectangle(WhitePen.Brush, TextRect);
                graph.DrawRectangle(ParentValueGraph.linePen, TextRect);
               
                graph.DrawString(caption, nameFont, thickBlackPen.Brush, TextRect.X + text_padding, TextRect.Y + text_padding);
                graph.DrawString(text, valueFont, thickBlackPen.Brush, TextRect.X + text_padding, TextRect.Y + text_padding + (int)nameFont.GetHeight());
                graph.DrawString(unit, unitFont, thickBlackPen.Brush, TextRect.X + text_padding + graph.MeasureString(text, valueFont, 200).Width, TextRect.Y + text_padding + (int)nameFont.GetHeight());
                ParentValueGraph.linePen.Width = oldW;

        }

            
        }

        class TimeLine
        {
            public Int32 time;

            public int gposX;
            public double timeScale;
            public int GtimeOffset;
            public Color linecolor;
            public Pen linepen;
            public Rectangle ControlRect;
            public Boolean visible;
            public Boolean control;
            public Boolean mouseDragmode;
            public Boolean waitForDrag;
            //public GyroGraph ParentGyroGraph;
            int vertoffset = 20;
            public TimeLine(Color _linecolor, Boolean _control)
            {
                //ParentGyroGraph = _ParentGyroGraph;
                time = 0;
                gposX=0;
                timeScale = 0.01;
                GtimeOffset = 0;
                visible=false;
                control = _control;
                mouseDragmode = false;
                waitForDrag = false;
                ControlRect = new Rectangle(0,0,0,0);
                linecolor = _linecolor;
                linepen = new Pen(new SolidBrush(_linecolor), 1);
                linepen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            }
            public void SetTimeLine(int _NewTime)
            {
                time = _NewTime;
                gposX = (int)(time * timeScale) + GtimeOffset;
                ControlRect = new Rectangle(PaintX + ZeroX + gposX - 5, Yposmax , 10, 20);
                
            }
            public void Repoint(double _timeScale, int _GtimeOffset)
            {
                timeScale = _timeScale;
                GtimeOffset = _GtimeOffset;
                gposX = (int)(time * timeScale) + GtimeOffset;
                ControlRect = new Rectangle(PaintX + ZeroX + gposX-5, Yposmax, 10, 20);

             }
            public void PaintTimeLine()
            {
                if (visible)
                {
                    linepen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    graph.DrawLine(linepen, new Point(gposX + PaintX + ZeroX, Yposmin ), new Point(gposX + PaintX + ZeroX, Yposmax + vertoffset));
                    linepen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                    if (waitForDrag)
                    {
                        graph.FillRectangle(new SolidBrush(Color.Gray), ControlRect);
                    } 
                    

                        if (mouseDragmode)
                        {
                            graph.FillRectangle(new SolidBrush(Color.GreenYellow), ControlRect);
                        }
                         if ((!waitForDrag)&&(!mouseDragmode)&&(control))
                        { 
                            graph.FillRectangle(new SolidBrush(Color.White), ControlRect);
                        }
                    
                    if (control) 
                    {
                       
                        graph.DrawRectangle(linepen, ControlRect);
                        int arrow=5;
                        int pad = 2;
                        graph.DrawLine(doubleBlackPen, ControlRect.X - pad, ControlRect.Y + ControlRect.Height / 2 - arrow, ControlRect.X - arrow - pad, ControlRect.Y + ControlRect.Height / 2);
                        graph.DrawLine(doubleBlackPen, ControlRect.X - pad, ControlRect.Y + ControlRect.Height / 2 + arrow, ControlRect.X - arrow - pad, ControlRect.Y + ControlRect.Height / 2);
                        graph.DrawLine(doubleBlackPen, ControlRect.Right + arrow + pad, ControlRect.Y + ControlRect.Height / 2, ControlRect.Right + pad, ControlRect.Y + ControlRect.Height / 2 + arrow);
                        graph.DrawLine(doubleBlackPen, ControlRect.Right + arrow + pad, ControlRect.Y + ControlRect.Height / 2, ControlRect.Right + pad, ControlRect.Y + ControlRect.Height / 2 - arrow);
                    
                    }
                    float stringSize = graph.MeasureString(((float)((float)time / 1000)).ToString(), valueFont, 200).Width + graph.MeasureString("c", unitFont, 200).Width + text_padding;

                    graph.FillRectangle(WhitePen.Brush, gposX + PaintX + ZeroX - stringSize / 2, Yposmax + vertoffset, stringSize, 22);
                    graph.DrawRectangle(thickBlackPen, gposX + PaintX + ZeroX - stringSize / 2, Yposmax + vertoffset, stringSize, 22);
                    graph.DrawString(((float)((float)time / 1000)).ToString(), valueFont, thickBlackPen.Brush, PaintX + ZeroX + gposX - stringSize / 2 + text_padding, Yposmax + vertoffset + text_padding);
                    graph.DrawString("c", unitFont, thickBlackPen.Brush, PaintX + ZeroX + gposX - stringSize / 2 + text_padding + graph.MeasureString(((float)((float)time / 1000)).ToString(), valueFont, 200).Width-1, Yposmax + vertoffset + text_padding+2);
                   // ParentValueGraph.linePen.Width = oldW;
                }
            }
        }
        class GyroGraph
        {
            public float TimeScale;
            public float ValueScale;
            public Int32 TimeMin;
            public Int32 TimeMax;
            public Int32 GtimeOffset;
            public Int32 GvalueOffset;
            //public ValueGraph TimeBox;
           // public ValueGraph Ang0;
            public ValueGraph AngX;
            public ValueGraph AngY;
            public ValueGraph AngAccX;
            public ValueGraph AngAccY;
           // public ValueGraph AccX;
            //public ValueGraph AccY;
           // public ValueGraph AccZ;
            //public ValueGraph Dist;
            public ValueGraph Speed;
           // public ValueGraph Acc00;
            public ValueGraph Acc01;
           public ValueGraph Acc02;
           public ValueGraph Acc03;

           public ValueGraph AngXB;
           public ValueGraph AngYB;
           
           public ValueGraph Acc01B;
           public ValueGraph Acc02B;
           public ValueGraph Acc03B;
           public ValueGraph SpeedB;
           public ValueGraph DistB;

            public ValueBox[] ValueBoxes;
            public TimeLine[] TimeLines;
            public Dictionary<Int32, int> TimePoints;
            public Int32 BeginPointTime=0;
            public bool NewBeginPoint=false;
            public bool IsBeginPoint=false;

            public Graphics grapher;
            public GyroGraph()
            {
                grapher = graph;
                TimeScale = 0.01F;
                ValueScale = 1;
                TimeMin = 0;
                TimeMax = 90000;
                GtimeOffset = 0;
                
                //GvalueOffset = 0;
                TimeLines = new TimeLine[4];
                TimeLines[0] = new TimeLine(Color.Black,false);
                TimeLines[1] = new TimeLine(Color.Black, false);
                TimeLines[2] = new TimeLine(Color.Black, true);
                TimeLines[3] = new TimeLine(Color.Black, true);
                TimePoints = new Dictionary<Int32, int>();
                RectForDraw = new Rectangle(0, 0, 0, 0);
                //TimeBox = new ValueGraph(Color.Black, 1, "Время", "сек");
               // TimeBox.valueScale = 0;
               // TimeBox.GvalueOffset = -303;
              //  Ang0 = new ValueGraph(Color.Red, 2, "Угол от исх.", "°");
                AngX = new ValueGraph( Color.LightSkyBlue, 1, "Угол X", "°");
                AngY = new ValueGraph( Color.LightCoral, 1, "Угол Y", "°");
                AngAccX = new ValueGraph(Color.Gold, 1, "Угл. ускорение X", "°/c²");
                AngAccY = new ValueGraph(Color.Indigo, 1, "Угл. ускорение Y", "°/c²");
               // AccX= new ValueGraph(Color.Red, 1, "Ускорение X", "g");
                //AccY= new ValueGraph(Color.Green, 1, "Ускорение Y", "g");
                //AccZ= new ValueGraph(Color.Blue, 1, "Ускорение Z", "g");
                //Dist = new ValueGraph(Color.SpringGreen, 2, "Дистанция", "км");
                Speed = new ValueGraph( Color.Orchid, 2, "Скорость", "км/ч");
                Acc01 = new ValueGraph(Color.Green, 3, "Ускорение X (ф)", "g");
                Acc02 = new ValueGraph(Color.Red, 1, "Ускорение Y (ф)", "g");
                Acc03 = new ValueGraph(Color.Blue, 1, "Ускорение Z (ф)", "g");

                AngXB = new ValueGraph(Color.LightSkyBlue, 1, "Угол X (расч)", "°");
                AngYB = new ValueGraph(Color.LightCoral, 1, "Угол Y (расч)", "°");
                Acc01B = new ValueGraph(Color.Green, 3, "Ускорение X (расч)", "g");
                Acc02B = new ValueGraph(Color.Red, 1, "Ускорение Y (расч)", "g");
                Acc03B = new ValueGraph(Color.Blue, 1, "Ускорение Z (расч)", "g");
DistB = new ValueGraph(Color.SpringGreen, 2, "Дистанция (расч)", "км");
                SpeedB = new ValueGraph( Color.Orchid, 2, "Скорость (расч)", "км/ч");
                //Acc02 = new ValueGraph( Color.Cyan, 2, "Ускорение X+Y (ф)", "g");
                //Acc03 = new ValueGraph( Color.RoyalBlue, 2, "Ускорение X cos Y (ф)", "g");
            }
            public void Clear()
            {
                TimeScale = 0.01F;
                ValueScale = 1;
                TimeMin = 0;
                TimeMax = 90000;
                GvalueOffset = 0;
                GtimeOffset = 0;
                TimePoints.Clear();
                //Ang0.Clear();
                AngX.Clear();
                AngY.Clear();
                AngAccX.Clear();
                AngAccY.Clear();
               // AccX.Clear();
                //AccY.Clear();
                //AccZ.Clear();
                //Dist.Clear();
                Speed.Clear();
                //Acc00.Clear();
                Acc01.Clear();
                Acc02.Clear();
                Acc03.Clear();
                if (IsBeginPoint)
                {
                    AngXB.Clear();
                    AngYB.Clear();
                    Acc01B.Clear();
                    Acc02B.Clear();
                    Acc03B.Clear();
                    SpeedB.Clear();
                    DistB.Clear();
                }
                ValueBoxes = new ValueBox[0];

            }

            public Int32 FindTimeByGraph(int _posX)
            {
                //GraphTimes[_time] = GtimeOffset + (int)(_time * timeScale);
                //Int32 _time = (_posX - GtimeOffset) / TimeScale;

                Int32 _time = (Int32)((_posX - PaintX - ZeroX - GtimeOffset) / (double)TimeScale) ;
                //if (_time0) _time=0;
                if (_time>LogTime) _time=LogTime;
                return FindTime(_time);

            }
             public Int32 FindTime(Int32 _time)
            {
                if (!TimePoints.ContainsKey(_time))

                    while (!TimePoints.ContainsKey(_time))
                    {
                        _time++;
                        if (_time >= 900000) return -1;
                    }
                return _time;
            }

             public void AddGraphs(Int32 _time, float _angleX, float _angleY, float _angAccX, float _angAccY,float _accX,float _accY, float _accZ, float _dist, float _speed, float _acc00, float _acc01, float _acc02, float _acc03)
            {
                if (!TimePoints.ContainsKey(_time))
                {
                    TimePoints.Add(_time, (int)(TimeScale * _time) + GtimeOffset);

                    //TimeBox.AddValue(_time, (float)_time / 1000);
                    //Ang0.AddValue(_time, _angle);
                    AngX.AddValue(_time, _angleX);
                    AngY.AddValue(_time, _angleY);
                    AngAccX.AddValue(_time, _angAccX);
                    AngAccY.AddValue(_time, _angAccY);
                    //AccX.AddValue(_time, _accX);
                  //  AccY.AddValue(_time, _accY);
                  //  AccZ.AddValue(_time, _accZ);
                    AngAccY.AddValue(_time, _angAccY);
                  //  Dist.AddValue(_time, _dist);
                    Speed.AddValue(_time, _speed);
                    //Acc00.AddValue(_time, _acc00);
                    Acc01.AddValue(_time, _acc01);
                    Acc02.AddValue(_time, _acc02);
                    Acc03.AddValue(_time, _acc03);
                }

            }
             public void Repoint()  // Пересчитываем с новыми масштабами
             {
                //TimeBox.Repoint( TimeScale,  TimeBox.valueScale,  GtimeOffset,  TimeBox.GvalueOffset) ;

                /*Ang0.Repoint(TimeScale, Ang0.valueScale, GtimeOffset, Ang0.GvalueOffset + GvalueOffset);
                AngX.Repoint(TimeScale, AngX.valueScale, GtimeOffset, AngX.GvalueOffset + GvalueOffset);
                AngY.Repoint(TimeScale, AngY.valueScale, GtimeOffset, AngY.GvalueOffset + GvalueOffset);
                Dist.Repoint(TimeScale, Dist.valueScale, GtimeOffset, Dist.GvalueOffset + GvalueOffset);
                Speed.Repoint(TimeScale, Speed.valueScale, GtimeOffset, Speed.GvalueOffset + GvalueOffset);
                Acc00.Repoint(TimeScale, Acc00.valueScale, GtimeOffset, Acc00.GvalueOffset + GvalueOffset);
                Acc01.Repoint(TimeScale, Acc01.valueScale, GtimeOffset, Acc01.GvalueOffset + GvalueOffset);
                Acc02.Repoint(TimeScale, Acc02.valueScale, GtimeOffset, Acc02.GvalueOffset + GvalueOffset);
                Acc03.Repoint(TimeScale, Acc03.valueScale, GtimeOffset, Acc03.GvalueOffset + GvalueOffset);*/
               // TimeBox.Repoint(TimeScale, TimeBox.valueScale , GtimeOffset, TimeBox.GvalueOffset);

                //Ang0.Repoint(TimeScale, Ang0.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                AngX.Repoint(TimeScale, AngX.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                AngY.Repoint(TimeScale, AngY.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                AngAccX.Repoint(TimeScale, AngAccX.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                AngAccY.Repoint(TimeScale, AngAccY.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                //AccX.Repoint(TimeScale, AccX.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                //AccY.Repoint(TimeScale, AccY.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                //AccZ.Repoint(TimeScale, AccZ.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                //Dist.Repoint(TimeScale, Dist.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                Speed.Repoint(TimeScale, Speed.valueScale * ValueScale, GtimeOffset, GvalueOffset);
               // Acc00.Repoint(TimeScale, Acc00.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                Acc01.Repoint(TimeScale, Acc01.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                Acc02.Repoint(TimeScale, Acc02.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                Acc03.Repoint(TimeScale, Acc03.valueScale * ValueScale, GtimeOffset, GvalueOffset);

                if (IsBeginPoint)
                {
                    AngXB.Repoint(TimeScale, AngXB.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                    AngYB.Repoint(TimeScale, AngYB.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                    SpeedB.Repoint(TimeScale, SpeedB.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                    DistB.Repoint(TimeScale, DistB.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                    Acc01B.Repoint(TimeScale, Acc01.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                    Acc02B.Repoint(TimeScale, Acc02.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                    Acc03B.Repoint(TimeScale, Acc03.valueScale * ValueScale, GtimeOffset, GvalueOffset);
                }
                 for (int tl = 0; tl < TimeLines.Length; tl++)
                 {
                     TimeLines[tl].Repoint(TimeScale, GtimeOffset);

                 }
            
             }
             public void CreateTimeLines()
             {
                 if (logging)
                 {
                     TimeLines[0].visible = true;
                     //TimeLines[0].SetTimeLine((int)((LogTime) * TimeScale) + GtimeOffset);
                     TimeLines[0].SetTimeLine(LogTime);
                     /*{
                     if CursorPosX
                     }*/

                 }
                 else TimeLines[0].visible = false;

                 if ((CursorPosX != -1)&&(!controlmode)&&(ShowCursorline))
                 {
                     //CursorTime = gyro.FindTimestamp(CursorTime);

                     Int32 CursorTime = FindTimeByGraph(CursorPosX);
                     if ((CursorTime > 0) && (CursorTime < LogTime))
                     {
                         TimeLines[1].visible = true;
                         TimeLines[1].SetTimeLine(CursorTime);

                     }
                     else TimeLines[1].visible = false;
                 }
                 else TimeLines[1].visible = false;

                  if (timeStamp1 != -1)
                  {
                      TimeLines[2].visible = true;
                      // grapher.DrawLine(thickGrayPen, timeStamp1++);

                      TimeLines[2].SetTimeLine(timeStamp1time);
                  }
                  else TimeLines[2].visible = false;

                 if (timeStamp2 != -1)
                 {
                     TimeLines[3].visible = true;
                     TimeLines[3].SetTimeLine(timeStamp2time);
                 }
                 else TimeLines[3].visible = false;
                 
             }

             public void PaintTimeLines()
             {
                 for (int tl=0; tl<TimeLines.Length;tl++)
                 {
                     TimeLines[tl].PaintTimeLine();
                 }
                 
                
             }

             public void PaintZoomRect()
             {
             }
            public void UpdateValueBoxes()
            {
                ValueBoxes=new ValueBox[0];
                if (logging)
                {
                    AddValueBoxes(LogTime, 30);
                }
                if (timeStamp1 != -1)
                {
                    //Int32 timeStamp1time=(Int32)((timeStamp1 )/ timeScale - GtimeOffset);
                   // if ((timeStamp1time > 0) && (timeStamp1time < LogTime))
                    AddValueBoxes(timeStamp1time, -30);
                }
                if (timeStamp2 != -1)
                {
                    //Int32 timeStamp2time = (Int32)((timeStamp2) / timeScale - GtimeOffset);
                   // if ((timeStamp2time > 0) && (timeStamp2time < LogTime))
                    //if (timeStamp1<100)
                    AddValueBoxes(timeStamp2time, 30);
                }
                if ((CursorPosX != -1)&&(!controlmode)&&(ShowCursorline))
                {
                    //CursorTime = gyro.FindTimestamp(CursorTime);
                    //Int32 CursorTime = (Int32)((CursorPosX - PaintX - ZeroX - GtimeOffset)/ timeScale );
                    Int32 CursorTime =FindTimeByGraph(CursorPosX);
                    if ((CursorTime > 0) && (CursorTime < LogTime))
                    {
                        AddValueBoxes(CursorTime, -30);

                    }
                }
            }
            public void AddValueBoxes(int CTime, int insertDistance)
            {
                
                //AddValueBox(ref TimeBox, CTime, insertDistance);
                AddValueBox(ref AngX, CTime, insertDistance);
                AddValueBox(ref AngY, CTime, insertDistance);
                AddValueBox(ref AngAccX, CTime, insertDistance);
                AddValueBox(ref AngAccY, CTime, insertDistance);
                //AddValueBox(ref AccX, CTime, insertDistance);
               // AddValueBox(ref AccY, CTime, insertDistance);
                //AddValueBox(ref AccZ, CTime, insertDistance);
                //AddValueBox(ref Dist, CTime, insertDistance);
                AddValueBox(ref Speed, CTime, insertDistance);
                //AddValueBox(ref Acc00, CTime, insertDistance);
                AddValueBox(ref Acc01, CTime, insertDistance);

                AddValueBox(ref Acc02, CTime, insertDistance);
                AddValueBox(ref Acc03, CTime, insertDistance);


                if (IsBeginPoint)
                {
                    AddValueBox(ref AngXB, CTime, insertDistance);
                    AddValueBox(ref AngYB, CTime, insertDistance);
                    AddValueBox(ref Acc01B, CTime, insertDistance);
                    AddValueBox(ref Acc02B, CTime, insertDistance);
                    AddValueBox(ref Acc03B, CTime, insertDistance);
                    AddValueBox(ref DistB, CTime, insertDistance);
                    AddValueBox(ref SpeedB, CTime, insertDistance);
                    
                }
            }

            public void PaintAxes()
            {
                
                Rectangle PanelRect = new Rectangle(0, 0,  CanvasWidth, CanvasHeight);
                graph.FillRectangle(new SolidBrush(System.Drawing.SystemColors.Control), PanelRect);
                Rectangle GraphWindow = new Rectangle(Xposmin, Yposmin, Xposmax - Xposmin, Yposmax - Yposmin);
                graph.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255)), GraphWindow);
                graph.DrawRectangle(thickBlackPen, GraphWindow);
                int t = 0;
                while ( t < 270 )
                {
                    if (((PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset) >= Xposmin) && ((PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset) <= Xposmax))
                    {
                        if (t % 5 == 0)
                        {
                            if (t!=0)
                                graph.DrawLine(doubleAxePen, new Point((PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset), PaintY + CanvasMargin), new Point((PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset), PaintY + CanvasHeight- 2*CanvasMargin-LineForTime));
                            graph.DrawString(t.ToString(), unitFont, thickBlackPen.Brush, (PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset) - graph.MeasureString(t.ToString(), valueFont, 100).Width / 2 + 3, Yposmax);
                            //graph.DrawString(text, valueFont, thickBlackPen.Brush, TextRect.X + text_padding, TextRect.Y + text_padding + (int)nameFont.GetHeight());
                            //graph.DrawString(unit, unitFont, thickBlackPen.Brush, TextRect.X + text_padding + graph.MeasureString(text, valueFont, 200).Width, TextRect.Y + text_padding + (int)nameFont.GetHeight());
               
                        }
                        else
                        {
                            if (TimeScale >= 0.015)
                            {
                                graph.DrawLine(singleAxePen, new Point((PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset), PaintY + CanvasMargin), new Point((PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset), PaintY + CanvasHeight - 2 * CanvasMargin - LineForTime));
                            }
                            if (graph.MeasureString("99", unitFont, 100).Width  < (int)(800 * TimeScale))
                            {
                                graph.DrawString(t.ToString(), unitFont, thickBlackPen.Brush, (PaintX + ZeroX + (int)((t * 1000) * TimeScale) + GtimeOffset) - graph.MeasureString(t.ToString(), unitFont, 100).Width / 2 , Yposmax);
                           
                            }
                        }
                        
                    }
                    t++;
                }
                if (((PaintX + ZeroX+GtimeOffset)>=Xposmin)&&((PaintX + ZeroX+GtimeOffset)<=Xposmax))
                {
                    graph.DrawLine(doubleBlackPen, new Point(PaintX + ZeroX + GtimeOffset, Yposmin), new Point(PaintX + ZeroX + GtimeOffset, Yposmax));
                }
                graph.DrawLine(doubleBlackPen, new Point(Xposmin, PaintY + ZeroY - GvalueOffset), new Point(Xposmax, PaintY + ZeroY - GvalueOffset));

            }
            Int32 CurrSeachPoint=0;
            Int32 CurrTime = 0;
            public void SeachBeginningPoint()
            {
                //BeginPointTime = FindTime(1000);
                CurrSeachPoint += 500;

                CurrTime = CurrSeachPoint;
                for (int i = 0; i < 10; i++)
                {
                    CurrTime = FindTime(CurrTime + 1);
                }
                if (!Acc01.Values.ContainsKey(CurrTime))
                    CurrSeachPoint = FindTime(0);       //Возвращаемся на начало
                else
                CurrSeachPoint = FindTime(CurrSeachPoint);
           
                NewBeginPoint = false;
                while (!NewBeginPoint)          // Пока точка не найдена
                {
                    //CurrSeachPoint = FindTime(CurrTime);
                    CurrTime = CurrSeachPoint;
                    float[] vals = new float[10];
                    for (int i = 0; i < 10; i++)
                    {
                        CurrTime=FindTime(CurrTime+1);
                        if (!Acc01.Values.ContainsKey(CurrTime))
                        {
                            break;
                        }
                        vals[i] = Acc01.Values[CurrTime];
                    }
                    if ((vals.Max() - vals.Min()) <= 0.03 * vals.Max())
                    {
                        BeginPointTime = CurrSeachPoint;
                        NewBeginPoint = true;
                        
                    }
                    else
                    {
                        CurrSeachPoint = FindTime(CurrSeachPoint+1);
                        CurrTime = CurrSeachPoint;
                    }
                }

            }
            Pen BeginPointPen = new Pen(Brushes.Red, 2);
            public void PaintBeginningPoint()
            {
                if (NewBeginPoint)
                {
                    int InsertPointX = PaintX + ZeroX + (int)(timeScale * (BeginPointTime)) + GtimeOffset;

                    int InsertPointY = PaintY + ZeroY - (int)(Acc01.Values[BeginPointTime] * Acc01.valueScale) - GvalueOffset;
                    int radius=8;
                    
                    graph.FillEllipse(Brushes.LightPink, InsertPointX - radius, InsertPointY - radius, 2 * radius, 2 * radius);
               graph.DrawEllipse(BeginPointPen, InsertPointX - radius, InsertPointY - radius, 2* radius, 2* radius); 
                }
            }

            public void RecalculateForBeginPoint()
            {
                AngX.BeginPointValue = AngX.Values[BeginPointTime];
                AngY.BeginPointValue = AngY.Values[BeginPointTime];
                //AngZ.BeginPointValue = AngZ.Values[BeginPointTime];

                Acc01.BeginPointValue = Acc01.Values[BeginPointTime];
                Acc02.BeginPointValue = Acc02.Values[BeginPointTime];
                Acc03.BeginPointValue = Acc03.Values[BeginPointTime];
                AngXB.Clear();
                AngYB.Clear();
                Acc01B.Clear();
                Acc02B.Clear();
                Acc03B.Clear();

                foreach (Int32 key in Acc01.Values.Keys)
                {
                    AngXB.AddValue(key,(AngX.Values[key] - AngX.BeginPointValue));
                    AngYB.AddValue(key, (AngY.Values[key] - AngY.BeginPointValue));
                    Acc01B.AddValue(key, (float)((Acc01.Values[key] - 1 * Math.Sin(AngY.Values[key] * 0.0175) )));
                    
                    //Acc01B.AddValue(key, (Acc01.Values[key] - Acc01.BeginPointValue));
                    //Acc02B.AddValue(key, (Acc02.Values[key] - Acc02.BeginPointValue));
                }
                Acc01B.SetAverage(10);
                IsBeginPoint = true;
            }
            public void PaintValueGraphs()
            {
                PaintAxes();
               PaintBeginningPoint();

                if (TimePoints.Count > 0)
                {
                 
                        // TimeBox.PaintValueGraph(TimeMin, TimeMax);
                        //Ang0.PaintValueGraph(TimeMin, TimeMax);
                        AngX.PaintValueGraph(TimeMin, TimeMax);
                        AngY.PaintValueGraph(TimeMin, TimeMax);
                        AngAccX.PaintValueGraph(TimeMin, TimeMax);
                        AngAccY.PaintValueGraph(TimeMin, TimeMax);
                        // AccX.PaintValueGraph(TimeMin, TimeMax);
                        //  AccY.PaintValueGraph(TimeMin, TimeMax);
                        // AccZ.PaintValueGraph(TimeMin, TimeMax);
                        // Dist.PaintValueGraph(TimeMin, TimeMax);
                        Speed.PaintValueGraph(TimeMin, TimeMax);
                        //  Acc00.PaintValueGraph(TimeMin, TimeMax);
                        Acc01.PaintValueGraph(TimeMin, TimeMax);
                        Acc02.PaintValueGraph(TimeMin, TimeMax);
                        Acc03.PaintValueGraph(TimeMin, TimeMax);
                    
                    if (IsBeginPoint)
                    {

                        AngXB.PaintValueGraph(TimeMin, TimeMax);
                        AngYB.PaintValueGraph(TimeMin, TimeMax);
                        Acc01B.PaintValueGraph(TimeMin, TimeMax);
                        Acc02B.PaintValueGraph(TimeMin, TimeMax);
                        Acc03B.PaintValueGraph(TimeMin, TimeMax);
                        SpeedB.PaintValueGraph(TimeMin, TimeMax);
                        DistB.PaintValueGraph(TimeMin, TimeMax);
      
                    }
                    UpdateValueBoxes();
                    PaintValueBoxes();
                }
                CreateTimeLines();
                PaintTimeLines();
                if (mouseZoomRectMode)
                {
                    graph.DrawRectangle(thickBlackPen, mouseZoomRect);
                }

            }
            public void PaintValueBoxes()
            {
                for (int curr = 0; curr < ValueBoxes.Length; curr++)// Счетчик положения первого прямоугольника в списке 
                {
                    if (ValueBoxes[curr].ParentValueGraph.visible)
                    {
                        ValueBoxes[curr].PaintValueBox(); 
                    }
                }
            }
            public void AddValueBox( ref ValueGraph ParentValueGraph, Int32 timestamp, int InsertDistance)
            {
                // int CreateValueBox(Graphics grapher,Point InsertPoint, string text1, string text2, int InsertDistance)
                
                if (!ParentValueGraph.Values.ContainsKey(timestamp))
                {
                    while (!ParentValueGraph.Values.ContainsKey(timestamp)) timestamp++;
                }



                string caption=ParentValueGraph.Caption;
                string text = ParentValueGraph.Values[timestamp].ToString();
                string unit = ParentValueGraph.Unit;

                Rectangle textrect = new Rectangle(0, 0, 0, 0);
                int InsertPointX = PaintX + ZeroX + (int)(ParentValueGraph.timeScale * (timestamp)) + ParentValueGraph.GtimeOffset;
                int InsertPointY=PaintY + ZeroY- (int)(ParentValueGraph.valueScale*(ParentValueGraph.Values[timestamp]))-ParentValueGraph.GvalueOffset;

                if (InsertPointY+(int)nameFont.GetHeight() + (int)valueFont.GetHeight() + 2 * text_padding > Yposmax) InsertPointY = Yposmax;
                if (InsertPointY < Yposmin) InsertPointY = Yposmin;

                float stringSize = Math.Max(graph.MeasureString(caption, nameFont, 200).Width, graph.MeasureString(text, valueFont, 200).Width + graph.MeasureString(unit, unitFont, 200).Width);
                if ((InsertPointX + InsertDistance - (int)stringSize - text_padding) < Xposmin ) InsertDistance = Math.Abs(InsertDistance);
                else
                    if ((InsertPointX + InsertDistance + (int)stringSize + text_padding) >= Xposmax) InsertDistance = -Math.Abs(InsertDistance);

                ValueBox newBox = new ValueBox(ref grapher, ref ParentValueGraph, timestamp, new Point(InsertPointX, InsertPointY), InsertDistance,caption, text, unit);
                newBox.caption = caption;
                newBox.text = text;
                newBox.unit = unit;
                //newBox.InsertPoint.X = InsertPointX;
                //newBox.InsertPoint.Y = InsertPointY;



                if (InsertDistance >= 0)
                {
                    int Xpoint = InsertPointX + InsertDistance;
                    int Ypoint = InsertPointY - vertcalDistance;
                    textrect = new Rectangle(Xpoint, Ypoint, (int)stringSize + text_padding, (int)nameFont.GetHeight() + (int)valueFont.GetHeight() + 2 * text_padding);
                    newBox.LineBegin.X = textrect.X;
                }
                else
                {
                    int Xpoint = InsertPointX + InsertDistance - (int)stringSize - text_padding;
                    int Ypoint = InsertPointY - vertcalDistance;
                    textrect = new Rectangle(Xpoint, Ypoint, (int)stringSize + text_padding, (int)nameFont.GetHeight() + (int)valueFont.GetHeight() + 2 * text_padding);
                    newBox.LineBegin.X = textrect.X + textrect.Width;
                }
                 newBox.TextRect = textrect;

                if (InsertPointY >= newBox.TextRect.Y)
                        newBox.LineBegin.Y = newBox.TextRect.Y + newBox.TextRect.Height; 
                else 
                        newBox.LineBegin.Y = newBox.TextRect.Y;
               
                Array.Resize(ref ValueBoxes, ValueBoxes.Length + 1);
                ValueBoxes[ValueBoxes.Length - 1] = newBox;
                OffsetIntersectedBoxes(ref ValueBoxes, ref ValueBoxes[ValueBoxes.Length - 1], 10);
                //return (ValueBoxes.Length - 1);
            }
            void OffsetIntersectedBoxes(ref ValueBox[] _ValueBoxes, ref ValueBox currValueBox, int Ydist)
            {
                /*if (currValueBox.TextRect.Y < Yposmin)
                {
                    currValueBox.TextRect.Y = Yposmin;
                    currValueBox.blocked = true;
                    OffsetIntersectedBoxes(ref _ValueBoxes, ref currValueBox, Math.Abs(Ydist));
                }
                else*/
                if (currValueBox.TextRect.Y + currValueBox.TextRect.Height > Yposmax)
                {
                    currValueBox.TextRect.Y = Yposmax - currValueBox.TextRect.Height;
                    currValueBox.blocked = true;
                    OffsetIntersectedBoxes(ref _ValueBoxes, ref currValueBox, -Math.Abs(Ydist));
                }
                else
                {
                    for (int d = 0; d < _ValueBoxes.Length; d++)
                    {
                        if ((currValueBox.TextRect != _ValueBoxes[d].TextRect) && (_ValueBoxes[d].ParentValueGraph.visible))
                        {

                            if (CheckRectangle(currValueBox.TextRect, _ValueBoxes[d].TextRect))// Если текущий пересекается с перебираемым значением
                            {
                                //if (_ValueBoxes[d].blocked)
                                {
                                    
                                }
                               // else
                                {
                                    while (CheckRectangle(currValueBox.TextRect, _ValueBoxes[d].TextRect))
                                    {
                                        //currRect.Offset(0, Ydist);
                                        _ValueBoxes[d].TextRect.Offset(0, Ydist);
                                    }
                                    //for (int offseted = 0; offseted < RectArray.Length; offseted++)
                                    // {
                                    OffsetIntersectedBoxes(ref _ValueBoxes, ref _ValueBoxes[d], Ydist);
                                    //}
                                }
                            }
                        }

                        //OffsetIntersectedBoxes(ref RectArray, ref ValueBoxes[d], Ydist);
                        // RectArray[d].Offset(300, 0);
                    }
                }

            }
            Boolean CheckRectangle(Rectangle rect1, Rectangle rect2)
            {
                if (((rect1.IntersectsWith(rect2)) || (rect1.Contains(rect2)) || (rect2.Contains(rect1))) || (rect1 == rect2) || (rect1.Location == rect2.Location)) return true; else return false;
            }
        }

        /*class GyroValue
        {
            public Int32 time;
            public float dist;
            public float angle;
            public float speed;
            public float acc00;
            public float acc01;
            public float acc02;
            public float acc03;
            public int gtime;
            public Point anglePoint;
            public Point speedPoint;
            public Point acc00Point;
            public Point acc01Point;
            public Point acc02Point;
            public Point acc03Point;
            
            public GyroValue(Int32 Ntime, float Nangle, float Ndist, float Nspeed, float Nacc00, float Nacc01, float Nacc02,float Nacc03)
            {
                angle = Nangle;
                dist = Ndist;
                speed = Nspeed;
                acc00 = Nacc00;
                acc01 = Nacc01;
                acc02 = Nacc02;
                acc03 = Nacc03;
                time = Ntime;
                gtime = (int)(Ntime * timeScale);

                anglePoint = new Point(PaintX + ZeroX + gtime, PaintY + ZeroY - (int)(angle * ang0Scale));
                speedPoint = new Point(PaintX + ZeroX + gtime, PaintY + ZeroY - (int)(speed * speedScale));
                acc00Point = new Point(PaintX + ZeroX + gtime, PaintY + ZeroY - (int)(acc00 * acc0Scale));
                acc01Point = new Point(PaintX + ZeroX + gtime, PaintY + ZeroY - (int)(acc01 * acc0Scale));
                acc02Point = new Point(PaintX + ZeroX + gtime, PaintY + ZeroY - (int)(acc02 * acc0Scale));
                acc03Point = new Point(PaintX + ZeroX + gtime, PaintY + ZeroY - (int)(acc03 * acc0Scale));
            }
        }*/
       /* class GyroValue
        {
            public float angle;
            public float dist;
            public float speed;
            public float acc01;
            public float acc02;

        }*/
        class Gyroscope
        {
            public Int32 time;
            public int gtime;
            public int angleX;
            public int angleY;
            public int angleZ;
            public float angAccX;
           public float  angAccY;
            public float accX;
            public float accY;
            public float accZ;
            public float speed0;
            public float dist0;

            public float angle0;
            public float acc00;
            public float acc01;
            public float acc02;
            public float acc03;

            public GyroGraph Graphs;
           // public Dictionary<Int32, GyroValue> GyroValues = new Dictionary<Int32, GyroValue>();
            public Gyroscope()
            {
                Graphs = new GyroGraph();
                ClearGyro();
            }
            public void ClearGyro()
            {
                angleX = 0;
                angleY=0;
                angleZ=0;
                angle0 = 0;
                angAccX=0;
            angAccY=0;
                //accX=0;
                //accY=0;
               // accZ=0;
                //acc00 = 0;
                acc01 = 0;
                acc02 = 0;
                acc03 = 0;
                speed0=0;
                dist0 = 0;
                Graphs.Clear();
               // GyroValues.Clear();
            }
           /*public  int FindTimestamp(int timestamp)
            {
                if (!GyroValues.ContainsKey(timestamp))
                {
                    while (!GyroValues.ContainsKey(timestamp)) timestamp++;
                }
                return timestamp;
            }*/
            public void UpdateGyro()
            {
               Graphs.AddGraphs(time, angleX, angleY,angAccX,angAccY ,accX,accY,accZ, dist0, speed0, acc00, acc01, acc02, acc03);
               gtime = (int)(LogTime * Graphs.TimeScale) + Graphs.GtimeOffset;
               /* gpTime = time;
               int timestamp = gtime;
               if (GyroValues.ContainsKey(timestamp)) 
                {
                    GyroValues[timestamp] = new GyroValue(time, angle0, dist0, speed0, acc00, acc01, acc02,acc03);
                }
                else
                {
                    GyroValues.Add(timestamp, new GyroValue(time, angle0, dist0, speed0, acc00, acc01, acc02, acc03));
                }
                Ang0Array.Add(GyroValues[timestamp].anglePoint);
                Speed0Array.Add(GyroValues[timestamp].speedPoint);
                Acc00Array.Add(GyroValues[timestamp].acc00Point);
                Acc01Array.Add(GyroValues[timestamp].acc01Point);
                Acc02Array.Add(GyroValues[timestamp].acc02Point);
                Acc03Array.Add(GyroValues[timestamp].acc03Point);*/
            }


        }
        public void UpdateLabels()
        {
            times.Text = String.Format("{0:0.0}", (float)gyro.time / 1000);
            angX.Text = String.Format("{0:0}", gyro.angleX);
            angY.Text = String.Format("{0:0}", gyro.angleY);
            angAccXlabel.Text = String.Format("{0:0.00}", gyro.angAccX);
            angAccYlabel.Text = String.Format("{0:0.00}", gyro.angAccY);

           // accX.Text = String.Format("{0:0.000}", gyro.accX);
           // accY.Text = String.Format("{0:0.000}", gyro.accY);
          //  accZ.Text = String.Format("{0:0.000}", gyro.accZ);

           // angle0.Text = String.Format("{0:0.0}", gyro.angle0);
            
            Speed0.Text = String.Format("{0:0.00}", gyro.speed0);
            Speedometr.Text = String.Format("{0:0.00}", gyro.speed0);
          //  Dist0.Text = String.Format("{0:0.000}", gyro.dist0);

            //acc00.Text = String.Format("{0:0.000}", gyro.acc00);
            acc01.Text = String.Format("{0:0.000}", gyro.acc01);
            acc02.Text = String.Format("{0:0.000}", gyro.acc02);
            acc03.Text = String.Format("{0:0.000}", gyro.acc03);
        }
        BufferedGraphicsContext currentContext;
        static BufferedGraphics myBuffer;

        public GyroForm()
        {
           
            InitializeComponent();
            DataInvoked = false;
        

        }

        private void GyroForm_Load(object sender, EventArgs e)
        {
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(this.panelGraph.CreateGraphics(),
               this.panelGraph.DisplayRectangle);
            //
            graph = myBuffer.Graphics;
            Gyroscope gyro=new Gyroscope(); 
            PaintY = 0;
            PaintX = 0;
            CanvasWidth = panelGraph.Width;
            CanvasHeight = panelGraph.Height;
            LineForTime = 50;
            CanvasMargin = 5;
            ZeroY = (CanvasHeight - LineForTime) / 2;
            ZeroX = CanvasMargin+5;

            Yposmin = CanvasMargin;
            Yposmax = (CanvasHeight - LineForTime) - 2 * CanvasMargin;

            Xposmin = ZeroX - CanvasMargin;
            Xposmax = CanvasWidth - 2 * CanvasMargin;

            
                       
             
            GraphInit(); 
            comm.parentForm = this;
            
            RecreateCOMbuttons();

            
            SetStadardScales();
            logging = false;
            painting = true;
            WithCaption = false;
            runcontrol = true;
            

            this.MouseWheel += new MouseEventHandler(panelGraph_MouseWheel);
            panelGraph.Select();
           // gyro.Graphs.Repoint();
            ClearGraphic();
            GraphValues();
            //comm.SensorLog = SensorLog;
            
        }
       // private void panel1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
         void panelGraph_MouseWheel(object sender, MouseEventArgs e)
        {
            int numberOfMove = Math.Abs(e.Delta/120) ;

            if (e.Delta > 0)
            {
              /*  gyro.Graphs.TimeScale *= (float)(Math.Pow(1.1,numberOfMove));
                gyro.Graphs.ValueScale = (float)(Math.Pow(1.1, numberOfMove));*/
                float leftline = (float)gyro.Graphs.GtimeOffset / gyro.Graphs.TimeScale;
                gyro.Graphs.TimeScale *= (float)(Math.Pow(1.05, numberOfMove));
                gyro.Graphs.ValueScale = (float)(Math.Pow(1.05, numberOfMove));
                float newleftline = (float)gyro.Graphs.GtimeOffset / gyro.Graphs.TimeScale;

                gyro.Graphs.GtimeOffset += (int)((leftline-newleftline) * gyro.Graphs.TimeScale);
                gyro.Graphs.Repoint();
                gyro.Graphs.ValueScale = 1;
            }
            else if (e.Delta < 0)
            {
                //float ios = (float)(Math.Pow(0.9, numberOfMove));
                float leftline = (float)gyro.Graphs.GtimeOffset / gyro.Graphs.TimeScale;
                gyro.Graphs.TimeScale *= (float)(Math.Pow(0.95, numberOfMove));
                gyro.Graphs.ValueScale = (float)(Math.Pow(0.95, numberOfMove));
                float newleftline = (float)gyro.Graphs.GtimeOffset / gyro.Graphs.TimeScale;
                gyro.Graphs.GtimeOffset += (int)((leftline - newleftline) * gyro.Graphs.TimeScale);
                gyro.Graphs.Repoint();
                gyro.Graphs.ValueScale = 1;
            }
            //gyro.Graphs.Repoint();
           GraphValues();
        }
        static Graphics graph;
        static int CanvasWidth = 0;
        static int CanvasHeight = 0;
        static int CanvasMargin = 5;
        static int LineForTime = 100;
        static int Yposmin = 0;
        static int Yposmax = 0;
        static int Xposmin = 0;
        static int Xposmax = 0;
        static int ZeroY = 0;
        static int ZeroX = 0;
          static int PaintY = -800;
          static int PaintX = 100;
        int gTime;
        static int gpTime;
            double accXScale;
            double accYScale;
            double accZScale;
            static double acc0Scale;
            double angXScale;
            double angYScale;
            static double ang0Scale;
            static double speedScale;
            double accXmax;
            double accYmax;
            double accZmax;
            double acc0max;
            double angXmax;
            double angYmax;
            double ang0max;
            double speedMax;
            double distMax;
            double distScale;
            double angAccMax;
            double angAccScale;
            static double timeScale;

            Point[] AngXArray;
            Point[] AngYArray;
            static List<Point> Ang0Array;
            static List<Point> Speed0Array;
            Point[] AccXArray;
            Point[] AccYArray;
            Point[] AccZArray;
            static List<Point> Acc00Array;
            static List<Point> Acc01Array;
            static List<Point> Acc02Array;
            static List<Point> Acc03Array;
    
        public static void UpdateGraphArray(ref Point[] pointArray,float value,double scale)
        {
            
            Array.Resize(ref pointArray, pointArray.Length + 1);
            pointArray[pointArray.Length - 1] = new Point(PaintX + ZeroX + (int)(gpTime * timeScale), PaintY + ZeroY - (int)(value * scale));

        }
        public void PaintGraphArray( Graphics grapher,Pen linePen, ref Point[] pointArray)
        {
            //graph.DrawCurve(linePen, pointArray, 1F);
            graph.DrawLines(linePen, pointArray);
        }

        public void PaintGraphArray(Graphics grapher, Pen linePen, ref List<Point> pointList)
        {
            Point[] Listarr=pointList.ToArray();
            graph.DrawCurve(linePen, pointList.ToArray(), 0.5F);
        }

        static Pen thickBlackPen = new Pen(Brushes.Black, 1);
        static Pen doubleBlackPen = new Pen(Brushes.Black, 2);

        static Pen thickGrayPen = new Pen(new SolidBrush(Color.FromArgb(250, 250, 250)), 1);
        static Pen doubleGrayPen = new Pen(new SolidBrush(Color.FromArgb(250, 250, 250)), 1);

        static Pen singleAxePen = new Pen(new SolidBrush(Color.FromArgb(200, 200, 200)), 1);
        static Pen doubleAxePen = new Pen(new SolidBrush(Color.FromArgb(200, 200, 200)), 1);

        static Pen ts1Pen = new Pen(new SolidBrush(Color.FromArgb(65, 105, 225)), 1);
        static Pen ts2Pen = new Pen(new SolidBrush(Color.FromArgb(50, 205, 50)), 1);


        static Pen WhitePen = new Pen(new SolidBrush(Color.White), 1);
        static Pen accXPen = new Pen(new SolidBrush(Color.FromArgb(200, 0, 0)), 2);
        static Pen accYPen = new Pen(new SolidBrush(Color.FromArgb(0, 200, 0)), 2);
        static Pen accZPen = new Pen(new SolidBrush(Color.FromArgb(0, 0, 200)), 2);
        static Pen angXPen = new Pen(new SolidBrush(Color.Orange), 2);
        static Pen angYPen = new Pen(new SolidBrush(Color.Indigo), 2);
        static Pen angAccXPen = new Pen(new SolidBrush(Color.Aqua), 1);
        static Pen angAccYPen = new Pen(new SolidBrush(Color.PaleTurquoise), 1);

        static Pen acc00Pen = new Pen(new SolidBrush(Color.Blue), 1);
        static Pen acc01Pen = new Pen(new SolidBrush(Color.Red), 2);
        static Pen acc02Pen = new Pen(new SolidBrush(Color.Purple), 2);
        static Pen acc03Pen = new Pen(new SolidBrush(Color.FromArgb(50, 205, 50)), 2);

        static Pen speed0Pen = new Pen(new SolidBrush(Color.Lime), 2);

        static Pen currTimePen = new Pen(Color.FromArgb(200, 200, 200), 1);
        
    


         int CreateValueBox(Graphics grapher,Point InsertPoint, string text1, string text2, int InsertDistance)
         {
             Rectangle textrect = new Rectangle(0, 0, 0, 0);
             float stringSize = Math.Max(graph.MeasureString(text1, nameFont, 200).Width, graph.MeasureString(text2, valueFont, 200).Width);
             if (InsertDistance >= 0)
             {
                 int Xpoint = InsertPoint.X + InsertDistance;
                 int Ypoint = InsertPoint.Y - vertcalDistance;
                 textrect = new Rectangle(Xpoint, Ypoint, (int)stringSize + text_padding, (int)nameFont.GetHeight() + (int)valueFont.GetHeight() + 2 * text_padding);
             }
             else
             {
                 int Xpoint = InsertPoint.X + InsertDistance - (int)stringSize - text_padding;
                 int Ypoint = InsertPoint.Y - vertcalDistance;
                 textrect = new Rectangle(Xpoint, Ypoint, (int)stringSize + text_padding, (int)nameFont.GetHeight() + (int)valueFont.GetHeight() + 2 * text_padding);
             }
             //OffsetIntersectedBoxes(ref ValueBoxes, ref textrect, 10);
             Array.Resize(ref ValueBoxes, ValueBoxes.Length + 1);
             ValueBoxes[ValueBoxes.Length - 1] = textrect;
             return (ValueBoxes.Length - 1);
         }

         void ShowValueBox( int boxnum,Graphics grapher, Point InsertPoint, Pen LinePen, string text1, string text2, int InsertDistance)
        {
   
            float oldW = LinePen.Width;
            LinePen.Width = 1;
            //LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            Point LineBegin =new Point(0,0);
            if (InsertPoint.Y >= ValueBoxes[boxnum].Y) LineBegin.Y = ValueBoxes[boxnum].Y + ValueBoxes[boxnum].Height; else LineBegin.Y = ValueBoxes[boxnum].Y;
            if (InsertDistance >= 0) LineBegin.X = ValueBoxes[boxnum].X; else LineBegin.X = ValueBoxes[boxnum].X + ValueBoxes[boxnum].Width;
            grapher.DrawLine(LinePen, InsertPoint, LineBegin);
            //LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

            grapher.FillRectangle(WhitePen.Brush, ValueBoxes[boxnum]);
            grapher.DrawRectangle(LinePen, ValueBoxes[boxnum]);
            grapher.DrawString(text1, nameFont, thickBlackPen.Brush, ValueBoxes[boxnum].X + text_padding, ValueBoxes[boxnum].Y + text_padding);
            grapher.DrawString(text2, valueFont, thickBlackPen.Brush, ValueBoxes[boxnum].X + text_padding, ValueBoxes[boxnum].Y + text_padding + (int)nameFont.GetHeight());
                LinePen.Width = oldW;

        }
        void RecheckValueBoxes(int Yposmin,int Yposmax)
        {
           
            for (int fcount = 0; fcount < ValueBoxes.Length; fcount++)// Счетчик положения первого прямоугольника в списке 
            {     
                 if (ValueBoxes[fcount].Y <= Yposmin)     // Верхняя граница
                 {
                     ValueBoxes[fcount].Y = Yposmin;    // Устанавливаем к верхней границе
                      for (int scount = 0; scount < ValueBoxes.Length; scount++)// Счетчик положения второго прямоугольника в списке
                      {
                      if (ValueBoxes[fcount] != ValueBoxes[scount])       // Не сравниваем с самим собой
                       {
                           ValueBoxes[scount].Y +=10;   // Сдвигаем вниз
                             for (int third = 0; third < ValueBoxes.Length; third++)// Счетчик положения второго прямоугольника в списке
                             {
                                 }

                                    //ValueBoxes[fcount].Y = Yposmin;


                                }
                            }
                }
            }
        }
       


        void PaintGraphAxes(Graphics grapher)
        {

            Rectangle GraphWindow = new Rectangle(PaintX, PaintY, CanvasWidth - 1, CanvasHeight - 1);
            grapher.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255)), GraphWindow);
            grapher.DrawRectangle(thickBlackPen, GraphWindow);
            grapher.DrawLine(thickBlackPen, new Point(PaintX + ZeroX, PaintY + CanvasMargin), new Point(PaintX + ZeroX, PaintY + CanvasHeight - CanvasMargin));
            grapher.DrawLine(thickBlackPen, new Point(PaintX + ZeroX - 10, PaintY + ZeroY), new Point(PaintX + CanvasWidth - CanvasMargin, PaintY + ZeroY));

        }
        static int timeStamp1 = -1;
        static int timeStamp2 = -1;
                static Int32 timeStamp1time;
                static Int32 timeStamp2time;

            public void UpdateTimeStampLog()
        {
               /* string Log="";
                if ((painting)&&(LogTime!=0))
                {
                    if (timeStamp1 != -1)
                    {
                        // timeStamp1time = gyro.Graphs.FindTimeByGraph(timeStamp1);

                        Log += "Начальная позиция:\r\n";
                        Log += "  Время    \t" + (gyro.Graphs.TimePoints[timeStamp1time] * 0.001).ToString() + " сек\r\n";
                        Log += "  Дистанция\t" + gyro.Graphs.Dist.Values[timeStamp1time].ToString() + " км\r\n";
                        Log += "  Скорость\t" + gyro.Graphs.Speed.Values[timeStamp1time].ToString() + " км/ч\r\n";
                        Log += "  Ускорение\t" + gyro.Graphs.Speed.Values[timeStamp1time].ToString() + " g\r\n";
                    }
                    if (timeStamp2 != -1)
                    {
                        // timeStamp2time = gyro.Graphs.FindTimeByGraph(timeStamp2);
                        Log += "Начальная позиция:\r\n";
                        Log += "  Время    \t" + (gyro.Graphs.TimePoints[timeStamp2time] * 0.001).ToString() + " сек\r\n";
                        Log += "  Дистанция\t" + gyro.Graphs.Dist.Values[timeStamp2time].ToString() + " км\r\n";
                        Log += "  Скорость\t" + gyro.Graphs.Speed.Values[timeStamp2time].ToString() + " км/ч\r\n";
                        Log += "  Ускорение\t" + gyro.Graphs.Acc02.Values[timeStamp2time].ToString() + " g\r\n";
                    }
                    if ((timeStamp1 != -1) && (timeStamp2 != -1))
                    {
                        Log += "\r\nРазница:\r\n";
                        Log += "  Время   \t" + ((gyro.Graphs.TimePoints[timeStamp2time] - gyro.Graphs.TimePoints[timeStamp1time]) * 0.001).ToString() + " сек\r\n";
                        Log += "  Дистанция\t" + (gyro.Graphs.Dist.Values[timeStamp2time] - gyro.Graphs.Dist.Values[timeStamp1time]).ToString() + " км\r\n";
                        Log += "  Скорость\t" + (gyro.Graphs.Speed.Values[timeStamp2time] - gyro.Graphs.Speed.Values[timeStamp1time]).ToString() + " км/ч\r\n";
                        Log += "  Ускорение\t" + (gyro.Graphs.Acc02.Values[timeStamp2time] - gyro.Graphs.Acc02.Values[timeStamp1time]).ToString() + " g\r\n";
                    }
                    TimeStampsLog.Text = Log;
                }*/
        }
         void CreateTimelines(Graphics grapher)
            {
            }
       /* void PaintTimelines(Graphics grapher)
        {
        
            ValueBoxes = new Rectangle[1];
            ValueBoxes[0]= new Rectangle(0, 0, 0, 0);
            if (logging) PaintTimeLine(grapher, (int)(LogTime * timeScale), currTimePen, 10);
            int Off1 = 10;
            int Off2 = -10;
            if (timeStamp1 < timeStamp2) { Off1 = -10; Off2 = 10; } else { Off1 = 10; Off2 = -10; }
            if ((timeStamp1 != -1)&&(timeStamp2 == -1)) Off1=-10;
            if (timeStamp1 != -1)
            {
                timeStamp1 = gyro.FindTimestamp(timeStamp1);

                PaintTimeLine(grapher, timeStamp1, ts1Pen, Off1);
            }
            if (timeStamp2 != -1)
            {
                timeStamp2 = gyro.FindTimestamp(timeStamp2);
                PaintTimeLine(grapher, timeStamp2, ts2Pen, Off2);
            }

            //if (CursorPosX != -1)
            {
                int CursorTime = CursorPosX - PaintX - ZeroX;
                if ((CursorTime > 0) && (CursorTime < gyro.gtime))
                {
                    CursorTime = gyro.FindTimestamp(CursorTime);
                    PaintTimeLine(grapher, CursorTime, currTimePen, -10);
                }
            }
        }
        */
        /*void CreateTimeLine(Graphics grapher, int lineTime, Pen linePen, int Xoffset)
        {
            int timestamp = (int)(lineTime);
            if (!gyro.GyroValues.ContainsKey(timestamp))
            {
                while (!gyro.GyroValues.ContainsKey(timestamp)) timestamp++;
            }
            linePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            grapher.DrawLine(linePen, new Point(PaintX + ZeroX + (int)(lineTime), PaintY + CanvasMargin), new Point(PaintX + ZeroX + (int)(lineTime), PaintY + CanvasHeight - CanvasMargin));
            int box1 = CreateValueBox(grapher, new Point(timestamp + ZeroX + PaintX, CanvasHeight - CanvasMargin), "время", (gyro.GyroValues[timestamp].time * 0.001).ToString() + "сек", Xoffset);

            int box2 = CreateValueBox(grapher, gyro.GyroValues[timestamp].anglePoint, "угол", gyro.GyroValues[timestamp].angle.ToString() + "°", Xoffset);
            int box3 = CreateValueBox(grapher, gyro.GyroValues[timestamp].speedPoint, "скорость", gyro.GyroValues[timestamp].speed.ToString() + " км/ч", Xoffset);
            int box4 = CreateValueBox(grapher, gyro.GyroValues[timestamp].acc01Point, "ускорение", gyro.GyroValues[timestamp].acc01.ToString() + " g", Xoffset);
            int box5 = CreateValueBox(grapher, gyro.GyroValues[timestamp].acc02Point, "ускорение (ф)", gyro.GyroValues[timestamp].acc02.ToString() + " g", Xoffset);
        }

        void PaintTimeLine(Graphics grapher,int lineTime, Pen linePen,int Xoffset)
        {
            int timestamp=(int)(lineTime );
            if (!gyro.GyroValues.ContainsKey(timestamp))
            {
                while (!gyro.GyroValues.ContainsKey(timestamp)) timestamp++;
            }
            linePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            grapher.DrawLine(linePen, new Point(PaintX + ZeroX + (int)(lineTime ), PaintY + CanvasMargin), new Point(PaintX + ZeroX + (int)(lineTime ), PaintY + CanvasHeight - CanvasMargin));
            int box1=CreateValueBox(grapher, new Point(timestamp + ZeroX + PaintX, CanvasHeight - CanvasMargin-30), "время", (gyro.GyroValues[timestamp].time * 0.001).ToString() + "сек", Xoffset);

            int box2 = CreateValueBox(grapher, gyro.GyroValues[timestamp].anglePoint, "угол", gyro.GyroValues[timestamp].angle.ToString() + "°", Xoffset);
            int box3 = CreateValueBox(grapher, gyro.GyroValues[timestamp].speedPoint, "скорость", gyro.GyroValues[timestamp].speed.ToString() + " км/ч", Xoffset);
            int box4 = CreateValueBox(grapher, gyro.GyroValues[timestamp].acc00Point, "ускорение x", gyro.GyroValues[timestamp].acc00.ToString() + " g", Xoffset);
            int box5 =CreateValueBox(grapher, gyro.GyroValues[timestamp].acc01Point, "ускорение x(ф)", gyro.GyroValues[timestamp].acc01.ToString() + " g", Xoffset);
            int box6 = CreateValueBox(grapher, gyro.GyroValues[timestamp].acc02Point, "ускорение x+y(ф)", gyro.GyroValues[timestamp].acc02.ToString() + " g", Xoffset);
            int box7 = CreateValueBox(grapher, gyro.GyroValues[timestamp].acc03Point, "ускорение x cos(y)(ф)", gyro.GyroValues[timestamp].acc03.ToString() + " g", Xoffset);

            
            ShowValueBox(box1,grapher, new Point(timestamp + ZeroX + PaintX, CanvasHeight - CanvasMargin), thickBlackPen, "время", (gyro.GyroValues[timestamp].time*0.001 ).ToString() + "сек", Xoffset);

            ShowValueBox(box2,grapher, gyro.GyroValues[timestamp].anglePoint, angYPen, "угол", gyro.GyroValues[timestamp].angle.ToString() + "°", Xoffset);
            ShowValueBox(box3,grapher, gyro.GyroValues[timestamp].speedPoint, speed0Pen, "скорость", gyro.GyroValues[timestamp].speed.ToString() + " км/ч", Xoffset);
            ShowValueBox(box4,grapher, gyro.GyroValues[timestamp].acc00Point, acc00Pen, "ускорение x", gyro.GyroValues[timestamp].acc00.ToString() + " g", Xoffset);
            ShowValueBox(box5,grapher, gyro.GyroValues[timestamp].acc01Point, acc01Pen, "ускорение x(ф)", gyro.GyroValues[timestamp].acc01.ToString() + " g", Xoffset);
            ShowValueBox(box6, grapher, gyro.GyroValues[timestamp].acc02Point, acc02Pen, "ускорение x+y(ф)", gyro.GyroValues[timestamp].acc02.ToString() + " g", Xoffset);
            ShowValueBox(box7, grapher, gyro.GyroValues[timestamp].acc03Point, acc03Pen, "ускорение x cos(y)(ф)", gyro.GyroValues[timestamp].acc03.ToString() + " g", Xoffset);
        }

        */
        static public void GraphValues()
        {

           // if (painting)
            {
               // gyro.Graphs.grapher = graph;
                
                //PaintGraphAxes(graph);
                //PaintTimelines(graph);

                GyroForm.gyro.Graphs.Repoint();
                
                gyro.Graphs.PaintValueGraphs();
               
                Rectangle GraphWindow = new Rectangle(Xposmin, Yposmin, Xposmax - Xposmin, Yposmax - Yposmin);
                graph.DrawRectangle(thickBlackPen, GraphWindow);
                myBuffer.Render();
            }


        }
        public void SetStadardScales()
        {

           /* if (opened)
            {
                gyro.Graphs.Ang0.valueScale = ang0Scale/3.55;
                gyro.Graphs.AngX.valueScale = ang0Scale / 3.55;
                gyro.Graphs.AngY.valueScale = ang0Scale / 3.55;
                gyro.Graphs.Dist.valueScale = distScale / 3.55;
                gyro.Graphs.Speed.valueScale = speedScale / 3.55;
                gyro.Graphs.Acc00.valueScale = acc0Scale / 3.55;
                gyro.Graphs.Acc01.valueScale = acc0Scale / 3.55;
                gyro.Graphs.Acc02.valueScale = acc0Scale / 3.55;
                gyro.Graphs.Acc03.valueScale = acc0Scale / 3.55;
            }
            else*/
            {
                accXmax = 1;
                accYmax = 1;
                accZmax = 1;
                acc0max = 1;

                angXmax = 90;
                angYmax = 90;
                ang0max = 90;
                speedMax = 10;
                distMax = 30;
                 angAccMax=360;
                 angAccScale = (Yposmax-Yposmin) / angAccMax/2; ;

                 accXScale = (Yposmax - Yposmin) / accXmax/2;
                 accYScale = (Yposmax - Yposmin) /accYmax/2;
                 accZScale = (Yposmax - Yposmin) / accZmax/2;

                 acc0Scale = (Yposmax - Yposmin) / acc0max/2;
                 speedScale = (Yposmax - Yposmin) / speedMax/2;

                 angXScale = (Yposmax - Yposmin) / angXmax/2;
                 angYScale = (Yposmax - Yposmin) / angYmax/2;
                 ang0Scale = (Yposmax - Yposmin) / ang0max/2;
                distScale = (Yposmax - Yposmin) / distMax/2;

                //gyro.Graphs.Ang0.valueScale = ang0Scale;
                gyro.Graphs.AngX.valueScale = ang0Scale;
                gyro.Graphs.AngY.valueScale = ang0Scale;
                gyro.Graphs.AngAccX.valueScale = angAccScale;
                gyro.Graphs.AngAccY.valueScale = angAccScale;
                //gyro.Graphs.AccX.valueScale = acc0Scale;
              //  gyro.Graphs.AccY.valueScale = acc0Scale;
                //gyro.Graphs.AccZ.valueScale = acc0Scale;
                //gyro.Graphs.Dist.valueScale = distScale;
                gyro.Graphs.Speed.valueScale = speedScale;
               // gyro.Graphs.Acc00.valueScale = acc0Scale;
                gyro.Graphs.Acc01.valueScale = acc0Scale;
                gyro.Graphs.Acc02.valueScale = acc0Scale;
                gyro.Graphs.Acc03.valueScale = acc0Scale;

                gyro.Graphs.AngXB.valueScale = ang0Scale;
                gyro.Graphs.AngYB.valueScale = ang0Scale;
                gyro.Graphs.SpeedB.valueScale = speedScale;
                gyro.Graphs.DistB.valueScale = distScale;
                gyro.Graphs.Acc01B.valueScale = acc0Scale;
                gyro.Graphs.Acc02B.valueScale = acc0Scale;
                gyro.Graphs.Acc03B.valueScale = acc0Scale;
            }

        }
        public void GraphInit()
        {
           
            SetStadardScales();

            currTimePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            singleAxePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            doubleAxePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            int leftcontrol = 186;
            gyro.Graphs.AngX.AddControls(groupData, leftcontrol, angX.Top);
            gyro.Graphs.AngY.AddControls(groupData, leftcontrol, angY.Top);

            gyro.Graphs.AngAccX.AddControls(groupData, leftcontrol, angAccXlabel.Top);
            gyro.Graphs.AngAccY.AddControls(groupData, leftcontrol, angAccYlabel.Top);


           // gyro.Graphs.AccX.AddControls(groupData, leftcontrol, accX.Top);
           // gyro.Graphs.AccX.setVisible(false);
         //   gyro.Graphs.AccY.AddControls(groupData, leftcontrol, accY.Top);
          //  gyro.Graphs.AccY.setVisible(false);
          //  gyro.Graphs.AccZ.AddControls(groupData, leftcontrol, accZ.Top);
          //  gyro.Graphs.AccZ.setVisible(false);
           // gyro.Graphs.Ang0.AddControls(groupData, leftcontrol, angle0.Top);
         //   gyro.Graphs.Dist.AddControls(groupData, leftcontrol, Dist0.Top);
         //   gyro.Graphs.Dist.setVisible(false);
            gyro.Graphs.Speed.AddControls(groupData, leftcontrol, Speed0.Top);
            gyro.Graphs.Speed.setVisible(false);
           // gyro.Graphs.Acc00.AddControls(groupData, leftcontrol, acc02.Top);
            gyro.Graphs.Acc01.AddControls(groupData, leftcontrol, acc01.Top);
            gyro.Graphs.Acc02.AddControls(groupData, leftcontrol, acc02.Top);
           gyro.Graphs.Acc03.AddControls(groupData, leftcontrol, acc03.Top);

            timeScale = 0.01; // 100 мс на один пиксель по горизонтали
            RectForDraw = new Rectangle(0, 0, 0, 0);
          RectForDraw = Rectangle.FromLTRB(Xposmin,Yposmin,Xposmax,Yposmax);
           // gyro.Graphs.Repoint();
            




        }

        public void ClearGraphic()
        {
            LogView.Clear();
            gpTime = 0;
            LogTime = 0;
            timeStamp1 = -1;
            timeStamp2 = -1;
            timeStamp1time = 0;
            timeStamp2time = 0;
            gyro.ClearGyro();
            GraphInit();
            GraphValues();

        }

        public void InvokedMessage()      // Обарабатываем входящие из последовательного порта данные  
        {

            //InvokedMsg = msg;
            InvokedMsg = COMLog.Text;

            string[] stringSeparators = new string[] { "\n" };
            string[] sensorMsg = InvokedMsg.Split(stringSeparators, StringSplitOptions.None);
            if (sensorMsg[sensorMsg.Length - 1].Length > 14)
                LastLine = sensorMsg[sensorMsg.Length - 1];
            else
                LastLine = sensorMsg[sensorMsg.Length - 2];
            SubLines = LastLine.Split(';');
            if ((SubLines != null) && (SubLines.Length == 10))
            {
               
                if (!DataInvoked)
                {
                    DataInvoked = true;
                    btLogRun.Visible = true;
                    //btReset.Visible = true;
                    groupForCombt.Visible = false;
                    groupData.Top = groupCOM.Top + 38;
                    btReloadComList.Visible = true;
                    groupCOM.Visible = false;
                    opened = false;
                    //groupData.Top = 75;
                }

                if (String.IsNullOrEmpty(SubLines[0]) || !Int32.TryParse(SubLines[0], out gyro.time)) ;
                if (String.IsNullOrEmpty(SubLines[1]) || !int.TryParse(SubLines[1], out gyro.angleX)) ;
                if (String.IsNullOrEmpty(SubLines[2]) || !int.TryParse(SubLines[2], out gyro.angleY)) ;
                if (String.IsNullOrEmpty(SubLines[3]) || !float.TryParse(SubLines[3], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.angAccX)) ;
                if (String.IsNullOrEmpty(SubLines[4]) || !float.TryParse(SubLines[4], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.angAccY)) ;
              //  if (String.IsNullOrEmpty(SubLines[5]) || !float.TryParse(SubLines[5], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.accX)) ;
                /*if (String.IsNullOrEmpty(SubLines[6]) || !float.TryParse(SubLines[6], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.accY)) ;
                if (String.IsNullOrEmpty(SubLines[7]) || !float.TryParse(SubLines[7], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.accZ)) ;*/

               
               //if (String.IsNullOrEmpty(SubLines[8]) || !float.TryParse(SubLines[8], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.dist0)) ;
               if (String.IsNullOrEmpty(SubLines[5]) || !float.TryParse(SubLines[5], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.speed0)) ;
               if (String.IsNullOrEmpty(SubLines[6]) || !float.TryParse(SubLines[6], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc01)) ;
               if (String.IsNullOrEmpty(SubLines[7]) || !float.TryParse(SubLines[7], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc02)) ;
               if (String.IsNullOrEmpty(SubLines[8]) || !float.TryParse(SubLines[8], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc03)) ;
                //if (String.IsNullOrEmpty(SubLines[12]) || !float.TryParse(SubLines[12], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc02)) ;
                //if (String.IsNullOrEmpty(SubLines[13]) || !float.TryParse(SubLines[13], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc03)) ;
                //if (gyro.acc01 < 0) gyro.acc02 = -gyro.acc02;
                if (SettingZero)
                {
                    if (gyro.acc01 != 0)
                    {
                        labelZeroed.Text = "OK";
                        btZero.Text = "Установить в исходное ";
                    }
                }
               
                UpdateLabels();
                if (logging)
                {

                    gyro.time = gyro.time - LogBeginTime;
                    LogTime = gyro.time;
                    //gyro.time = LogTime;
                    //gpTime = LogTime;

                    gyro.UpdateGyro();
                    LogTimer.Text = String.Format("{0:0.0}", (float)LogTime / 1000);
                    string separator = ";\t";
                    if (!WithCaption)
                         //LogView.AppendText(LogTime + separator+ gyro.angleX + separator  + gyro.angleY + separator +  gyro.accX + separator + gyro.accY + separator  + gyro.accZ + "\r\n");

                        LogView.AppendText(LogTime + separator + 
                                            gyro.angleX + separator + 
                                            gyro.angleY + separator + 
                                            gyro.angAccX + separator + 
                                            gyro.angAccY + separator + 
                                            
                                            gyro.speed0 + separator + 
                                            gyro.acc01 + separator +
                                            gyro.acc02 + separator +
                                            gyro.acc03 + "\r\n");
                    else
                        //LogView.AppendText("time=" + LogTime + separator + "angleX=" + gyro.angleX + separator + "angleY=" + gyro.angleY + separator + "accX=" + gyro.accX + separator + "accY=" + gyro.accY + separator + "accZ=" + gyro.accZ + "\r\n");
                       // LogView.AppendText("time=" + LogTime + separator + "angle=" + gyro.angle0 + separator + "dist=" + gyro.dist0 + separator + "speed=" + gyro.speed0 + separator + "acc x=" + gyro.acc00 + separator + "acc x(Σ)=" + gyro.acc01 + separator + "acc x+y(Σ)=" + gyro.acc02 +   separator +"acc x cos y(Σ)=" + gyro.acc03 +  "\r\n");
                        LogView.AppendText(LogTime + separator +
                                            gyro.angleX + separator +
                                            gyro.angleY + separator +
                                            gyro.angAccX + separator +
                                            gyro.angAccY + separator +

                                            gyro.speed0 + separator +
                                            gyro.acc01 + separator +
                                            gyro.acc02 + separator +
                                            gyro.acc03 + "\r\n");
                    //LogView.AppendText("\n");
                    LogView.ScrollToCaret();
                    
                    GraphValues();
                    if (LogTime >= 180000) LogStop();
                }
                COMLog.Clear();
            }
           
        }
        static string[] portlist =new string[0]; 
        public void RecreateCOMbuttons()
        {
            int btwidth = 85;
            int btheight = 32;
            int insX=3;
            int insY=0;
            portlist=new string[0];
            groupForCombt.Controls.Clear();
            groupForCombt.Visible = true;
            comm.SetPortNameValues(ref portlist);
            groupCOM.Height = (portlist.Length / 2+1) * (btheight + 5)+15;
            groupForCombt.Height = groupCOM.Height-20;
            groupData.Top = groupCOM.Top + groupCOM.Height;

            if (portlist.Length == 0)
            {
                Label NoComsAviable = new Label() { Left = 2, Top = 6, AutoSize=false,Width=150, Text = "Нет доступных портов",  Parent = groupForCombt };

            }
            else
            for (int p=0;p<portlist.Length;p++)
            {
                if (p >= 2) { insX = 3; insY += btheight ; }
                Button OpenComButton = new Button() { Name = "btCOM" + p, AutoSize = false, Width = btwidth, Height = btheight, Top = insY, Left = insX, Text = portlist[p], Parent = groupForCombt };
                OpenComButton.Image = global::GyroControl.Properties.Resources.com_plug24x24;
                OpenComButton.ImageAlign = ContentAlignment.MiddleLeft;
                OpenComButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                OpenComButton.TextAlign = ContentAlignment.MiddleLeft;
                OpenComButton.Click += new EventHandler(btCOMopen_Click);
                groupForCombt.Controls.Add(OpenComButton);
                insX+=btwidth;
            }
            
        }


        private void btCOMopen_Click(object sender, EventArgs e)
        {
            comm.PortName = (sender as Button).Text;
            comm.Parity = "None";
            comm.StopBits = "One";
            comm.DataBits = "8";
            comm.BaudRate = "9600";
            comm.DisplayWindow = COMLog;
            if (comm.OpenPort())
            {

                groupForCombt.Controls.Clear();
                Label NoDataAviable = new Label() { Left = 4, Top = 0, AutoSize = false,  Width = 150, Height=40, Text = "Порт открыт.\nожидание данных...", Parent = groupForCombt };
                    comm.WriteData("run;");
            }
            GraphInit();
        }

        private void btLogRun_Click(object sender, EventArgs e)
        {
            painting = true;
            if (logging)
            {

               LogStop();
               //GraphValues();
                //GraphInit();
                //GraphValues();
            }
            else
            {
                logging = true;
                LogBeginTime = gyro.time;
                btLogRun.Text = "Остановить";
                btLogRun.Image=global::GyroControl.Properties.Resources.Pause24x24;
                LogTime = 0;
                LogTimer.Text = "0";

                GraphInit();
                ClearGraphic();
                gyro.ClearGyro();
                SetStadardScales();

                gyro.Graphs.Repoint();
                //graph.Clear(Color.White);
                //PaintGraphAxes(graph);
            }
        }

        void LogStop()
        {
            btLogRun.Text = "Запись";
            btLogRun.Image = global::GyroControl.Properties.Resources.play24x24;
            logging = false;
            

            //LogTime = 0;
            //LogTimer.Text = "0";


            
        }

        private void btLogClear_Click(object sender, EventArgs e)
        {

            LogStop();
           // GraphInit();
            //GraphValues();
            LogView.Clear();
            ClearGraphic();
           // gyro.ClearGyro();
           
           // GraphInit();
           // GraphValues();
           // painting = false;
            //graph.Clear(Color.White);
            //PaintGraphAxes(graph);
            if (comm.comPort.IsOpen)
            {

                comm.WriteData("reset;");
                LogStop();
                LogView.Clear();
                ClearGraphic();
            }
        }



        private void btLogSave_Click(object sender, EventArgs e)
        {
            DateTime saveNow = DateTime.Now;
            string datePatt = @"dd.MM.yyyy_HH.mm";
            SaveFileDialog saveLog = new SaveFileDialog();
            saveLog.OverwritePrompt = true;
            saveLog.FileName = "GyroLog_" + saveNow.ToString(datePatt);
            saveLog.DefaultExt = "log";
            saveLog.Filter =
                "Log files (*.log)|*.log|All files (*.*)|*.*";

  
           
            if (saveLog.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveLog.FileName.Length > 0)
                using (StreamWriter sw = new StreamWriter(saveLog.FileName, true))
                {
                 
                    sw.WriteLine(LogView.Text);
                    sw.Close();
                }
        }

        private void btRun_Click(object sender, EventArgs e)
        {

                runcontrol = true;
                if (comm.comPort.IsOpen)
                    comm.WriteData("run;");
  

        }

        private void btStop_Click(object sender, EventArgs e)
        {
            
                runcontrol = false;
                if (comm.comPort.IsOpen)
                    comm.WriteData("stop;");
        
        }

        private void COMsendtext_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //String command;
                //if (String.IsNullOrEmpty(COMsendtext.Text) || !Int32.TryParse(COMsendtext.Text, out command))
                if (String.IsNullOrEmpty(COMsendtext.Text) )
                {
                    return;
                }
                else
                {
                    if (comm.comPort.IsOpen)
                        comm.WriteData(COMsendtext.Text);
                    COMsendtext.Text = "";
                }
            }

        }

        private void btReset_Click_1(object sender, EventArgs e)
        {
            if (comm.comPort.IsOpen)
            {

                comm.WriteData("reset;");
                LogStop();
                LogView.Clear();
                ClearGraphic();
            }
        }
        Boolean SettingZero = false;
        private void btZero_Click(object sender, EventArgs e)
        {
            if (comm.comPort.IsOpen)
            {
                comm.WriteData("zero;");
                SettingZero = true;
                labelZeroed.Text = "";
                btZero.Text = "Установка...";
                gyro.acc00 = 0;
            }
        }

        private void timeInt_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int SensorFreq;
                if (String.IsNullOrEmpty(timeInt.Text) || !Int32.TryParse(timeInt.Text, out SensorFreq))
                {
                    return;
                }
                else
                {
                    string setmsg = "ti=" + SensorFreq.ToString() + ";";
                    if (comm.comPort.IsOpen)
                        comm.WriteData(setmsg);
                }
            }
        }
        Font timeFont = new Font("Consolas", 12);
        SolidBrush timeBrush = new SolidBrush(Color.FromArgb(50, 50, 50));
        static int CursorPosX=-1;
        static int CursorPosY= -1;

        static int OldCursorPosX = -1;
        static int OldCursorPosY = -1;
        static Boolean controlmode = false;
        static Boolean mouseDragMode = false;
        static Boolean mouseMoveRectMode = false;
        static Boolean mouseZoomRectMode = false;
        static Boolean timeStam1setmode = false;
        static Boolean timeStam2setmode = false;
        private void panelGraph_MouseMove(object sender, MouseEventArgs e)
        {
            panelGraph.Select();
            //if e.panelGraph_MouseWheel(this, e);
            if ((timeStamp1 != -1) && (timeStam1setmode))
            {
                timeStamp1 = e.Location.X;
                //if (timeStamp1 > gyro.gtime) timeStamp1 = gyro.gtime;
                timeStamp1time = gyro.Graphs.FindTimeByGraph(timeStamp1);

            }
            else
            if ((timeStamp1 != -1) && (timeStam2setmode))
            {

                timeStamp2 = e.Location.X;
                //if (timeStamp1 > gyro.gtime) timeStamp1 = gyro.gtime;
                timeStamp2time = gyro.Graphs.FindTimeByGraph(timeStamp2);

            }
            else
                if (CursorMoveMode)
                {
                    if (mouseMoveRectMode)      //Нажат кнопка при перемещении
                    {
                        int offsetX = CursorPosX - OldCursorPosX;
                        int offsetY = CursorPosY - OldCursorPosY;
                        gyro.Graphs.GtimeOffset += offsetX;
                        gyro.Graphs.GvalueOffset -= offsetY;
                        gyro.Graphs.Repoint();
                        //GraphValues();
                    }
                }
                else
                {
                    // controlmode = false;
                    this.Cursor = Cursors.Default;
                    for (int tl = 0; tl < gyro.Graphs.TimeLines.Length; tl++)
                    {
                        if ((gyro.Graphs.TimeLines[tl].visible) && (gyro.Graphs.TimeLines[tl].control))
                            if (gyro.Graphs.TimeLines[tl].ControlRect.Contains(e.Location))
                            {
                                gyro.Graphs.TimeLines[tl].waitForDrag = true;
                                this.Cursor = Cursors.SizeWE;
                                controlmode = true;
                            }
                            else
                            {
                                gyro.Graphs.TimeLines[tl].waitForDrag = false;
                            }


                    }

                    if (mouseDragMode)     //режим перемещения ползунка
                    {
                        for (int tl = 0; tl < gyro.Graphs.TimeLines.Length; tl++)
                        {
                            if (gyro.Graphs.TimeLines[tl].mouseDragmode)
                            {
                                int offset = CursorPosX - OldCursorPosX;
                                if (offset != 0)
                                {
                                    gyro.Graphs.TimeLines[tl].SetTimeLine(gyro.Graphs.FindTimeByGraph(gyro.Graphs.TimeLines[tl].gposX + offset));
                                    if (tl == 2)
                                    {
                                        /*int gtimeStamp1 = (int)((timeStamp1 - gyro.Graphs.GtimeOffset) * gyro.Graphs.TimeScale);
                                        gtimeStamp1 += offset;*/
                                        timeStamp1 += offset;
                                        timeStamp1time = gyro.Graphs.FindTimeByGraph(timeStamp1);
                                    }
                                    if (tl == 3)
                                    {
                                        /* int gtimeStamp2 = (int)((timeStamp2 - gyro.Graphs.GtimeOffset) * gyro.Graphs.TimeScale);
                                         gtimeStamp2 += offset;
                                         timeStamp2 = gyro.Graphs.FindTimeByGraph(gtimeStamp2);*/
                                        timeStamp2 += offset;
                                        timeStamp2time = gyro.Graphs.FindTimeByGraph(timeStamp2);
                                    }
                                }
                            }

                        }
                    }
             }

                 if ((CursorRectZoomMode) && (mouseZoomRectMode))
                 {
                     mouseZoomRectW = Math.Abs(mouseZoomRectX - e.Location.X);
                     mouseZoomRectH = Math.Abs(mouseZoomRectY - e.Location.Y);
                     if (Math.Abs(mouseZoomRectW) > 10)
                     {

                         mouseZoomRect = new Rectangle(Math.Min(mouseZoomRectX, e.Location.X), Math.Min(mouseZoomRectY, e.Location.Y), mouseZoomRectW, mouseZoomRectH);
                   }
                 }
            int w = panelGraph.Width;
            int h = panelGraph.Height;
           // if (controlmode)
            OldCursorPosX = CursorPosX;
            OldCursorPosY = CursorPosY;
            CursorPosX = e.Location.X;
            CursorPosY = e.Location.Y;
            UpdateTimeStampLog();
            GraphValues();
            
        }


        private void panelGraph_Paint(object sender, PaintEventArgs e)
        {

        }



        private void AverageTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int AverageTimes;
                if (String.IsNullOrEmpty(AverageTime.Text) || !Int32.TryParse(AverageTime.Text, out AverageTimes))
                {
                    return;
                }
                else
                {
                    string setmsg = "at=" + AverageTimes.ToString() + ";";
                    if (comm.comPort.IsOpen)
                        comm.WriteData(setmsg);
                }
            }
        }

        private void GyroForm_Paint(object sender, PaintEventArgs e)
        {
            GraphValues();
        }

        private void panelGraph_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                CursorMoveMode = false;
                panelGraph.Cursor = Cursors.Default;
                button8.FlatStyle = FlatStyle.Standard;
                button8.BackColor = Color.Transparent;
            }
            
            if (CursorMoveMode)
            {
                mouseMoveRectMode = false;
                controlmode = false;
                /* using (var stream = new MemoryStream(global::GyroControl.Properties.Resources..CursorMoveMoving.cur))
             {
                 this.Cursor = new System.Windows.Forms.Cursor(stream);
                 this.Cursor = new System.Windows.Forms.Cursor(stream);
             }*/
                //panelGraph.Cursor = global::GyroControl.Properties.Resources.CursorMoveMoving;
                mouseMoveRectX = e.Location.X;
                mouseMoveRectY = e.Location.Y;
            }
            else
            if (CursorRectZoomMode)
            {
                controlmode = false;
                mouseZoomRectMode = false;
                mouseZoomRectW = Math.Abs(mouseZoomRectX - e.Location.X);
                mouseZoomRectH = Math.Abs(mouseZoomRectY - e.Location.Y);
                if (mouseZoomRectW > 10)
                {
                    mouseZoomRect = new Rectangle(Math.Min(mouseZoomRectX, e.Location.X), Math.Min(mouseZoomRectY, e.Location.Y), mouseZoomRectW, mouseZoomRectH);
                    Int32 time1 = (Int32)((mouseZoomRectX - gyro.Graphs.GtimeOffset) / gyro.Graphs.TimeScale);
                    Int32 time2 = (Int32)((e.Location.X - gyro.Graphs.GtimeOffset) / gyro.Graphs.TimeScale);

                    Int32 timeoffset = (Int32)((double)gyro.Graphs.GtimeOffset / (double)gyro.Graphs.TimeScale);
                    //timeoffset = (timeoffset - time1);

                    // gyro.Graphs.GtimeOffset =- (mouseZoomRectX - ZeroX - PaintX);
                    float rescale = (float)mouseZoomRectW / (float)(CanvasWidth - 2 * CanvasMargin - ZeroX - PaintX);
                    gyro.Graphs.TimeScale = gyro.Graphs.TimeScale / rescale;
                    gyro.Graphs.ValueScale = 1 / rescale;
                    //int LastOffset = gyro.Graphs.GtimeOffset;
                    gyro.Graphs.GtimeOffset = -(int)((double)time1 * (double)gyro.Graphs.TimeScale);

                    //tabPage1.Text = "График :scale=" + gyro.Graphs.TimeScale + "; offset=" + gyro.Graphs.GtimeOffset;
                    //gyro.Graphs.GvalueOffset -= e.Location.Y-ZeroY;
                    gyro.Graphs.Repoint();
                    gyro.Graphs.ValueScale = 1;
                    GraphValues();

                }

                /* using (var stream = new MemoryStream(global::GyroControl.Properties.Resources..CursorMoveMoving.cur))
             {
                 this.Cursor = new System.Windows.Forms.Cursor(stream);
                 this.Cursor = new System.Windows.Forms.Cursor(stream);
             }*/
                //panelGraph.Cursor = global::GyroControl.Properties.Resources.CursorMoveMoving;
                mouseMoveRectX = e.Location.X;
                mouseMoveRectY = e.Location.Y;
            }
            else
            {
                if ((timeStamp1 != -1) && (timeStam1setmode))
                {
                    timeStamp1 = e.Location.X;
                    //if (timeStamp1 > gyro.gtime) timeStamp1 = gyro.gtime;
                    timeStamp1time = gyro.Graphs.FindTimeByGraph(timeStamp1);
                    timeStam1setmode = false;
                    controlmode = false;
                }
                if ((timeStamp1 != -1) && (timeStam2setmode))
                {
                    timeStamp2 = e.Location.X;
                    //if (timeStamp1 > gyro.gtime) timeStamp1 = gyro.gtime;
                    timeStamp2time = gyro.Graphs.FindTimeByGraph(timeStamp2);
                    timeStam2setmode = false;
                    controlmode = false;
                }

                /*  if (!controlmode)
                  {
                      if (e.Button == System.Windows.Forms.MouseButtons.Left)
                      {
                          if (timeStamp1 == -1)
                          {
                              timeStamp1 = e.Location.X ;
                              //if (timeStamp1 > gyro.gtime) timeStamp1 = gyro.gtime;
                              timeStamp1time = gyro.Graphs.FindTimeByGraph(timeStamp1);
                          }
                          else timeStamp1 = -1;

                      }
                      if (e.Button == System.Windows.Forms.MouseButtons.Right)
                      {
                          if (timeStamp2 == -1)
                          {
                              timeStamp2 = e.Location.X ;
                              // if (timeStamp2 > gyro.gtime) timeStamp2 = gyro.gtime;
                              timeStamp2time = gyro.Graphs.FindTimeByGraph(timeStamp2);
                          }
                          else timeStamp2 = -1;

                      }
               
                  }*/
                GraphValues();
                UpdateTimeStampLog();

                if (mouseDragMode)
                {
                    mouseDragMode = false;
                    for (int tl = 0; tl < gyro.Graphs.TimeLines.Length; tl++)
                    {
                        gyro.Graphs.TimeLines[tl].mouseDragmode = false;
                    }
                }
                mouseZoomRectW = Math.Abs(mouseZoomRectX - e.Location.X);
                mouseZoomRectH = Math.Abs(mouseZoomRectY - e.Location.Y);
                if ((mouseZoomRectMode) && (mouseZoomRectW > 10))
                {


                    mouseZoomRect = Rectangle.FromLTRB(mouseZoomRectX, mouseZoomRectY, e.Location.X, e.Location.Y);
                    Int32 time1 = (Int32)((mouseZoomRectX - gyro.Graphs.GtimeOffset) / gyro.Graphs.TimeScale);
                    Int32 time2 = (Int32)((e.Location.X - gyro.Graphs.GtimeOffset) / gyro.Graphs.TimeScale);

                    Int32 timeoffset = (Int32)((double)gyro.Graphs.GtimeOffset / (double)gyro.Graphs.TimeScale);
                    //timeoffset = (timeoffset - time1);

                    // gyro.Graphs.GtimeOffset =- (mouseZoomRectX - ZeroX - PaintX);
                    float rescale = (float)mouseZoomRectW / (float)(CanvasWidth - 2 * CanvasMargin - ZeroX - PaintX);
                    gyro.Graphs.TimeScale = gyro.Graphs.TimeScale / rescale;
                    gyro.Graphs.ValueScale = 1 / rescale;
                    //int LastOffset = gyro.Graphs.GtimeOffset;
                    gyro.Graphs.GtimeOffset = -(int)((double)time1 * (double)gyro.Graphs.TimeScale);

                    //tabPage1.Text = "График :scale=" + gyro.Graphs.TimeScale + "; offset=" + gyro.Graphs.GtimeOffset;
                    //gyro.Graphs.GvalueOffset -= e.Location.Y-ZeroY;
                    gyro.Graphs.Repoint();
                    gyro.Graphs.ValueScale = 1;
                    GraphValues();

                }
                mouseZoomRectMode = false;

                if (controlmode)
                {
                    controlmode = false;
                }
            }
        }
        string OpenTxt;
        string[] OpenLines;
        string[] OneLine;
        //StreamReader fileStream; // Дескриптор файла
        OpenFileDialog openLog ;
        Boolean opened = false; 
        string separator = ";\t";
        private void button1_Click(object sender, EventArgs e)
        {
            
             openLog = new OpenFileDialog();

            //openLog.Filter = "Text files|*.txt";
             openLog.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";
             openLog.FilterIndex = 1;
             openLog.RestoreDirectory = true;
             if (openLog.ShowDialog() == DialogResult.OK) //MessageBox.Show(openLog.FileName);
           {
               LogStop();
               opened = true;
               GraphInit();
               ClearGraphic();
               OpenTxt = System.IO.File.ReadAllText(openLog.FileName);
               this.Text = "Прием данных с MPU-6050 - " + openLog.FileName;
               //fileStream = new System.IO.StreamReader(openLog.FileName);
              // OpenTxt=(fileStream.ReadToEnd());
               //LogView.AppendText(OpenTxt);
               string[] stringSeparators = new string[] { "\r\n" };
               OpenLines = OpenTxt.Split(stringSeparators, StringSplitOptions.None);
               stringSeparators = new string[] { ";\t" };
               gyro.ClearGyro();
               //SetStadardScales();
               GraphInit();
               ClearGraphic();
               string separator = ";\t";
               for (int str = 0; str < OpenLines.Length; str++)
               {
                   if (!(String.IsNullOrEmpty(OpenLines[str])))
                   {

                       SubLines = OpenLines[str].Split(stringSeparators, StringSplitOptions.None);
                       if (SubLines.Length == 9)
                   {
                       Int32.TryParse(SubLines[0], out gyro.time);
                       LogTime = gyro.time;

                       int.TryParse(SubLines[1], out gyro.angleX);
                       int.TryParse(SubLines[2], out gyro.angleY);
                       float.TryParse(SubLines[3], out gyro.angAccX);
                       float.TryParse(SubLines[4], out gyro.angAccY);
                       float.TryParse(SubLines[5], out gyro.speed0);
                       float.TryParse(SubLines[6], out gyro.acc01);
                       float.TryParse(SubLines[7], out gyro.acc02);
                       float.TryParse(SubLines[8], out gyro.acc03);
  
                       //if (gyro.acc01 < 0) gyro.acc02 = -gyro.acc02;
                      // LogView.Clear();
               /*  if (String.IsNullOrEmpty(SubLines[0]) || !Int32.TryParse(SubLines[0], out gyro.time)) ;
                 LogTime = gyro.time;
                if (String.IsNullOrEmpty(SubLines[1]) || !int.TryParse(SubLines[1], out gyro.angleX)) ;
                if (String.IsNullOrEmpty(SubLines[2]) || !int.TryParse(SubLines[2], out gyro.angleY)) ;
                if (String.IsNullOrEmpty(SubLines[3]) || !float.TryParse(SubLines[3], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.accX)) ;
                if (String.IsNullOrEmpty(SubLines[4]) || !float.TryParse(SubLines[4], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.accY)) ;
                if (String.IsNullOrEmpty(SubLines[5]) || !float.TryParse(SubLines[5], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.accZ)) ;

               if (String.IsNullOrEmpty(SubLines[6]) || !float.TryParse(SubLines[6], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.angle0)) ;
               if (String.IsNullOrEmpty(SubLines[7]) || !float.TryParse(SubLines[7], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.dist0)) ;
               if (String.IsNullOrEmpty(SubLines[8]) || !float.TryParse(SubLines[8], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.speed0)) ;
               if (String.IsNullOrEmpty(SubLines[9]) || !float.TryParse(SubLines[9], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc00)) ;
               if (String.IsNullOrEmpty(SubLines[10]) || !float.TryParse(SubLines[10], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc01)) ;
                if (String.IsNullOrEmpty(SubLines[11]) || !float.TryParse(SubLines[11], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc02)) ;
                if (String.IsNullOrEmpty(SubLines[12]) || !float.TryParse(SubLines[12], NumberStyles.Any, CultureInfo.InvariantCulture, out gyro.acc03)) ;*/
                       UpdateLabels();
                       gyro.UpdateGyro();
                       
                           //LogView.AppendText(LogTime + separator+ gyro.angleX + separator  + gyro.angleY + separator +  gyro.accX + separator + gyro.accY + separator  + gyro.accZ + "\r\n");

                           LogView.AppendText(LogTime + separator +
                                               gyro.angleX + separator +
                                               gyro.angleY + separator +
                                               gyro.angAccX + separator +
                                               gyro.angAccY + separator +
                                               gyro.speed0 + separator +
                                               gyro.acc01 + separator +
                                               gyro.acc02 + separator +
                                               gyro.acc03 + "\r\n");
                   }
                   }
               }
               
               logging = false;
               painting = true;
               
               gyro.Graphs.Repoint();
               GraphValues();

            }

        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        
        }

        private void TimeStampsLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void btReloadComList_Click(object sender, EventArgs e)
        {
            
            groupForCombt.Visible = false;
            //comm.comPort.DataReceived += null;
            
            //comm.ClosePort();
           // comm = new CommunicationManager();
            RecreateCOMbuttons();
        }

        private void btShowSettings_Click(object sender, EventArgs e)
        {
            groupCommands.Visible = !groupCommControls.Visible;
            if (groupCommands.Visible) 
            {
                //btShowSettings.BackColor = Color.White;
                groupCommands.BringToFront();
            }
            else 
            {
                btShowSettings.BackColor = System.Drawing.SystemColors.Control;
            }
        }

        private void groupData_Enter(object sender, EventArgs e)
        {

        }

        private void btZoomIn_Click(object sender, EventArgs e)
        {
            gyro.Graphs.TimeScale *= 1.2F;
            gyro.Graphs.ValueScale = 1.2F;
            gyro.Graphs.Repoint();
            gyro.Graphs.ValueScale = 1;
            GraphValues();

        }

        private void btZoomOut_Click(object sender, EventArgs e)
        {
            gyro.Graphs.TimeScale *= 0.8333333F;
            gyro.Graphs.ValueScale = 0.8333333F;
            gyro.Graphs.Repoint();
            gyro.Graphs.ValueScale = 1;
            GraphValues();
        }
        static int mouseMoveRectX;
        static int mouseMoveRectY;
        static int mouseZoomRectX;
        static int mouseZoomRectY;
        static int mouseZoomRectW;
        static int mouseZoomRectH;
        static Rectangle mouseZoomRect;
        private void panelGraph_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
               
                CursorRectZoomMode = false;
                btRectZoomingMode.FlatStyle = FlatStyle.Standard;
                btRectZoomingMode.BackColor = Color.Transparent;
                panelGraph.Cursor = Cursors.Default;
                 CursorMoveMode = true;
                panelGraph.Cursor = Cursors.SizeAll;
                button8.FlatStyle = FlatStyle.Popup;
                button8.BackColor = Color.PaleGreen;
                controlmode = true;
            }
            if (CursorMoveMode)
            {
                mouseMoveRectMode = true;
                controlmode = true;
               /* using (var stream = new MemoryStream(global::GyroControl.Properties.Resources..CursorMoveMoving.cur))
            {
                this.Cursor = new System.Windows.Forms.Cursor(stream);
                this.Cursor = new System.Windows.Forms.Cursor(stream);
            }*/
                //panelGraph.Cursor = global::GyroControl.Properties.Resources.CursorMoveMoving;
                mouseMoveRectX = e.Location.X;
                mouseMoveRectY = e.Location.Y;
            }
            if (CursorRectZoomMode)
            {
                mouseMoveRectMode = true;
                mouseZoomRectMode = true;
                mouseZoomRectX = e.Location.X;
                mouseZoomRectY = e.Location.Y;
                controlmode = true;
               /* using (var stream = new MemoryStream(global::GyroControl.Properties.Resources..CursorMoveMoving.cur))
            {
                this.Cursor = new System.Windows.Forms.Cursor(stream);
                this.Cursor = new System.Windows.Forms.Cursor(stream);
            }*/
                //panelGraph.Cursor = global::GyroControl.Properties.Resources.CursorMoveMoving;
                mouseMoveRectX = e.Location.X;
                mouseMoveRectY = e.Location.Y;
            }
            else
          
            {
                for (int tl = 0; tl < gyro.Graphs.TimeLines.Length; tl++)
                {
                    if ((gyro.Graphs.TimeLines[tl].visible) && (gyro.Graphs.TimeLines[tl].control))
                        if (gyro.Graphs.TimeLines[tl].ControlRect.Contains(e.Location))
                        {
                            gyro.Graphs.TimeLines[tl].mouseDragmode = true;
                            controlmode = true;
                            mouseDragMode = true;
                        }

                }

                if (!controlmode)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        if (timeStamp1 == -1)
                        {
                            timeStamp1 = e.Location.X;
                            //if (timeStamp1 > gyro.gtime) timeStamp1 = gyro.gtime;
                            timeStamp1time = gyro.Graphs.FindTimeByGraph(timeStamp1);
                            timeStam1setmode = true;
                            controlmode = true;
                        }
                        else timeStamp1 = -1;

                    }
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        if (timeStamp2 == -1)
                        {
                            timeStamp2 = e.Location.X;
                            // if (timeStamp2 > gyro.gtime) timeStamp2 = gyro.gtime;
                            timeStamp2time = gyro.Graphs.FindTimeByGraph(timeStamp2);
                            timeStam2setmode = true;
                            controlmode = true;
                        }
                        else timeStamp2 = -1;

                    }

                }
                /*if ((!mouseDragMode) && (!timeStam1setmode) && (!timeStam2setmode))
                {

                    mouseZoomRectMode = true;
                    mouseZoomRectX = e.Location.X;
                    mouseZoomRectY = e.Location.Y;
                }*/
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            gyro.Graphs.GtimeOffset -= 50;
            gyro.Graphs.Repoint();
            GraphValues();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            gyro.Graphs.GtimeOffset += 50;
            gyro.Graphs.Repoint();
            GraphValues();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            gyro.Graphs.GtimeOffset = 0;
            gyro.Graphs.GvalueOffset = 0;
            gyro.Graphs.TimeScale = 0.01F;
            gyro.Graphs.ValueScale = 1;
 
            SetStadardScales();

            gyro.Graphs.Repoint();
            GraphValues();
        }

        private void btZoonToTL_Click(object sender, EventArgs e)
        {

            if ((timeStamp1 != -1) && (timeStamp2 != -1))
            {
                // gyro.Graphs.GtimeOffset =- (mouseZoomRectX - ZeroX - PaintX);
                float rescale = (float)Math.Abs(timeStamp1 - timeStamp2) / (float)(CanvasWidth - 2 * CanvasMargin - ZeroX - PaintX);

                gyro.Graphs.TimeScale = gyro.Graphs.TimeScale / rescale;
                gyro.Graphs.GtimeOffset = -(int)((double)Math.Min(timeStamp1time, timeStamp2time) * (double)gyro.Graphs.TimeScale);
                tabGraph.Text = "График :scale=" + gyro.Graphs.TimeScale + "; offset=" + gyro.Graphs.GtimeOffset;

                gyro.Graphs.Repoint();
               /* int minTimestamp;
                int maxTimestamp;
                if (timeStamp1 > timeStamp2)
                {
                    minTimestamp = timeStamp2;
                    maxTimestamp = timeStamp1;
                }
                else
                {
                    minTimestamp = timeStamp1;
                    maxTimestamp = timeStamp2;
                }

                timeStamp1 = maxTimestamp;
                timeStamp1time = gyro.Graphs.FindTimeByGraph(timeStamp1);
                timeStamp2 = minTimestamp;
                timeStamp2time = gyro.Graphs.FindTimeByGraph(timeStamp2);
            */
                GraphValues();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            gyro.Graphs.GvalueOffset += 10;
            gyro.Graphs.Repoint();
            GraphValues();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            gyro.Graphs.GvalueOffset -= 10;
            gyro.Graphs.Repoint();
            GraphValues();
        }

        private void angAccYlabel_Click(object sender, EventArgs e)
        {

        }

        private void checkAllLines_CheckedChanged(object sender, EventArgs e)
        {
            Boolean checkedAll = checkAllLines.Checked;
            gyro.Graphs.AngX.setVisible(checkedAll);
            gyro.Graphs.AngY.setVisible(checkedAll);
            gyro.Graphs.AngAccX.setVisible(checkedAll);
            gyro.Graphs.AngAccY.setVisible(checkedAll);
           // gyro.Graphs.Dist.setVisible(checkedAll);
            gyro.Graphs.Speed.setVisible(checkedAll);
            //gyro.Graphs.Acc00.setVisible(checkedAll);
            gyro.Graphs.Acc01.setVisible(checkedAll);
            gyro.Graphs.Acc02.setVisible(checkedAll);
            gyro.Graphs.Acc03.setVisible(checkedAll);
            GraphValues();
        }
        static Boolean smoothMode = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            smoothMode = checkBox1.Checked;
            GraphValues();
        }

        static Boolean ShowCursorline = false;

        private void btShowCursorline_Click(object sender, EventArgs e)
        {
            ShowCursorline = !ShowCursorline;
            if (ShowCursorline)
            {
                
                btShowCursorline.FlatStyle = FlatStyle.Popup;
                btShowCursorline.BackColor = Color.PaleGreen;
                btShowCursorline.Image = global::GyroControl.Properties.Resources.ShowCursorRect;
            }
            else
            {
              
                btShowCursorline.BackColor = Color.Transparent;
                panelGraph.Cursor = Cursors.Default;
                btShowCursorline.Image = global::GyroControl.Properties.Resources.HideCursorRect;
            }

           

        }
        static Boolean CursorMoveMode = false;
        private void button8_Click(object sender, EventArgs e)
        {
            CursorMoveMode = !CursorMoveMode;
            //button8.FlatAppearance.checked=true;
            if (CursorMoveMode)
            {

                panelGraph.Cursor = Cursors.SizeAll;
                button8.FlatStyle = FlatStyle.Popup;
                button8.BackColor = Color.PaleGreen;
   
            }
            else
            {
                button8.FlatStyle = FlatStyle.Standard;
                button8.BackColor = Color.Transparent;
                panelGraph.Cursor = Cursors.Default;
        
            }
        }

        private void GyroForm_Activated(object sender, EventArgs e)
        {
            GraphValues();
        }
        static Boolean CursorRectZoomMode = false;

        private void btRectZoomingMode_Click(object sender, EventArgs e)
        {
            CursorRectZoomMode = !CursorRectZoomMode;
          
            //button8.FlatAppearance.checked=true;
            if (CursorRectZoomMode)
            {

                panelGraph.Cursor = Cursors.Cross;
                btRectZoomingMode.FlatStyle = FlatStyle.Popup;
                btRectZoomingMode.BackColor = Color.PaleGreen;
                
            }
            else
            {
                btRectZoomingMode.FlatStyle = FlatStyle.Standard;
                btRectZoomingMode.BackColor = Color.Transparent;
                panelGraph.Cursor = Cursors.Default;
               
            }
        }

        private void panelGraph_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                gyro.Graphs.GtimeOffset = 0;
                gyro.Graphs.GvalueOffset = 0;
                gyro.Graphs.TimeScale = 0.01F;
                gyro.Graphs.ValueScale = 1;

                SetStadardScales();

                gyro.Graphs.Repoint();
                GraphValues();
            }
        }

        private void btShowHelp_Click(object sender, EventArgs e)
        {
            Form HelpForm1 = new HelpForm();
            HelpForm1.ShowDialog();
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            panelGraph.Select();
            GraphValues();
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            panelGraph.Select();
            GraphValues();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
           // GraphValues();
        }

        private void buttonFilter_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
           /* gyro.Graphs.AccX.SetAverage(trackBar1.Value);
            gyro.Graphs.AccY.SetAverage(trackBar1.Value);
            gyro.Graphs.AccZ.SetAverage(trackBar1.Value);
            label29.Text = "~" + trackBar1.Value.ToString();
            GraphValues();*/
        }

        private void btBeginSeach_Click(object sender, EventArgs e)
        {
            gyro.Graphs.SeachBeginningPoint();
            btCalculate.Enabled = true;
            GraphValues();

        }

        private void btCalculate_Click(object sender, EventArgs e)
        {
            if (gyro.Graphs.NewBeginPoint)
            {
                
                gyro.Graphs.RecalculateForBeginPoint();
                GraphValues();
            }
        }
 

     













    }
}
