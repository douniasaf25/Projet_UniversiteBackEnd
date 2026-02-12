using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.UeUseCases.Create;

namespace UniversiteDomainUnitTest;

public class UeUnitTest
{
    [Test]
    public async Task CreateUeUseCase_Success()
    {
        long idUe = 1;
        string numero = "UE101";
        string intitule = "Programmation";

        Ue ueAvant = new Ue { NumeroUe = numero, Intitule = intitule };
        Ue ueFinale = new Ue { Id = idUe, NumeroUe = numero, Intitule = intitule };

        var mockUeRepo = new Mock<IUeRepository>();

        // numéro UE pas déjà utilisé
        mockUeRepo
            .Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>());

        // création renvoie l’UE avec Id
        mockUeRepo
            .Setup(r => r.CreateAsync(ueAvant))
            .ReturnsAsync(ueFinale);

        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);

        var useCase = new CreateUeUseCase(mockFactory.Object);

        var result = await useCase.ExecuteAsync(ueAvant);

        Assert.That(result.Id, Is.EqualTo(idUe));
        Assert.That(result.NumeroUe, Is.EqualTo(numero));
        Assert.That(result.Intitule, Is.EqualTo(intitule));
    }
}