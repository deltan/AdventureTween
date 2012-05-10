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
using System.Threading.Tasks;

namespace OpenTween.UpdateLimitNotification
{
    /// <summary>
    /// 規制通知を行うクラス
    /// 
    /// このクラスはスレッドセーフ、にしているつもりです。
    /// 
    /// このクラスは、Twitterクラスに依存していて、Twitterクラスが接続しているアカウントの規制情報を管理し、
    /// 規制通知の投稿もTwitterクラスが接続しているアカウントに対して行います。
    /// Twitterクラスが接続しているアカウントが変更された場合は、
    /// 直ちにそれを認識して、変更された新しいアカウントの規制情報を作成します。
    /// 
    /// 現在AdventureTweenはTwitterクラスを1つだけ使っていて、
    /// アカウント簡易切り替え時には、Twitterクラスが接続しているアカウントを切り替えて、
    /// 同時に1つのアカウントだけ使うような使い方になっていますので、
    /// このクラスも同時に1つのアカウントの規制情報だけ管理するようになります。
    /// 
    /// 最終的に、マルチアカウントを考慮するなら、アカウントごとにTwitterクラスを持ち、
    /// そのTwitterアカウントごとにこのクラスがあるようになればいいかもしれません。
    /// </summary>
    sealed class UpdateLimitNotification
    {
        /// <summary>
        /// 規制通知のための情報を保持するクラス
        /// </summary>
        private class NotifyInformation
        {
            public PostClass SectionStartPost { get; set; }
            public DateTime? LastSectionEndTime { get; set; }
            public IDictionary<long, PostClass> PostInSection { get; set; }
            public IList<PostClass> PostInFinding { get; set; }

            public bool IsAccuracy { get; set; }
            public bool IsFinding { get; set; }
            public bool IsNoticed { get; set; }
        }

        /// <summary>
        /// 規制通知の状態を表すクラス
        /// </summary>
        public class Status
        {
            /// <summary>
            /// 規制通知が開始されているかどうかをあらわすフラグ。
            /// </summary>
            public bool IsStarted { get; set; }

            /// <summary>
            /// セクションを検索中かどうかをあらわすフラグ。
            /// </summary>
            public bool IsFinding { get; set; }

            /// <summary>
            /// 通知情報が正確かどうかをあらわすフラグ。
            /// </summary>
            public bool IsAccuracy { get; set; }

            /// <summary>
            /// セクション中に規制通知を行ったかをあらわすフラグ。
            /// </summary>
            public bool IsNoticed { get; set; }

            /// <summary>
            /// セクションの開始ポスト
            /// </summary>
            public PostClass SectionStartPost { get; set; }

            /// <summary>
            /// セクション中に投稿した回数。
            /// </summary>
            public int CountInSection { get; set; }

            /// <summary>
            /// 規制解除時刻
            /// </summary>
            public DateTime ReleaseDate { get; set; }

            /// <summary>
            /// 規制解除時刻を設定したフォーマットで文字列化したもの
            /// </summary>
            public string ReleaseDateString { get; set; }
        }

        #region イベント

        /// <summary>
        /// FindingCompletedイベント
        /// 
        /// このイベントは、StartAsyncメソッドを実行すると開始される
        /// セクションを探す処理が成功したことを通知するイベントです。
        /// このイベントが発生した場合、規制通知は開始されています。
        /// </summary>
        public event EventHandler FindingCompleted;

        /// <summary>
        /// FindingErrorイベント
        /// 
        /// このイベントは、StartAsyncメソッドを実行すると開始される
        /// セクションを探す処理がエラーで失敗したことを通知するイベントです。
        /// このイベントが発生した場合、規制通知は開始されません。
        /// </summary>
        public event EventHandler<Event.AggregateExceptionEventArgs> FindingError;

        /// <summary>
        /// NotifyErrorイベント
        /// 
        /// このイベントは規制通知の際に発生したエラーを通知するイベントです。
        /// </summary>
        public event EventHandler<Event.AggregateExceptionEventArgs> NotifyError;
        #endregion

        private const int FINDING_GET_COUNT = 200;
        private const int SECTION_HOUR = 3;

        // 他クラス
        private Twitter Twitter { get; set; }

        // 設定
        private int NotifyCount { get; set; }
        private string NotificationMessage { get; set; }
        private string LimitReleaseDateFormat { get; set; }
        private string NotAccuracyMessage { get; set; }

        private NotifyInformation NotifyInfo { get; set; }
        
        public bool IsStart { get; private set; }

