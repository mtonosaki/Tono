// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �d���`�F�b�N��񋟂���C���^�[�t�F�[�X
    /// </summary>
    public interface IPartsFriendship
    {
        void AddFriendParts(PartsBase value);
        void RemoveFriendParts(PartsBase value);
        void ClearFriendParts();
        bool ContainsFriendParts(PartsBase value);

        bool HasFriend
        {
            get;
        }

        ICollection GetFriendPartsID();
    }
}
