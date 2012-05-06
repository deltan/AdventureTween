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
    class UpdateLimitNotification
    {
        private const int FINDING_GET_COUNT = 200;
        private const int SECTION_HOUR = 3;

        private Twitter Twitter { get; set; }
        private int NotifyCount { get; set; }

        private bool IsStart { get; set; }
        private PostClass SectionStartPost { get; set; }
        private DateTime? LastSectionEndTime { get; set; }
        private IDictionary<long, PostClass> PostInSection { get; set; }
        private IList<PostClass> PostInFinding { get; set; }

        private bool IsFinding { get; set; }
        private bool IsNoticed { get; set; }
        public bool IsAccuracy { get; private set; }

        private string NotificationMessage { get; set; }
        private string LimitReleaseDateFormat { get; set; }
        private string NotAccuracyMessage { get; set; }

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

            IsStart = false;
            SectionStartPost = null;
            LastSectionEndTime = null;

            SyncObj = new object();
            
            TabInformations.GetInstance().AddPostCalled +=
                new EventHandler<Event.PostClassEventArgs>(UpdateLimitNotification_AddPostCalled);
            twitter.ChangedUserName += new EventHandler<EventArgs>(twitter_ChangedUserName);
        }

        /// <summary>
        /// 規制通知を開始します。
        /// まず最初に、過去のポストを遡り、セクションを探します。
        /// その後、投稿されたポストから規制通知を行います。
        /// </summary>
        public void Start()
        {
            lock (SyncObj)
            {
                if (IsStart)
                {
                    return;
                }
                IsStart = true;

                StartFindSection();                
            }
        }

        /// <summary>
        /// 規制通知を終了します。
        /// 現在のアカウントの規制通知のための情報はすべてクリアされます。
        /// </summary>
        public void Stop()
        {
            lock (SyncObj)
            {
                IsStart = false;

                PostInSection = null;
                PostInFinding = null;
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
            lock (SyncObj)
            {
                if (!IsStart)
                {
                    return;
                }

                NotifyCount = notifyCount;
                NotificationMessage = notificationMassage;
                LimitReleaseDateFormat = limitReleaseDateFormat;
                NotAccuracyMessage = notAccuracyMessage;
            }
        }

        /// <summary>
        /// 規制通知を再度開始します。
        /// これはStopメソッドを呼び出した後にStartメソッドを呼び出した動作と同じです。
        /// 現在の規制通知情報はクリアされます。
        /// 
        /// Startメソッドでは、過去のポストからセクションを探し出そうとするので、
        /// 規制通知が不正確な時にこのメソッドを呼び出すと正確になるかもしれません。
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// セクションを探す前に初期化を行い、別スレッドでセクションを探すように設定します。
        /// 
        /// セクションを探している間に新しいポストを受け付けると、
        /// 規制通知のための情報がおかしくなるので、ここでIsFindingフラグをtrueにセットし、
        /// セクションを探している間は新しいポストをバッファに貯めるようにしています。
        /// </summary>
        private void StartFindSection()
        {
            PostInSection = new Dictionary<long, PostClass>();
            PostInFinding = new List<PostClass>();

            IsFinding = true;
            IsNoticed = false;
            IsAccuracy = false;

            var t = Task.Factory.StartNew(FindSection);
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
        private void FindSection()
        {
            DateTime now = DateTime.Now;
            IList<PostClass> postList = Twitter.GetUserTimelinePostClassApi(FINDING_GET_COUNT, 0);

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
                        LastSectionEndTime = post.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                        IsAccuracy = true;
                        break;
                    }
                }
                else
                {
                    if (nextPost.PostedOrRetweetedAt > post.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
                    {
                        foundNoPostSection = true;
                        LastSectionEndTime = post.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                        SectionStartPost = nextPost;
                        IsAccuracy = true;
                        break;
                    }
                }
                nextPost = post;
            }

            if (!foundNoPostSection)
            {
                IsAccuracy = false;

                var postCreatedDescArray = postCreatedDescQuery.ToArray();
                LastSectionEndTime = postCreatedDescArray[126].PostedOrRetweetedAt.AddSeconds(-1);
                SectionStartPost = postCreatedDescArray[126];
            }

            if (SectionStartPost != null)
            {
                while (
                    SectionStartPost != null && 
                    now > SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
                {
                    LastSectionEndTime = SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                    SectionStartPost =
                        (from post in postList
                         where post.PostedOrRetweetedAt >= LastSectionEndTime
                         orderby post.PostedOrRetweetedAt
                         select post).FirstOrDefault();
                }

                var postInSectionQuery =
                    from post in postList
                    where post.PostedOrRetweetedAt >= SectionStartPost.PostedOrRetweetedAt
                    select post;

                PostInSection.Clear();
                if (SectionStartPost != null)
                {
                    foreach (var post in postInSectionQuery)
                    {
                        if (!PostInSection.ContainsKey(post.StatusId))
                        {
                            PostInSection[post.StatusId] = post;
                        }
                    }
                }
            }

            lock (SyncObj)
            {
                IsFinding = false;
                if (PostInFinding.Count() >= 1)
                {
                    foreach (var post in PostInFinding)
                    {
                        CheckPost(post);
                    }
                    PostInFinding.Clear();
                }
            }
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
            lock (SyncObj)
            {
                if (!IsStart)
                {
                    return;
                }

                if (IsFinding)
                {
                    PostInFinding.Add(e.Post);
                }
                else
                {
                    CheckPost(e.Post);
                }
            }
        }

        /// <summary>
        /// PostClassをチェックします。
        /// 
        /// FindSectionで求められたセクション情報から、現在のセクションのポスト数を求め、
        /// 通知ポスト数NotifyCountに達したときに規制通知を行います。
        /// </summary>
        /// <param name="post">チェックするPostClass</param>
        private void CheckPost(PostClass post)
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
            if (LastSectionEndTime > post.PostedOrRetweetedAt)
            {
                return;
            }

            if (!PostInSection.ContainsKey(post.StatusId))
            {
                PostInSection[post.StatusId] = post;
            }

            if (SectionStartPost == null ||
                SectionStartPost.PostedOrRetweetedAt > post.PostedOrRetweetedAt)
            {
                SectionStartPost = post;
            }

            if (post.PostedOrRetweetedAt > SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
            {
                LastSectionEndTime = SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                SectionStartPost = post;

                var outOfSectionArray =
                    (from postPair in PostInSection
                        where postPair.Value.PostedOrRetweetedAt < LastSectionEndTime
                        orderby postPair.Value.PostedOrRetweetedAt
                        select postPair).ToArray();

                foreach (var outPost in outOfSectionArray)
                {
                    PostInSection.Remove(outPost);
                }

                IsNoticed = false;
            }
            else
            {
                if (PostInSection.Count >= NotifyCount)
                {
                    if (!IsNoticed)
                    {
                        IsNoticed = true;
                        var t = Task.Factory.StartNew(
                            () =>
                            {
                                DateTime limitReleaseDate = SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                                string limitReleaseDateString = limitReleaseDate.ToString(LimitReleaseDateFormat);
                                string notAccuracyMessage = "";
                                if (!IsAccuracy)
                                {
                                    notAccuracyMessage = NotAccuracyMessage;
                                }
                                Twitter.PostStatus(
                                    String.Format(NotificationMessage,
                                    PostInSection.Count(), limitReleaseDateString, notAccuracyMessage),
                                    0);
                            });
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
            Restart();
        }
    }
}
