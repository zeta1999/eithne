﻿using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class ImageButton : Button
	{
		private int n;

		public ImageButton(Gtk.Widget w, int n) : base(w)
		{
			this.n = n;
		}
		
		public int Num
		{
			get { return n; }
		}
	}

	public class ResultView
	{
		[Widget] Window		ResultViewWindow;
		[Widget] Button		CloseButton;
		[Widget] Label		CounterText;
		[Widget] Image		CurrentImage;
		[Widget] Image		RecognizedImage;
		[Widget] Image		TestIcon;
		[Widget] Image		BaseIcon;
		[Widget] Label		TestCategory;
		[Widget] Label		BaseCategory;
		[Widget] Label		MatchedLabel;
		[Widget] ScrolledWindow	ImageListSocket;

		private static Gdk.Pixbuf NoMatchIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "no-base.png");

		private Gdk.Pixbuf[] img1, img2;
		private int[] res, cat1, cat2;
		private bool[] match;

		public ResultView(Gdk.Pixbuf[] img1, Gdk.Pixbuf[] img2, Gdk.Pixbuf[] thumbs, int[] res, int[] cat1, int[] cat2,
				bool[] match)
		{
			this.img1 = img1;
			this.img2 = img2;
			this.res = res;
			this.cat1 = cat1;
			this.cat2 = cat2;
			this.match = match;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "ResultView.glade", "ResultViewWindow", null);
			gxml.BindFields(this);

			ResultViewWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			CounterText.Text = String.Format(Catalog.GetPluralString("<i>Viewing results for {0} image</i>", "<i>Viewing results for {0} images</i>", img2.Length), img2.Length);
			CounterText.UseMarkup = true;

			TestIcon.FromPixbuf = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "image-test-22.png");
			BaseIcon.FromPixbuf = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "image-base-22.png");

			SetDisplay(0);

			HButtonBox hb = new HButtonBox();

			hb.Layout = ButtonBoxStyle.Start;
			for(int i=0; i<thumbs.Length; i++)
			{
				ImageButton b = new ImageButton(new Image(thumbs[i]), i);
				b.Clicked += OnClicked;
				hb.PackEnd(b, false, false, 0);
			}

			ImageListSocket.AddWithViewport(hb);

			ResultViewWindow.ShowAll();
		}

		private void SetDisplay(int n)
		{
			CurrentImage.FromPixbuf = img2[n];
			TestCategory.Text = String.Format(Catalog.GetString("Category: {0}"), cat2[n]);

			if(match[n])
			{
				RecognizedImage.FromPixbuf = img1[res[n]];
				BaseCategory.Text = String.Format(Catalog.GetString("Category: {0}"), cat1[res[n]]);

				if(cat2[n] == cat1[res[n]])
				{
					MatchedLabel.Text = Catalog.GetString("Correct match");
					MatchedLabel.ModifyFg(StateType.Normal, new Gdk.Color(0, 127, 0));
				}
				else
				{
					MatchedLabel.Text = Catalog.GetString("Incorrect match");
					MatchedLabel.ModifyFg(StateType.Normal, new Gdk.Color(255, 0, 0));
				}
			}
			else
			{
				RecognizedImage.FromPixbuf = NoMatchIcon;
				BaseCategory.Text = Catalog.GetString("No match found");

				MatchedLabel.Text = Catalog.GetString("No match found");
				MatchedLabel.ModifyFg(StateType.Normal, new Gdk.Color(255, 0, 0));
			}
		}

		private void OnClicked(object o, EventArgs args)
		{
			SetDisplay((o as ImageButton).Num);
		}

		private void CloseWindow(object o, EventArgs args)
		{
			img1 = null;
			img2 = null;
			res = null;
			cat1 = null;
			cat2 = null;
			ResultViewWindow.Destroy();
		}
	}
}
