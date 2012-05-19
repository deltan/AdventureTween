// OpenTween - Client of Twitter
// Copyright (c) 2012      kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
// All rights reserved.
// 
// This file is part of OpenTween.
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
// 
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General public License
// for more details.
// 
// You should have received a copy of the GNU General public License along
// with this program. If not, see <http://www.gnu.org/licenses/>, or write to
// the Free Software Foundation, Inc., 51 Franklin Street - Fifth Floor,
// Boston, MA 02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTween
{
    /// <summary>
    /// アプリケーション固有の情報を格納します
    /// </summary>
    /// <remarks>
    /// OpenTween の派生版を作る方法は http://sourceforge.jp/projects/opentween/wiki/HowToFork を参照して下さい。
    /// </remarks>
    internal sealed class ApplicationSettings
    {
        //=====================================================================
        // フィードバック送信先
        // 異常終了時などにエラーログ等とともに表示されます。
        
        /// <summary>
        /// フィードバック送信先 (メール)
        /// </summary>
        public const string FeedbackEmailAddress = "deltanpayo@gmail.com";

        /// <summary>
        /// フィードバック送信先 (Twitter)
        /// </summary>
        public const string FeedbackTwitterName = "@deltan12345";

        //=====================================================================
        // Web サイト

        /// <summary>
        /// 「ヘルプ」メニューの「(アプリ名) ウェブサイト」クリック時に外部ブラウザで表示する URL
        /// </summary>
        public const string WebsiteUrl = "https://github.com/deltan/AdventureTween";

        /// <summary>
        /// 「ヘルプ」メニューの「ショートカットキー一覧」クリック時に外部ブラウザで表示する URL
        /// </summary>
        /// <remarks>
        /// Tween の Wiki ページのコンテンツはプロプライエタリなため転載不可
        /// </remarks>
        public const string ShortcutKeyUrl = "http://sourceforge.jp/projects/tween/wiki/%E3%82%B7%E3%83%A7%E3%83%BC%E3%83%88%E3%82%AB%E3%83%83%E3%83%88%E3%82%AD%E3%83%BC";

        //=====================================================================
        // アップデートチェック関連

        /// <summary>
        /// 最新バージョンの情報を取得するためのURL
        /// </summary>
        /// <remarks>
        /// version.txt のフォーマットについては http://sourceforge.jp/projects/opentween/wiki/VersionTxt を参照。
        /// </remarks>
        public const string VersionInfoUrl = "http://www.opentween.org/status/version.txt";

        //=====================================================================
        // Twitter
        // https://dev.twitter.com/ から取得できます。

        /// <summary>
        /// Twitter コンシューマーキー
        /// </summary>
        public const string TwitterConsumerKey = "efO6ZLdzivdjVwGMNwAhw";
        public const string TwitterConsumerSecret = "bHjLiyrjoTth3uCT4p3H81SSjC7PmBdOTLUuQfeA9w";

        //=====================================================================
        // Lockerz (旧Plixi)
        // https://admin.plixi.com/Api.aspx から取得できます。

        /// <summary>
        /// Lockerz APIキー
        /// </summary>
        public const string LockerzApiKey = "faf25f85-3563-4dfa-94ff-27a87f6174c8";

        //=====================================================================
        // Twitpic
        // http://dev.twitpic.com/apps/new から取得できます。

        /// <summary>
        /// Twitpic APIキー
        /// </summary>
        public const string TwitpicApiKey = "7a5a708032fb57a4ac385d4455c68eba";

        //=====================================================================
        // TwitVideo
        // http://twitvideo.jp/api_forms/ から申請できます。

        /// <summary>
        /// TwitVideo コンシューマキー
        /// </summary>
        public const string TwitVideoConsumerKey = "";

        //=====================================================================
        // yfrog
        // http://stream.imageshack.us/api/ から取得できます。

        /// <summary>
        /// yfrog APIキー
        /// </summary>
        public const string YfrogApiKey = "05DEQSVWab6be890e09762ab895153a1726a3920";

        //=====================================================================
        // Bing
        // http://www.bing.com/toolbox/bingdeveloper/ から取得できます。

        /// <summary>
        /// Bing AppId
        /// </summary>
        public const string BingAppId = "481A885564E130699BC12D46885352F751A9A294";

        //=====================================================================
        // Foursquare
        // https://developer.foursquare.com/ から取得できます。

        /// <summary>
        /// Foursquare Client Id
        /// </summary>
        public const string FoursquareClientId = "WXPNTZ0ABJQ2BUPTCOIZPNFZY1KN12OSBQZ2QYZIQD35T0J4";

        /// <summary>
        /// Foursquare Client Secret
        /// </summary>
        public const string FoursquareClientSecret = "WFWI4JSYVNY2OWTRXXKMA2JBNJMGWKSFH1KVMG4SMXAPUUE2";

        //=====================================================================
        // bit.ly
        // https://bitly.com/a/account から取得できます。

        /// <summary>
        /// bit.ly ログイン名
        /// </summary>
        public const string BitlyLoginId = "adventuretween";

        /// <summary>
        /// bit.ly APIキー
        /// </summary>
        public const string BitlyApiKey = "R_d2fdac04dd213233f37228dc96ab5ea2";

        //=====================================================================
        // TINAMI
        // http://www.tinami.com/api/ から取得できます。

        /// <summary>
        /// TINAMI APIキー
        /// </summary>
        public const string TINAMIApiKey = "4f8ad0b7dec6f";
    }
}
