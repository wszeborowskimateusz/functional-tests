using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Blog.DAL.Infrastructure;
using Blog.DAL.Model;
using Blog.DAL.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using TDD.DbTestHelpers.Core;
using System.Data.Entity.Validation;

namespace Blog.DAL.Tests
{
    [TestClass]
    public class RepositoryTests : DbBaseTest<BlogFixtures>
    {
        [TestInitialize]
        public void Init()
        {
            BaseSetUp();
        }

        [TestMethod]
        public void GetAllPost_TwoPostsInDb_ReturnOnePost()
        {
            // arrange
            var repository = new BlogRepository();
            // act
            var result = repository.GetAllPosts();
            // assert
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(DbEntityValidationException))]
        public void AddPost_NullContent_ThrowsException()
        {
            // arrange
            var repository = new BlogRepository();
            // act
            repository.AddPost(new Post() { Id = 10, Author = "Autor" });
            // assert
        }

        [TestMethod]
        public void GetAllCommentsForPost_TwoCommentForOnePostAndOneCommentForSecondPost_ReturnAllTheComents()
        {
            // arrange
            var repository = new BlogRepository();
            // act
            var posts = repository.GetAllPosts();

            var commentsForFirstPost = repository.GetAllCommentsForPost(posts.First());
            var commentsForSecondPost = repository.GetAllCommentsForPost(posts.Last());
            // assert
            Assert.AreEqual(2, commentsForFirstPost.Count());
            Assert.AreEqual(1, commentsForSecondPost.Count());
        }

        [TestMethod]
        public void AddComment_ThreeCommentsInDb_ShouldAddFourthComment()
        {
            // arrange
            var repository = new BlogRepository();
            // act
            var posts = repository.GetAllPosts();
            repository.AddComment(new Comment() {Post = posts.First(), Content = "some comment" });

            var commentsForPost = repository.GetAllCommentsForPost(posts.First());

            // assert
            Assert.AreEqual(3, commentsForPost.Count());
        }
    }
}
