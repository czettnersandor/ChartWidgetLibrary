/*
Copyright (c) 2009 Sandor Czettner (sandor@czettner.hu)

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using Gtk;
using Pango;

namespace ChartWidgetLibrary
{
	// Berakjuk a toolboxba
	[System.ComponentModel.Category("pick a name")]
	[System.ComponentModel.ToolboxItem(true)]
	public class BarChart : Gtk.DrawingArea
	{
		// Ez a tömb fogja tárolni az adatokat.
		private ChartDataType[] chartdata;
		int checksize;
		int spacing;
		int lastselectedbar = 0;
		int numofdata;
		float maximum = 0; // Maximum érték
		private float animpercent = 100;
		
		// Létrehozunk egy eseményt, ami a tüske kiválasztásakor fut le
		
	    public event TuskeValasztasHandler TuskeValasztasEvent;
		
	    protected void OnTuskeValasztasEvent(ChartDataType cdata)
	    {
			if (TuskeValasztasEvent != null)
				TuskeValasztasEvent(cdata);
	    }

		// Constructor
		public BarChart()
		{
			
		}
		
		public ChartDataType[] ChartData {
		 get { return chartdata; }
		 set
			{
				float max;
				max = 0;
				chartdata = value;
				foreach (ChartDataType ct in chartdata) {
					if (ct.Val > max) max = ct.Val;
				}
				
				maximum = max;
				if (animpercent!=0)
				{
					this.GdkWindow.Clear();
					DrawRulers();
					DrawChart();
				}
			}
		}

		public int Spacing {
			get {
				return spacing;
			}
			set {
				spacing = value;
			}
		}

		public int Checksize {
			get {
				return checksize;
			}
			set {
				checksize = value;
			}
		}
		public int NumOfData {
			get {
				return numofdata;
			}
			set {
				numofdata = value;
				chartdata = new ChartDataType[numofdata];
				for (int i=0; i<numofdata; i++) {
					chartdata[i] = new ChartDataType(DateTime.Now , 0, 0);
				}
			}
		}
		
		protected override bool OnButtonPressEvent(Gdk.EventButton ev)
		{
			// Insert button press handling code here.
			return base.OnButtonPressEvent(ev);
		}
		
		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evnt)
		{
			int x = Convert.ToInt32( Math.Floor ((evnt.X-15) / (spacing + checksize)));
			if (x < numofdata && lastselectedbar != x && x >= 0) {
				DrawBar(lastselectedbar, ChartData[lastselectedbar].Val, true);
				DrawBar( x, ChartData[x].Val, false);
				lastselectedbar = x;
				OnTuskeValasztasEvent(ChartData[x]);
			}
			return base.OnMotionNotifyEvent (evnt);
		}
		
		protected override bool OnExposeEvent(Gdk.EventExpose ev)
		{
			base.OnExposeEvent(ev);
			// Kirajzoljuk a BarChart-ot
			DrawChart();
			DrawRulers();
			Console.Write("Expose");
			return true;
		}
		
		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
			
			// Insert layout code here.
		}
		
		private void DrawChart (  )
		{
			int xcount;
			xcount = 0;
			foreach (ChartDataType cd in chartdata)
			{
				if (xcount == lastselectedbar)
					DrawBar(xcount, cd.Val, false);
				else
					DrawBar(xcount, cd.Val, true);
				xcount++;
			}
		}
		private void DrawRulers ()
		{
			int x=0;
			int korr=0;
			Gdk.GC gc;
			Gdk.GC gc1 = new Gdk.GC (this.GdkWindow);
			gc1.RgbFgColor = new Gdk.Color (10, 10, 20);
			Gdk.GC gc2 = new Gdk.GC (this.GdkWindow);
			gc2.RgbFgColor = new Gdk.Color (100, 100, 120);
			Pango.Layout layout = new Pango.Layout(PangoContext);
			layout.FontDescription = Pango.FontDescription.FromString("Sans 9");
			layout.SetText("0");
			gc = gc1;
			
			this.GdkWindow.DrawLine(gc1,0,0,5,0);
			this.GdkWindow.DrawLine(gc1,0,150,5,150);
			this.GdkWindow.DrawLine(gc1,0,299,5,299);
			layout.SetText(Convert.ToString(maximum));
			this.GdkWindow.DrawLayout(gc1, 5, 0, layout);
			layout.SetText(Convert.ToString(maximum/2));
			this.GdkWindow.DrawLayout(gc1, 5, 150, layout);

			
			for (int i=0; i<numofdata ; i++) {
				x=i*(spacing+Checksize)+15;
				if (i % 5 == 0) { // Minden Ötödik
					layout.SetText(Convert.ToString(i));
					this.GdkWindow.DrawLayout(gc1, x, 307, layout);
					korr=3;
					gc = gc1;
				}
				this.GdkWindow.DrawLine(gc,x,302,x,307+korr);
				korr = 0;
				gc = gc2;
			}
			
			
		}
		private void DrawBar(int x, float height, bool kitoltes)
		{
			int chartX;
			int chartY;
			int chartH;
			int kcorrection = 0; // Kitöltés korrekció (szélesség -1, magasság -1)
			if (height > 0) {
				Gdk.GC gc;
				Gdk.GC gc1 = new Gdk.GC (this.GdkWindow);
				gc1.RgbFgColor = new Gdk.Color (20, 50, 117);
		
				Gdk.GC gc2 = new Gdk.GC (this.GdkWindow);
				gc2.RgbFgColor = new Gdk.Color (20, 20, 255);
	
				Gdk.GC gckeret = new Gdk.GC (this.GdkWindow);
				gckeret.RgbFgColor = new Gdk.Color (5, 10, 10);
				Gdk.GC gckijeloles = new Gdk.GC (this.GdkWindow);
				gckijeloles.RgbFgColor = new Gdk.Color (200, 200, 255);
	
				chartX = x * (checksize + spacing)+15;
				chartH = Convert.ToInt32((height * (1 / maximum) * 300) * (animpercent/100) ) ;
				chartY = 300 - chartH;
				
				
				if(kitoltes) {
					if (x % 2 != 0)
						gc = gc1;
					else
						gc = gc2;
				} else {
					// Windowson végtelen ExposeEvent-et okoz :(
					// this.GdkWindow.ClearAreaE(chartX, chartY, checksize, chartH);
					// Helyette:
					this.GdkWindow.DrawRectangle (gckijeloles, true, chartX, chartY, checksize-kcorrection, chartH-kcorrection);
					kcorrection = 1;
					gc = gckeret;
				}
				this.GdkWindow.DrawRectangle (gc, kitoltes, chartX, chartY, checksize-kcorrection, chartH-kcorrection);
			}
		}
		
		public void Animate(ChartDataType[] cd)
		{
			animpercent=0;
			ChartData = cd;
			this.GdkWindow.Clear();
			DrawRulers();
			GLib.Timeout.Add (20, new GLib.TimeoutHandler (AnimateChart));
		}
		private bool AnimateChart()
		{
			animpercent = animpercent+2;
			int xcount;
			xcount = 0;
			foreach (ChartDataType cd in chartdata)
			{
				DrawBar(xcount, cd.Val, true);
				xcount++;
			}
			if (animpercent >= 100) return false;
			return true;
		}
		
	}
	public delegate void TuskeValasztasHandler(ChartDataType cdata);
	public class ChartDataType
	{
		public ChartDataType(DateTime datum, float val, float meroora)
		{
			this.Datum = datum;
			this.Val = val;
			this.Meroora = meroora;
		}
		public DateTime Datum {get; set;}
		public float Val {get; set;}
		public float Meroora {get; set;}
	}
}
