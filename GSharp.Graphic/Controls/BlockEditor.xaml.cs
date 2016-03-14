﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using GSharp.Base.Cores;
using GSharp.Base.Scopes;
using GSharp.Base.Singles;
using GSharp.Graphic.Core;
using GSharp.Graphic.Holes;
using GSharp.Graphic.Scopes;
using GSharp.Graphic.Logics;
using GSharp.Graphic.Objects;
using GSharp.Graphic.Statements;

namespace GSharp.Graphic.Controls
{
    /// <summary>
    /// BlockEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BlockEditor : UserControl
    {
        #region 객체
        // 선택한 블럭
        private BaseBlock SelectedBlock;

        // 블럭을 잡은 마우스 위치
        private Point SelectedPosition;

        // 자석 효과 대상
        private BaseHole MargnetTarget;

        // 자석 효과 대상 하이라이팅 효과
        private Rectangle Highlighter = new Rectangle
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Width = 10,
            Height = 10,
            Fill = Brushes.Green
        };

        // 자석 효과 대상 목록
        private List<BaseHole> HoleList = new List<BaseHole>();

        // 선택된 대상이 움직였는지 체크
        private bool IsSelectedBlockMoved = false;

        private Dictionary<string, GDefine> DefineList = new Dictionary<string, GDefine>
        {
            {"test1", new GDefine("test1") }
        };
        #endregion

        #region 생성자
        public BlockEditor()
        {
            InitializeComponent();

            AddBlock(new NumberBlock());
            AddBlock(new NumberBlock());
            AddBlock(new VariableBlock());
            AddBlock(new VariableBlock());
            AddBlock(new VoidBlock());
            AddBlock(new IFBlock());
            AddBlock(new PrintBlock());
            AddBlock(new SetBlock());
            AddBlock(new CompareBlock());

            Panel.SetZIndex(Highlighter, int.MaxValue - 1);
            BlockCanvas.Children.Add(Highlighter);
        }
        #endregion

        #region 이벤트
        private void Block_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            CaptureMouse();

            SelectedBlock = (BaseBlock)sender;
            SelectedPosition = e.GetPosition(SelectedBlock);

            // 부모 블럭 클릭되지 않도록
            e.Handled = true;
            IsSelectedBlockMoved = false;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // 선택한 블럭이 없으면 안됨
            if (SelectedBlock == null)
            {
                return;
            }

            if (!IsSelectedBlockMoved)
            {
                // 부모가 캔버스가 아닐때만
                if (SelectedBlock.Parent != this)
                {
                    // 부모에서 선택한 요소를 제거
                    if (SelectedBlock.Parent is Panel)
                    {
                        ((Panel)SelectedBlock.Parent).Children.Remove(SelectedBlock);
                    }
                    else if (SelectedBlock.Parent is Border)
                    {
                        ((Border)SelectedBlock.Parent).Child = null;
                    }

                    BlockCanvas.Children.Add(SelectedBlock);

                    Point pos = e.GetPosition(this);
                    SelectedBlock.Position = new Point(pos.X - SelectedPosition.X, pos.Y - SelectedPosition.Y);
                }

                IsSelectedBlockMoved = true;
            }

            // 마우스 좌표로 블럭 이동
            Point position = e.GetPosition(this);
            SelectedBlock.Position = new Point(position.X - SelectedPosition.X, position.Y - SelectedPosition.Y);

            // 연결할 대상을 찾고 하이라이팅
            MargnetBlock(SelectedBlock, e);
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 선택한 블럭이 없으면 안됨
            if (SelectedBlock == null)
            {
                return;
            }

            // 마우스를 움직인 경우에만
            if (IsSelectedBlockMoved)
            {
                // 마우스 좌표로 블럭 이동
                Point position = e.GetPosition(this);
                SelectedBlock.Position = new Point(position.X - SelectedPosition.X, position.Y - SelectedPosition.Y);

                // 연결할 대상이 있다면 연결
                if (MargnetTarget != null)
                {
                    ConnectBlock(SelectedBlock, e);
                }
            }

            // Selected 해제
            SelectedBlock = null;
            Highlighter.Visibility = Visibility.Hidden;
            MargnetTarget = null;

