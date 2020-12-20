using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using System.Text;
using System.ServiceModel.Syndication;

namespace Blog.Controllers
{
    public class PostsController : Controller
    {
        private string _sql =
                "Data Source=DESKTOP-HDJOHNA;Initial Catalog=Blog;Integrated Security=True;Pooling=False;MultipleActiveResultSets=True;Application Name=EntityFramework"
            ;

        BlogModel _model = new BlogModel();
        const int _postsPerPage = 4;

        const int _postsPerFeed = 25;

        // GET: Posts
        public ActionResult Index(int? id)
        {
            int pageNumber = id ?? 0;
            List<PostModel> posts = new List<PostModel>();
            DataTable dtPost = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(_sql))
            {
                sqlCon.Open();
                string query = "SELECT * FROM Posts where [DateTime] < GETDATE() ORDER BY [DateTime] desc";
                SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                da.Fill(dtPost);
                foreach (DataRow row in dtPost.Rows)
                {
                    PostModel post = new PostModel();
                    post.ID = int.Parse(row["ID"].ToString());
                    post.Title = row["Title"].ToString();
                    post.DateTime = (DateTime) row["DateTime"];
                    post.Body = row["Body"].ToString();
                    posts.Add(post);
                }
                foreach (PostModel post in posts)
                {
                    int id1 = post.ID;
                    query = "SELECT [ID],[Name] FROM Tags as T inner join PostsTag as P on p.TagID=T.ID AND p.PostID=" +
                            id1;
                    da = new SqlDataAdapter(query, sqlCon);
                    dtPost.Clear();
                    da.Fill(dtPost);
                    List<TagModel> tags = new List<TagModel>();
                    foreach (DataRow row in dtPost.Rows)
                    {
                        TagModel tag = new TagModel();
                        tag.ID = int.Parse(row["ID"].ToString());
                        tag.Name = row["Name"].ToString();
                        tags.Add(tag);
                    }
                    post.Tags = tags;
                    List<CommentModel> comments = new List<CommentModel>();
                    query = "SELECT * FROM Comments where PostId=" + id1;
                    da = new SqlDataAdapter(query, sqlCon);
                    dtPost.Clear();
                    da.Fill(dtPost);
                    foreach (DataRow row in dtPost.Rows)
                    {
                        CommentModel comment = new CommentModel();
                        comment.ID = int.Parse(row["ID"].ToString());
                        comment.Name = row["Name"].ToString();
                        comment.Body = row["Body"].ToString();
                        comment.Email = row["Email"].ToString();
                        comment.DateTime = (DateTime) row["DateTime"];
                        comments.Add(comment);
                    }
                    post.Comments = comments;
                    query = "SELECT * FROM Likes where PostId=" + id1;
                    da = new SqlDataAdapter(query, sqlCon);
                    dtPost.Clear();
                    da.Fill(dtPost);
                    List<LikeModel> likes = new List<LikeModel>();
                    foreach (DataRow row in dtPost.Rows)
                    {
                        LikeModel like = new LikeModel();
                        like.DateTime = (DateTime) row["DateTime"];
                        like.ID = int.Parse(row["ID"].ToString());
                        like.UserID = int.Parse(row["UserID"].ToString());
                        likes.Add(like);
                    }
                    post.Likes = likes;
                }
            }
            posts = posts.Skip(pageNumber * _postsPerPage).Take(_postsPerPage + 1).ToList();
            ViewBag.IsPreviousLinkVisible = pageNumber > 0;
            ViewBag.IsNextLinkVisible = posts.Count() > _postsPerPage;
            ViewBag.PageNumber = pageNumber;
            ViewBag.IsAdmin = IsAdmin;
            return View(posts.Take(_postsPerPage));
        }

