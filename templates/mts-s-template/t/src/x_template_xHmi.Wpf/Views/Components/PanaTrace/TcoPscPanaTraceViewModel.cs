using System;
using System.Collections.Generic;
using System.Text;
using Vortex.Presentation.Wpf;

namespace x_template_xPlc
{
    public partial class TcoPscPanaTraceViewModel : RenderableViewModel
    {
        public TcoPscPanaTraceViewModel()
        { }

        public TcoPscPanaTrace Component { get; private set; } = new TcoPscPanaTrace();

        public override object Model { get => Component; set { Component = (TcoPscPanaTrace)value; } }


    }

    public class TcoPscPanaTraceServiceViewModel : TcoPscPanaTraceViewModel
    {
    }
}
