using Microsoft.VisualBasic.CompilerServices;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Util;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create
{
    public class CreateParcoursUseCase
    { private readonly IParcoursRepository parcoursRepository;

        public CreateParcoursUseCase(IRepositoryFactory repositoryFactory)
        {
            parcoursRepository = repositoryFactory.ParcoursRepository();
        }
        public bool IsAuthorized(string role)
        {
            return Roles.Responsable.Equals(role)
                   || Roles.Scolarite.Equals(role);
        }
        public async Task<Parcours> ExecuteAsync(string nomParcours, int anneeFormation)
        {
            var parcours = new Parcours
            {
                NomParcours = nomParcours,
                AnneeFormation = anneeFormation
            };
            return await ExecuteAsync(parcours);
        }

        public async Task<Parcours> ExecuteAsync(Parcours parcours)
        {
            await CheckBusinessRules(parcours);
            var p = await parcoursRepository.CreateAsync(parcours);
            parcoursRepository.SaveChangesAsync().Wait();
            return p;
        }

        private async Task CheckBusinessRules(Parcours parcours)
        {
            ArgumentNullException.ThrowIfNull(parcours);
            ArgumentNullException.ThrowIfNull(parcours.NomParcours);
            ArgumentNullException.ThrowIfNull(parcours.AnneeFormation);
            ArgumentNullException.ThrowIfNull(parcoursRepository);

            // Vérifier si le nom du parcours existe déjà
            var existe = await parcoursRepository.FindByConditionAsync(p => p.NomParcours.Equals(parcours.NomParcours));

            if (existe is { Count: > 0 })
                throw new ($"{parcours.NomParcours} - ce nom de parcours existe déjà");

            // Vérification du format de l’année de formation (ex: 2024/2025)
            if (!CheckAnneeFormation.IsValidAnnee(parcours.AnneeFormation))
                throw new InvalidAnneeFormationException($"{parcours.AnneeFormation} - année de formation invalide");

            // Vérification du nom (doit avoir au moins 3 caractères)
            if (parcours.NomParcours.Length < 3)
                throw new ParcoursNotFoundException($"{parcours.NomParcours} incorrect - le nom d’un parcours doit contenir au moins 3 caractères");
        }
    }
}
