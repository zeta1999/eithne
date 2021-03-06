﻿using System;
using System.Collections;
using System.Threading;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class EngineThread
	{
		private Plugin.Base Plugin;
		private Block b;
		private Thread t;
		private Exception Error;
		private Engine2 engine;
		private bool finished = false;

		private void ThreadedWork()
		{
			Error = null;

			try
			{
				Plugin.Work();
			}
			catch(Exception e)
			{
				Error = e;
			}
		}

		public EngineThread(Engine2 engine, Block b)
		{
			this.b = b;
			this.engine = engine;

			b.Working = true;
			MainWindow.RedrawSchematic();

			try
			{
				if(b.Plugin.NumIn != 0)
				{
					CommSocket cs = new CommSocket(b.Plugin.NumIn);

					for(int i=0; i<b.Plugin.NumIn; i++)
					{
						Socket other = b.SocketIn[i].Other;

						if(other.Parent.Plugin.Out == null)
						{
							b.Working = false;
							b = other.Parent;
							throw new PluginException(Catalog.GetString("Plugin has no data on output sockets."));
						}

						cs[i] = other.Parent.Plugin.Out[other.Num];
					}

					b.Plugin.In = cs;
				}

				Plugin = b.Plugin;
				t = new Thread(ThreadedWork);
				t.Start();
			}
			catch(Exception e)
			{
				engine.Stop();

				b.ShowError = true;
				MainWindow.RedrawSchematic();
				new PluginError(e, b, false);
			}
		}

		public void Stop()
		{
			if(!finished)
			{
				Plugin.Lock();
				t.Abort();
				Plugin.Unlock();
			}

			b.Working = false;
			MainWindow.RedrawSchematic();
		}

		public bool Finished
		{
			get
			{
				if(finished)
					return true;

				try
				{
					if(t.Join(0))
					{
						finished = true;
						b.Working = false;

						if(Error != null)
							throw Error;

						MainWindow.RedrawSchematic();
					}
				}
				catch(Exception e)
				{
					engine.Stop();

					b.ShowError = true;
					MainWindow.RedrawSchematic();
					new PluginError(e, b, false);
					finished = true;
				}

				return finished;
			}
		}
	}

	class Engine2
	{
		private static int ConfigThreads = Config.Get("engine/threads", 2);
		private static bool ConfigProgress = Config.Get("block/progress", true);
		private Schematic s;
		private ArrayList Threads = new ArrayList();
		private bool stop = false;
		private bool running = false;
		private FinishCallback finish;
		private Progress progress;
		private DateTime start, end;

		public delegate void FinishCallback();
		public delegate void Progress();

		public Engine2(Schematic s, FinishCallback finish, Progress progress)
		{
			this.s = s;
			this.finish = finish;
			this.progress = progress;
		}

		private bool Tick()
		{
			if(stop)
			{
				running = false;
				end = DateTime.Now;
				finish();
				return false;
			}

			// check if any thread has finished work
			for(int i=Threads.Count-1; i>=0; i--)
				if(((EngineThread)Threads[i]).Finished)
				{
					// in case of error working is stopped
					if(stop)
					{
						running = false;
						end = DateTime.Now;
						finish();
						return false;
					}

					Threads.RemoveAt(i);
				}

			// if there are free slots for threads try to fill them
			if(Threads.Count < ConfigThreads)
			{
				ArrayList Blocks = s.Blocks;

				for(int i=0; i<Blocks.Count; i++)
				{
					// check if there are free threads
					if(Threads.Count >= ConfigThreads)
						break;

					Block b = (Block)Blocks[i];

					if(b.CheckState() == Block.State.Ready && b.Working != true)
						Threads.Add(new EngineThread(this, b));
				}
			}

			// check if there are some threads left
			if(Threads.Count == 0)
			{
				running = false;
				end = DateTime.Now;
				finish();
				return false;
			}

			progress();

			if(ConfigProgress)
				MainWindow.RedrawSchematic();

			return true;
		}

		public void Start()
		{
			// shouldn't happen
			if(running)
				throw new Exception(Catalog.GetString("Engine is already running."));

			start = DateTime.Now;

			ArrayList Blocks = s.Blocks;

			stop = false;
			running = true;

			// check all blocks and add ones ready to work to the thread list
			for(int i=0; i<Blocks.Count; i++)
			{
				// check if there are free threads
				if(Threads.Count >= ConfigThreads)
					break;

				Block b = (Block)Blocks[i];

				if(b.CheckState() == Block.State.Ready)
					Threads.Add(new EngineThread(this, b));
			}

			// check threads status every 50 ms
			GLib.Timeout.Add(50, new GLib.TimeoutHandler(Tick));
		}

		// stops all threads
		public void Stop()
		{
			stop = true;

			foreach(EngineThread t in Threads)
				t.Stop();

			Threads.RemoveRange(0, Threads.Count);
		}

		public static void CheckGConf()
		{
			ConfigThreads = Config.Get("engine/threads", 2);
			ConfigProgress = Config.Get("block/progress", true);
		}

		public bool Running
		{
			get { return running; }
		}

		public string ElapsedTime
		{
			get
			{
				TimeSpan ts = end - start;

				if(ts.Days == 0)
					if(ts.Hours == 0)
						if(ts.Minutes == 0)
							return String.Format("{0:00}.{1}", ts.Seconds, ts.Milliseconds);
						else
							return String.Format("{0:00}:{1:00}.{2}", ts.Minutes, ts.Seconds, ts.Milliseconds);
					else
						return String.Format("{0:00}:{1:00}:{2:00}.{3}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
				else
					return String.Format("{0}.{1:00}:{2:00}:{3:00}.{4}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
			}
		}
	}
}
