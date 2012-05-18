// AdventureTween - Client of Twitter
// Copyright (c) 2012      deltan (@deltan12345) <deltanpayo@gmail.com>
// All rights reserved.
// 
// This file is part of AdventureTween.
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
// 
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
// for more details. 
// 
// You should have received a copy of the GNU General Public License along
// with this program. If not, see <http://www.gnu.org/licenses/>, or write to
// the Free Software Foundation, Inc., 51 Franklin Street - Fifth Floor,
// Boston, MA 02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenTween.Utility
{
    static class FileUtility
    {
        /// <summary>
        /// 指定したパスに使用できない文字が含まれているかチェックします。
        /// Path.GetInvalidPathChars()メソッドの結果を利用しています。
        /// このメソッドの説明にあるように、すべての無効な文字が取得できることを保証されていないため、
        /// すべての無効な文字をチェックすることも保証できません。
        /// </summary>
        /// <param name="path">チェックするパス</param>
        /// <returns>パスに無効な文字が含まれていたらtrue,含まれていなかったらfalse。
        /// 実際には無効の文字が含まれていても、チェックされずにスルーしてしまった場合はfalseが返されます</returns>
        public static bool ContainsInvalidPathChars(string path)
        {
            var invalidChars = Path.GetInvalidPathChars();
            return path.IndexOfAny(invalidChars) >= 0;
        }

        /// <summary>
        /// 指定したファイル名に使用できない文字が含まれているかチェックします。
        /// Path.GetInvalidFileNameChars()メソッドの結果を利用しています。
        /// このメソッドの説明にあるように、すべての無効な文字が取得できることを保証されていないため、
        /// すべての無効な文字をチェックすることも保証できません。
        /// </summary>
        /// <param name="fileName">チェックするファイル名</param>
        /// <returns>ファイル名に無効な文字が含まれていたらtrue,含まれていなかったらfalse。
        /// 実際には無効の文字が含まれていても、チェックされずにスルーしてしまった場合はfalseが返されます。</returns>
        public static bool ContainsInvalidFileNameChars(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return fileName.IndexOfAny(invalidChars) >= 0;
        }
    }
}
