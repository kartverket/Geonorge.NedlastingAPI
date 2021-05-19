using System.Collections.Generic;
using System.Data.Entity;
using EntityFramework.MoqHelper;
using FluentAssertions;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Moq;
using Xunit;
using System.Linq;
using Geonorge.NedlastingApi.V1;

namespace Kartverket.Geonorge.Download.Tests.Services
{
    public class CapabilitiesServiceTest
    {
        [Fact]
        public void GetAreaShouldReturnArea()
        {
            var uuid = "d1422d17-6d95-4ef1-96ab-8af31744dd63";
            var capability = new List<Dataset>();
            var dataset = new Dataset { MetadataUuid = uuid };
            var filListe = new List<File>();
            var file = new File { Division = "kommune", DivisionKey = "0919", Projection = "25833", Format = "FGDB 10.0" };
            file.Dataset = dataset;
            filListe.Add(file);
            capability.Add(dataset);
            dataset.filliste = filListe;

            var capabilitiesService = CreateCapabilitiesService(capability);

            var areas = capabilitiesService.GetAreas(uuid);
            areas.Should().NotBeNull();
        }

        private static CapabilitiesService CreateCapabilitiesService(List<Dataset> dataset)
        {
            var dbContext = CreateDbContextMock(dataset);
            Mock<IRegisterFetcher> registerMock = CreateRegisterFetcherMock();

            return new CapabilitiesService(dbContext, registerMock.Object, null, null);
        }

        private static Mock<IRegisterFetcher> CreateRegisterFetcherMock()
        {
            AreaType area = new AreaType { code = "0919", name = "Kommunenavn" };
            ProjectionType projection = new ProjectionType { code = "25833", name = "Projeksjonnavn" };
            var mockRegister = new Mock<IRegisterFetcher>();
            mockRegister.Setup(m => m.GetArea("kommune", "0919")).Returns(area);
            mockRegister.Setup(m => m.GetProjection("25833")).Returns(projection);

            return mockRegister;
        }

        private static DownloadContext CreateDbContextMock(List<Dataset> dataset)
        {
            Mock<DbSet<Dataset>> mockCapabilities = EntityFrameworkMoqHelper
                .CreateMockForDbSet<Dataset>().SetupForQueryOn(dataset);

            Mock<DbSet<File>> mockFileList = EntityFrameworkMoqHelper
            .CreateMockForDbSet<File>().SetupForQueryOn(dataset[0].filliste.ToList());

            Mock<DownloadContext> mockDbContext = EntityFrameworkMoqHelper
                .CreateMockForDbContext<DownloadContext>();


            mockDbContext.Setup(m => m.Capabilities).Returns(mockCapabilities.Object);
            mockDbContext.Setup(m => m.FileList).Returns(mockFileList.Object);

            return mockDbContext.Object;
        }
    }
}
