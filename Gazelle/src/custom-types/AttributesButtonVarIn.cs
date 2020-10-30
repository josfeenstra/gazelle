using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Gazelle
{
    internal class AttributesButtonVarIn : GH_ComponentAttributes
    {
        public bool IsOn;
        public bool mouseDown;

        private bool mouseOver;
        private RectangleF buttonArea;
        private RectangleF textArea;
        private string buttonTextOn;
        private string buttonTextOff;
        ComponentNodeIn RealOwner;

        public AttributesButtonVarIn(ComponentNodeIn owner, string _buttonTextOn, string _buttonTextOff)
            : base(owner)
        {
            RealOwner = owner;
            mouseOver = false;
            mouseDown = false;
            IsOn = true;
            //   PerformLayout();
            this.buttonTextOn = _buttonTextOn;
            this.buttonTextOff = _buttonTextOff;
        }

        protected override void Layout()
        {
            Bounds = RectangleF.Empty;
            base.Layout();
            buttonArea = new RectangleF(Bounds.Left, Bounds.Bottom, Bounds.Width, 15);
            textArea = buttonArea;
            // textArea.Inflate(-3, -3);
            Bounds = RectangleF.Union(Bounds, buttonArea);
        }

        protected void Flip()
        {
            if (this.IsOn)
            {
                this.IsOn = false;
                RealOwner.Expand();
            }
            else
            {
                this.IsOn = true;
                RealOwner.Collapse();
            }
        }



        /// <summary>
        /// i did not write this
        /// </summary>
        #region Drawing

        protected override void Render(Grasshopper.GUI.Canvas.GH_Canvas canvas, System.Drawing.Graphics graphics, Grasshopper.GUI.Canvas.GH_CanvasChannel channel)
        {

            base.Render(canvas, graphics, channel);
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_PaletteStyle style = GH_CapsuleRenderEngine.GetImpliedStyle(GH_Palette.Black, Selected, Owner.Locked, true);

                GH_Capsule button = GH_Capsule.CreateTextCapsule(buttonArea, textArea, GH_Palette.Black, buttonTextOn, GH_FontServer.Small, 1, 9);
                if (this.IsOn)
                    button.Text = buttonTextOn;
                else
                    button.Text = buttonTextOff;
                
                button.RenderEngine.RenderBackground(graphics, canvas.Viewport.Zoom, style);
                if (!mouseDown)
                {
                    button.RenderEngine.RenderHighlight(graphics);
                }

                button.RenderEngine.RenderOutlines(graphics, canvas.Viewport.Zoom, style);
                if (mouseOver)
                {
                    button.RenderEngine.RenderBackground_Alternative(graphics, Color.FromArgb(50, Color.Blue), false);

                    GH_Component buttonComp = Owner as GH_Component;
                    Pen highlightPen = new Pen(Color.Blue, 4);
                }
                button.RenderEngine.RenderText(graphics, Color.White);

                button.Dispose();
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, Grasshopper.GUI.GH_CanvasMouseEvent e)
        {
            // rechtermuisknop klik
            if (e.Button == System.Windows.Forms.MouseButtons.Right && sender.Viewport.Zoom >= 0.5f && buttonArea.Contains(e.CanvasLocation))
            {

                mouseDown = true;
                //Owner.ButtonDown = true;
                GH_Component owner = Owner as GH_Component;

                return GH_ObjectResponse.Capture;
            }

            // linkermuisknop klik
            if (e.Button == System.Windows.Forms.MouseButtons.Left && sender.Viewport.Zoom >= 0.5f && buttonArea.Contains(e.CanvasLocation))
            {
                // swap button type 
                Flip();

                mouseDown = true;
                //Owner.ButtonDown = true;
                Owner.RecordUndoEvent("Update Selection", new Grasshopper.Kernel.Undo.Actions.GH_GenericObjectAction(Owner));
                GH_Component owner = Owner as GH_Component;
                Owner.ExpireSolution(true);
                return GH_ObjectResponse.Capture;
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!buttonArea.Contains(e.CanvasLocation))
            {
                mouseOver = false;
            }
            if (mouseDown)
            {
                mouseDown = false;
                sender.Invalidate();
                return GH_ObjectResponse.Release;
            }
            return base.RespondToMouseUp(sender, e);
        }


        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            System.Drawing.Point pt = GH_Convert.ToPoint(e.CanvasLocation);
            if (e.Button != System.Windows.Forms.MouseButtons.None)
            {
                return base.RespondToMouseMove(sender, e);
            }
            if (buttonArea.Contains(pt))
            {
                if (mouseOver != true)
                {
                    mouseOver = true;
                    sender.Invalidate();
                }
                return GH_ObjectResponse.Capture;
            }
            if (mouseOver != false)
            {
                mouseOver = false;
                sender.Invalidate();
            }
            return GH_ObjectResponse.Release;
        }
        #endregion
    }
}