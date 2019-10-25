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
	/// GuiTimer �̊T�v�̐����ł��B
	/// �t�B�[�`���[�Ƀ^�C�}�[�@�\��ǉ�����ꍇ�Ɏg�p����B
	/// FeatureTripSelector���Q��
	/// </summary>
	public class GuiTimer : IDisposable
	{
#region �^�C�}�[�n���h��

		/// <summary>
		/// �^�C�}�[��~�p�n���h��
		/// </summary>
		public class Handle
		{
			long _sigTicks;
			int _setms;
			object _function;
			object[] _args;

			/// <summary>
			/// �B��̃R���X�g���N�^�i�A�Z���u�����璼�ڃR�[�������G���[�U�[���s�֎~�j
			/// </summary>
			/// <param name="ms">�^�C�}�[�̃~���b��</param>
			/// <param name="function">�^�C�}�[�N���I�u�W�F�N�g</param>
			/// <param name="args">�^�C�}�[�N�����̈���</param>
			private Handle(int ms, object function, object[] args)
			{
				_setms = ms;
				_function = function;
				_args = args;
				_sigTicks = DateTime.Now.Ticks + ms * 10000 + (_counter % 100);
			}

			/// <summary>
			/// �^�C�}�[�ɐݒ肳��Ă���R�}���h�����s����
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
			/// �^�C�}�[���N������Ƃ���DateTime.Ticks
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

#region �֐��^

		/// <summary>�����Ȃ��֐��^</summary>
		public delegate void Proc0();

		/// <summary>�����ЂƂ֐��^</summary>
		public delegate void Proc1(object arg);

		/// <summary>�����}���`�֐��^</summary>
		public delegate void ProcN(object[] args);

#endregion

#region �����i�V���A���C�Y���Ȃ��j

		private static int _counter = 0;
		private SortedList _dat = new SortedList(new uSorter.IComparerLong());
		private Handle _current = null;
		private static Thread _hThread = null;

#endregion

#region IDisposable �����o

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
		/// �^�C�}�[�I�u�W�F�N�g�̃��C������
		/// </summary>
		public GuiTimer()
		{
		}

		/// <summary>
		/// �X���b�h�Ń^�C�}�[�L�b�N���Ď��E���s����
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
		/// Handle�̃C���X�^���X�𐶐�����
		/// </summary>
		/// <returns>�V����Handle</returns>
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

			// �^�C�}�[���g�p����ꍇ�X���b�h���N������
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
		/// �^�C�}�[���폜����
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
		/// �����Ȃ��̃^�C�}�[�֐���o�^����
		/// </summary>
		/// <param name="delay_ms">�N���~���b</param>
		/// <param name="function">�N������֐�</param>
		public Handle AddTrigger(int delay_ms, Proc0 function)
		{
			Handle ret = _createHandle(delay_ms, function, null);
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
			Handle ret = _createHandle(delay_ms, function, new object[]{arg});
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
			Handle ret = _createHandle(delay_ms, function, args);
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
