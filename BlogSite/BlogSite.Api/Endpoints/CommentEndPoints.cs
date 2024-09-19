﻿using BlogSite.Api.Data;
using BlogSite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSite.Api.Endpoints
{
    public static class CommentEndPoints
    {
        public static void MapCommentEndPoints(this WebApplication app)
        {
            app.MapGet("/comments", GetComments);
            app.MapPost("/comments/{blogId}/{userId}", CreateComment);
            app.MapDelete("/comments/{commentId}/{userId}", DeleteComment);
        }

        public static async Task<IResult> GetComments(BlogDbContext db)
        {
            var allComments = await db.Comments.ToListAsync();
            return Results.Ok(allComments);
        }
        public static async Task<IResult> CreateComment(BlogDbContext db, Comment comment, int blogId, int userId)
        {
            var blog = await db.Blogs.FindAsync(blogId);
            if (blog == null)
            {
                return Results.NotFound($"Blog with ID {blogId} not found.");
            }
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return Results.NotFound($"User with ID {userId} not found.");
            }
            comment.BlogId = blogId;
            comment.UserId = userId;
            db.Comments.Add(comment);
            blog.Comments.Add(comment);
            await db.SaveChangesAsync();

            var createdComment = new
            {
                Id = comment.Id,
                Author = user.Username,
                Content = comment.Content,
                DateCreated = comment.DateCreated
            };

            return Results.Created($"/comments/{comment.Id}", createdComment);
        }

        public static async Task<IResult> DeleteComment(BlogDbContext db, int commentId, int userId)
        {
            var commentToDelete = await db.Comments
                .Include(c => c.User)
                .Include(c => c.Blog)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return Results.NotFound($"User with ID {userId} not found.");
            }
            if (commentToDelete == null)
            {
                return Results.NotFound("Comment not found");
            }
            if (commentToDelete.User.Id != userId && commentToDelete.Blog.User.Id != userId)
            {
                return Results.NotFound("Only commentor or blog owner can deleted comment");
            }

            db.Comments.Remove(commentToDelete);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
    }
}
