﻿using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class FFTInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Fast Fourier Transform"); }
		}

		public override string ShortName
		{
			get { return "FFT"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin performs fast fourier transform."); }
		}
	}

	public class FFTFactory : IFactory
	{
		IInfo _info = new FFTInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.ImgProc; }
		}

		public void Initialize()
		{
		}

		public Plugin.Base Create()
		{
			return new FFTPlugin();
		}
	}

	public class FFTPlugin : Plugin.ImgProc
	{
		private bool zero = true;
		private float progress = 0;

		public FFTPlugin()
		{
			_info = new FFTInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(bool z)
		{
			zero = z;
			_block.SlotsChanged();
		}

		public override void Setup()
		{
			new FFTSetup(zero, UpdateValue, false);
		}

		public override void Work()
		{
			progress = 0;

			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] res = new IImage[img.Length];
			_out = new CommSocket(1);

			PreparePlan(img[0].W, img[0].H);

			for(int i=0; i<img.Length; i++)
			{
				res[i] = FFT(img[i]);
				progress = (float)i/img.Length;
			}

			_out[0] = new ICommImage(res, socket.OriginalImages, socket.Categories);

			FFTW.Cleanup();

			_workdone = true;
		}

		// this function does nothing, but without it, when first plan is created using real images, for some parameters
		// the first output image is empty and the rest is fine, so it's better to create here this no-use plan just
		// to be sure that everything works
		private void PreparePlan(int w, int h)
		{
			double[] d1 = new double[w * h * 2];
			double[] d2 = new double[w * h * 2];

			IntPtr plan = FFTW.PlanDFT2D(h, w, d1, d2, FFTW.Direction.Forward, 0);

			FFTW.DestroyPlan(plan);
		}

		private IImage FFT(IImage img)
		{
			double[] datain = new double[img.W * img.H * 2];
			double[] dataout = new double[img.W * img.H * 2];

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W; x++)
				{
					datain[(x + y*img.W) * 2] = (byte)img[x, y];
					datain[(x + y*img.W) * 2 + 1] = 0;
				}

			IntPtr plan = FFTW.PlanDFT2D(img.H, img.W, datain, dataout, FFTW.Direction.Forward, 0);

			FFTW.Execute(plan);

			FFTW.DestroyPlan(plan);

			IImage ret = new IImage(BPP.Float, img.W, img.H);

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W; x++)
				{
					double re = dataout[(x + y*img.W) * 2];
					double im = dataout[(x + y*img.W) * 2 + 1];
					double val = Math.Sqrt(re * re + im * im);

					int newx = (x + img.W/2)%img.W;
					int newy = (y + img.H/2)%img.H;

					ret[newx, newy] = (float)val/255f;
				}

			if(zero)
				ret[img.W/2, img.H/2] = 0f;

			return ret;
		}

		public override void Lock()
		{
			FFTW.mutex.WaitOne();
		}

		public override void Unlock()
		{
			FFTW.mutex.ReleaseMutex();
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			if(zero)
				root.InnerText = "1";
			else
				root.InnerText = "0";
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			if(root.InnerText == "1")
				UpdateValue(true);
			else
				UpdateValue(false);
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("FFT of image.");
		}

		private static string[] matchin   = new string[] { "image/grayscale" };
		private static string[] matchout  = new string[] { "image/float" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }

		public override float Progress { get { return progress; } }
	}
}
