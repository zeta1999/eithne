﻿using System;

namespace Eithne
{
	public class ICommResult : ICommObject
	{
		private readonly double identity;
		private readonly IImage[] origbase;
		private readonly IImage[] origtest;
		private readonly IResult[] res;
		private readonly int[] catbase;
		private readonly int[] cattest;
		private readonly bool[] match;

		public ICommResult(IResult[] res, double identity, IImage[] origbase, IImage[] origtest, int[] catbase, int[] cattest)
		{
			this.identity = identity;
			this.res = res;
			this.origbase = origbase;
			this.origtest = origtest;
			this.catbase = catbase;
			this.cattest = cattest;

			match = new bool[res.Length];

			for(int i=0; i<res.Length; i++)
				match[i] = true;
		}

		public ICommResult(IResult[] res, double identity, IImage[] origbase, IImage[] origtest, int[] catbase, int[] cattest,
				bool[] match)
		{
			this.identity = identity;
			this.res = res;
			this.origbase = origbase;
			this.origtest = origtest;
			this.catbase = catbase;
			this.cattest = cattest;
			this.match = match;
		}

		public double this [int itest, int ibase]
		{
			get { return res[itest][ibase]; }
		}

		public IResult this [int n]
		{
			get { return res[n]; }
		}

		public int Length
		{
			get { return res.Length; }
		}

		public double Identity
		{
			get { return identity; }
		}

		public IImage[] OriginalBaseImages
		{
			get { return origbase; }
		}

		public IImage[] OriginalTestImages
		{
			get { return origtest; }
		}

		public int[] TestCategories
		{
			get { return cattest; }
		}

		public int[] BaseCategories
		{
			get { return catbase; }
		}

		public bool[] Match
		{
			get { return match; }
		}

		public int TestCategory(int n)
		{
			return cattest[n];
		}

		public int BaseCategory(int n)
		{
			return catbase[n];
		}

		public double Difference(int itest, int ibase)
		{
			return Math.Abs(identity - this[itest, ibase]);
		}

		public int[] FindResultsSimple()
		{
			int[] res = new int[Length];

			for(int i=0; i<Length; i++)
			{
				double min = Difference(i, 0);
				int n = 0;

				for(int j=1; j<this[i].Length; j++)
					if(Difference(i, j) < min)
					{
						min = Difference(i, j);
						n = j;
					}

				res[i] = n;
			}

			return res;
		}

		public int[][] FindResults()
		{
			int[][] res = new int[Length][];

			for(int i=0; i<Length; i++)
			{
				ResultSorter rs = new ResultSorter(Identity, this[i].Data);

				res[i] = new int[this[i].Length];
				for(int j=0; j<this[i].Length; j++)
					res[i][j] = j;

				Array.Sort(res[i], rs);
			}

			return res;
		}
	}
}
