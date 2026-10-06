using Geonorge.AuthLib.Common;
using Geonorge.Download.Models;
using Geonorge.Download.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Geonorge.Download.Tests.Services
{
    public class FileServiceTest
    {
        // HasAccess does not touch the database
        private readonly FileService _fileService = new(NullLogger<FileService>.Instance, null!);

        [Fact]
        public void HasAccessWhenNoRolesAreRequired()
        {
            var file = CreateFile(datasetRole: null, fileRole: null);

            Assert.True(_fileService.HasAccess(file, TestPrincipals.User("testuser")));
        }

        [Fact]
        public void HasAccessWhenUserHasRequiredDatasetRole()
        {
            var file = CreateFile(datasetRole: "role_a, role_b", fileRole: null);

            Assert.True(_fileService.HasAccess(file, TestPrincipals.User("testuser", "role_b")));
        }

        [Fact]
        public void NoAccessWhenUserLacksRequiredDatasetRole()
        {
            var file = CreateFile(datasetRole: "role_a", fileRole: null);

            Assert.False(_fileService.HasAccess(file, TestPrincipals.User("testuser", "role_x")));
        }

        [Fact]
        public void FileRoleOverridesDatasetRole()
        {
            var file = CreateFile(datasetRole: "role_a", fileRole: "role_file");

            Assert.False(_fileService.HasAccess(file, TestPrincipals.User("testuser", "role_a")));
            Assert.True(_fileService.HasAccess(file, TestPrincipals.User("testuser", "role_file")));
        }

        [Fact]
        public void MetadataAdminAlwaysHasAccess()
        {
            var file = CreateFile(datasetRole: "role_a", fileRole: "role_file");

            Assert.True(_fileService.HasAccess(file, TestPrincipals.User("admin", GeonorgeRoles.MetadataAdmin)));
        }

        private static Download.Models.File CreateFile(string? datasetRole, string? fileRole) => new()
        {
            Filename = "test.zip",
            Url = "https://example.com/test.zip",
            AccessConstraintRequiredRole = fileRole,
            Dataset = new Dataset
            {
                AccessConstraint = AccessConstraint.Restricted,
                AccessConstraintRequiredRole = datasetRole
            }
        };
    }
}
