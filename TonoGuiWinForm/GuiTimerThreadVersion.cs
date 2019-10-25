// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#if false

using System;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace DS
{
	/// <summary>
	/// GuiTimer の概要の説明です。
	/// フィーチャーにタイマー機能を追加する場合に使用する。
	/// FeatureTripSelectorを参照
	/// </summary>
	public class GuiTimer : IDisposable
	{
#region タイマーハンドラ

		/// <summary>
		/// タイマー停止用ハンドラ
		/// </summary>
		public class Handle
		{
			long _sigTicks;
			int _setms;
			object _function;
			object[] _args;

			/// <summary>
			/// 唯一のコンストラクタ（アセンブリから直接コールされる；ユーザー実行禁止）
			/// </summary>
			/// <param name="ms">タイマーのミリ秒数</param>
			/// <param name="function">タイマー起動オブジェクト</param>
			/// <param name="args">タイマー起動時の引数</param>
			private Handle(int ms, object function, object[] args)
			{
				_setms = ms;
				_function = function;
				_args = args;
				_sigTicks = DateTime.Now.Ticks + ms * 10000 + (_counter % 100);
			}

			/// <summary>
			/// タイマーに設定されているコマンドを実行する
			/// </summary>
			public void Invoke()
			{
				if( _function is Proc0 )
				{
					((Proc0)_function)();
				}
				if( _function is Proc1 )
				{
					((Proc1)_function)(_args[0]);
				}
				if( _function is ProcN )
				{
					((ProcN)_function)(_args);
				}
			}

			/// <summary>
			/// タイマーが起動するときのDateTime.Ticks
			/// </summary>
			public long SignalTicks
			{
				get
				{
					return _sigTicks;
				}
				set
				{
					_sigTicks = value;
				}
			}
		}
#endregion

#region 関数型

		/// <summary>引数なし関数型</summary>
		public delegate void Proc0();

		/// <summary>引数ひとつ関数型</summary>
		public delegate void Proc1(object arg);

		/// <summary>引数マルチ関数型</summary>
		public delegate void ProcN(object[] args);

#endregion

#region 属性（シリアライズしない）

		private static int _counter = 0;
		private SortedList _dat = new SortedList(new uSorter.IComparerLong());
		private Handle _current = null;
		private static Thread _hThread = null;

#endregion

#region IDisposable メンバ

		public virtual void Dispose()
		{
			if( _hThread != null )
			{
				_hThread.Abort();
				_hThread = null;
				_dat.Clear();
				_current = null;
			}
		}

#endregion

		/// <summary>
		/// タイマーオブジェクトのメイン処理
		/// </summary>
		public GuiTimer()
		{
		}

		/// <summary>
		/// スレッドでタイマーキックを監視・実行する
		/// </summary>
		private void timerThread()
		{
			for( ;; )
			{
				Thread.Sleep(50);
				if( _current != null )
				{
					//System.Diagnostics.Debug.WriteLine("            TIMER WAITING..." + (aa++).ToString());
					if( DateTime.Now.Ticks > _current.SignalTicks )
					{
						_current.Invoke();
						Stop(_current);
					}
				}
			}
		}

		/// <summary>
		/// Handleのインスタンスを生成する
		/// </summary>
		/// <returns>新しいHandle</returns>
		private Handle _createHandle(int ms, object function, object[] args)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			ConstructorInfo ci = typeof(Handle).GetConstructor(flags, null, new Type[]{typeof(int), typeof(object), typeof(object[])}, null);
			Handle h = (Handle)ci.Invoke(new object[]{ms, function, args});

			while( _dat.Contains(h.SignalTicks))
			{
				h.SignalTicks = h.SignalTicks + 1;
				_counter++;
			}
			_dat[h.SignalTicks] = h;
			if( (long)_dat.GetKey(0) == h.SignalTicks )
			{
				_current = h;
			}

			// タイマーを使用する場合スレッドを起動する
			if( _hThread == null )
			{
				_hThread = new Thread(new ThreadStart(timerThread));
				_hThread.Name = "GuiTimer.timerThread";
				_hThread.Priority = ThreadPriority.Highest;
				System.Diagnostics.Debug.WriteLine("GuiTimer Process is started");
				_hThread.Start();
			}
			return (Handle)h;
		}

		/// <summary>
		/// タイマーを削除する
		/// </summary>
		/// <param name="h"></param>
		public void Stop(Handle h)
		{
			if( h == null )
			{
				return;
			}
			_dat.Remove(h.SignalTicks);
			if( object.ReferenceEquals(h, _current))
			{
				if( _dat.Count > 0 )
				{
					_current = (Handle)_dat.GetByIndex(0);
				} 
				else 
				{
					_current = null;
				}
			}
		}

		/// <summary>
		/// 引数なしのタイマー関数を登録する
		/// </summary>
		/// <param name="delay_ms">起動ミリ秒</param>
		/// <param name="function">起動する関数</param>
		public Handle AddTrigger(int delay_ms, Proc0 function)
		{
			Handle ret = _createHandle(delay_ms, function, null);
			return ret;
		}

		/// <summary>
		/// 引数なしのタイマー関数を登録する
		/// </summary>
		/// <param name="delayTime">起動時間</param>
		/// <param name="function">起動する関数</param>
		public Handle AddTrigger(uTime delayTime, Proc0 function)
		{
			return AddTrigger(delayTime.TotalSeconds * 1000, function);
		}

		/// <summary>
		/// 引数付きのタイマー関数を登録する
		/// </summary>
		/// <param name="id">受信時の引数</param>
		/// <param name="delay_ms">起動ミリ秒</param>
		/// <param name="function">起動する関数</param>
		public Handle AddTrigger(object arg, int delay_ms, Proc1 function)
		{
			Handle ret = _createHandle(delay_ms, function, new object[]{arg});
			return ret;
		}

		/// <summary>
		/// 引数付きのタイマー関数を登録する
		/// </summary>
		/// <param name="id">受信時の引数</param>
		/// <param name="delayTime">起動時間</param>
		/// <param name="function">起動する関数</param>
		public Handle AddTrigger(object arg, uTime delayTime, Proc1 function)
		{
			return AddTrigger(arg, delayTime.TotalSeconds * 1000, function);
		}

		/// <summary>
		/// 引数付きのタイマー関数を登録する
		/// </summary>
		/// <param name="id">受信時の引数</param>
		/// <param name="delay_ms">起動ミリ秒</param>
		/// <param name="function">起動する関数</param>
		public Handle AddTrigger(object[] args, int delay_ms, ProcN function)
		{
			Handle ret = _createHandle(delay_ms, function, args);
			return ret;
		}

		/// <summary>
		/// 引数付きのタイマー関数を登録する
		/// </summary>
		/// <param name="id">受信時の引数</param>
		/// <param name="delayTime">起動時間</param>
		/// <param name="function">起動する関数</param>
		public Handle AddTrigger(object[] args, uTime delayTime, ProcN function)
		{
			return AddTrigger(args, delayTime.TotalSeconds * 1000, function);
		}
	}
}
#endif
