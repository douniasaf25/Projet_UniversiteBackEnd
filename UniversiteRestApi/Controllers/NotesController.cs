using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.UseCases.NoteUseCases.Csv;
using UniversiteDomain.UseCases.NoteUseCases.Import;
using System.Security.Claims;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.SecurityUseCases.Get;


namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotesController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // ============================
    // Téléchargement du template CSV
    // ============================
    // Cette méthode permet à la scolarité de télécharger
    // un fichier CSV contenant tous les étudiants d’une UE.
    // La colonne Note est vide ou contient la note existante.
    // GET api/Notes/template/ue/
    [HttpGet("template/ue/{idUe:long}")]
    public async Task<IActionResult> DownloadCsvTemplate(long idUe)
    {
        // Vérification que l'utilisateur est authentifié
        // et récupération de son rôle
        string role = "", email = "";
        IUniversiteUser user;
        try
        {
            CheckSecu(out role, out email, out user);
        }
        catch
        {
            return Unauthorized();
        }

        // Vérification que seul le rôle Scolarité est autorisé
        var uc = new ExportNotesCsvTemplateUseCase(repositoryFactory);
        if (!uc.IsAuthorized(role)) return Unauthorized();

        
        var rows = await uc.ExecuteAsync(idUe);

        await using var mem = new MemoryStream();

        // On laisse le MemoryStream ouvert,
       // sinon le StreamWriter le fermerait automatiquement
        await using (var writer = new StreamWriter(mem, new System.Text.UTF8Encoding(false), leaveOpen: true))
        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(rows);
            await writer.FlushAsync();   
        }
 
        // Conversion du stream en tableau d’octets
        var bytes = mem.ToArray(); // plus de stream à gérer
        var fileName = $"notes_ue_{idUe}.csv";

        // Envoi du fichier au client
        return File(bytes, "text/csv", fileName);
    }
    
    // ============================
    // Import du fichier CSV
    // ============================
    // Cette méthode permet à la scolarité d'uploader
    // un fichier CSV rempli avec les notes.
    [HttpPost("import/ue/{idUe:long}")]
    public async Task<IActionResult> ImportNotes(long idUe, IFormFile file)
    {
        // Vérification présence fichier
        if (file == null || file.Length == 0)
            return BadRequest("Fichier CSV manquant.");

        // Vérification sécurité (authentification + rôle)
        string role = "";
        string email = "";
        IUniversiteUser user = null;

        try
        {
            CheckSecu(out role, out email, out user);
        }
        catch
        {
            return Unauthorized();
        }
        
        var uc = new ImportNotesCsvUseCase(repositoryFactory);
        // Seule la scolarité peut importer les notes
        if (!uc.IsAuthorized(role))
            return Unauthorized(new { message = "Accès refusé : réservé à la scolarité." });

        // Lecture du fichier CSV uploadé
        List<ImportNoteRow> rows;
        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            rows = csv.GetRecords<ImportNoteRow>().ToList();
        }

        // Validation des notes (format + plage 0..20)
        var errors = new List<string>();
        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
  
            // Note vide autorisée (pas de modification)
            if (string.IsNullOrWhiteSpace(r.Note))
                continue;

            // Vérification format numérique
            if (!float.TryParse(r.Note.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                errors.Add($"Ligne {i + 2}: note invalide '{r.Note}'");
            // Vérification règle métier : note entre 0 et 20
            else if (val < 0 || val > 20)
                errors.Add($"Ligne {i + 2}: note hors plage (0..20) : {val}");
        }

        // Si au moins une erreur donc aucune insertion
        if (errors.Count > 0)
            return BadRequest(new { message = "Erreurs CSV", errors });

        // Appel du UseCase pour enregistrer les notes
        await uc.ExecuteAsync(idUe, rows);

        return Ok(new { message = "Import terminé", count = rows.Count });
    }

    private void CheckSecu(out string role, out string email, out IUniversiteUser user)
    {
        role = "";
        ClaimsPrincipal claims = HttpContext.User;
        if (claims.FindFirst(ClaimTypes.Email)==null) throw new UnauthorizedAccessException();
        email = claims.FindFirst(ClaimTypes.Email).Value;
        if (email==null) throw new UnauthorizedAccessException();
        //user = repositoryFactory.UniversiteUserRepository().FindByEmailAsync(email).Result;
        user = new FindUniversiteUserByEmailUseCase(repositoryFactory).ExecuteAsync(email).Result;
        if (user==null) throw new UnauthorizedAccessException();
        if (claims.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException();
        var ident = claims.Identities.FirstOrDefault();
        if (ident == null)throw new UnauthorizedAccessException();
        if (claims.FindFirst(ClaimTypes.Role)==null) throw new UnauthorizedAccessException();
        role = ident.FindFirst(ClaimTypes.Role).Value;
        if (role == null) throw new UnauthorizedAccessException();
    }
}