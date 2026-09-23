using BlogDataLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDataLibrary.Data
{
    internal class SqlData
    {
        private ISqlDataAccess _db;
        private const string connectionStringName = "SqlDb";

        public SqlData(ISqlDataAccess db)
        {
            _db = db;
        }
    }
}