            ReleaseMouseCapture();
        }
        #endregion

        #region 변수 선언
        public void DefineVariable(String varName)
        {
            DefineList[varName] = new GDefine(varName);
        }

        public void UnDefineVariable(String varName)
        {
            DefineList.Remove(varName);
        }

        public List<string> GetVariableList()
        {
            return DefineList.Keys.ToList();
        }

        public List<GDefine> GetDefineList()
        {
            return DefineList.Values.ToList();
        }
        #endregion

        #region 블럭 자석
        private void MargnetBlock(BaseBlock block, MouseEventArgs e)
        {
            MargnetTarget = null;

            Highlighter.Visibility = Visibility.Hidden;

            if (SelectedBlock is StatementBlock)
            {
                MargnetBlock((StatementBlock)SelectedBlock, e);
            }
            else if (SelectedBlock is LogicBlock)
            {
                MargnetBlock((LogicBlock)SelectedBlock, e);
            }
            else if (SelectedBlock is ObjectBlock)
            {
                MargnetBlock((ObjectBlock)SelectedBlock, e);
            }
        }

        private void MargnetBlock(StatementBlock block, MouseEventArgs e)
        {
            foreach (BaseHole hole in HoleList)
            {
                if (!(hole is NextConnectHole))
                {
                    continue;
                }

                NextConnectHole nextConnectHole = (NextConnectHole)hole;
                Point position = hole.TranslatePoint(new Point(0, 0), BlockCanvas);

                Double distance = GetDistance(position, block.Position);

                if (distance > 20)
                {
                    continue;
                }

                MargnetTarget = nextConnectHole;
                Highlighter.Margin = new Thickness(position.X, position.Y, 0, 0);
                Highlighter.Visibility = Visibility.Visible;
            }
        }

        private void MargnetBlock(LogicBlock block, MouseEventArgs e)
        {
            foreach (BaseHole hole in HoleList)
            {
                if (!(hole is LogicHole))
                {
                    continue;
                }

                LogicHole logicHole = (LogicHole)hole;
                Point position = hole.TranslatePoint(new Point(0, 0), BlockCanvas);

                double distance = GetDistance(position, block.Position);

                if (distance > 20)
                {
                    continue;
                }

                MargnetTarget = logicHole;
                Highlighter.Margin = new Thickness(position.X, position.Y, 0, 0);
                Highlighter.Visibility = Visibility.Visible;
            }
        }

        private void MargnetBlock(ObjectBlock block, MouseEventArgs e)
        {
            foreach (BaseHole hole in HoleList)
            {
                if (!(hole is ObjectHole))
                {
                    continue;
                }

                ObjectHole objectHole = (ObjectHole)hole;
                Point position = hole.TranslatePoint(new Point(0, 0), BlockCanvas);

                double distance = GetDistance(position, block.Position);

                if (distance > 20)
                {
                    continue;
                }

                MargnetTarget = objectHole;
                Highlighter.Margin = new Thickness(position.X, position.Y, 0, 0);
                Highlighter.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region 블럭 연결
        private void ConnectBlock(BaseBlock block, MouseEventArgs e)
        {
            if (SelectedBlock is StatementBlock)
            {
                ConnectBlock((StatementBlock)SelectedBlock, e);
            }
            else if (SelectedBlock is LogicBlock)
            {
                ConnectBlock((LogicBlock)SelectedBlock, e);
            }
            else if (SelectedBlock is ObjectBlock)
            {
                ConnectBlock((ObjectBlock)SelectedBlock, e);
            }
        }

        private void ConnectBlock(StatementBlock block, MouseEventArgs e)
        {
            if (!(MargnetTarget is NextConnectHole))
            {
                return;
            }

            NextConnectHole nextConnectHole = (NextConnectHole)MargnetTarget;
            BlockCanvas.Children.Remove(block);

            nextConnectHole.StatementBlock = block;
            block.Position = new Point(0, 0);
        }

        private void ConnectBlock(LogicBlock block, MouseEventArgs e)
        {
            if (!(MargnetTarget is LogicHole))
            {
                return;
            }

            LogicHole logicHole = (LogicHole)MargnetTarget;
            BlockCanvas.Children.Remove(block);

            logicHole.LogicBlock = block;
            block.Position = new Point(0, 0);
        }

        private void ConnectBlock(ObjectBlock block, MouseEventArgs e)
        {
            if (!(MargnetTarget is ObjectHole))
            {
                return;
            }

            ObjectHole objectHole = (ObjectHole)MargnetTarget;
            BlockCanvas.Children.Remove(block);

            objectHole.ObjectBlock = block;
            block.Position = new Point(0, 0);
        }
        #endregion

        #region 내부 함수
        private double GetDistance2(Point a, Point b)
        {
            return Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2);
        }

        private double GetDistance(Point a, Point b)
        {
            return Math.Sqrt(GetDistance2(a, b));
        }
        #endregion

        #region 사용자 함수
        public void AddBlock(BaseBlock block)
        {
            block.MouseLeftButtonDown += Block_MouseLeftButtonDown;
            BlockCanvas.Children.Add(block);

            List<BaseHole> holeList = block.GetHoleList();
            HoleList.AddRange(holeList);
        }
        #endregion

        public void Compile()
        {
            try
            {
                GBlank root = new GBlank();
                List<GBase> list = new List<GBase>();

                foreach (GDefine define in DefineList.Values)
                {
                    root.Append(define);
                }

                foreach (var block in BlockCanvas.Children)
                {
                    if (!(block is ScopeBlock)) continue;

                    ScopeBlock scopeBlock = (ScopeBlock)block;

                    root.Append(scopeBlock.ToObject()[0]);
                }

                MessageBox.Show(root.ToSource(), "컴파일 소스");
            }
            catch (ToObjectException e)
            {
                MessageBox.Show(e.Message, "오류");
            }
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Compile();
        }
    }
}