        /// <summary>
        /// 現時点における規制通知の状態を、Statusクラスにセットした形で取得できます。
        /// すべての値はオリジナルのコピーです。
        /// 取得したクラスの値を変更しても、規制通知の動作および状態には影響を及ぼしません。
        /// 
        /// 規制通知が開始されていない場合、IsStartedにfalseがセットされ、
        /// それ以外の値は初期値がセットされます。
        /// セクション検索中の場合、IsStartedとIsFindingにtrueがセットされ、
        /// それ以外の値は初期値がセットされます。
        /// 
        /// それ以外で、規制通知が正常に稼動している場合は、
        /// ある１つの時点における値がすべての変数にセットされることが保証されます。
        /// </summary>
        public Status CurrentStatus
        {
            get
            {
                NotifyInformation notifyInfo;
                Status currentStatus = new Status();
                lock (SyncObj)
                {
                    notifyInfo = NotifyInfo;
                    currentStatus.IsStarted = IsStart;
                    if (notifyInfo == null || !currentStatus.IsStarted)
                    {
                        return currentStatus;
                    }
                }
                lock (notifyInfo)
                {
                    currentStatus.IsFinding = notifyInfo.IsFinding;
                    if (currentStatus.IsFinding)
                    {
                        return currentStatus;
                    }

                    currentStatus.IsAccuracy = notifyInfo.IsAccuracy;
                    currentStatus.IsNoticed = notifyInfo.IsNoticed;
                    if (notifyInfo.SectionStartPost != null)
                    {
                        currentStatus.SectionStartPost =
                            Utility.CopyUtility.DeepCopy<PostClass>(notifyInfo.SectionStartPost);
                        currentStatus.ReleaseDate = currentStatus.SectionStartPost.CreatedAt.AddHours(SECTION_HOUR);
                        currentStatus.ReleaseDateString = currentStatus.ReleaseDate.ToString(LimitReleaseDateFormat);
                    }
                    currentStatus.CountInSection = notifyInfo.PostInSection.Count();
                }
                return currentStatus;
            }
        }

        private object SyncObj { get; set; }

        /// <summary>
        /// UpdateLimitNotificationクラスを初期化します。
        /// </summary>
        /// <param name="twitter">Twitterクラス</param>
        /// <param name="notifyCount">規制通知を行うポスト数</param>
        /// <param name="notificationMassage">通知メッセージ</param>
        /// <param name="limitReleaseDateFormat">規制解除時刻フォーマット</param>
        /// <param name="notAccuracyMessage">不正確時に付加されるメッセージ</param>
        public UpdateLimitNotification(
            Twitter twitter,
            int notifyCount,
            string notificationMassage,
            string limitReleaseDateFormat,
            string notAccuracyMessage)
        {
            Twitter = twitter;

            NotifyCount = notifyCount;
            NotificationMessage = notificationMassage;
            LimitReleaseDateFormat = limitReleaseDateFormat;
            NotAccuracyMessage = notAccuracyMessage;

            SyncObj = new object();
            
            TabInformations.GetInstance().AddPostCalled +=
                new EventHandler<Event.PostClassEventArgs>(UpdateLimitNotification_AddPostCalled);
            twitter.ChangedUserName += new EventHandler<EventArgs>(twitter_ChangedUserName);
        }

        
        /// 規制通知を開始します。
        /// 
        /// このメソッドを実行すると、最初に過去のポストを遡り、セクションを探す処理が行われます。
        /// セクションが探し終わると規制通知が開始されます。
        /// 
        /// このメソッドは非同期です。すぐに処理が戻ります。
        /// 非同期で行われる処理が終了した後に呼び出したい処理がある場合は、
        /// 引数にActionデリゲートを指定します。
        /// </summary>
        /// <param name="callback">
        /// 処理が終了したあとに呼び出したいデリゲート。
        /// エラーで処理が終了した場合、原因となる例外がAggregateException型の引数として渡されます。
        /// 正常に処理が終了した場合、この値はnullです。
        /// </param>
        public void StartAsync(Action<AggregateException> callback)
        {
            lock (SyncObj)
            {
                if (IsStart)
                {
                    return;
                }
                IsStart = true;

                StartFindSection(callback);
            }
        }

