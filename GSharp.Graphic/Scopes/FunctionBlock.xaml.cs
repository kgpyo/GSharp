﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GSharp.Graphic.Core;
using GSharp.Graphic.Holes;
using GSharp.Base.Cores;
using GSharp.Base.Scopes;
using GSharp.Extension;

namespace GSharp.Graphic.Scopes
{
    /// <summary>
    /// MethodBlock.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FunctionBlock : ScopeBlock
    {
        #region Hole List
        // Hole List
        public override List<BaseHole> HoleList
        {
            get
            {
                return _HoleList;
            }
        }
        private List<BaseHole> _HoleList;
        #endregion

        #region Objects
        // GFunction
        public GFunction GFunction
        {
            get
            {
                return _GFunction;
            }
        }
        private GFunction _GFunction;

        // GScope
        public override GScope GScope
        {
            get
            {
                return GFunction;
            }
        }

        // GObjectList
        public override List<GBase> GObjectList
        {
            get
            {
                return _GObjectList;
            }
        }
        private List<GBase> _GObjectList;
        #endregion

        #region Constructor
        // Constructor
        public FunctionBlock(GFunction function)
        {
            // Initialize Component
            InitializeComponent();

            // Initialize Hole List
            _HoleList = new List<BaseHole> { RealNextConnectHole };

            // Initialize Objects
            _GFunction = function;
            _GObjectList = new List<GBase> { GScope };

            StackContentText.Text = function.FunctionName;

            // Initialize Events
            RealNextConnectHole.BlockAttached += RealNextConnectHole_BlockChanged;
            RealNextConnectHole.BlockDetached += RealNextConnectHole_BlockChanged;

            // Initialize Block
            InitializeBlock();
        }
        #endregion

        #region Events
        // RealNextConnectHole BlockAttached & BlockDetached Event
        private void RealNextConnectHole_BlockChanged(BaseBlock block)
        {
            _GFunction.Content.Clear();

            List<GBase> content = RealNextConnectHole.StatementBlock?.GObjectList;
            if (content == null) return;

            foreach (GBase gbase in content)
            {
                if (gbase is GStatement)
                {
                    _GFunction.Append(gbase as GStatement);
                }
            }
        }
        #endregion
    }
}