        [ValidateInput(false)]
        public ActionResult Update(int? id, string title, string body, DateTime dateTime, string tags)
        {
            if (!IsAdmin)
                return RedirectToAction("Index");
            PostModel post = GetPost(id);
            post.Title = title;
            post.Body = body;
            post.DateTime = dateTime;
            if (post.Tags == null)
            {
                post.Tags = new List<TagModel>();
            }
            post.Tags.Clear();

            tags = tags ?? string.Empty;
            string[] tagNames = tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<TagModel> tags1 = new List<TagModel>();
            foreach (string tagName in tagNames)
            {
                //post.Tags.Add(GetTag(tagName));
                DataTable dt = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(_sql))
                {
                    string query = "SELECT * FROM Tags WHERE [Name]='" + tagName + "'";
                    SqlDataAdapter da = new SqlDataAdapter(query,sqlCon);
                    da.Fill(dt);
                    TagModel tag = new TagModel();
                    if (dt.Rows.Count == 0)
                    {
                        query = "insert into tags([Name]) OUTPUT INSERTED.ID,inserted.[Name] VALUES('" + tagName + "')";
                        da = new SqlDataAdapter(query,sqlCon);
                        da.Fill(dt);
                    }
                    tag.ID = int.Parse(dt.Rows[0]["ID"].ToString());
                    tag.Name = dt.Rows[0]["Name"].ToString();
                    tags1.Add(tag);
                }
            }
            post.Tags = tags1;
            if (!id.HasValue)
            {
                string query = "INSERT INTO Posts([Title],[Body],[DateTime]) values('" + post.Title + "','" + post.Body +
                               "','" + DateTime.Now+"'); SELECT SCOPE_IDENTITY();";
                DataTable dt = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(_sql))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                    da.Fill(dt);
                    post.ID = int.Parse(dt.Rows[0][0].ToString());
                }
            }
            using (SqlConnection sqlCon = new SqlConnection(_sql))
            {
                sqlCon.Open();
                DataTable dt = new DataTable();
                foreach (TagModel tag in tags1)
                {
                    int tagId = tag.ID;
                    string query = "SELECT * FROM PostsTag where PostID=" + post.ID + " AND TagID=" + tagId;
                    SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                    dt.Clear();
                    da.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        query = "INSERT INTO PostsTag values(" + post.ID + "," + tagId + ")";
                        da = new SqlDataAdapter(query, sqlCon);
                        dt.Clear();
                        da.Fill(dt);
                    }
                }
            }
            return RedirectToAction("Details", new { id = post.ID });
        }
        public ActionResult Edit(int? id)
        {
            PostModel post = GetPost(id);
            StringBuilder tagList = new StringBuilder();
            if (id.HasValue)
            {
                foreach (TagModel tag in post.Tags)
                {
                    tagList.AppendFormat("{0} ", tag.Name);
                }
            }
            ViewBag.Tags = tagList.ToString();
            return View(post);
        }

        public ActionResult Details(int id)
        {
            PostModel post = GetPost(id);
            ViewBag.IsAdmin = IsAdmin;
            post.Body.Replace("\n", "<br />");
            return View(post);
        }

        [ValidateInput(false)]
        public ActionResult Comment(int id, string name, string email, string body)
        {
            PostModel post = GetPost(id);
            CommentModel comment = new CommentModel();
            comment.DateTime = DateTime.Now;
            comment.Email = email;
            comment.Body = body;
            comment.Name = name;
            string query = "INSERT INTO COMMENTS([PostID],[DateTime],[Name],[Email],[Body]) values("+post.ID+",'"+DateTime.Now+"','"+comment.Name+",'"+comment.Email+"','"+comment.Body+"')";
            DataTable dt = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(_sql))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                dt.Clear();
                da.Fill(dt);
            }
            return RedirectToAction("Details", new { id = id });
        }
        public ActionResult Like(int id, int? pageNumber)
        {
            PostModel post = GetPost(id);
            LikeModel like = new LikeModel();//post.Likes.SingleOrDefault(x => x.UserID == Convert.ToInt32(Session["UserID"].ToString()));
            string query = "SELECT * FROM Likes where UserID="+int.Parse(Session["UserID"].ToString()) + " AND PostID="+post.ID;
            DataTable dt = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(_sql))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                dt.Clear();
                da.Fill(dt);
            }
            if (dt.Rows.Count != 0)
            {
                query = "DELETE FROM LIKES WHERE UserID=" + int.Parse(Session["UserID"].ToString()) + " AND PostID="+post.ID;
                dt.Clear();
                using (SqlConnection sqlCon = new SqlConnection(_sql))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                    da.Fill(dt);
                }
            }
            else
            {
                like.UserID = int.Parse(Session["UserID"].ToString());
                like.DateTime = DateTime.Now;
                query = "Insert INTO LIKES([PostID],[UserID],[DateTime]) values(" + post.ID + ","+ like.UserID + ",'"+like.DateTime+"')";
                dt.Clear();
                using (SqlConnection sqlCon = new SqlConnection(_sql))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                    da.Fill(dt);
                }
            }
            if (pageNumber > -1)
            {

                return RedirectToAction("Index", new { id = pageNumber });
            }
            return RedirectToAction("Details", new { id = id});
        }
        public ActionResult Delete(int id)
        {
            if (IsAdmin)
            {
                PostModel post = GetPost(id);
                string query = "DELETE from Posts where ID="+post.ID;
                DataTable dt = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(_sql))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                    dt.Clear();
                    da.Fill(dt);
                }

            }
            return RedirectToAction("Index");
        }

        public ActionResult DeleteComment(int id)
        {
            Comment comment = _model.Comments.Where(x => x.ID == id).First();
            if (IsAdmin)
            {
                _model.Comments.Remove(comment);
                _model.SaveChanges();
            }
            return RedirectToAction("Details", new {id = comment.PostId});
        }

        public ActionResult RSS()
        {
            IEnumerable<SyndicationItem> posts =
            (from post in _model.Posts
                where post.DateTime < DateTime.Now
                orderby post.DateTime descending
                select post).Take(_postsPerFeed).ToList().Select(x => GetSyndicationItem(x));

            SyndicationFeed feed = new SyndicationFeed("Danial Ahmed", "DBMS Project BLOG", new Uri("http://localhost"),
                posts);
            Rss20FeedFormatter formattedFeed = new Rss20FeedFormatter(feed);
            return new FeedResult(formattedFeed);
        }

        private SyndicationItem GetSyndicationItem(Post post)
        {
            return new SyndicationItem(post.Title, post.Body, new Uri("http://localhost/posts/detail/" + post.ID));
        }

        public ActionResult Tags(string id)
        {
            List<PostModel> posts = GetTag(id);
            ViewBag.IsAdmin = IsAdmin;
            return View("Index", posts);
        }

        private List<PostModel> GetTag(string tagName)
        {
            DataTable dtTag = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(_sql))
            {
                sqlCon.Open();
                string query = "SELECT ID FROM Tags where [Name]='" + tagName + "'";
                SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                da.Fill(dtTag);
                if (dtTag == null)
                    return new List<PostModel>();
                TagModel tag = new TagModel();
                int tagId = int.Parse(dtTag.Rows[0][0].ToString());
                query =
                    "select [ID],[Title],[DateTime],[Body] from Posts inner join PostsTag on ID = PostID AND TagID =" +
                    tagId;
                da = new SqlDataAdapter(query, sqlCon);
                dtTag.Clear();
                da.Fill(dtTag);
                List<PostModel> posts = new List<PostModel>();
                foreach (DataRow row1 in dtTag.Rows)
                {
                    PostModel post = new PostModel();
                    post.Body = row1["Body"].ToString();
                    post.ID = int.Parse(row1["ID"].ToString());
                    post.DateTime = (DateTime) row1["DateTime"];
                    post.Title = row1["Title"].ToString();
                    int id1 = post.ID;
                    DataTable dt = new DataTable();
                    query = "SELECT [ID],[Name] FROM Tags as T inner join PostsTag as P on p.TagID=T.ID AND p.PostID=" +
                            id1;
                    da = new SqlDataAdapter(query, sqlCon);
                    dt.Clear();
                    da.Fill(dt);
                    List<TagModel> tags = new List<TagModel>();
                    foreach (DataRow row in dt.Rows)
                    {
                        TagModel tag1 = new TagModel();
                        tag1.ID = int.Parse(row["ID"].ToString());
                        tag1.Name = row["Name"].ToString();
                        query =
                            "select [ID],[Title],[DateTime],[Body] from Posts inner join PostsTag on ID = PostID AND TagID =" +
                            tag1.ID;
                        tags.Add(tag1);
                    }
                    post.Tags = tags;
                    List<CommentModel> comments = new List<CommentModel>();
                    query = "SELECT * FROM Comments where PostId=" + id1;
                    da = new SqlDataAdapter(query, sqlCon);
                    dt.Clear();
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        CommentModel comment = new CommentModel();
                        comment.ID = int.Parse(row["ID"].ToString());
                        comment.Name = row["Name"].ToString();
                        comment.Body = row["Body"].ToString();
                        comment.Email = row["Email"].ToString();
                        comment.DateTime = (DateTime) row["DateTime"];
                        comments.Add(comment);
                    }
                    post.Comments = comments;
                    query = "SELECT * FROM Likes where PostId=" + id1;
                    da = new SqlDataAdapter(query, sqlCon);
                    dt.Clear();
                    da.Fill(dt);
                    List<LikeModel> likes = new List<LikeModel>();
                    foreach (DataRow row in dt.Rows)
                    {
                        LikeModel like = new LikeModel();
                        like.DateTime = (DateTime) row["DateTime"];
                        like.ID = int.Parse(row["ID"].ToString());
                        like.UserID = int.Parse(row["UserID"].ToString());
                        likes.Add(like);
                    }
                    post.Likes = likes;
                    dt.Clear();
                    posts.Add(post);
                }
                return posts;
            }
            //return _model.Tags.Where(x => x.Name == tagName).FirstOrDefault() ?? new Tag() { Name = tagName };
        }

        private tblUser GetUser(int? id)
        {
            // return id.HasValue ? _model.tblUsers.Where(x => x.ID == id).FirstOrDefault() : new tblUser() { ID = -1 };
            return _model.tblUsers.FirstOrDefault(x => x.ID == id);
        }

        private PostModel GetPost(int? id)
        {
            if (id.HasValue)
            {
                DataTable dtPost = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(_sql))
                {
                    sqlCon.Open();
                    string query = "SELECT TOP 1 * FROM Posts where ID=" + id;
                    SqlDataAdapter da = new SqlDataAdapter(query, sqlCon);
                    da.Fill(dtPost);
                    PostModel post = new PostModel();
                    post.ID = int.Parse(dtPost.Rows[0][0].ToString());
                    post.Title = dtPost.Rows[0][1].ToString();
                    post.DateTime = (DateTime) dtPost.Rows[0][2];
                    post.Body = dtPost.Rows[0][3].ToString();
                    query = "SELECT [ID],[Name] FROM Tags as T inner join PostsTag as P on p.TagID=T.ID AND p.PostID=" +
                            id;
                    //query = "SELECT * FROM PostsTag where PostID=" + id;
                    da = new SqlDataAdapter(query, sqlCon);
                    dtPost.Clear();
                    da.Fill(dtPost);
                    List<TagModel> tags = new List<TagModel>();
                    foreach (DataRow row in dtPost.Rows)
                    {
                        TagModel tag = new TagModel();
                        tag.ID = int.Parse(row["ID"].ToString());
                        tag.Name = row["Name"].ToString();
                        tags.Add(tag);
                    }
                    post.Tags = tags;
                    //for (int i = 0; i < dtPost.Rows.Count; i++)
                    //{
                    //    tag.ID = int.Parse(dtPost.Rows[i][0].ToString());
                    //    tag.Name = dtPost.Rows[i][1].ToString();
                    //    post.Tags.Add(tag);
                    //}
                    List<CommentModel> comments = new List<CommentModel>();
                    query = "SELECT * FROM Comments where PostId=" + id;
                    da = new SqlDataAdapter(query, sqlCon);
                    dtPost.Clear();
                    da.Fill(dtPost);
                    foreach (DataRow row in dtPost.Rows)
                    {
                        CommentModel comment = new CommentModel();
                        comment.ID = int.Parse(row["ID"].ToString());
                        comment.Name = row["Name"].ToString();
                        comment.Body = row["Body"].ToString();
                        comment.Email = row["Email"].ToString();
                        comment.DateTime = (DateTime) row["DateTime"];
                        comments.Add(comment);
                    }
                    post.Comments = comments;
                    query = "SELECT * FROM Likes where PostId=" + id;
                    da = new SqlDataAdapter(query, sqlCon);
                    dtPost.Clear();
                    da.Fill(dtPost);
                    List<LikeModel> likes = new List<LikeModel>();
                    foreach (DataRow row in dtPost.Rows)
                    {
                        LikeModel like = new LikeModel();
                        like.DateTime = (DateTime) row["DateTime"];
                        like.ID = int.Parse(row["ID"].ToString());
                        like.UserID = int.Parse(row["UserID"].ToString());
                        likes.Add(like);
                    }
                    post.Likes = likes;

                    return post;
                }
            }
            return new PostModel() {ID = -1};
            //return id.HasValue ? _model.Posts.Where(x => x.ID == id).FirstOrDefault() : new Post() { ID = -1 };
        }

        public bool IsAdmin
        {
            get { return Session["IsAdmin"] != null && (bool) Session["IsAdmin"]; }
        }
    }
}