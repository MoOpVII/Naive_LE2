using BlogDataLibrary.Database;
using BlogDataLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDataLibrary.Data
{
    public class SqlData : ISqlData
    {
        private ISqlDataAccess _db;
        private const string connectionStringName = "SqlDb";

        public SqlData(ISqlDataAccess db)
        {
            _db = db;
        }

        public UserModel Authenticate(string username, string password)
        {
            UserModel result = _db.LoadData<UserModel, dynamic>("dbo.spUsers_Authenticate", new { username, password }, connectionStringName, true).FirstOrDefault();
            return result;
        }

        public void Register(string username, string firstName, string lastName, string password)
        {
            _db.SaveData<dynamic>("dbo.spUsers_Register", new { username, firstName, lastName, password }, connectionStringName, true);
        }

        public void AddPost(PostModel post)
        {
            _db.SaveData("spPosts_Insert", new { post.UserId, post.Title, post.Body, post.DateCreated }, connectionStringName, true);
        }

        public List<ListPostModel> ListPosts()
        {
            return _db.LoadData<ListPostModel, dynamic>("dbo.spPosts_List", new { }, connectionStringName, true).ToList();
        }

        public ListPostModel ShowPostDetails(int id)
        {
            return _db.LoadData<ListPostModel, dynamic>("dbo.spPosts_Details", new { id }, connectionStringName, true).FirstOrDefault();
        }
    }
}