        /// <summary>
        /// 規制通知を終了します。
        /// 現在のアカウントの規制通知のための情報はすべてクリアされます。
        /// 
        /// このメソッドは非同期ではありませんが、すぐに処理が終わります。
        /// 現在動作中の処理があっても、その終了を待ちません。
        /// タイミングによってはStopメソッド実行後も規制通知が行われるかもしれませんが、
        /// エラーなどは出ませんし、直ちにStartメソッドを呼び出して次の規制通知を開始することもできます。
        /// </summary>
        public void Stop()
        {
            lock (SyncObj)
            {
                IsStart = false;
                NotifyInfo = null;
            }
        }

        /// <summary>
        /// セクションを探す前に初期化を行い、別スレッドでセクションを探すように設定します。
        /// 
        /// セクションを探している間に新しいポストを受け付けると、
        /// 規制通知のための情報がおかしくなるので、ここでIsFindingフラグをtrueにセットし、
        /// セクションを探している間は新しいポストをバッファに貯めるようにしています。
        /// 
        /// セクションを探し終えたら、セクションを探している最中に受信したポストをすべて処理します。
        /// </summary>
        private void StartFindSection(Action<AggregateException> callback)
        {
            NotifyInfo = new NotifyInformation();
            NotifyInfo.PostInSection = new Dictionary<long, PostClass>();
            NotifyInfo.PostInFinding = new List<PostClass>();
            NotifyInfo.IsFinding = true;
            NotifyInfo.IsNoticed = false;
            NotifyInfo.IsAccuracy = false;

            var notifyInfo = NotifyInfo;

            var t = Task.Factory.StartNew(
                () =>
                    {
                        FindSection(notifyInfo);
                    });

            // セクションを探し中に例外が発生した場合は、Stopメソッドを呼び出して規制通知を停止します。
            // 再度規制通知の開始を試みるかは使用者に委ねられます。
            t.ContinueWith(
                (task) =>
                {
                    Stop();
                    Utility.DelegateUtility.CallAction<AggregateException>(callback, task.Exception);
                    Utility.DelegateUtility.CallEvent<Event.AggregateExceptionEventArgs>
                        (FindingError, this, new Event.AggregateExceptionEventArgs(task.Exception));
                }, TaskContinuationOptions.OnlyOnFaulted);
            t.ContinueWith(
                (task) =>
                {
                    Utility.DelegateUtility.CallAction<AggregateException>(callback, task.Exception);
                    Utility.DelegateUtility.CallEvent(FindingCompleted, this, new EventArgs());
                }, TaskContinuationOptions.NotOnFaulted);
            t.ContinueWith(
                (task) =>
                {
                    lock (notifyInfo)
                    {
                        notifyInfo.IsFinding = false;
                    }
                    foreach (var post in notifyInfo.PostInFinding)
                    {
                        CheckPost(post, notifyInfo);
                    }
                    notifyInfo.PostInFinding.Clear();
                }, TaskContinuationOptions.NotOnFaulted);
        }

