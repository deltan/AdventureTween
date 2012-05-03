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

namespace OpenTween.PostLimitNotice
{
    /// <summary>
    /// 規制通知を行うクラス
    /// TabInformationクラスに依存していて、同時に複数アカウントには対応していません。
    /// </summary>
    class PostLimitNotice
    {
        private const int FINDING_GET_COUNT = 200;
        private const int SECTION_HOUR = 3;
        private const string BASE_MESSAGE_FORMAT = "【規制情報】規制が近いので注意しましょう。規制解除時刻(予想)は、{1}です。セクション中、{0}回ポストしました。{2}";
        private const string LIMIT_RELEASE_DATE_FORMAT = "HH時mm分ss秒";
        private const string NOT_ACCURACY_MESSAGE_FORMAT = "（この規制情報は不正確です。）";

        private Twitter Twitter { get; set; }
        private int NoticeCount { get; set; }

        private bool IsStart { get; set; }
        private PostClass SectionStartPost { get; set; }
        private DateTime? LastSectionEndTime { get; set; }
        private IDictionary<long, PostClass> PostInSection { get; set; }
        private IList<PostClass> PostInFinding { get; set; }

        private bool IsFinding { get; set; }
        private bool IsNoticed { get; set; }
        public bool IsAccuracy { get; private set; }

        private object SyncObj { get; set; }

        public PostLimitNotice(Twitter twitter, int noticeCount)
        {
            Twitter = twitter;
            NoticeCount = noticeCount;

            IsStart = false;
            SectionStartPost = null;
            LastSectionEndTime = null;

            SyncObj = new object();
            
            TabInformations.GetInstance().AddPostCalled +=
                new EventHandler<Event.PostClassEventArgs>(PostLimitNotice_AddPostCalled);
        }

        public void Start()
        {
            lock (SyncObj)
            {
                if (IsStart)
                {
                    return;
                }
                IsStart = true;

                PostInSection = new Dictionary<long, PostClass>();
                PostInFinding = new List<PostClass>();

                IsFinding = true;
                IsNoticed = false;
                IsAccuracy = false;

                var t = Task.Factory.StartNew(FindSection);
            }
        }

        public void Stop()
        {
            lock (SyncObj)
            {
                IsStart = false;
            }
        }

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
                while (now > SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR))
                {
                    LastSectionEndTime = SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                    SectionStartPost = 
                        (from post in postList
                         where post.PostedOrRetweetedAt >= LastSectionEndTime
                         orderby post.PostedOrRetweetedAt
                         select post).First();
                }

                var postInSectionQuery =
                    from post in postList
                    where post.PostedOrRetweetedAt >= SectionStartPost.PostedOrRetweetedAt
                    select post;

                PostInSection.Clear();
                foreach (var post in postInSectionQuery)
                {
                    if (!PostInSection.ContainsKey(post.StatusId))
                    {
                        PostInSection[post.StatusId] = post;
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

        private void PostLimitNotice_AddPostCalled(object sender, Event.PostClassEventArgs e)
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

        private void CheckPost(PostClass post)
        {
            if (post.IsDm)
            {
                return;
            }
            if (!Twitter.IsCurrentUser(post.ScreenName))
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
                if (PostInSection.Count >= NoticeCount)
                {
                    if (!IsNoticed)
                    {
                        IsNoticed = true;
                        var t = Task.Factory.StartNew(
                            () =>
                            {
                                DateTime limitReleaseDate = SectionStartPost.PostedOrRetweetedAt.AddHours(SECTION_HOUR);
                                string limitReleaseDateString = limitReleaseDate.ToString(LIMIT_RELEASE_DATE_FORMAT);
                                string notAccuracyMessage = "";
                                if (!IsAccuracy)
                                {
                                    notAccuracyMessage = NOT_ACCURACY_MESSAGE_FORMAT;
                                }
                                Twitter.PostStatus(
                                    String.Format(BASE_MESSAGE_FORMAT,
                                    PostInSection.Count(), limitReleaseDateString, notAccuracyMessage),
                                    0);
                            });
                    }
                }
            }
            
        }
    }
}
