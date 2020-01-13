// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#if false

using System;
using System.Threading;

namespace DS
{
	/// <summary>
	/// GuiTimer の概要の説明です。
	/// </summary>
	public class GuiTimer
	{
#region タイマーハンドラ

		/// <summary>
		/// タイマー停止用ハンドラ
		/// </summary>
		public class Handle
		{
			Timer _timer = null;

			/// <summary>
			/// 実行禁止。Handle作成
			/// </summary>
			/// <param name="delay_ms">起動時間[ms]</param>
			/// <param name="call">コールバック関数</param>
			/// <param name="arg">コールバックパラメータ</param>
			internal Handle(int delay_ms, TimerCallback call, object arg)
			{
				_timer = new Timer(call, arg, delay_ms, System.Threading.Timeout.Infinite);
			}

			internal void _stop()
			{
				if( _timer != null )
				{
					_timer.Dispose();
					_timer = null;
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
#endregion

#region IDisposable メンバ

		public virtual void Dispose()
		{
		}

#endregion

		/// <summary>
		/// タイマーを削除する
		/// </summary>
		/// <param name="h"></param>
		public void Stop(Handle h)
		{
			if( h != null )
			{
				h._stop();
			}
		}

		/// <summary>
		/// タイマー実行
		/// </summary>
		/// <param name="arg"></param>
		private void _onTimer(object arg)
		{
			uSet val = (uSet)arg;
			((Handle)val[0])._stop();


			// 型に応じた処理
			if( val[1] is Proc0 )
			{
				Proc0 p = (Proc0)val[1];
				try
				{
					p.Method.Invoke(p.Target, null);
				}
				catch( Exception e )
				{
					System.Diagnostics.Debug.WriteLine(e.Message + " " + p.Method.Name);
				}
				return;
			}
			if( val[1] is Proc1 )
			{
				Proc1 p = (Proc1)val[1];
				try 
				{
					p.Method.Invoke(p.Target, new object[]{val[2]});
				}
				catch( Exception e)
				{
					System.Diagnostics.Debug.WriteLine(e.Message + " " + p.Method.Name);
				}
				return;
			}
			if( val[1] is ProcN )
			{
				ProcN p = (ProcN)val[1];
				try 
				{
					p.Method.Invoke(p.Target, (object[])val[2]);
				}
				catch( Exception e )
				{
					System.Diagnostics.Debug.WriteLine(e.Message + " " + p.Method.Name);
				}
				return;
			}
			System.Diagnostics.Debug.Assert(false, "タイマー致命的エラー");
		}

		/// <summary>
		/// 引数なしのタイマー関数を登録する
		/// </summary>
		/// <param name="delay_ms">起動ミリ秒</param>
		/// <param name="function">起動する関数</param>
		public Handle AddTrigger(int delay_ms, Proc0 function)
		{
			uSet st = new uSet(null, function, null);
			Handle ret = new Handle(delay_ms, new TimerCallback(_onTimer), st);
			st[0] = ret;
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
			uSet st = new uSet(null, function, arg);
			Handle ret = new Handle(delay_ms, new TimerCallback(_onTimer), st);
			st[0] = ret;
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
			uSet st = new uSet(null, function, args);
			Handle ret = new Handle(delay_ms, new TimerCallback(_onTimer), st);
			st[0] = ret;
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
