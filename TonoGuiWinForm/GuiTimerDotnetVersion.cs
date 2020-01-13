// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#if false

using System;
using System.Threading;

namespace DS
{
	/// <summary>
	/// GuiTimer �̊T�v�̐����ł��B
	/// </summary>
	public class GuiTimer
	{
#region �^�C�}�[�n���h��

		/// <summary>
		/// �^�C�}�[��~�p�n���h��
		/// </summary>
		public class Handle
		{
			Timer _timer = null;

			/// <summary>
			/// ���s�֎~�BHandle�쐬
			/// </summary>
			/// <param name="delay_ms">�N������[ms]</param>
			/// <param name="call">�R�[���o�b�N�֐�</param>
			/// <param name="arg">�R�[���o�b�N�p�����[�^</param>
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

#region �֐��^

		/// <summary>�����Ȃ��֐��^</summary>
		public delegate void Proc0();

		/// <summary>�����ЂƂ֐��^</summary>
		public delegate void Proc1(object arg);

		/// <summary>�����}���`�֐��^</summary>
		public delegate void ProcN(object[] args);

#endregion

#region �����i�V���A���C�Y���Ȃ��j
#endregion

#region IDisposable �����o

		public virtual void Dispose()
		{
		}

#endregion

		/// <summary>
		/// �^�C�}�[���폜����
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
		/// �^�C�}�[���s
		/// </summary>
		/// <param name="arg"></param>
		private void _onTimer(object arg)
		{
			uSet val = (uSet)arg;
			((Handle)val[0])._stop();


			// �^�ɉ���������
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
			System.Diagnostics.Debug.Assert(false, "�^�C�}�[�v���I�G���[");
		}

		/// <summary>
		/// �����Ȃ��̃^�C�}�[�֐���o�^����
		/// </summary>
		/// <param name="delay_ms">�N���~���b</param>
		/// <param name="function">�N������֐�</param>
		public Handle AddTrigger(int delay_ms, Proc0 function)
		{
			uSet st = new uSet(null, function, null);
			Handle ret = new Handle(delay_ms, new TimerCallback(_onTimer), st);
			st[0] = ret;
			return ret;
		}

		/// <summary>
		/// �����Ȃ��̃^�C�}�[�֐���o�^����
		/// </summary>
		/// <param name="delayTime">�N������</param>
		/// <param name="function">�N������֐�</param>
		public Handle AddTrigger(uTime delayTime, Proc0 function)
		{
			return AddTrigger(delayTime.TotalSeconds * 1000, function);
		}

		/// <summary>
		/// �����t���̃^�C�}�[�֐���o�^����
		/// </summary>
		/// <param name="id">��M���̈���</param>
		/// <param name="delay_ms">�N���~���b</param>
		/// <param name="function">�N������֐�</param>
		public Handle AddTrigger(object arg, int delay_ms, Proc1 function)
		{
			uSet st = new uSet(null, function, arg);
			Handle ret = new Handle(delay_ms, new TimerCallback(_onTimer), st);
			st[0] = ret;
			return ret;
		}

		/// <summary>
		/// �����t���̃^�C�}�[�֐���o�^����
		/// </summary>
		/// <param name="id">��M���̈���</param>
		/// <param name="delayTime">�N������</param>
		/// <param name="function">�N������֐�</param>
		public Handle AddTrigger(object arg, uTime delayTime, Proc1 function)
		{
			return AddTrigger(arg, delayTime.TotalSeconds * 1000, function);
		}

		/// <summary>
		/// �����t���̃^�C�}�[�֐���o�^����
		/// </summary>
		/// <param name="id">��M���̈���</param>
		/// <param name="delay_ms">�N���~���b</param>
		/// <param name="function">�N������֐�</param>
		public Handle AddTrigger(object[] args, int delay_ms, ProcN function)
		{
			uSet st = new uSet(null, function, args);
			Handle ret = new Handle(delay_ms, new TimerCallback(_onTimer), st);
			st[0] = ret;
			return ret;
		}

		/// <summary>
		/// �����t���̃^�C�}�[�֐���o�^����
		/// </summary>
		/// <param name="id">��M���̈���</param>
		/// <param name="delayTime">�N������</param>
		/// <param name="function">�N������֐�</param>
		public Handle AddTrigger(object[] args, uTime delayTime, ProcN function)
		{
			return AddTrigger(args, delayTime.TotalSeconds * 1000, function);
		}
	}
}

#endif