        /// <summary>
        /// セクションを探します。
        /// 
        /// セクションは3時間以上発言していない区間からみて最初のポストから始まります。
        /// 次のセクションは、そこから3時間経過後の最初のポストから始まります。
        /// これを現在時刻まで繰り返して現在のセクションを探します。
        /// この方法で求められたセクションは正確です。
        /// IsAccuracyフラグがtrueにセットされます。
        /// 
        /// セクションが見つからない場合は、現在の実装では、126番目のポストをセクションの最初のポストとみなしています。
        /// これは適当な実装です。がある程度きっちりした実装にしても不正確になるでしょう。
        /// そのためIsAccuracyフラグをfalseにセットし、不正確であることを周知します。
        /// 次のセクションからは前述した動作で探します。
        /// </summary>
        private void FindSection(NotifyInformation notifyInfo)
        {
            DateTime now = DateTime.Now;

            IList<PostClass> postList = null;
            postList = Twitter.GetUserTimelinePostClassApi(FINDING_GET_COUNT, 0);

            var postCreatedDescQuery =
                from post in postList
                orderby post.PostedOrRetweetedAt descending
                select post;

            bool foundNoPostSection = false;
            PostClass nextPost = null;
            foreach (var post in postCreatedDescQuery)
            {
                if (nextPost == null)
                {
                    if (now > post.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
                    {
                        foundNoPostSection = true;
                        notifyInfo.LastSectionEndTime = post.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                        notifyInfo.IsAccuracy = true;
                        break;
                    }
                }
                else
                {
                    if (nextPost.PostedOrRetweetedAt > post.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
                    {
                        foundNoPostSection = true;
                        notifyInfo.LastSectionEndTime = post.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                        notifyInfo.SectionStartPost = nextPost;
                        notifyInfo.IsAccuracy = true;
                        break;
                    }
                }
                nextPost = post;
            }

            if (!foundNoPostSection)
            {
                var postCreatedDescArray = postCreatedDescQuery.ToArray();

                if (postCreatedDescArray.Count() == 0)
                {
                    notifyInfo.LastSectionEndTime = now;
                    notifyInfo.IsAccuracy = true;
                }
                else if (postCreatedDescArray.Count() <= FINDING_GET_COUNT - 1)
                {
                    int firstPostIndex = postCreatedDescArray.Count() - 1;
                    notifyInfo.LastSectionEndTime = postCreatedDescArray[firstPostIndex].PostedOrRetweetedAt.AddSeconds(-1);
                    notifyInfo.SectionStartPost = postCreatedDescArray[firstPostIndex];
                    notifyInfo.IsAccuracy = true;
                }
                else
                {
                    notifyInfo.LastSectionEndTime = postCreatedDescArray[126].PostedOrRetweetedAt.AddSeconds(-1);
                    notifyInfo.SectionStartPost = postCreatedDescArray[126];
                    notifyInfo.IsAccuracy = false;
                }
            }

            if (notifyInfo.SectionStartPost != null)
            {
                while (
                    notifyInfo.SectionStartPost != null &&
                    now > notifyInfo.SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
                {
                    notifyInfo.LastSectionEndTime = notifyInfo.SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                    notifyInfo.SectionStartPost =
                        (from post in postList
                         where post.PostedOrRetweetedAt >= notifyInfo.LastSectionEndTime
                         orderby post.PostedOrRetweetedAt
                         select post).FirstOrDefault();
                }

                var postInSectionQuery =
                    from post in postList
                    where post.PostedOrRetweetedAt >= notifyInfo.SectionStartPost.PostedOrRetweetedAt
                    select post;

                notifyInfo.PostInSection.Clear();
                if (notifyInfo.SectionStartPost != null)
                {
                    foreach (var post in postInSectionQuery)
                    {
                        if (!notifyInfo.PostInSection.ContainsKey(post.StatusId))
                        {
                            notifyInfo.PostInSection[post.StatusId] = post;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 設定を変更します。
        /// 
        /// このメソッドは通知を行う条件や、メッセージなどを変更するだけです。
        /// 現在の規制通知を止めたり、規制通知情報をクリアしたりなど、主要な動作は変更しません。
        /// </summary>
        /// <param name="notifyCount">規制通知を行うポスト数</param>
        /// <param name="notificationMassage">通知メッセージ</param>
        /// <param name="limitReleaseDateFormat">規制解除時刻フォーマット</param>
        /// <param name="notAccuracyMessage">不正確時に付加されるメッセージ</param>
        public void ChangeSetting(
            int notifyCount,
            string notificationMassage,
            string limitReleaseDateFormat,
            string notAccuracyMessage)
        {
            var notifyInfo = NotifyInfo;

            if (NotifyCount != notifyCount)
            {
                if (notifyInfo != null)
                {
                    lock (notifyInfo)
                    {
                        notifyInfo.IsNoticed = false;
                    }
                }
            }

            NotifyCount = notifyCount;
            NotificationMessage = notificationMassage;
            LimitReleaseDateFormat = limitReleaseDateFormat;
            NotAccuracyMessage = notAccuracyMessage;
            
        }

        /// <summary>
        /// 規制通知を再度開始します。
        /// これはStopメソッドを呼び出した後にStartメソッドを呼び出した動作と同じです。
        /// 現在の規制通知情報はクリアされます。
        /// 
        /// Startメソッドでは、過去のポストからセクションを探し出そうとするので、
        /// 規制通知が不正確な時にこのメソッドを呼び出すと正確になるかもしれません。
        /// 
        /// このメソッドは非同期です。すぐに処理が戻ります。
        /// 非同期で行われる処理が終了した後に呼び出したい処理がある場合は、
        /// 引数にActionデリゲートを指定します。
        /// </summary>
        /// <param name="callback">
        /// 処理が終了したあとに呼び出したいデリゲート。
        /// エラーで処理が終了した場合、原因となる例外がAggregateException型の引数として渡されます。
        /// 正常に処理が終わった場合、この値はnullです。
        /// </param>
        public void RestartAsync(Action<AggregateException> callback)
        {
            Stop();
            StartAsync(callback);
        }

        /// <summary>
        /// TabInformationクラスにポストが追加されたときに呼び出されるイベントです。
        /// 
        /// タイムラインや検索を含めあらゆるポストが追加されたときに呼び出されるので、
        /// 常に新しいポストとは限りません。
        /// 
        /// セクションを探している最中で、IsFindingがtrueの場合、
        /// ポストはすべてPostInFindingバッファに貯められます。
        /// IsFindingがfalseになってからポストのチェックが開始されます。
        /// </summary>
        /// <param name="sender">呼び出し元</param>
        /// <param name="e">追加されたPostClassが入っているPostClassEventArgs</param>
        private void UpdateLimitNotification_AddPostCalled(object sender, Event.PostClassEventArgs e)
        {
            NotifyInformation notifyInfo;

            lock (SyncObj)
            {
                if (!IsStart)
                {
                    return;
                }

                notifyInfo = NotifyInfo;
            }

            if (notifyInfo == null)
            {
                return;
            }

            lock (notifyInfo)
            {
                if (notifyInfo.IsFinding)
                {
                    notifyInfo.PostInFinding.Add(e.Post);
                    return;
                }
            }

            CheckPost(e.Post, notifyInfo);
        }

        /// <summary>
        /// PostClassをチェックします。
        /// 
        /// FindSectionで求められたセクション情報から、現在のセクションのポスト数を求め、
        /// 通知ポスト数NotifyCountに達したときに規制通知を行います。
        /// </summary>
        /// <param name="post">チェックするPostClass</param>
        private void CheckPost(PostClass post, NotifyInformation notifyInfo)
        {
            if (post.IsDm)
            {
                return;
            }
            if (!String.IsNullOrEmpty(post.RetweetedBy) && !Twitter.IsCurrentUser(post.RetweetedBy) ||
                String.IsNullOrEmpty(post.RetweetedBy) && !Twitter.IsCurrentUser(post.ScreenName))
            {
                return;
            }

            lock (notifyInfo)
            {
                if (notifyInfo.LastSectionEndTime > post.PostedOrRetweetedAt)
                {
                    return;
                }

                if (!notifyInfo.PostInSection.ContainsKey(post.StatusId))
                {
                    notifyInfo.PostInSection[post.StatusId] = post;
                }

                if (notifyInfo.SectionStartPost == null ||
                    notifyInfo.SectionStartPost.PostedOrRetweetedAt > post.PostedOrRetweetedAt)
                {
                    notifyInfo.SectionStartPost = post;
                }

                if (post.PostedOrRetweetedAt > notifyInfo.SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
                {
                    notifyInfo.LastSectionEndTime = notifyInfo.SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                    notifyInfo.SectionStartPost = post;

                    var outOfSectionArray =
                        (from postPair in notifyInfo.PostInSection
                         where postPair.Value.PostedOrRetweetedAt < notifyInfo.LastSectionEndTime
                         orderby postPair.Value.PostedOrRetweetedAt
                         select postPair).ToArray();

                    foreach (var outPost in outOfSectionArray)
                    {
                        notifyInfo.PostInSection.Remove(outPost);
                    }

                    notifyInfo.IsNoticed = false;
                }
                else
                {
                    if (notifyInfo.PostInSection.Count >= NotifyCount)
                    {
                        if (!notifyInfo.IsNoticed)
                        {
                            notifyInfo.IsNoticed = true;
                            var t = Task.Factory.StartNew(
                                () =>
                                {
                                    DateTime limitReleaseDate = notifyInfo.SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                                    string limitReleaseDateString = limitReleaseDate.ToString(LimitReleaseDateFormat);
                                    string notAccuracyMessage = "";
                                    if (!notifyInfo.IsAccuracy)
                                    {
                                        notAccuracyMessage = NotAccuracyMessage;
                                    }
                                    Twitter.PostStatus(
                                        String.Format(NotificationMessage,
                                        notifyInfo.PostInSection.Count(), limitReleaseDateString, notAccuracyMessage),
                                        0);
                                });
                            t.ContinueWith(
                                (task) =>
                                {
                                    Utility.DelegateUtility.CallEvent<Event.AggregateExceptionEventArgs>(
                                        NotifyError, this, new Event.AggregateExceptionEventArgs(task.Exception));
                                }, TaskContinuationOptions.OnlyOnFaulted);
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// Twitterクラスが接続しているアカウントが変更されたときに呼び出されるイベントです
        /// 
        /// Restartメソッドを呼び出し、新しいアカウントで規制通知を開始します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void twitter_ChangedUserName(object sender, EventArgs e)
        {
            RestartAsync(null);
        }
    }
}
