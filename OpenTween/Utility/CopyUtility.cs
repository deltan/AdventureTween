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
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenTween.Utility
{
    /// <summary>
    /// コピーに関するユーティリティー
    /// </summary>
    static class CopyUtility
    {
        /// <summary>
        /// ディープコピーします。
        /// コピーするクラスにSerializable()属性が必要です。
        /// コピーしたいインスタンスがnullの場合は例外が発生します。
        /// </summary>
        /// <typeparam name="T">コピーしたいクラスの型（Serializable属性がついていること）</typeparam>
        /// <param name="original">コピーしたいインスタンス</param>
        /// <returns>コピーされた新しいインスタンス</returns>
        public static T DeepCopy<T>(T original)
        {            
            T copy = default(T);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, original);
                ms.Seek(0, SeekOrigin.Begin);
                copy = (T)formatter.Deserialize(ms);
            }
            return copy;
        }
    }
}
