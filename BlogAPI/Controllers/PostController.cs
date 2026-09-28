using BlogDataLibrary.Data;
using BlogDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BlogDataLibrary;

namespace BlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private ISqlData _db;

        public PostController(ISqlData db)
        {
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ListPosts()
        {
            List<ListPostModel> posts = _db.ListPosts();

            return Ok(posts);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/api/[controller]/{id}")]
        public ActionResult ShowPostDetails(int id)
        {
            ListPostModel post = _db.ShowPostDetails(id);

            return Ok(post);
        }

        private int GetCurrentUserId()
        {
            ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;
                string id = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (id != null)
                {
                    return Convert.ToInt32(id);
                }
            }
            return 0;
        }

        [Authorize]
        [HttpPost]
        [Route("/api/[controller]/add")]
        public ActionResult AddPost([FromBody] PostForm form)
        {
            PostModel post = new PostModel();
            post.Title = form.Title;
            post.Body = form.Body;
            post.DateCreated = DateTime.Now;
            post.UserId = GetCurrentUserId();
            _db.AddPost(post);

            return Ok("Post created.");
        }
    }
}
