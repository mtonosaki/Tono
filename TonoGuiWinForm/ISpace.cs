namespace Tono.GuiWinForm
{
    /// <summary>
    /// Class1 �̊T�v�̐����ł��B
    /// </summary>
    public interface ISpace
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsIn(object value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Transfer(object value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Inflate(object value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Deflate(object value);
    }
}
