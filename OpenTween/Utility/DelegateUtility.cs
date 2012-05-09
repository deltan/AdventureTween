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

namespace OpenTween.Utility
{
    /// <summary>
    /// デリゲートに関するユーティリティー
    /// </summary>
    static class DelegateUtility
    {
        /// <summary>
        /// 指定したEventHandlerデリゲートを安全に呼び出します。
        /// EventHandlerがnullであってもエラーは発生しません。
        /// </summary>
        /// <param name="eventHandler">EventHanderデリゲート</param>
        /// <param name="sender">イベント送信元</param>
        /// <param name="eventArgs">イベントに渡すEventArgs型の値</param>
        public static void CallEvent(EventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, eventArgs);
            }
        }

        /// <summary>
        /// 指定したEventHanderデリゲートを安全に呼び出します。
        /// EventHandlerがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T">EventArgsを継承した型</typeparam>
        /// <param name="eventHandler">EventHanderデリゲート</param>
        /// <param name="sender">イベント送信元</param>
        /// <param name="eventArgs">イベントに渡すEventArgsを継承した型の値</param>
        public static void CallEvent<T>(EventHandler<T> eventHandler, object sender, T eventArgs) where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, eventArgs);
            }
        }

        /// <summary>
        /// 指定したActionデリゲートを安全に呼び出します。
        /// Actionデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <param name="action">Actionデリゲート</param>
        public static void CallAction(Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        /// <summary>
        /// 指定したActionデリゲートを安全に呼び出します。
        /// Actionデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T">引数の型</typeparam>
        /// <param name="action">Actionデリゲート</param>
        /// <param name="t">引数の値</param>
        public static void CallAction<T>(Action<T> action, T t)
        {
            if (action != null)
            {
                action(t);
            }
        }

        /// <summary>
        /// 指定したActionデリゲートを安全に呼び出します。
        /// Actionデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T1">1番目の引数の型</typeparam>
        /// <typeparam name="T2">2番目の引数の型</typeparam>
        /// <param name="action">Actionデリゲート</param>
        /// <param name="t1">1番目の引数</param>
        /// <param name="t2">2番目の引数</param>
        public static void CallAction<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2)
        {
            if (action != null)
            {
                action(t1, t2);
            }
        }

        /// <summary>
        /// 指定したActionデリゲートを安全に呼び出します。
        /// Actionデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T1">1番目の引数の型</typeparam>
        /// <typeparam name="T2">2番目の引数の型</typeparam>
        /// <typeparam name="T3">3番目の引数の型</typeparam>
        /// <param name="action">Actionデリゲート</param>
        /// <param name="t1">1番目の引数</param>
        /// <param name="t2">2番目の引数</param>
        /// <param name="t3">3番目の引数</param>
        public static void CallAction<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            if (action != null)
            {
                action(t1, t2, t3);
            }
        }

        /// <summary>
        /// 指定したActionデリゲートを安全に呼び出します。
        /// Actionデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T1">1番目の引数の型</typeparam>
        /// <typeparam name="T2">2番目の引数の型</typeparam>
        /// <typeparam name="T3">3番目の引数の型</typeparam>
        /// <typeparam name="T4">4番目の引数の型</typeparam>
        /// <param name="action">Actionデリゲート</param>
        /// <param name="t1">1番目の引数</param>
        /// <param name="t2">2番目の引数</param>
        /// <param name="t3">3番目の引数</param>
        /// <param name="t4">4番目の引数</param>
        public static void CallAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (action != null)
            {
                action(t1, t2, t3, t4);
            }
        }

        /// <summary>
        /// 指定したFuncデリゲートを安全に呼び出します。
        /// Funcデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="TR">戻り値の型</typeparam>
        /// <param name="func">Funcデリゲート</param>
        /// <returns>戻り値。Funcデリゲートがnullの場合は、戻り値の型が参照型であればnull、値型であれば0が返る。</returns>
        public static TR CallFunc<TR>(Func<TR> func)
        {
            if (func != null)
            {
                return func();
            }
            else
            {
                return default(TR);
            }
        }

        /// <summary>
        /// 指定したFuncデリゲートを安全に呼び出します。
        /// Funcデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T">引数の型</typeparam>
        /// <typeparam name="TR">戻り値の型</typeparam>
        /// <param name="func">Funcデリゲート</param>
        /// <param name="t">引数</param>
        /// <returns>戻り値。Funcデリゲートがnullの場合は、戻り値の型が参照型であればnull、値型であれば0が返る。</returns>
        public static TR CallFunc<T, TR>(Func<T, TR> func, T t)
        {
            if (func != null)
            {
                return func(t);
            }
            else
            {
                return default(TR);
            }
        }

        /// <summary>
        /// 指定したFuncデリゲートを安全に呼び出します。
        /// Funcデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T1">1番目の引数の型</typeparam>
        /// <typeparam name="T2">2番目の引数の型</typeparam>
        /// <typeparam name="TR">戻り値の型</typeparam>
        /// <param name="func">Funcデリゲート</param>
        /// <param name="t1">1番目の引数</param>
        /// <param name="t2">2番目の引数</param>
        /// <returns>戻り値。Funcデリゲートがnullの場合は、戻り値の型が参照型であればnull、値型であれば0が返る。</returns>
        public static TR CallFunc<T1, T2, TR>(Func<T1, T2, TR> func, T1 t1, T2 t2)
        {
            if (func != null)
            {
                return func(t1, t2);
            }
            else
            {
                return default(TR);
            }
        }

        /// <summary>
        /// 指定したFuncデリゲートを安全に呼び出します。
        /// Funcデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T1">1番目の引数の型</typeparam>
        /// <typeparam name="T2">2番目の引数の型</typeparam>
        /// <typeparam name="T3">3番目の引数の型</typeparam>
        /// <typeparam name="TR">戻り値の型</typeparam>
        /// <param name="func">Funcデリゲート</param>
        /// <param name="t1">1番目の引数</param>
        /// <param name="t2">2番目の引数</param>
        /// <param name="t3">3番目の引数</param>
        /// <returns>戻り値。Funcデリゲートがnullの場合は、戻り値の型が参照型であればnull、値型であれば0が返る。</returns>
        public static TR CallFunc<T1, T2, T3, TR>(Func<T1, T2, T3, TR> func, T1 t1, T2 t2, T3 t3)
        {
            if (func != null)
            {
                return func(t1, t2, t3);
            }
            else
            {
                return default(TR);
            }
        }

        /// <summary>
        /// 指定したFuncデリゲートを安全に呼び出します。
        /// Funcデリゲートがnullであってもエラーは発生しません。
        /// </summary>
        /// <typeparam name="T1">1番目の引数の型</typeparam>
        /// <typeparam name="T2">2番目の引数の型</typeparam>
        /// <typeparam name="T3">3番目の引数の型</typeparam>
        /// <typeparam name="T4">4番目の引数の型</typeparam>
        /// <typeparam name="TR">戻り値の型</typeparam>
        /// <param name="func">Funcデリゲート</param>
        /// <param name="t1">1番目の引数</param>
        /// <param name="t2">2番目の引数</param>
        /// <param name="t3">3番目の引数</param>
        /// <param name="t4">4番目の引数</param>
        /// <returns>戻り値。Funcデリゲートがnullの場合は、戻り値の型が参照型であればnull、値型であれば0が返る。</returns>
        public static TR CallFunc<T1, T2, T3, T4, TR>(Func<T1, T2, T3, T4, TR> func, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (func != null)
            {
                return func(t1, t2, t3, t4);
            }
            else
            {
                return default(TR);
            }
        }
    }
}
