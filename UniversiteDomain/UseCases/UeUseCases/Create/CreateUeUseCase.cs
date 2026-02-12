using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.UeUseCases.Create;

public class CreateUeUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role)
    {
        return Roles.Responsable.Equals(role)
               || Roles.Scolarite.Equals(role);
    }

    public async Task<Ue> ExecuteAsync(string numeroUe, string intitule)
    {
        var ue = new Ue { NumeroUe = numeroUe, Intitule = intitule };
        return await ExecuteAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);
        var created = await repositoryFactory.UeRepository().CreateAsync(ue);
        await repositoryFactory.UeRepository().SaveChangesAsync();
        return created;
    }

    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(repositoryFactory.UeRepository());

        // Intitulé > 3 caractères
        if (ue.Intitule.Length <= 3)
            throw new InvalidIntituleUeException("L’intitulé d’une UE doit contenir plus de 3 caractères.");

        // Numero UE unique
        var existe = await repositoryFactory.UeRepository()
            .FindByConditionAsync(u => u.NumeroUe == ue.NumeroUe);

        if (existe is { Count: > 0 })
            throw new DuplicateNumeroUeException($"{ue.NumeroUe} - ce numéro d’UE existe déjà");
    }
}